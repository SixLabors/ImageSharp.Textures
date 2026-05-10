// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

/// <summary>
/// Fused block info computed in a single pass from raw ASTC block bits
/// </summary>
internal struct BlockInfo
{
    private static readonly int[] WeightRanges =
        [-1, -1, 1, 2, 3, 4, 5, 7, -1, -1, 9, 11, 15, 19, 23, 31];

    private static readonly int[] ExtraCemBitsForPartition = [0, 2, 5, 8];

    // Valid BISE endpoint ranges in descending order (only these produce valid encodings)
    private static readonly int[] ValidEndpointRanges =
        [255, 191, 159, 127, 95, 79, 63, 47, 39, 31, 23, 19, 15, 11, 9, 7, 5];

    public bool IsValid;
    public bool IsVoidExtent;

    // Weight grid
    public int GridWidth;
    public int GridHeight;
    public int WeightRange;
    public int WeightBitCount;

    // Partitions
    public int PartitionCount;

    // Dual plane
    public bool IsDualPlane;
    public int DualPlaneChannel; // only valid if IsDualPlane

    // Color endpoints
    public int ColorStartBit;
    public int ColorBitCount;
    public int ColorValuesRange;
    public int ColorValuesCount;

    // Endpoint modes (up to 4 partitions). Indexed via GetEndpointMode / SetEndpointMode.
    public EndpointModeBuffer EndpointModes;

    public ColorEndpointMode EndpointMode0
    {
        readonly get => this.EndpointModes[0];
        set => this.EndpointModes[0] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ColorEndpointMode GetEndpointMode(int partition)
        => (uint)partition < 4
            ? this.EndpointModes[partition]
            : this.EndpointModes[0];

    /// <summary>
    /// Returns true if any of this block's active partitions uses an HDR endpoint mode.
    /// Does not detect HDR void-extent blocks (those carry their own HDR flag and have
    /// <see cref="PartitionCount"/> == 0); callers that need to reject both cases should
    /// also check <see cref="IsVoidExtent"/> against the HDR flag in the raw bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasHdrEndpoints()
    {
        for (int i = 0; i < this.PartitionCount; i++)
        {
            if (this.GetEndpointMode(i).IsHdr())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Decode all block info from raw 128-bit ASTC block data in a single pass.
    /// Returns a BlockInfo with IsValid=false if the block is illegal or reserved.
    /// Returns a BlockInfo with IsVoidExtent=true for void extent blocks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BlockInfo Decode(UInt128 bits)
    {
        ulong lowBits = bits.Low();

        // Void extent: bits[0:9] == 0x1FC (9 bits). See ASTC spec §C.2.23.
        if ((lowBits & 0x1FF) == 0x1FC)
        {
            return new BlockInfo
            {
                IsVoidExtent = true,
                IsValid = !CheckVoidExtentIsIllegal(bits, lowBits)
            };
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

        Span<ColorEndpointMode> cems = stackalloc ColorEndpointMode[4];
        int colorValuesCount = DecodeEndpointModes(bits, lowBits, partitionCount, weightBitCount, cems, out int numExtraCEMBits);
        if (colorValuesCount < 0 || colorValuesCount > 18)
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

        BlockInfo result = new()
        {
            IsValid = true,
            IsVoidExtent = false,
            GridWidth = gridWidth,
            GridHeight = gridHeight,
            WeightRange = weightRange,
            WeightBitCount = weightBitCount,
            PartitionCount = partitionCount,
            IsDualPlane = isDualPlane,
            DualPlaneChannel = dualPlaneChannel,
            ColorStartBit = colorStartBit,
            ColorBitCount = colorBitCount,
            ColorValuesRange = colorValuesRange,
            ColorValuesCount = colorValuesCount,
        };
        result.EndpointModes[0] = cems[0];
        result.EndpointModes[1] = cems[1];
        result.EndpointModes[2] = cems[2];
        result.EndpointModes[3] = cems[3];
        return result;
    }

    /// <summary>
    /// Decodes the block-mode / weight-grid dimensions section of the block mode.
    /// Returns false for reserved block-mode encodings.
    /// See ASTC spec §C.2.8 Table 24 (block mode layout A).
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
    /// Looks up the weight range from the 3-bit r selector plus high-precision h bit.
    /// Returns false if the resulting index points at a reserved slot in <see cref="WeightRanges"/>.
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
    /// Validates weight count constraints and resolves weight bit count. Rejects blocks with
    /// more than 64 weights, illegal 4-partition-with-dual-plane combos, and weight bit totals
    /// outside the [24, 96] window.
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
    /// Decodes per-partition color endpoint modes and returns the total color-values count.
    /// Returns -1 on any structural error. The shared-CEM and non-shared-CEM paths both
    /// populate <paramref name="cems"/> (length 4) and tell the caller how many extra CEM bits
    /// were consumed, which affects subsequent bit layout.
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
            return (((int)mode / 4) + 1) * 2;
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
    /// <paramref name="maxColorBits"/>. Returns false if the minimum encoding
    /// already exceeds the budget.
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
    /// Inline void extent validation (replaces PhysicalBlock.CheckVoidExtentIsIllegal).
    /// </summary>
    private static bool CheckVoidExtentIsIllegal(UInt128 bits, ulong lowBits)
    {
        if (BitOperations.GetBits(bits, 10, 2).Low() != 0x3UL)
        {
            return true;
        }

        int c0 = (int)BitOperations.GetBits(lowBits, 12, 13);
        int c1 = (int)BitOperations.GetBits(lowBits, 25, 13);
        int c2 = (int)BitOperations.GetBits(lowBits, 38, 13);
        int c3 = (int)BitOperations.GetBits(lowBits, 51, 13);

        const int all1s = (1 << 13) - 1;
        bool coordsAll1s = c0 == all1s && c1 == all1s && c2 == all1s && c3 == all1s;

        return !coordsAll1s && (c0 >= c1 || c2 >= c3);
    }

    [System.Runtime.CompilerServices.InlineArray(4)]
    public struct EndpointModeBuffer
    {
#pragma warning disable CS0169, IDE0051, S1144 // Accessed by runtime via [InlineArray]
        private ColorEndpointMode element0;
#pragma warning restore CS0169, IDE0051, S1144
    }
}
