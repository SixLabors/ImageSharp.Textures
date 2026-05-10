// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

/// <summary>
/// One row of the block-mode layout table used when encoding an ASTC block mode
/// (see ASTC spec §C.2.8 Table 24). Each row describes the range of weight-grid
/// dimensions the layout supports and the bit positions into which the range-selector
/// bits and grid-offset bits are placed in the 11-bit encoded block mode.
/// </summary>
/// <param name="MinWeightGridDimX">Inclusive minimum weight-grid X dimension.</param>
/// <param name="MaxWeightGridDimX">Inclusive maximum weight-grid X dimension.</param>
/// <param name="MinWeightGridDimY">Inclusive minimum weight-grid Y dimension.</param>
/// <param name="MaxWeightGridDimY">Inclusive maximum weight-grid Y dimension.</param>
/// <param name="R0BitPos">Bit position for range selector bit 0.</param>
/// <param name="R1BitPos">Bit position for range selector bit 1.</param>
/// <param name="R2BitPos">Bit position for range selector bit 2.</param>
/// <param name="WeightGridXOffsetBitPos">Bit position for the X-offset bits, or -1 when the layout's X dimension is fixed.</param>
/// <param name="WeightGridYOffsetBitPos">Bit position for the Y-offset bits, or -1 when the layout's Y dimension is fixed.</param>
/// <param name="RequireSinglePlaneLowPrec">True if this layout only encodes single-plane low-precision blocks
/// (no dual plane and no high-precision weight range).</param>
internal readonly record struct BlockModeInfo(
    int MinWeightGridDimX,
    int MaxWeightGridDimX,
    int MinWeightGridDimY,
    int MaxWeightGridDimY,
    int R0BitPos,
    int R1BitPos,
    int R2BitPos,
    int WeightGridXOffsetBitPos,
    int WeightGridYOffsetBitPos,
    bool RequireSinglePlaneLowPrec);
