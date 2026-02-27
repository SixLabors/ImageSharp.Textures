// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Pre-computed weight infill data for a specific (footprint, weightGridX, weightGridY) combination.
/// Stores bilinear interpolation indices and factors in a transposed layout.
/// </summary>
internal sealed class DecimationInfo
{
    // Transposed layout: [contribution * TexelCount + texel]
    // 4 contributions per texel (bilinear interpolation from weight grid).
    // For edge texels where some grid points are out of bounds, factor is 0 and index is 0.
    public DecimationInfo(int texelCount, int[] weightIndices, int[] weightFactors)
    {
        this.TexelCount = texelCount;
        this.WeightIndices = weightIndices;
        this.WeightFactors = weightFactors;
    }

    public int TexelCount { get; }

    public int[] WeightIndices { get; }

    public int[] WeightFactors { get; }
}
