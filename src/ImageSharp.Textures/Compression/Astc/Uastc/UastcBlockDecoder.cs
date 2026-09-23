// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Uastc;

/// <summary>
/// Decodes a single 16-byte UASTC LDR block (4x4) to RGBA8, reusing the standard ASTC
/// interpolation/pixel-write back end via <see cref="LogicalBlock.WriteDecodedLdr{TMode}"/>.
/// </summary>
internal static class UastcBlockDecoder
{
    private const int TexelCount = 16;

    private static readonly Footprint Footprint4x4 = Footprint.FromFootprintType(FootprintType.Footprint4x4);

    /// <summary>
    /// Decodes the block into <paramref name="rgba"/> (64 bytes, 4x4 RGBA8).
    /// </summary>
    /// <returns>True if decoded; false for reserved/unsupported modes (caller emits magenta).</returns>
    public static bool TryDecode<TMode>(ReadOnlySpan<byte> block, Span<byte> rgba)
        where TMode : struct, ILdrColorMode
    {
        if (!UastcMode.TryDecodeMode(block[0], out int mode))
        {
            return false;
        }

        if (mode == UastcMode.SolidColorModeIndex)
        {
            return TryDecodeSolid(block, rgba);
        }

        return TryDecodeBlock<TMode>(block, mode, rgba);
    }

    private static bool TryDecodeSolid(ReadOnlySpan<byte> block, Span<byte> rgba)
    {
        UastcBitReader reader = new(block);
        reader.Skip(UastcMode.HuffCodes[UastcMode.SolidColorModeIndex].Length);

        byte r = (byte)reader.ReadBits(8);
        byte g = (byte)reader.ReadBits(8);
        byte b = (byte)reader.ReadBits(8);
        byte a = (byte)reader.ReadBits(8);

        for (int i = 0; i < TexelCount; i++)
        {
            int o = i * 4;
            rgba[o] = r;
            rgba[o + 1] = g;
            rgba[o + 2] = b;
            rgba[o + 3] = a;
        }

        return true;
    }

    private static bool TryDecodeBlock<TMode>(ReadOnlySpan<byte> block, int mode, Span<byte> rgba)
        where TMode : struct, ILdrColorMode
    {
        UastcBitReader reader = new(block);
        reader.Skip(UastcMode.HuffCodes[mode].Length);

        // Skip the hint fields (per-mode total) — not needed for direct RGBA decode.
        reader.Skip(UastcMode.TotalHintBits[mode]);

        int subsets = UastcMode.Subsets[mode];
        int totalPlanes = UastcMode.Planes[mode];

        if (!TryReadPartition(ref reader, mode, subsets, out ReadOnlySpan<int> partition, out ReadOnlySpan<byte> anchors))
        {
            return false;
        }

        int ccs = ReadDualPlaneChannel(ref reader, mode, totalPlanes);

        Span<ColorEndpointPair> endpoints = stackalloc ColorEndpointPair[3];
        endpoints = endpoints[..subsets];
        ReadEndpoints(ref reader, mode, subsets, endpoints);

        // One weight per texel per plane (16 or 32), unquantized to [0,64].
        Span<int> weights = stackalloc int[TexelCount * 2];
        weights = weights[..(TexelCount * totalPlanes)];
        ReadWeights(ref reader, mode, subsets, totalPlanes, anchors, weights);

        if (totalPlanes == 1)
        {
            LogicalBlock.WriteDecodedLdr<TMode>(Footprint4x4, endpoints, partition, weights, rgba);
            return true;
        }

        // Dual plane: weights are interleaved [plane0, plane1] per texel; the CCS channel uses plane1.
        Span<int> primary = stackalloc int[TexelCount];
        Span<int> secondary = stackalloc int[TexelCount];
        for (int i = 0; i < TexelCount; i++)
        {
            primary[i] = weights[i * 2];
            secondary[i] = weights[(i * 2) + 1];
        }

        LogicalBlock.WriteDecodedLdrDualPlane<TMode>(Footprint4x4, endpoints, partition, primary, secondary, ccs, rgba);
        return true;
    }

    /// <summary>
    /// Reads the partition for a block: the single-subset map for non-partitioned modes, or the
    /// pattern selected by the mode's 4-bit (3-subset) or 5-bit (2-subset) index.
    /// </summary>
    /// <returns>False if the pattern index is out of range.</returns>
    private static bool TryReadPartition(ref UastcBitReader reader, int mode, int subsets, out ReadOnlySpan<int> partition, out ReadOnlySpan<byte> anchors)
    {
        if (subsets == 1)
        {
            partition = UastcPartitionTables.SingleSubset;
            anchors = UastcPartitionTables.SingleSubsetAnchors;
            return true;
        }

        int patternIndex = subsets == 3 ? (int)reader.ReadBits(4) : (int)reader.ReadBits(5);
        return UastcPartitionTables.TryGet(mode, subsets, patternIndex, out partition, out anchors);
    }

    /// <summary>
    /// Reads the dual-plane component selector (CCS), or returns -1 for single-plane modes. Modes
    /// 6/11/13 store a 2-bit selector; mode 17 has no field and is fixed to alpha (3).
    /// </summary>
    private static int ReadDualPlaneChannel(ref UastcBitReader reader, int mode, int totalPlanes)
    {
        if (totalPlanes != 2)
        {
            return -1;
        }

        return mode == 17 ? 3 : (int)reader.ReadBits(2);
    }

    /// <summary>
    /// Reads and unquantizes the per-subset endpoint pairs
    /// </summary>
    private static void ReadEndpoints(ref UastcBitReader reader, int mode, int subsets, scoped Span<ColorEndpointPair> endpoints)
    {
        int totalComps = UastcMode.Comps[mode];
        int endpointRangeIndex = UastcMode.EndpointRanges[mode];
        int valuesPerSubset = totalComps * 2;

        Span<int> values = stackalloc int[4 * 2 * 3]; // max 4 comps × 2 × 3 subsets
        values = values[..(valuesPerSubset * subsets)];
        ReadEndpointValues(ref reader, endpointRangeIndex, values.Length, values);
        Quantization.UnquantizeCEValuesBatch(values, BoundedIntegerSequenceCodec.MaxRanges[endpointRangeIndex]);

        for (int s = 0; s < subsets; s++)
        {
            endpoints[s] = BuildEndpoint(mode, totalComps, values.Slice(s * valuesPerSubset, valuesPerSubset));
        }
    }

    /// <summary>
    /// Reads the per-texel-per-plane weights into <paramref name="weights"/> and unquantizes them to [0,64].
    /// </summary>
    /// <remarks>
    /// Anchor texels (one per subset) store one fewer bit with the high bit implied 0.
    /// UASTC weights are always pure-bit BISE, so unquantization is the standard ASTC weight path.
    /// </remarks>
    private static void ReadWeights(ref UastcBitReader reader, int mode, int subsets, int totalPlanes, scoped ReadOnlySpan<byte> anchors, scoped Span<int> weights)
    {
        int weightBits = UastcMode.WeightBits[mode];

        // Fold the (≤3) anchor texel indices into a 16-bit mask so the loop is a single bit test.
        int anchorTexels = 0;
        for (int s = 0; s < subsets; s++)
        {
            anchorTexels |= 1 << anchors[s];
        }

        int planeShift = totalPlanes - 1;
        for (int i = 0; i < weights.Length; i++)
        {
            int texel = i >> planeShift;
            bool isAnchor = ((anchorTexels >> texel) & 1) != 0;
            int bits = isAnchor ? weightBits - 1 : weightBits;
            weights[i] = (int)reader.ReadBits(bits);
        }

        Quantization.UnquantizeWeightsBatch(weights, BoundedIntegerSequenceCodec.MaxRanges[UastcMode.WeightRanges[mode]]);
    }

    /// <summary>
    /// Reads <paramref name="totalValues"/> BISE-encoded endpoint values for the given range.
    /// </summary>
    /// <remarks>
    /// The UASTC layout is non-interleaved (Basis order)
    /// </remarks>
    private static void ReadEndpointValues(ref UastcBitReader reader, int rangeIndex, int totalValues, scoped Span<int> values)
    {
        (byte epBits, byte epTrits, byte epQuints) = UastcMode.BiseRangeTable[rangeIndex];

        if (epTrits == 0 && epQuints == 0)
        {
            // Pure-bit range: each value is simply epBits, no trit/quint bundle.
            for (int i = 0; i < totalValues; i++)
            {
                values[i] = (int)reader.ReadBits(epBits);
            }

            return;
        }

        bool isTrit = epTrits != 0;
        int valuesPerBundle = isTrit ? 5 : 3; // a bundle packs this many trits/quints
        int radix = isTrit ? 3 : 5; // base of each packed digit

        Span<int> bundles = stackalloc int[8];
        int bundleCount = ReadBundleCodes(ref reader, totalValues, isTrit, valuesPerBundle, bundles);

        CombineLowBitsWithDigits(ref reader, totalValues, epBits, bundles[..bundleCount], valuesPerBundle, radix, values);
    }

    /// <summary>
    /// Reads the trit/quint bundle codes that precede the endpoint low bits, returning the number
    /// of bundles read. The final bundle is truncated to only the bits its remaining values need.
    /// </summary>
    private static int ReadBundleCodes(ref UastcBitReader reader, int totalValues, bool isTrit, int valuesPerBundle, scoped Span<int> bundles)
    {
        int bundleCount = (totalValues + valuesPerBundle - 1) / valuesPerBundle;
        int fullBundleBits = isTrit ? 8 : 7;

        for (int i = 0; i < bundleCount; i++)
        {
            int remaining = totalValues - (i * valuesPerBundle);
            int numBits = remaining >= valuesPerBundle ? fullBundleBits : FinalBundleBits(isTrit, remaining, fullBundleBits);
            bundles[i] = (int)reader.ReadBits(numBits);
        }

        return bundleCount;
    }

    /// <summary>
    /// Bit count for a partial final bundle holding <paramref name="remaining"/> values
    /// (ASTC spec §C.2.12 truncation).
    /// </summary>
    private static int FinalBundleBits(bool isTrit, int remaining, int fullBundleBits)
        => isTrit
            ? remaining switch { 1 => 2, 2 => 4, 3 => 5, 4 => 7, _ => fullBundleBits }
            : remaining switch { 1 => 3, 2 => 5, _ => fullBundleBits };

    /// <summary>
    /// Reads each value's low bits and folds in its trit/quint digit, unpacked from the bundle
    /// codes one base-<paramref name="radix"/> digit at a time.
    /// </summary>
    private static void CombineLowBitsWithDigits(
        ref UastcBitReader reader,
        int totalValues,
        int epBits,
        scoped ReadOnlySpan<int> bundles,
        int valuesPerBundle,
        int radix,
        scoped Span<int> values)
    {
        int accum = 0, accumRemaining = 0, nextBundle = 0;
        for (int i = 0; i < totalValues; i++)
        {
            if (accumRemaining == 0)
            {
                accum = bundles[nextBundle++];
                accumRemaining = valuesPerBundle;
            }

            int digit = accum % radix;
            accum /= radix;
            accumRemaining--;

            values[i] = (int)reader.ReadBits(epBits) | (digit << epBits);
        }
    }

    private static ColorEndpointPair BuildEndpoint(int mode, int totalComps, ReadOnlySpan<int> v)
    {
        if (UastcMode.IsLa[mode] != 0)
        {
            // LA: components are (L, A); output swizzled to {L,L,L,A}.
            byte ll = (byte)v[0], lh = (byte)v[1], al = (byte)v[2], ah = (byte)v[3];
            return ColorEndpointPair.Ldr(new Rgba32(ll, ll, ll, al), new Rgba32(lh, lh, lh, ah));
        }

        // RGB (comps 3) or RGBA (comps 4); values are interleaved low/high per component.
        byte rL = (byte)v[0], rH = (byte)v[1];
        byte gL = (byte)v[2], gH = (byte)v[3];
        byte bL = (byte)v[4], bH = (byte)v[5];
        byte aL = totalComps == 4 ? (byte)v[6] : (byte)255;
        byte aH = totalComps == 4 ? (byte)v[7] : (byte)255;

        return ColorEndpointPair.Ldr(new Rgba32(rL, gL, bL, aL), new Rgba32(rH, gH, bH, aH));
    }
}
