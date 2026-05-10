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
    private readonly int[] weights;
    private readonly Partition partition;
    private readonly DualPlaneData? dualPlane;

    private LogicalBlock(Footprint footprint, UInt128 bits, bool isHdrVoidExtent)
    {
        this.endpoints = [DecodeVoidExtentEndpoint(bits, isHdrVoidExtent)];
        this.partition = Partition.GetSinglePartition(footprint);
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
        this.partition = ResolvePartition(bits, in info, footprint);
        this.weights = new int[footprint.PixelCount];
        this.dualPlane = DecodeAndInfillWeights(bits, in info, footprint, this.weights);
    }

    /// <summary>
    /// BISE-decodes (spec §C.2.22) + unquantises (spec §C.2.18) the per-partition color
    /// endpoint values and returns the decoded <see cref="ColorEndpointPair"/> array (one
    /// entry per partition, colour value count per mode from §C.2.14).
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
    /// Returns the cached partition-assignment map for this block. Multi-partition blocks
    /// use the 10-bit partition id from bits [13..22] (spec §C.2.10) to look up the partition
    /// hash result (spec §C.2.21); single-partition blocks get a synthetic all-zero map.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Partition ResolvePartition(UInt128 bits, in BlockInfo info, Footprint footprint)
        => info.PartitionCount > 1
            ? Partition.GetASTCPartition(
                footprint,
                info.PartitionCount,
                (int)BitOperations.GetBits(bits.Low(), 13, 10))
            : Partition.GetSinglePartition(footprint);

    /// <summary>
    /// BISE-decodes (spec §C.2.22), unquantises (spec §C.2.18), and infills the weight grid
    /// (spec §C.2.17). Fills <paramref name="texelWeights"/> in place and returns the
    /// <see cref="DualPlaneData"/> if the block uses a dual plane (spec §C.2.20), otherwise null.
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

        // Spec §C.2.20: the two planes' weights are interleaved — even indices drive the
        // main plane, odd the secondary plane.
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

        int[] secondaryWeights = new int[footprint.PixelCount];
        DecimationTable.InfillWeights(plane1, decimationInfo, secondaryWeights);
        return new DualPlaneData(info.DualPlaneChannel, secondaryWeights);
    }

    /// <summary>
    /// Writes all pixels in the block as RGBA floats into <paramref name="buffer"/>.
    /// </summary>
    /// <remarks>
    /// Per ASTC spec §C.2.15: HDR endpoints are interpolated in LNS (Log-Normalised Space),
    /// then the result is converted to FP16 via <see cref="Fp16.FromLns"/> and widened to float.
    /// For mode 14 (HDR RGB + LDR Alpha, §C.2.14), the alpha channel is UNORM16 instead of LNS.
    /// LDR endpoints produce UNORM16 values that are normalised to 0.0-1.0 (§C.2.24).
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
    /// Writes the four HDR-endpoint channels for a single pixel per ASTC spec §C.2.15: LNS →
    /// FP16 → float. Mode 14 alpha is LDR-as-UNORM16 (§C.2.14); HDR void-extent values are
    /// already FP16 bit patterns (§C.2.23) and skip the LNS conversion.
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
            this.WriteLdrSinglePartitionDualPlane(footprint, buffer, in endpoint0, this.dualPlane.Value);
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
            // ASTC spec §C.2.23: bit 9 of the block mode is the HDR flag for void-extent
            // (set → FP16, clear → UNORM16 LDR). Matches ARM's astcenc reference decoder.
            bool isHdrVoidExtent = (bits.Low() & (1UL << 9)) != 0;
            return new LogicalBlock(footprint, bits, isHdrVoidExtent);
        }

        return new LogicalBlock(footprint, bits, in info);
    }

    /// <summary>
    /// Reads the four 16-bit RGBA channels from the high half of a void-extent block (ASTC spec §C.2.23)
    /// and wraps them in a <see cref="ColorEndpointPair"/>. LDR void-extent channels are UNORM16
    /// (reduced to the byte range for the LDR output path); HDR channels are FP16 bit patterns.
    /// </summary>
    private static ColorEndpointPair DecodeVoidExtentEndpoint(UInt128 bits, bool isHdr)
    {
        ulong high = bits.High();
        ushort r = (ushort)(high & 0xFFFF);
        ushort g = (ushort)((high >> 16) & 0xFFFF);
        ushort b = (ushort)((high >> 32) & 0xFFFF);
        ushort a = (ushort)((high >> 48) & 0xFFFF);

        if (isHdr)
        {
            Rgba64 hdrColor = new(r, g, b, a);
            return ColorEndpointPair.Hdr(hdrColor, hdrColor, valuesAreLns: false);
        }

        Rgba32 ldrColor = new((byte)(r >> 8), (byte)(g >> 8), (byte)(b >> 8), (byte)(a >> 8));
        return ColorEndpointPair.Ldr(ldrColor, ldrColor);
    }

    /// <summary>
    /// Interpolates an LDR channel value and returns the full 16-bit UNORM result
    /// (before reduction to byte). Used by the HDR output path for LDR endpoints.
    /// </summary>
    private static ushort InterpolateLdrAsUnorm16(int p0, int p1, int weight)
        => (ushort)Math.Clamp(Interpolation.BlendLdrReplicated(p0, p1, weight), 0, 0xFFFF);

    /// <summary>
    /// Interpolates an HDR channel value between two endpoints using the specified weight.
    /// HDR endpoints are already 16-bit values (FP16 bit patterns), unlike LDR interpolation
    /// which expands 8-bit to 16-bit first.
    /// </summary>
    private static ushort InterpolateChannelHdr(int p0, int p1, int weight)
        => (ushort)Math.Clamp(Interpolation.BlendWeighted(p0, p1, weight), 0, 0xFFFF);

    /// <summary>
    /// General writer for the LDR output pipeline. Handles multi-partition blocks and
    /// single-partition dual-plane blocks. HDR-endpoint blocks never reach this method: the
    /// LDR entry points in <see cref="AstcDecoder"/> throw on any block with an HDR endpoint
    /// mode or HDR void-extent, so every partition's endpoint is LDR by the time we get here.
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
            int dstOffset = i * 4;

            if (dualPlaneWeights is not null)
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
                    dualPlaneWeights[i]);
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
}
