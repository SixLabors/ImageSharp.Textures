// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

internal sealed class LogicalBlock
{
    private readonly ColorEndpointPair[] endpoints;
    private readonly int endpointCount;
    private readonly int[] weights;
    private readonly Partition partition;
    private readonly DualPlaneData? dualPlane;

    private LogicalBlock(Footprint footprint, IntermediateBlock.VoidExtentData block)
    {
        this.endpoints = new ColorEndpointPair[1];
        this.endpointCount = DecodeEndpoints(block, this.endpoints);
        this.partition = GenerateSinglePartition(footprint);
        this.weights = new int[footprint.PixelCount];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalBlock"/> class.
    /// Decodes directly from raw bits + BlockInfo,
    /// bypassing IntermediateBlock and using batch unquantize operations.
    /// </summary>
    private LogicalBlock(Footprint footprint, UInt128 bits, in BlockInfo info)
    {
        this.endpoints = DecodeEndpointsFromBits(bits, in info);
        this.endpointCount = info.PartitionCount;
        this.partition = ResolvePartition(bits, in info, footprint);
        this.weights = new int[footprint.PixelCount];
        this.dualPlane = DecodeAndInfillWeights(bits, in info, footprint, this.weights);
    }

    /// <summary>
    /// BISE-decodes + unquantises the per-partition color endpoint values and returns the
    /// decoded <see cref="ColorEndpointPair"/> array (one entry per partition).
    /// </summary>
    private static ColorEndpointPair[] DecodeEndpointsFromBits(UInt128 bits, in BlockInfo info)
    {
        Span<int> colors = stackalloc int[info.ColorValuesCount];
        FusedBlockDecoder.DecodeBiseValues(
            bits,
            info.ColorStartBit,
            info.ColorBitCount,
            info.ColorValuesRange,
            info.ColorValuesCount,
            colors);
        Quantization.UnquantizeCEValuesBatch(colors, info.ColorValuesCount, info.ColorValuesRange);

        ColorEndpointPair[] endpoints = new ColorEndpointPair[info.PartitionCount];
        int colorIndex = 0;
        for (int i = 0; i < info.PartitionCount; i++)
        {
            ColorEndpointMode mode = info.GetEndpointMode(i);
            int colorCount = mode.GetColorValuesCount();
            ReadOnlySpan<int> slice = colors.Slice(colorIndex, colorCount);
            endpoints[i] = EndpointCodec.DecodeColorsForModePolymorphicUnquantized(slice, mode);
            colorIndex += colorCount;
        }

        return endpoints;
    }

    /// <summary>
    /// Returns the cached partition for multi-partition blocks, or a synthetic single
    /// partition that assigns every texel to subset 0.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Partition ResolvePartition(UInt128 bits, in BlockInfo info, Footprint footprint)
        => info.PartitionCount > 1
            ? Partition.GetASTCPartition(
                footprint,
                info.PartitionCount,
                (int)BitOperations.GetBits(bits.Low(), 13, 10))
            : GenerateSinglePartition(footprint);

    /// <summary>
    /// BISE-decodes, unquantises, and infills the weight grid for a block. Fills
    /// <paramref name="texelWeights"/> in place and returns the <see cref="DualPlaneData"/>
    /// if the block uses a dual plane (otherwise null).
    /// </summary>
    private static DualPlaneData? DecodeAndInfillWeights(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        int[] texelWeights)
    {
        int gridSize = info.GridWidth * info.GridHeight;
        bool isDualPlane = info.IsDualPlane;
        int totalWeights = isDualPlane ? gridSize * 2 : gridSize;

        Span<int> rawWeights = stackalloc int[totalWeights];
        FusedBlockDecoder.DecodeBiseWeights(
            bits,
            info.WeightBitCount,
            info.WeightRange,
            totalWeights,
            rawWeights);

        DecimationInfo decimationInfo = DecimationTable.Get(footprint, info.GridWidth, info.GridHeight);

        if (!isDualPlane)
        {
            Quantization.UnquantizeWeightsBatch(rawWeights, gridSize, info.WeightRange);
            DecimationTable.InfillWeights(rawWeights[..gridSize], decimationInfo, texelWeights);
            return null;
        }

        // Dual-plane weights are interleaved: even indices drive the main plane, odd the secondary.
        Span<int> plane0 = stackalloc int[gridSize];
        Span<int> plane1 = stackalloc int[gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            plane0[i] = rawWeights[i * 2];
            plane1[i] = rawWeights[(i * 2) + 1];
        }

        Quantization.UnquantizeWeightsBatch(plane0, gridSize, info.WeightRange);
        Quantization.UnquantizeWeightsBatch(plane1, gridSize, info.WeightRange);

        DecimationTable.InfillWeights(plane0, decimationInfo, texelWeights);

        DualPlaneData dualPlane = new()
        {
            Channel = info.DualPlaneChannel,
            Weights = new int[footprint.PixelCount]
        };
        DecimationTable.InfillWeights(plane1, decimationInfo, dualPlane.Weights);
        return dualPlane;
    }

    /// <summary>
    /// Writes all pixels in the block as RGBA floats into <paramref name="buffer"/>.
    /// </summary>
    /// <remarks>
    /// For HDR endpoints, values are in LNS (Log-Normalized Space). After interpolation
    /// in LNS, the result is converted to FP16 via <see cref="Fp16.FromLns"/> then widened to float.
    /// For Mode 14 (HDR RGB + LDR Alpha), the alpha channel is UNORM16 instead of LNS.
    /// For LDR endpoints, the interpolated UNORM16 value is normalized to 0.0-1.0.
    /// </remarks>
    public void WriteAllPixelsHdr(Footprint footprint, Span<float> buffer)
    {
        int dualPlaneChannel = this.dualPlane?.Channel ?? -1;
        int[]? dualPlaneWeights = this.dualPlane?.Weights;
        int pixelCount = footprint.PixelCount;

        for (int i = 0; i < pixelCount; i++)
        {
            ref ColorEndpointPair endpoint = ref this.endpoints[this.partition.Assignment[i]];
            int weight = this.weights[i];
            int dpWeight = dualPlaneWeights?[i] ?? weight;
            Span<float> pixel = buffer.Slice(i * 4, 4);

            if (endpoint.IsHdr)
            {
                WriteHdrPixelChannels(pixel, in endpoint, weight, dpWeight, dualPlaneChannel);
            }
            else
            {
                WriteLdrAsHdrPixelChannels(pixel, in endpoint, weight, dpWeight, dualPlaneChannel);
            }
        }
    }

    /// <summary>
    /// Writes the four HDR-endpoint channels for a single pixel: LNS → FP16 → float, with
    /// mode-14 alpha (LDR-as-UNORM16) and void-extent (direct FP16) special cases.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrPixelChannels(
        Span<float> pixel,
        in ColorEndpointPair endpoint,
        int weight,
        int dpWeight,
        int dualPlaneChannel)
    {
        bool alphaIsLdr = endpoint.AlphaIsLdr;
        bool valuesAreLns = endpoint.ValuesAreLns;
        for (int channel = 0; channel < 4; ++channel)
        {
            int channelWeight = channel == dualPlaneChannel ? dpWeight : weight;
            ushort interpolated = InterpolateChannelHdr(
                endpoint.HdrLow.GetChannel(channel),
                endpoint.HdrHigh.GetChannel(channel),
                channelWeight);

            if (channel == 3 && alphaIsLdr)
            {
                // Mode 14: alpha is UNORM16, normalise directly.
                pixel[channel] = interpolated / 65535.0f;
            }
            else if (valuesAreLns)
            {
                // Normal HDR block: convert from LNS to FP16, then widen to float.
                pixel[channel] = (float)BitConverter.UInt16BitsToHalf(Fp16.FromLns(interpolated));
            }
            else
            {
                // Void-extent HDR: values are already FP16 bit patterns.
                pixel[channel] = (float)BitConverter.UInt16BitsToHalf(interpolated);
            }
        }
    }

    /// <summary>
    /// Writes the four LDR-endpoint channels for a single pixel as HDR floats: UNORM16 → [0,1].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrAsHdrPixelChannels(
        Span<float> pixel,
        in ColorEndpointPair endpoint,
        int weight,
        int dpWeight,
        int dualPlaneChannel)
    {
        for (int channel = 0; channel < 4; ++channel)
        {
            int channelWeight = channel == dualPlaneChannel ? dpWeight : weight;
            ushort unorm16 = InterpolateLdrAsUnorm16(
                endpoint.LdrLow.GetChannel(channel),
                endpoint.LdrHigh.GetChannel(channel),
                channelWeight);
            pixel[channel] = unorm16 / 65535.0f;
        }
    }

    /// <summary>
    /// Writes all pixels in the block directly to the output buffer in RGBA byte format.
    /// Avoids per-pixel method call overhead, type dispatch, and Rgba32 allocation.
    /// </summary>
    public void WriteAllPixelsLdr(Footprint footprint, Span<byte> buffer)
    {
        ref ColorEndpointPair endpoint0 = ref this.endpoints[0];
        bool isSinglePartitionLdr = !endpoint0.IsHdr && this.partition.PartitionCount == 1;

        if (!isSinglePartitionLdr)
        {
            this.WriteAllPixelsGeneral(footprint, buffer);
            return;
        }

        if (this.dualPlane is null)
        {
            this.WriteLdrSinglePartitionSinglePlane(footprint, buffer, in endpoint0);
        }
        else
        {
            this.WriteLdrSinglePartitionDualPlane(footprint, buffer, in endpoint0, this.dualPlane);
        }
    }

    /// <summary>
    /// Hot-path writer: single-partition LDR block, one weight plane. Every pixel
    /// interpolates the same <paramref name="endpoint"/>.
    /// </summary>
    private void WriteLdrSinglePartitionSinglePlane(Footprint footprint, Span<byte> buffer, in ColorEndpointPair endpoint)
    {
        Rgba32 low = endpoint.LdrLow;
        Rgba32 high = endpoint.LdrHigh;
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            SimdHelpers.WriteSinglePixelLdr(
                buffer,
                i * 4,
                low.R,
                low.G,
                low.B,
                low.A,
                high.R,
                high.G,
                high.B,
                high.A,
                this.weights[i]);
        }
    }

    /// <summary>
    /// Single-partition LDR block with a secondary weight plane (ASTC spec §C.2.20): the
    /// channel identified by <see cref="DualPlaneData.Channel"/> uses the second plane's
    /// weights; the others use the primary plane.
    /// </summary>
    private void WriteLdrSinglePartitionDualPlane(
        Footprint footprint,
        Span<byte> buffer,
        in ColorEndpointPair endpoint,
        DualPlaneData dualPlane)
    {
        Rgba32 low = endpoint.LdrLow;
        Rgba32 high = endpoint.LdrHigh;
        int dpChannel = dualPlane.Channel;
        int[] dpWeights = dualPlane.Weights;
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            SimdHelpers.WriteSinglePixelLdrDualPlane(
                buffer,
                i * 4,
                low.R,
                low.G,
                low.B,
                low.A,
                high.R,
                high.G,
                high.B,
                high.A,
                this.weights[i],
                dpChannel,
                dpWeights[i]);
        }
    }

    public static LogicalBlock? UnpackLogicalBlock(Footprint footprint, UInt128 bits, in BlockInfo info)
    {
        if (!info.IsValid)
        {
            return null;
        }

        if (info.IsVoidExtent)
        {
            // Void extent blocks are rare; fall back to existing PhysicalBlock path
            PhysicalBlock pb = PhysicalBlock.Create(bits);
            IntermediateBlock.VoidExtentData? voidExtentData = IntermediateBlock.UnpackVoidExtent(pb);
            if (voidExtentData is null)
            {
                return null;
            }

            return new LogicalBlock(footprint, voidExtentData.Value);
        }
        else
        {
            return new LogicalBlock(footprint, bits, in info);
        }
    }

    private static int DecodeEndpoints(IntermediateBlock.VoidExtentData block, ColorEndpointPair[] endpointPair)
    {
        if (block.IsHdr)
        {
            // HDR void extent: ushort values are FP16 bit patterns (not LNS)
            Rgba64 hdrColor = new(block.R, block.G, block.B, block.A);
            endpointPair[0] = ColorEndpointPair.Hdr(hdrColor, hdrColor, valuesAreLns: false);
        }
        else
        {
            // LDR void extent: ushort values are UNORM16, convert to byte range
            Rgba32 ldrColor = new(
                (byte)(block.R >> 8),
                (byte)(block.G >> 8),
                (byte)(block.B >> 8),
                (byte)(block.A >> 8));
            endpointPair[0] = ColorEndpointPair.Ldr(ldrColor, ldrColor);
        }

        return 1;
    }

    private static Partition GenerateSinglePartition(Footprint footprint) => new(footprint, 1, 0)
    {
        Assignment = new int[footprint.PixelCount]
    };

    /// <summary>
    /// Interpolates an LDR channel value and returns the full 16-bit UNORM result
    /// (before reduction to byte). Used by the HDR output path for LDR endpoints.
    /// </summary>
    private static ushort InterpolateLdrAsUnorm16(int p0, int p1, int weight)
    {
        int c0 = (p0 << 8) | p0;
        int c1 = (p1 << 8) | p1;
        int c = ((c0 * (64 - weight)) + (c1 * weight) + 32) / 64;
        return (ushort)Math.Clamp(c, 0, 0xFFFF);
    }

    /// <summary>
    /// Interpolates an HDR channel value between two endpoints using the specified weight.
    /// </summary>
    /// <remarks>
    /// HDR endpoints are already 16-bit values (FP16 bit patterns). Unlike LDR interpolation
    /// which expands 8-bit to 16-bit before interpolating, HDR interpolation operates directly
    /// on the 16-bit values
    /// </remarks>
    private static ushort InterpolateChannelHdr(int p0, int p1, int weight)
    {
        int c = ((p0 * (64 - weight)) + (p1 * weight) + 32) / 64;
        return (ushort)Math.Clamp(c, 0, 0xFFFF);
    }

    /// <summary>
    /// General writer for the LDR output pipeline. Handles multi-partition blocks and
    /// blocks whose partition(s) use HDR endpoint modes decoded into an LDR byte buffer
    /// (ASTC spec §C.2.14 allows per-partition HDR/LDR mixing).
    /// </summary>
    private void WriteAllPixelsGeneral(Footprint footprint, Span<byte> buffer)
    {
        int dualPlaneChannel = this.dualPlane?.Channel ?? -1;
        int[]? dualPlaneWeights = this.dualPlane?.Weights;
        int pixelCount = footprint.PixelCount;

        for (int i = 0; i < pixelCount; i++)
        {
            int part = this.partition.Assignment[i];
            ref ColorEndpointPair endpoint = ref this.endpoints[part];
            int weight = this.weights[i];
            int dpWeight = dualPlaneWeights?[i] ?? weight;
            int dstOffset = i * 4;

            if (endpoint.IsHdr)
            {
                WriteHdrAsLdrPixel(buffer, dstOffset, in endpoint, weight, dpWeight, dualPlaneChannel);
            }
            else if (dualPlaneWeights is not null)
            {
                SimdHelpers.WriteSinglePixelLdrDualPlane(
                    buffer,
                    dstOffset,
                    endpoint.LdrLow.R,
                    endpoint.LdrLow.G,
                    endpoint.LdrLow.B,
                    endpoint.LdrLow.A,
                    endpoint.LdrHigh.R,
                    endpoint.LdrHigh.G,
                    endpoint.LdrHigh.B,
                    endpoint.LdrHigh.A,
                    weight,
                    dualPlaneChannel,
                    dpWeight);
            }
            else
            {
                SimdHelpers.WriteSinglePixelLdr(
                    buffer,
                    dstOffset,
                    endpoint.LdrLow.R,
                    endpoint.LdrLow.G,
                    endpoint.LdrLow.B,
                    endpoint.LdrLow.A,
                    endpoint.LdrHigh.R,
                    endpoint.LdrHigh.G,
                    endpoint.LdrHigh.B,
                    endpoint.LdrHigh.A,
                    weight);
            }
        }
    }

    /// <summary>
    /// Interpolates the four HDR channels of a single pixel and narrows each to the LDR
    /// byte range with a <c>&gt;&gt; 8</c> truncation. The dual-plane weight applies only
    /// to <paramref name="dualPlaneChannel"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrAsLdrPixel(
        Span<byte> buffer,
        int dstOffset,
        in ColorEndpointPair endpoint,
        int weight,
        int dpWeight,
        int dualPlaneChannel)
    {
        int rWeight = dualPlaneChannel == 0 ? dpWeight : weight;
        int gWeight = dualPlaneChannel == 1 ? dpWeight : weight;
        int bWeight = dualPlaneChannel == 2 ? dpWeight : weight;
        int aWeight = dualPlaneChannel == 3 ? dpWeight : weight;
        buffer[dstOffset + 0] = (byte)(InterpolateChannelHdr(endpoint.HdrLow.R, endpoint.HdrHigh.R, rWeight) >> 8);
        buffer[dstOffset + 1] = (byte)(InterpolateChannelHdr(endpoint.HdrLow.G, endpoint.HdrHigh.G, gWeight) >> 8);
        buffer[dstOffset + 2] = (byte)(InterpolateChannelHdr(endpoint.HdrLow.B, endpoint.HdrHigh.B, bWeight) >> 8);
        buffer[dstOffset + 3] = (byte)(InterpolateChannelHdr(endpoint.HdrLow.A, endpoint.HdrHigh.A, aWeight) >> 8);
    }

    private class DualPlaneData
    {
        public int Channel { get; set; }

        public int[] Weights { get; set; } = [];
    }
}
