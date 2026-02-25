// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.Core;

/// <summary>
/// Pre-computed weight infill data for a specific (footprint, weightGridX, weightGridY) combination.
/// Stores bilinear interpolation indices and factors in a transposed layout.
/// </summary>
internal sealed class DecimationInfo
{
    private readonly int texelCount;

    // Transposed layout: [contribution * TexelCount + texel]
    // 4 contributions per texel (bilinear interpolation from weight grid).
    // For edge texels where some grid points are out of bounds, factor is 0 and index is 0.
    private readonly int[] weightIndices;  // size: 4 * TexelCount
    private readonly int[] weightFactors;  // size: 4 * TexelCount

    public DecimationInfo(int texelCount, int[] weightIndices, int[] weightFactors)
    {
        this.texelCount = texelCount;
        this.weightIndices = weightIndices;
        this.weightFactors = weightFactors;
    }

    public int TexelCount => this.texelCount;

    public int[] WeightIndices => this.weightIndices;

    public int[] WeightFactors => this.weightFactors;
}
