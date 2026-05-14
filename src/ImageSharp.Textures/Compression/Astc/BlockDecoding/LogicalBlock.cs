// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// General-purpose ASTC block decoder for blocks the fused fast paths cannot handle —
/// void-extent (spec §C.2.23), multi-partition (spec §C.2.21), and dual-plane (spec §C.2.20).
/// </summary>
internal static class LogicalBlock
{
    /// <summary>
    /// Decodes a block to its UNORM8 RGBA pixels. HDR-endpoint blocks must not reach this
    /// method: the LDR entry points in <see cref="AstcDecoder"/> reject HDR content per
    /// ASTC spec §C.2.19, so every partition's endpoint here is LDR.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DecodeToBytes(UInt128 bits, in BlockInfo info, Footprint footprint, Span<byte> pixels)
    {
        if (!info.IsValid)
        {
            return;
        }

        // Conditional stackalloc isn't legal inside an expression; split the dual-plane case
        // into a separate frame so the secondary-plane buffer is only stackalloc'd when needed.
        if (info.IsDualPlane && !info.IsVoidExtent)
        {
            DecodeToBytesDualPlane(bits, in info, footprint, pixels);
            return;
        }

        EndpointBuffer endpoints = default;
        Span<int> weights = stackalloc int[footprint.PixelCount];
        int[] partitionAssignment = DecodeBlockState(bits, in info, footprint, ref endpoints, weights, default);

        WritePixelsLdr(footprint, pixels, in endpoints, partitionAssignment, weights, default, dualPlaneChannel: -1);
    }

    /// <summary>
    /// Decodes a block to its float RGBA pixels. Accepts both LDR and HDR endpoint modes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DecodeToFloats(UInt128 bits, in BlockInfo info, Footprint footprint, Span<float> pixels)
    {
        if (!info.IsValid)
        {
            return;
        }

        if (info.IsDualPlane && !info.IsVoidExtent)
        {
            DecodeToFloatsDualPlane(bits, in info, footprint, pixels);
            return;
        }

        EndpointBuffer endpoints = default;
        Span<int> weights = stackalloc int[footprint.PixelCount];
        int[] partitionAssignment = DecodeBlockState(bits, in info, footprint, ref endpoints, weights, default);

        WritePixelsHdr(footprint, pixels, in endpoints, partitionAssignment, weights, default, dualPlaneChannel: -1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DecodeToBytesDualPlane(UInt128 bits, in BlockInfo info, Footprint footprint, Span<byte> pixels)
    {
        EndpointBuffer endpoints = default;
        Span<int> weights = stackalloc int[footprint.PixelCount];
        Span<int> secondaryWeights = stackalloc int[footprint.PixelCount];
        int[] partitionAssignment = DecodeBlockState(bits, in info, footprint, ref endpoints, weights, secondaryWeights);

        WritePixelsLdr(footprint, pixels, in endpoints, partitionAssignment, weights, secondaryWeights, info.DualPlaneChannel);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DecodeToFloatsDualPlane(UInt128 bits, in BlockInfo info, Footprint footprint, Span<float> pixels)
    {
        EndpointBuffer endpoints = default;
        Span<int> weights = stackalloc int[footprint.PixelCount];
        Span<int> secondaryWeights = stackalloc int[footprint.PixelCount];
        int[] partitionAssignment = DecodeBlockState(bits, in info, footprint, ref endpoints, weights, secondaryWeights);

        WritePixelsHdr(footprint, pixels, in endpoints, partitionAssignment, weights, secondaryWeights, info.DualPlaneChannel);
    }

    /// <summary>
    /// Populates the endpoints, primary weights, and (for dual-plane blocks) secondary weights,
    /// returning the per-texel partition-subset map. Handles both standard and void-extent
    /// (spec §C.2.23) blocks. <paramref name="secondaryWeights"/> is ignored for non-dual-plane.
    /// </summary>
    private static int[] DecodeBlockState(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        ref EndpointBuffer endpoints,
        Span<int> weights,
        Span<int> secondaryWeights)
    {
        if (info.IsVoidExtent)
        {
            // Spec §C.2.23: bit 9 of the block mode flags HDR (set → FP16, clear → UNORM16 LDR).
            bool isHdrVoidExtent = (bits.Low() & (1UL << 9)) != 0;
            endpoints[0] = DecodeVoidExtentEndpoint(bits, isHdrVoidExtent);
            weights.Clear();
            return Partition.GetSinglePartition(footprint).Assignment;
        }

        DecodeEndpointsFromBits(bits, in info, ref endpoints);
        DecodeAndInfillWeights(bits, in info, footprint, weights, secondaryWeights);
        return ResolvePartitionAssignment(bits, in info, footprint);
    }

    /// <summary>
    /// BISE-decodes (spec §C.2.22) + unquantises (spec §C.2.18) the per-partition color
    /// endpoint values into <paramref name="endpoints"/> (one entry per partition, colour
    /// value count per mode from §C.2.14).
    /// </summary>
    private static void DecodeEndpointsFromBits(UInt128 bits, in BlockInfo info, ref EndpointBuffer endpoints)
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

        int colorIndex = 0;
        for (int i = 0; i < info.PartitionCount; i++)
        {
            ColorEndpointMode mode = info.GetEndpointMode(i);
            int colorCount = mode.GetColorValuesCount();
            ReadOnlySpan<int> slice = colors.Slice(colorIndex, colorCount);
            endpoints[i] = EndpointCodec.Decode(slice, mode);
            colorIndex += colorCount;
        }
    }

    /// <summary>
    /// Returns the cached partition-assignment map. Multi-partition blocks use the 10-bit
    /// partition id from bits [13..22] (spec §C.2.10) and the partition hash function
    /// (spec §C.2.21); single-partition blocks share an all-zero map per footprint.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int[] ResolvePartitionAssignment(UInt128 bits, in BlockInfo info, Footprint footprint)
        => info.PartitionCount > 1
            ? Partition.GetASTCPartition(
                footprint,
                info.PartitionCount,
                (int)BitOperations.GetBits(bits.Low(), 13, 10)).Assignment
            : Partition.GetSinglePartition(footprint).Assignment;

    /// <summary>
    /// BISE-decodes (spec §C.2.22), unquantises (spec §C.2.18), and infills the weight grid
    /// (spec §C.2.17) into <paramref name="primaryWeights"/>. For dual-plane blocks
    /// (spec §C.2.20) the secondary plane is decoded into <paramref name="secondaryWeights"/>;
    /// otherwise <paramref name="secondaryWeights"/> is ignored.
    /// </summary>
    private static void DecodeAndInfillWeights(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        Span<int> primaryWeights,
        Span<int> secondaryWeights)
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
            DecimationTable.InfillWeights(rawWeights[..gridSize], decimationInfo, primaryWeights);
            return;
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

        DecimationTable.InfillWeights(plane0, decimationInfo, primaryWeights);
        DecimationTable.InfillWeights(plane1, decimationInfo, secondaryWeights);
    }

    /// <summary>
    /// Reads the four 16-bit RGBA channels from the high half of a void-extent block
    /// (ASTC spec §C.2.23) and wraps them in a <see cref="ColorEndpointPair"/>. LDR void-extent
    /// channels are UNORM16 (reduced to byte range for the LDR output path); HDR channels are
    /// FP16 bit patterns.
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
    /// Writes UNORM8 RGBA pixels using the decoded block state.
    /// </summary>
    private static void WritePixelsLdr(
        Footprint footprint,
        Span<byte> buffer,
        in EndpointBuffer endpoints,
        int[] partitionAssignment,
        ReadOnlySpan<int> weights,
        ReadOnlySpan<int> secondaryWeights,
        int dualPlaneChannel)
    {
        bool hasDualPlane = !secondaryWeights.IsEmpty;
        int pixelCount = footprint.PixelCount;

        for (int i = 0; i < pixelCount; i++)
        {
            ref readonly ColorEndpointPair endpoint = ref endpoints[partitionAssignment[i]];
            int weight = weights[i];
            int dstOffset = i * 4;

            if (hasDualPlane)
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
                    secondaryWeights[i]);
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
    /// Writes float RGBA pixels using the decoded block state.
    /// </summary>
    /// <remarks>
    /// Per ASTC spec §C.2.15: HDR endpoints are interpolated in LNS, then converted to FP16
    /// via <see cref="Fp16.FromLns"/> and widened to <see cref="float"/>. For mode 14 (HDR RGB
    /// + LDR Alpha, §C.2.14), the alpha channel is UNORM16 instead of LNS. LDR endpoints
    /// produce UNORM16 values normalised to [0, 1] (§C.2.24).
    /// </remarks>
    private static void WritePixelsHdr(
        Footprint footprint,
        Span<float> buffer,
        in EndpointBuffer endpoints,
        int[] partitionAssignment,
        ReadOnlySpan<int> weights,
        ReadOnlySpan<int> secondaryWeights,
        int dualPlaneChannel)
    {
        bool hasDualPlane = !secondaryWeights.IsEmpty;
        int pixelCount = footprint.PixelCount;

        for (int i = 0; i < pixelCount; i++)
        {
            ref readonly ColorEndpointPair endpoint = ref endpoints[partitionAssignment[i]];
            int weight = weights[i];
            int dpWeight = hasDualPlane ? secondaryWeights[i] : weight;
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
                // Mode 14 (spec §C.2.14): alpha is UNORM16, normalise directly.
                pixel[channel] = interpolated / 65535.0f;
            }
            else if (valuesAreLns)
            {
                // Normal HDR block (spec §C.2.15): LNS → FP16 → float.
                pixel[channel] = Fp16.LnsToFloat(interpolated);
            }
            else
            {
                // Void-extent HDR (spec §C.2.23): values are already FP16 bit patterns.
                pixel[channel] = Fp16.Fp16ToFloat(interpolated);
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
    /// Interpolates an LDR channel value and returns the full 16-bit UNORM result
    /// (before reduction to byte). Used by the HDR output path for LDR endpoints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort InterpolateLdrAsUnorm16(int p0, int p1, int weight)
        => (ushort)Math.Clamp(Interpolation.BlendLdrReplicated(p0, p1, weight), 0, 0xFFFF);

    /// <summary>
    /// Interpolates an HDR channel value between two endpoints using the specified weight.
    /// HDR endpoints are already 16-bit values (FP16 bit patterns), unlike LDR interpolation
    /// which expands 8-bit to 16-bit first.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort InterpolateChannelHdr(int p0, int p1, int weight)
        => (ushort)Math.Clamp(Interpolation.BlendWeighted(p0, p1, weight), 0, 0xFFFF);

    /// <summary>
    /// Inline storage for up to 4 per-partition <see cref="ColorEndpointPair"/> values
    /// (spec §C.2.10 caps partition count at 4). Used as a stack-local buffer to hold the
    /// decoded endpoints during a single <see cref="DecodeToBytes"/>/<see cref="DecodeToFloats"/> call.
    /// </summary>
    [InlineArray(4)]
    private struct EndpointBuffer
    {
#pragma warning disable CS0169, IDE0051, S1144 // Accessed by runtime via [InlineArray]
        private ColorEndpointPair element0;
#pragma warning restore CS0169, IDE0051, S1144
    }
}
