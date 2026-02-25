// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.TexelBlock;

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

    // Endpoint modes (up to 4 partitions)
    public ColorEndpointMode EndpointMode0;
    public ColorEndpointMode EndpointMode1;
    public ColorEndpointMode EndpointMode2;
    public ColorEndpointMode EndpointMode3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ColorEndpointMode GetEndpointMode(int partition) => partition switch
    {
        0 => this.EndpointMode0,
        1 => this.EndpointMode1,
        2 => this.EndpointMode2,
        3 => this.EndpointMode3,
        _ => this.EndpointMode0
    };

    /// <summary>
    /// Decode all block info from raw 128-bit ASTC block data in a single pass.
    /// Returns a BlockInfo with IsValid=false if the block is illegal or reserved.
    /// Returns a BlockInfo with IsVoidExtent=true for void extent blocks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static BlockInfo Decode(UInt128 bits)
    {
        ulong lowBits = bits.Low();

        // ---- Step 1: Check void extent ----
        // Void extent: bits[0:9] == 0x1FC (9 bits)
        if ((lowBits & 0x1FF) == 0x1FC)
        {
            return new BlockInfo
            {
                IsVoidExtent = true,
                IsValid = !CheckVoidExtentIsIllegal(bits, lowBits)
            };
        }

        // ---- Step 2: Decode block mode, grid dims, weight range in ONE pass ----
        // This inlines DecodeBlockMode + DecodeWeightProperties
        int gridWidth, gridHeight;
        bool isWidthA6HeightB6 = false;
        uint rBits; // 3-bit range index component

        // bits[0:2] != 0
        if ((lowBits & 0x3) != 0)
        {
            ulong modeBits = (lowBits >> 2) & 0x3; // bits[2:4]
            int a = (int)((lowBits >> 5) & 0x3); // bits[5:7]

            (gridWidth, gridHeight) = modeBits switch
            {
                0 => ((int)((lowBits >> 7) & 0x3) + 4, a + 2),
                1 => ((int)((lowBits >> 7) & 0x3) + 8, a + 2),
                2 => (a + 2, (int)((lowBits >> 7) & 0x3) + 8),
                3 when ((lowBits >> 8) & 1) != 0 => ((int)((lowBits >> 7) & 0x1) + 2, a + 2),
                3 => (a + 2, (int)((lowBits >> 7) & 0x1) + 6),
                _ => default // unreachable
            };

            // Range r[2:0] = {bit4, bit1, bit0} for these modes
            rBits = (uint)(((lowBits >> 4) & 1) | (((lowBits >> 0) & 0x3) << 1));
        }
        else
        {
            // bits[0:2] == 0
            ulong modeBits = (lowBits >> 5) & 0xF; // bits[5:9]
            int a = (int)((lowBits >> 5) & 0x3); // bits[5:7]

            switch (modeBits)
            {
                case var _ when (modeBits & 0xC) == 0x0:
                    if ((lowBits & 0xF) == 0)
                    {
                        return default; // reserved block mode
                    }

                    gridWidth = 12;
                    gridHeight = a + 2;
                    break;
                case var _ when (modeBits & 0xC) == 0x4:
                    gridWidth = a + 2;
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
                case var _ when (modeBits & 0xC) == 0x8:
                    gridWidth = a + 6;
                    gridHeight = (int)((lowBits >> 9) & 0x3) + 6;
                    isWidthA6HeightB6 = true;
                    break;
                default:
                    return default; // reserved
            }

            // Range r[2:0] = {bit4, bit3, bit2} for these modes
            rBits = (uint)(((lowBits >> 4) & 1) | (((lowBits >> 2) & 0x3) << 1));
        }

        // ---- Step 3: Compute weight range from r and h bits ----
        uint hBit = isWidthA6HeightB6
            ? 0u
            : (uint)((lowBits >> 9) & 1);
        int rangeIdx = (int)((hBit << 3) | rBits);
        if ((uint)rangeIdx >= (uint)WeightRanges.Length)
        {
            return default;
        }

        int weightRange = WeightRanges[rangeIdx];
        if (weightRange < 0)
        {
            return default;
        }

        // ---- Step 4: Dual plane ----
        // WidthA6HeightB6 mode never has dual plane; otherwise check bit 10
        bool isDualPlane = !isWidthA6HeightB6 && ((lowBits >> 10) & 1) != 0;

        // ---- Step 5: Partition count ----
        int partitionCount = 1 + (int)((lowBits >> 11) & 0x3);

        // ---- Step 6: Validate weight count ----
        int numWeights = gridWidth * gridHeight;
        if (isDualPlane)
        {
            numWeights *= 2;
        }

        if (numWeights > 64)
        {
            return default;
        }

        // 4 partitions + dual plane is illegal
        if (partitionCount == 4 && isDualPlane)
        {
            return default;
        }

        // ---- Step 7: Weight bit count ----
        int weightBitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(numWeights, weightRange);
        if (weightBitCount < 24 || weightBitCount > 96)
        {
            return default;
        }

        // ---- Step 8: Endpoint modes + extra CEM bits ----
        ColorEndpointMode cem0 = default, cem1 = default, cem2 = default, cem3 = default;
        int colorValuesCount = 0;
        int numExtraCEMBits = 0;

        if (partitionCount == 1)
        {
            cem0 = (ColorEndpointMode)((lowBits >> 13) & 0xF);
            colorValuesCount = (((int)cem0 / 4) + 1) * 2;
        }
        else
        {
            // Multi-partition CEM decode
            ulong sharedCemMarker = (lowBits >> 23) & 0x3;

            if (sharedCemMarker == 0)
            {
                // Shared CEM: all partitions use the same mode
                var sharedCem = (ColorEndpointMode)((lowBits >> 25) & 0xF);
                cem0 = cem1 = cem2 = cem3 = sharedCem;
                for (int i = 0; i < partitionCount; i++)
                {
                    colorValuesCount += sharedCem.GetColorValuesCount();
                }
            }
            else
            {
                // Non-shared CEM: per-partition modes
                numExtraCEMBits = ExtraCemBitsForPartition[partitionCount - 1];

                int extraCemStartPos = 128 - numExtraCEMBits - weightBitCount;
                var extraCem = BitOperations.GetBits(bits, extraCemStartPos, numExtraCEMBits);

                ulong cemval = (lowBits >> 23) & 0x3F; // 6 bits starting at bit 23
                int baseCem = (int)(((cemval & 0x3) - 1) * 4);
                cemval >>= 2;

                ulong combined = cemval | (extraCem.Low() << 4);
                ulong cembits = combined;

                // Extract c bits (1 bit per partition)
                Span<int> c = stackalloc int[4];
                for (int i = 0; i < partitionCount; i++)
                {
                    c[i] = (int)(cembits & 0x1);
                    cembits >>= 1;
                }

                // Extract m bits (2 bits per partition)
                for (int i = 0; i < partitionCount; i++)
                {
                    int m = (int)(cembits & 0x3);
                    cembits >>= 2;
                    var mode = (ColorEndpointMode)(baseCem + (4 * c[i]) + m);
                    switch (i)
                    {
                        case 0:
                            cem0 = mode;
                            break;
                        case 1:
                            cem1 = mode;
                            break;
                        case 2:
                            cem2 = mode;
                            break;
                        case 3:
                            cem3 = mode;
                            break;
                    }

                    colorValuesCount += mode.GetColorValuesCount();
                }
            }
        }

        if (colorValuesCount > 18)
        {
            return default;
        }

        // ---- Step 9: Dual plane start position and channel ----
        int dualPlaneBitStartPos = 128 - weightBitCount - numExtraCEMBits;
        if (isDualPlane)
        {
            dualPlaneBitStartPos -= 2;
        }

        int dualPlaneChannel = isDualPlane
            ? (int)BitOperations.GetBits(bits, dualPlaneBitStartPos, 2).Low()
            : -1;

        // ---- Step 10: Color values info ----
        int colorStartBit = (partitionCount == 1) ? 17 : 29;
        int maxColorBits = dualPlaneBitStartPos - colorStartBit;

        // Minimum bits needed check
        int requiredColorBits = ((13 * colorValuesCount) + 4) / 5;
        if (maxColorBits < requiredColorBits)
        {
            return default;
        }

        // Find max color range that fits (only check valid BISE ranges: 17 vs up to 255)
        int colorValuesRange = 0, colorBitCount = 0;
        foreach (int rv in ValidEndpointRanges)
        {
            int bitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(colorValuesCount, rv);
            if (bitCount <= maxColorBits)
            {
                colorValuesRange = rv;
                colorBitCount = bitCount;
                break;
            }
        }

        if (colorValuesRange == 0)
        {
            return default;
        }

        // ---- Step 11: Validate endpoint modes are not HDR for batchable checks ----
        // (HDR blocks are still valid, just flagged for downstream use)
        return new BlockInfo
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
            EndpointMode0 = cem0,
            EndpointMode1 = cem1,
            EndpointMode2 = cem2,
            EndpointMode3 = cem3,
        };
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
}
