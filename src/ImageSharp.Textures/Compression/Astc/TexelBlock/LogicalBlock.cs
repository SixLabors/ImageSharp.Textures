// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

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
        // --- BISE decode + batch unquantize color endpoint values ---
        Span<int> colors = stackalloc int[info.ColorValuesCount];
        FusedBlockDecoder.DecodeBiseValues(
            bits,
            info.ColorStartBit,
            info.ColorBitCount,
            info.ColorValuesRange,
            info.ColorValuesCount,
            colors);
        Quantization.UnquantizeCEValuesBatch(colors, info.ColorValuesCount, info.ColorValuesRange);

        // --- Decode endpoints per partition ---
        this.endpointCount = info.PartitionCount;
        this.endpoints = new ColorEndpointPair[this.endpointCount];
        int colorIndex = 0;
        for (int i = 0; i < this.endpointCount; i++)
        {
            ColorEndpointMode mode = info.GetEndpointMode(i);
            int colorCount = mode.GetColorValuesCount();
            ReadOnlySpan<int> slice = colors.Slice(colorIndex, colorCount);
            this.endpoints[i] = EndpointCodec.DecodeColorsForModePolymorphicUnquantized(slice, mode);
            colorIndex += colorCount;
        }

        // --- Set up partition ---
        this.partition = info.PartitionCount > 1
            ? Partition.GetASTCPartition(
                footprint,
                info.PartitionCount,
                (int)BitOperations.GetBits(bits.Low(), 13, 10))
            : GenerateSinglePartition(footprint);

        // --- BISE decode + unquantize + infill weights ---
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
        this.weights = new int[footprint.PixelCount];

        if (!isDualPlane)
        {
            Quantization.UnquantizeWeightsBatch(rawWeights, gridSize, info.WeightRange);
            DecimationTable.InfillWeights(rawWeights[..gridSize], decimationInfo, this.weights);
        }
        else
        {
            // De-interleave: even indices -> plane0, odd indices -> plane1
            Span<int> plane0 = stackalloc int[gridSize];
            Span<int> plane1 = stackalloc int[gridSize];
            for (int i = 0; i < gridSize; i++)
            {
                plane0[i] = rawWeights[i * 2];
                plane1[i] = rawWeights[(i * 2) + 1];
            }

            Quantization.UnquantizeWeightsBatch(plane0, gridSize, info.WeightRange);
            Quantization.UnquantizeWeightsBatch(plane1, gridSize, info.WeightRange);

            DecimationTable.InfillWeights(plane0, decimationInfo, this.weights);

            this.dualPlane = new DualPlaneData
            {
                Channel = info.DualPlaneChannel,
                Weights = new int[footprint.PixelCount]
            };
            DecimationTable.InfillWeights(plane1, decimationInfo, this.dualPlane.Weights);
        }
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
            int part = this.partition.Assignment[i];
            ref ColorEndpointPair endpoint = ref this.endpoints[part];

            int weight = this.weights[i];
            int dpWeight = dualPlaneWeights?[i] ?? weight;
            int dstOffset = i * 4;

            if (endpoint.IsHdr)
            {
                for (int channel = 0; channel < 4; ++channel)
                {
                    int channelWeight = (channel == dualPlaneChannel) ? dpWeight : weight;
                    ushort interpolated = InterpolateChannelHdr(
                        endpoint.HdrLow.GetChannel(channel),
                        endpoint.HdrHigh.GetChannel(channel),
                        channelWeight);

                    if (channel == 3 && endpoint.AlphaIsLdr)
                    {
                        // Mode 14: alpha is UNORM16, normalize directly
                        buffer[dstOffset + channel] = interpolated / 65535.0f;
                    }
                    else if (endpoint.ValuesAreLns)
                    {
                        // Normal HDR block: convert from LNS to FP16, then to float
                        ushort halfFloatBits = Fp16.FromLns(interpolated);
                        buffer[dstOffset + channel] = (float)BitConverter.UInt16BitsToHalf(halfFloatBits);
                    }
                    else
                    {
                        // Void extent HDR: values are already FP16 bit patterns
                        buffer[dstOffset + channel] = (float)BitConverter.UInt16BitsToHalf(interpolated);
                    }
                }
            }
            else
            {
                for (int channel = 0; channel < 4; ++channel)
                {
                    int channelWeight = (channel == dualPlaneChannel) ? dpWeight : weight;
                    ushort unorm16 = InterpolateLdrAsUnorm16(
                        endpoint.LdrLow.GetChannel(channel),
                        endpoint.LdrHigh.GetChannel(channel),
                        channelWeight);
                    buffer[dstOffset + channel] = unorm16 / 65535.0f;
                }
            }
        }
    }

    /// <summary>
    /// Writes all pixels in the block directly to the output buffer in RGBA byte format.
    /// Avoids per-pixel method call overhead, type dispatch, and Rgba32 allocation.
    /// </summary>
    public void WriteAllPixelsLdr(Footprint footprint, Span<byte> buffer)
    {
        ref ColorEndpointPair endpoint0 = ref this.endpoints[0];

        if (!endpoint0.IsHdr && this.partition.PartitionCount == 1)
        {
            // Fast path: single-partition LDR block (most common case)
            int lowR = endpoint0.LdrLow.R, lowG = endpoint0.LdrLow.G, lowB = endpoint0.LdrLow.B, lowA = endpoint0.LdrLow.A;
            int highR = endpoint0.LdrHigh.R, highG = endpoint0.LdrHigh.G, highB = endpoint0.LdrHigh.B, highA = endpoint0.LdrHigh.A;

            if (this.dualPlane == null)
            {
                this.WriteLdrSinglePartition(buffer, footprint, lowR, lowG, lowB, lowA, highR, highG, highB, highA);
            }
            else
            {
                int dualPlaneChannel = this.dualPlane.Channel;
                int[] dpWeights = this.dualPlane.Weights;
                int pixelCount = footprint.PixelCount;
                for (int i = 0; i < pixelCount; i++)
                {
                    SimdHelpers.WriteSinglePixelLdrDualPlane(
                        buffer,
                        i * 4,
                        lowR,
                        lowG,
                        lowB,
                        lowA,
                        highR,
                        highG,
                        highB,
                        highA,
                        this.weights[i],
                        dualPlaneChannel,
                        dpWeights[i]);
                }
            }
        }
        else
        {
            // General path: multi-partition or HDR blocks
            this.WriteAllPixelsGeneral(footprint, buffer);
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

    private void WriteLdrSinglePartition(
        Span<byte> buffer,
        Footprint footprint,
        int lowR,
        int lowG,
        int lowB,
        int lowA,
        int highR,
        int highG,
        int highB,
        int highA)
    {
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            SimdHelpers.WriteSinglePixelLdr(
                buffer,
                i * 4,
                lowR,
                lowG,
                lowB,
                lowA,
                highR,
                highG,
                highB,
                highA,
                this.weights[i]);
        }
    }

    private void WriteAllPixelsGeneral(Footprint footprint, Span<byte> buffer)
    {
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            int part = this.partition.Assignment[i];
            ref ColorEndpointPair endpoint = ref this.endpoints[part];

            int weight = this.weights[i];
            if (!endpoint.IsHdr)
            {
                if (this.dualPlane is not null)
                {
                    SimdHelpers.WriteSinglePixelLdrDualPlane(
                        buffer,
                        i * 4,
                        endpoint.LdrLow.R,
                        endpoint.LdrLow.G,
                        endpoint.LdrLow.B,
                        endpoint.LdrLow.A,
                        endpoint.LdrHigh.R,
                        endpoint.LdrHigh.G,
                        endpoint.LdrHigh.B,
                        endpoint.LdrHigh.A,
                        weight,
                        this.dualPlane.Channel,
                        this.dualPlane.Weights[i]);
                }
                else
                {
                    SimdHelpers.WriteSinglePixelLdr(
                        buffer,
                        i * 4,
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
            else
            {
                int dualPlaneChannel = this.dualPlane?.Channel ?? -1;
                int dualPlaneWeight = this.dualPlane?.Weights[i] ?? weight;
                int rWeight = dualPlaneChannel == 0 ? dualPlaneWeight : weight;
                int gWeight = dualPlaneChannel == 1 ? dualPlaneWeight : weight;
                int bWeight = dualPlaneChannel == 2 ? dualPlaneWeight : weight;
                int aWeight = dualPlaneChannel == 3 ? dualPlaneWeight : weight;
                buffer[(i * 4) + 0] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow.R,
                    endpoint.HdrHigh.R,
                    rWeight) >> 8);
                buffer[(i * 4) + 1] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow.G,
                    endpoint.HdrHigh.G,
                    gWeight) >> 8);
                buffer[(i * 4) + 2] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow.B,
                    endpoint.HdrHigh.B,
                    bWeight) >> 8);
                buffer[(i * 4) + 3] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow.A,
                    endpoint.HdrHigh.A,
                    aWeight) >> 8);
            }
        }
    }

    private class DualPlaneData
    {
        public int Channel { get; set; }

        public int[] Weights { get; set; } = [];
    }
}
