// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// Single-pass parser for the 128-bit ASTC block mode (spec §C.2.7–§C.2.16). Produces a
/// populated <see cref="BlockInfo"/> record describing the block's weight grid, partition
/// count, colour endpoint modes, dual-plane flag, and the bit-range metadata the per-block
/// decoders need. Reserved and illegal encodings are rejected inline (IsValid = false).
/// </summary>
internal static class BlockModeDecoder
{
    // Spec §C.2.7 Table 23: weight range table indexed by r[2:0] + h. Entries marked -1 are
    // reserved and reject the block. Two six-entry groups (low precision, high precision).
    private static readonly int[] WeightRanges =
        [-1, -1, 1, 2, 3, 4, 5, 7, -1, -1, 9, 11, 15, 19, 23, 31];

    // Spec §C.2.11: extra-CEM bit count by partition count. Indexed [partitionCount - 1].
    private static readonly int[] ExtraCemBitsForPartition = [0, 2, 5, 8];

    // Spec §C.2.16: valid BISE endpoint ranges in descending order. Only these produce valid
    // quantisation encodings; the parser picks the largest that fits in the colour bit budget.
    private static readonly int[] ValidEndpointRanges =
        [255, 191, 159, 127, 95, 79, 63, 47, 39, 31, 23, 19, 15, 11, 9, 7, 5];

    /// <summary>
    /// Decodes all block-mode info from raw 128-bit ASTC block data in a single pass.
    /// Returns a <see cref="BlockInfo"/> with <c>IsValid = false</c> if the block is illegal or
    /// reserved, or with <c>IsVoidExtent = true</c> for void-extent blocks (spec §C.2.23).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BlockInfo Decode(UInt128 bits)
    {
        ulong lowBits = bits.Low();

        // Void extent: bits[0:9] == 0x1FC (9 bits). See ASTC spec §C.2.23.
        if ((lowBits & 0x1FF) == 0x1FC)
        {
            return IsVoidExtentWellFormed(bits, lowBits)
                ? new BlockInfo(
                    isVoidExtent: true,
                    weights: default,
                    partitionCount: 0,
                    dualPlane: default,
                    colors: default,
                    endpointModes: default)
                : BlockInfo.MalformedVoidExtent;
        }

        if (!TryDecodeWeightGrid(lowBits, out int gridWidth, out int gridHeight, out uint rBits, out bool isWidthA6HeightB6))
        {
            return default;
        }

        if (!TryResolveWeightRange(lowBits, rBits, isWidthA6HeightB6, out int weightRange))
        {
            return default;
        }

        // WidthA6HeightB6 mode never has dual plane; otherwise check bit 10.
        bool isDualPlane = !isWidthA6HeightB6 && ((lowBits >> 10) & 1) != 0;
        int partitionCount = 1 + (int)((lowBits >> 11) & 0x3);

        if (!TryComputeWeightBitCount(gridWidth, gridHeight, isDualPlane, partitionCount, weightRange, out int weightBitCount))
        {
            return default;
        }

        // Fixed 4 entries (max partition count per spec §C.2.10)
        Span<ColorEndpointMode> cems = stackalloc ColorEndpointMode[4];
        int colorValuesCount = DecodeEndpointModes(bits, lowBits, partitionCount, weightBitCount, cems, out int numExtraCEMBits);
        if (colorValuesCount is < 0 or > 18)
        {
            return default;
        }

        // Dual plane and color bit positions depend on weight + extra-CEM bit allocation.
        int dualPlaneBitStartPos = 128 - weightBitCount - numExtraCEMBits;
        if (isDualPlane)
        {
            dualPlaneBitStartPos -= 2;
        }

        int dualPlaneChannel = isDualPlane
            ? (int)BitOperations.GetBits(bits, dualPlaneBitStartPos, 2).Low()
            : -1;

        int colorStartBit = (partitionCount == 1) ? 17 : 29;
        int maxColorBits = dualPlaneBitStartPos - colorStartBit;

        if (!TryFitColorRange(colorValuesCount, maxColorBits, out int colorValuesRange, out int colorBitCount))
        {
            return default;
        }

        BlockInfo.EndpointModeBuffer modes = default;
        modes[0] = cems[0];
        modes[1] = cems[1];
        modes[2] = cems[2];
        modes[3] = cems[3];

        return new BlockInfo(
            isVoidExtent: false,
            weights: new WeightGrid(gridWidth, gridHeight, weightRange, weightBitCount),
            partitionCount,
            dualPlane: new DualPlaneInfo(isDualPlane, dualPlaneChannel),
            colors: new ColorEndpoints(colorStartBit, colorBitCount, colorValuesRange, colorValuesCount),
            endpointModes: modes);
    }

    /// <summary>
    /// Decodes the block-mode / weight-grid dimensions section of the block mode per ASTC spec
    /// §C.2.8 Table 24. Returns false for reserved block-mode encodings.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryDecodeWeightGrid(
        ulong lowBits,
        out int gridWidth,
        out int gridHeight,
        out uint rBits,
        out bool isWidthA6HeightB6)
    {
        isWidthA6HeightB6 = false;

        if ((lowBits & 0x3) != 0)
        {
            // bits[0..1] != 0 : layout A (modeBits = bits[2..3]).
            ulong modeBits = (lowBits >> 2) & 0x3;
            int a = (int)((lowBits >> 5) & 0x3);

            (gridWidth, gridHeight) = modeBits switch
            {
                0 => ((int)((lowBits >> 7) & 0x3) + 4, a + 2),
                1 => ((int)((lowBits >> 7) & 0x3) + 8, a + 2),
                2 => (a + 2, (int)((lowBits >> 7) & 0x3) + 8),
                3 when ((lowBits >> 8) & 1) != 0 => ((int)((lowBits >> 7) & 0x1) + 2, a + 2),
                3 => (a + 2, (int)((lowBits >> 7) & 0x1) + 6),
                _ => default // unreachable — modeBits is 2 bits wide.
            };

            // r[2:0] = { bit4, bit1, bit0 } for layout A.
            rBits = (uint)(((lowBits >> 4) & 1) | ((lowBits & 0x3) << 1));
            return true;
        }

        // bits[0..1] == 0 : layout B (modeBits = bits[5..8]).
        ulong layoutBBits = (lowBits >> 5) & 0xF;
        int aLow = (int)((lowBits >> 5) & 0x3);

        switch (layoutBBits)
        {
            case var _ when (layoutBBits & 0xC) == 0x0:
                if ((lowBits & 0xF) == 0)
                {
                    // Reserved: all of bits[0..4] are zero.
                    gridWidth = gridHeight = 0;
                    rBits = 0;
                    return false;
                }

                gridWidth = 12;
                gridHeight = aLow + 2;
                break;
            case var _ when (layoutBBits & 0xC) == 0x4:
                gridWidth = aLow + 2;
                gridHeight = 12;
                break;
            case 0xC:
                gridWidth = 6;
                gridHeight = 10;
                break;
            case 0xD:
                gridWidth = 10;
                gridHeight = 6;
                break;
            case var _ when (layoutBBits & 0xC) == 0x8:
                gridWidth = aLow + 6;
                gridHeight = (int)((lowBits >> 9) & 0x3) + 6;
                isWidthA6HeightB6 = true;
                break;
            default:
                // Reserved block mode.
                gridWidth = gridHeight = 0;
                rBits = 0;
                return false;
        }

        // r[2:0] = { bit4, bit3, bit2 } for layout B.
        rBits = (uint)(((lowBits >> 4) & 1) | (((lowBits >> 2) & 0x3) << 1));
        return true;
    }

    /// <summary>
    /// Looks up the weight range from the 3-bit r selector plus the high-precision h bit per
    /// ASTC spec §C.2.7 Table 23. Returns false if the resulting index points at a reserved slot.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryResolveWeightRange(ulong lowBits, uint rBits, bool isWidthA6HeightB6, out int weightRange)
    {
        uint hBit = isWidthA6HeightB6 ? 0u : (uint)((lowBits >> 9) & 1);
        int rangeIdx = (int)((hBit << 3) | rBits);
        if ((uint)rangeIdx >= (uint)WeightRanges.Length)
        {
            weightRange = 0;
            return false;
        }

        weightRange = WeightRanges[rangeIdx];
        return weightRange >= 0;
    }

    /// <summary>
    /// Validates weight count constraints and resolves the weight bit count per ASTC spec
    /// §C.2.11. Rejects blocks with more than 64 weights, illegal 4-partition-with-dual-plane
    /// combos, and weight bit totals outside the [24, 96] window.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryComputeWeightBitCount(
        int gridWidth,
        int gridHeight,
        bool isDualPlane,
        int partitionCount,
        int weightRange,
        out int weightBitCount)
    {
        int numWeights = gridWidth * gridHeight;
        if (isDualPlane)
        {
            numWeights *= 2;
        }

        // 4 partitions + dual plane is illegal per spec §C.2.11.
        if (numWeights > 64 || (partitionCount == 4 && isDualPlane))
        {
            weightBitCount = 0;
            return false;
        }

        weightBitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(numWeights, weightRange);
        return weightBitCount is >= 24 and <= 96;
    }

    /// <summary>
    /// Decodes per-partition colour endpoint modes per ASTC spec §C.2.11 and returns the total
    /// colour-values count. The shared-CEM and non-shared-CEM paths both populate
    /// <paramref name="cems"/> (length 4) and tell the caller how many extra CEM bits were
    /// consumed, which affects subsequent bit layout.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeEndpointModes(
        UInt128 bits,
        ulong lowBits,
        int partitionCount,
        int weightBitCount,
        Span<ColorEndpointMode> cems,
        out int numExtraCEMBits)
    {
        numExtraCEMBits = 0;

        if (partitionCount == 1)
        {
            ColorEndpointMode mode = (ColorEndpointMode)((lowBits >> 13) & 0xF);
            cems[0] = mode;
            return mode.GetColorValuesCount();
        }

        // Multi-partition: either shared CEM (marker 0) or per-partition (non-zero marker).
        ulong sharedCemMarker = (lowBits >> 23) & 0x3;
        if (sharedCemMarker == 0)
        {
            ColorEndpointMode sharedCem = (ColorEndpointMode)((lowBits >> 25) & 0xF);
            int colorValuesCount = 0;
            for (int i = 0; i < partitionCount; i++)
            {
                cems[i] = sharedCem;
                colorValuesCount += sharedCem.GetColorValuesCount();
            }

            return colorValuesCount;
        }

        numExtraCEMBits = ExtraCemBitsForPartition[partitionCount - 1];

        int extraCemStartPos = 128 - numExtraCEMBits - weightBitCount;
        UInt128 extraCem = BitOperations.GetBits(bits, extraCemStartPos, numExtraCEMBits);

        ulong cemval = (lowBits >> 23) & 0x3F;
        int baseCem = (int)(((cemval & 0x3) - 1) * 4);
        cemval >>= 2;
        ulong cembits = cemval | (extraCem.Low() << 4);

        // 1 selector bit per partition (c[i]), then 2 mode bits per partition (m).
        // Fixed 4 ints (16 bytes) — max partition count per spec §C.2.10.
        Span<int> c = stackalloc int[4];
        for (int i = 0; i < partitionCount; i++)
        {
            c[i] = (int)(cembits & 0x1);
            cembits >>= 1;
        }

        int total = 0;
        for (int i = 0; i < partitionCount; i++)
        {
            int m = (int)(cembits & 0x3);
            cembits >>= 2;
            ColorEndpointMode mode = (ColorEndpointMode)(baseCem + (4 * c[i]) + m);
            cems[i] = mode;
            total += mode.GetColorValuesCount();
        }

        return total;
    }

    /// <summary>
    /// Finds the greatest valid BISE endpoint range whose encoding fits within
    /// <paramref name="maxColorBits"/> per ASTC spec §C.2.16. Returns false if the minimum
    /// encoding already exceeds the budget.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFitColorRange(
        int colorValuesCount,
        int maxColorBits,
        out int colorValuesRange,
        out int colorBitCount)
    {
        // Spec §C.2.16 minimum: 13 bits per 5 color values, rounded up.
        int requiredColorBits = ((13 * colorValuesCount) + 4) / 5;
        if (maxColorBits < requiredColorBits)
        {
            colorValuesRange = 0;
            colorBitCount = 0;
            return false;
        }

        foreach (int rv in ValidEndpointRanges)
        {
            int bitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(colorValuesCount, rv);
            if (bitCount <= maxColorBits)
            {
                colorValuesRange = rv;
                colorBitCount = bitCount;
                return true;
            }
        }

        colorValuesRange = 0;
        colorBitCount = 0;
        return false;
    }

    /// <summary>
    /// Inline void-extent validation per ASTC spec §C.2.23: reserved bits 10..11 must be 0x3,
    /// and either the texel coordinates are all-ones (sentinel for "no constraint") or they
    /// form two valid [min, max] pairs with min &lt; max.
    /// </summary>
    private static bool IsVoidExtentWellFormed(UInt128 bits, ulong lowBits)
    {
        if (BitOperations.GetBits(bits, 10, 2).Low() != 0x3UL)
        {
            return false;
        }

        int c0 = (int)BitOperations.GetBits(lowBits, 12, 13);
        int c1 = (int)BitOperations.GetBits(lowBits, 25, 13);
        int c2 = (int)BitOperations.GetBits(lowBits, 38, 13);
        int c3 = (int)BitOperations.GetBits(lowBits, 51, 13);

        const int all1s = (1 << 13) - 1;
        bool coordsAll1s = c0 == all1s && c1 == all1s && c2 == all1s && c3 == all1s;

        return coordsAll1s || (c0 < c1 && c2 < c3);
    }
}
