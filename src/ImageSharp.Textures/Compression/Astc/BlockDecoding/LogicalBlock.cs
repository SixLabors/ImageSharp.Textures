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
    /// ASTC spec §C.2.19, so every partition's endpoint here is LDR. <typeparamref name="TMode"/>
    /// selects linear vs sRGB decode (ASTC spec §C.2.19).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DecodeToBytes<TMode>(UInt128 bits, in BlockInfo info, Footprint footprint, Span<byte> pixels)
        where TMode : struct, ILdrColorMode
    {
        if (!info.IsValid)
        {
            return;
        }

        if (info.DualPlane.Enabled && !info.IsVoidExtent)
        {
            DecodeDualPlane<LdrPixelWriter<TMode>, byte>(bits, in info, footprint, pixels);
            return;
        }

        // Up to 12×12 = 144 ints (576 bytes) for the largest 2D footprint per spec §C.2.4.
        Span<int> weights = stackalloc int[footprint.PixelCount];
        DecodedBlockState state = DecodeSinglePlane(bits, in info, footprint, weights);

        WriteAllPixels<LdrPixelWriter<TMode>, byte>(footprint, pixels, in state);
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

        if (info.DualPlane.Enabled && !info.IsVoidExtent)
        {
            DecodeDualPlane<HdrPixelWriter, float>(bits, in info, footprint, pixels);
            return;
        }

        // Up to 12×12 = 144 ints (576 bytes) for the largest 2D footprint per spec §C.2.4.
        Span<int> weights = stackalloc int[footprint.PixelCount];
        DecodedBlockState state = DecodeSinglePlane(bits, in info, footprint, weights);

        WriteAllPixels<HdrPixelWriter, float>(footprint, pixels, in state);
    }

    /// <summary>
    /// Decodes a dual-plane block (spec §C.2.20) and writes its pixels via
    /// <typeparamref name="TWriter"/>. Split into its own non-inlined frame so the
    /// secondary-plane buffer is only allocated for dual-plane blocks.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DecodeDualPlane<TWriter, T>(UInt128 bits, in BlockInfo info, Footprint footprint, Span<T> pixels)
        where TWriter : struct, IPixelWriter<T>
        where T : unmanaged
    {
        // Two weight planes for dual-plane blocks (spec §C.2.20). Up to 2 × 144 = 288 ints (1152 bytes) at the largest 12×12 footprint.
        Span<int> weights = stackalloc int[footprint.PixelCount];
        Span<int> secondaryWeights = stackalloc int[footprint.PixelCount];
        DecodedBlockState state = DecodeDualPlaneState(bits, in info, footprint, weights, secondaryWeights);
        DualPlane dualPlane = new() { Weights = secondaryWeights, Channel = info.DualPlane.Channel };

        WriteAllPixelsDualPlane<TWriter, T>(footprint, pixels, in state, in dualPlane);
    }

    /// <summary>
    /// Builds the <see cref="DecodedBlockState"/> for a single-plane or void-extent block.
    /// </summary>
    private static DecodedBlockState DecodeSinglePlane(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        Span<int> weights)
    {
        DecodedBlockState state = default;
        state.Weights = weights;

        if (info.IsVoidExtent)
        {
            state.Endpoints[0] = DecodeVoidExtentEndpoint(bits, info.IsHdr);
            weights.Clear();
            state.PartitionAssignment = Partition.GetSinglePartition(footprint).Assignment;
            return state;
        }

        DecodeEndpointsFromBits(bits, in info, ref state.Endpoints);
        DecodeAndInfillWeights(bits, in info, footprint, weights, default);
        state.PartitionAssignment = ResolvePartitionAssignment(bits, info.PartitionCount, footprint);
        return state;
    }

    /// <summary>
    /// Builds the <see cref="DecodedBlockState"/> for a dual-plane block (spec §C.2.20),
    /// filling <paramref name="secondaryWeights"/> with the second plane's per-texel weights.
    /// </summary>
    private static DecodedBlockState DecodeDualPlaneState(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        Span<int> weights,
        Span<int> secondaryWeights)
    {
        DecodedBlockState state = default;
        state.Weights = weights;
        DecodeEndpointsFromBits(bits, in info, ref state.Endpoints);
        DecodeAndInfillWeights(bits, in info, footprint, weights, secondaryWeights);
        state.PartitionAssignment = ResolvePartitionAssignment(bits, info.PartitionCount, footprint);
        return state;
    }

    /// <summary>
    /// BISE-decodes (spec §C.2.12) + unquantises (spec §C.2.13) the per-partition color
    /// endpoint values into <paramref name="endpoints"/> (one entry per partition, colour
    /// value count per mode from §C.2.14).
    /// </summary>
    private static void DecodeEndpointsFromBits(UInt128 bits, in BlockInfo info, ref EndpointBuffer endpoints)
    {
        // Up to BlockInfo.MaxColorValues ints — BlockModeDecoder rejects blocks that exceed it.
        Span<int> colors = stackalloc int[info.Colors.Count];
        FusedBlockDecoder.DecodeBiseValues(
            bits,
            info.Colors.StartBit,
            info.Colors.BitCount,
            info.Colors.Range,
            info.Colors.Count,
            colors);
        Quantization.UnquantizeCEValuesBatch(colors, info.Colors.Range);

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
    private static ReadOnlySpan<int> ResolvePartitionAssignment(UInt128 bits, int partitionCount, Footprint footprint)
        => partitionCount > 1
            ? Partition.GetASTCPartition(
                footprint,
                partitionCount,
                (int)BitOperations.GetBits(bits.Low(), 13, 10)).Assignment
            : Partition.GetSinglePartition(footprint).Assignment;

    /// <summary>
    /// BISE-decodes (spec §C.2.12), unquantises (spec §C.2.17), and infills the weight grid
    /// (spec §C.2.18) into <paramref name="primaryWeights"/>. For dual-plane blocks
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
        int gridSize = info.Weights.Width * info.Weights.Height;
        bool isDualPlane = info.DualPlane.Enabled;
        int totalWeights = isDualPlane ? gridSize * 2 : gridSize;

        // Up to 128 ints (512 bytes) — spec §C.2.11 caps total weights (gridSize × planes) at 64
        // for single-plane and 128 for dual-plane.
        Span<int> rawWeights = stackalloc int[totalWeights];
        FusedBlockDecoder.DecodeBiseWeights(
            bits,
            info.Weights.BitCount,
            info.Weights.Range,
            totalWeights,
            rawWeights);

        DecimationInfo decimationInfo = DecimationTable.Get(footprint, info.Weights.Width, info.Weights.Height);

        if (!isDualPlane)
        {
            Quantization.UnquantizeWeightsBatch(rawWeights, info.Weights.Range);
            DecimationTable.InfillWeights(rawWeights[..gridSize], decimationInfo, primaryWeights);
            return;
        }

        // Spec §C.2.20: the two planes' weights are interleaved — even indices drive the
        // main plane, odd the secondary plane. Each plane has up to 64 ints, spec
        // §C.2.11 caps gridSize × 2 ≤ 128, so gridSize ≤ 64 for dual-plane.
        Span<int> plane0 = stackalloc int[gridSize];
        Span<int> plane1 = stackalloc int[gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            plane0[i] = rawWeights[i * 2];
            plane1[i] = rawWeights[(i * 2) + 1];
        }

        Quantization.UnquantizeWeightsBatch(plane0, info.Weights.Range);
        Quantization.UnquantizeWeightsBatch(plane1, info.Weights.Range);

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
    /// Generic single-plane pixel-write loop. Each iteration looks up the partition's
    /// endpoint and dispatches to <typeparamref name="TWriter"/> for the actual write.
    /// Constraining <typeparamref name="TWriter"/> to a struct allows the JIT to specialise
    /// and inline the per-pixel call.
    /// </summary>
    private static void WriteAllPixels<TWriter, T>(Footprint footprint, Span<T> buffer, in DecodedBlockState state)
        where TWriter : struct, IPixelWriter<T>
        where T : unmanaged
    {
        TWriter writer = default;
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            ref readonly ColorEndpointPair endpoint = ref state.Endpoints[state.PartitionAssignment[i]];
            writer.WritePixel(buffer, i * BlockInfo.ChannelsPerPixel, in endpoint, state.Weights[i]);
        }
    }

    /// <summary>
    /// Generic dual-plane pixel-write loop (ASTC spec §C.2.20). Same shape as
    /// <see cref="WriteAllPixels{TWriter,T}"/> but the channel named by
    /// <paramref name="dualPlane"/> uses the secondary plane's per-texel weight.
    /// </summary>
    private static void WriteAllPixelsDualPlane<TWriter, T>(
        Footprint footprint,
        Span<T> buffer,
        in DecodedBlockState state,
        in DualPlane dualPlane)
        where TWriter : struct, IPixelWriter<T>
        where T : unmanaged
    {
        TWriter writer = default;
        int dpChannel = dualPlane.Channel;
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            ref readonly ColorEndpointPair endpoint = ref state.Endpoints[state.PartitionAssignment[i]];
            writer.WritePixelDualPlane(buffer, i * BlockInfo.ChannelsPerPixel, in endpoint, state.Weights[i], dpChannel, dualPlane.Weights[i]);
        }
    }

    /// <summary>
    /// Writes a block's LDR pixels from decoded components
    /// </summary>
    /// <param name="footprint">The block's footprint</param>
    /// <param name="endpoints">Per-partition colour endpoints</param>
    /// <param name="partitionAssignment">Per-texel partition assignment</param>
    /// <param name="weights">Per-texel weights</param>
    /// <param name="pixels">The output pixel buffer</param>
    public static void WriteDecodedLdr<TMode>(
        Footprint footprint,
        ReadOnlySpan<ColorEndpointPair> endpoints,
        ReadOnlySpan<int> partitionAssignment,
        Span<int> weights,
        Span<byte> pixels)
        where TMode : struct, ILdrColorMode
    {
        DecodedBlockState state = BuildState(endpoints, partitionAssignment, weights);
        WriteAllPixels<LdrPixelWriter<TMode>, byte>(footprint, pixels, in state);
    }

    /// <summary>
    /// Dual-plane variant of <see cref="WriteDecodedLdr{TMode}"/> (ASTC spec §C.2.20).
    /// </summary>
    /// <param name="footprint">The block's footprint</param>
    /// <param name="endpoints">Per-partition colour endpoints</param>
    /// <param name="partitionAssignment">Per-texel partition assignment</param>
    /// <param name="weights">Per-texel weights</param>
    /// <param name="secondaryWeights">The secondary plane's per-texel weights</param>
    /// <param name="dualPlaneChannel">The channel whose per-texel weights are used instead of the primary plane's</param>
    /// <param name="pixels">The output pixel buffer</param>
    public static void WriteDecodedLdrDualPlane<TMode>(
        Footprint footprint,
        ReadOnlySpan<ColorEndpointPair> endpoints,
        ReadOnlySpan<int> partitionAssignment,
        Span<int> weights,
        Span<int> secondaryWeights,
        int dualPlaneChannel,
        Span<byte> pixels)
        where TMode : struct, ILdrColorMode
    {
        DecodedBlockState state = BuildState(endpoints, partitionAssignment, weights);
        DualPlane dualPlane = new() { Weights = secondaryWeights, Channel = dualPlaneChannel };

        WriteAllPixelsDualPlane<LdrPixelWriter<TMode>, byte>(footprint, pixels, in state, in dualPlane);
    }

    /// <summary>
    /// Assembles a <see cref="DecodedBlockState"/> from externally decoded per-subset endpoints,
    /// per-texel partition assignment, and per-texel weights.
    /// </summary>
    private static DecodedBlockState BuildState(
        ReadOnlySpan<ColorEndpointPair> endpoints,
        ReadOnlySpan<int> partitionAssignment,
        Span<int> weights)
    {
        DecodedBlockState state = default;
        for (int i = 0; i < endpoints.Length; i++)
        {
            state.Endpoints[i] = endpoints[i];
        }

        state.Weights = weights;
        state.PartitionAssignment = partitionAssignment;
        return state;
    }

    /// <summary>
    /// Inline storage for up to 4 per-partition <see cref="ColorEndpointPair"/> values (spec §C.2.10)
    /// </summary>
    [InlineArray(BlockInfo.MaxPartitionCount)]
    private struct EndpointBuffer
    {
#pragma warning disable CS0169, IDE0051, S1144 // Accessed by runtime via [InlineArray]
        private ColorEndpointPair element0;
#pragma warning restore CS0169, IDE0051, S1144
    }

    /// <summary>
    /// State common to single-plane and dual-plane blocks
    /// </summary>
    private ref struct DecodedBlockState
    {
        public EndpointBuffer Endpoints;
        public Span<int> Weights;
        public ReadOnlySpan<int> PartitionAssignment;
    }

    /// <summary>
    /// Secondary weight plane for dual-plane blocks (ASTC spec §C.2.20). The channel
    /// identified by <see cref="Channel"/> uses these per-texel weights instead of the
    /// primary plane's.
    /// </summary>
    private ref struct DualPlane
    {
        public Span<int> Weights;
        public int Channel;
    }
}
