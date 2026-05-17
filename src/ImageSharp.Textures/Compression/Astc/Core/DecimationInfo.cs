// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Pre-computed weight infill data for a specific (footprint, weightGridX, weightGridY) combination.
/// Stores bilinear interpolation indices and factors in a transposed layout.
/// </summary>
internal sealed class DecimationInfo
{
    private readonly int[] weightIndices;
    private readonly int[] weightFactors;

    // Transposed layout: [contribution * TexelCount + texel]
    // 4 contributions per texel (bilinear interpolation from weight grid).
    // For edge texels where some grid points are out of bounds, factor is 0 and index is 0.
    public DecimationInfo(int texelCount, int[] weightIndices, int[] weightFactors)
    {
        this.TexelCount = texelCount;
        this.weightIndices = weightIndices;
        this.weightFactors = weightFactors;
    }

    public int TexelCount { get; }

    /// <summary>
    /// Gets the per-texel grid-point indices (length <c>4 * <see cref="TexelCount"/></c>) in the
    /// transposed [contribution * TexelCount + texel] layout. Cached and shared across blocks
    /// that resolve to the same (footprint, weight-grid) pair.
    /// </summary>
    public ReadOnlySpan<int> WeightIndices => this.weightIndices;

    /// <summary>
    /// Gets the per-texel bilinear weight factors (length <c>4 * <see cref="TexelCount"/></c>) in
    /// the same transposed layout as <see cref="WeightIndices"/>.
    /// </summary>
    public ReadOnlySpan<int> WeightFactors => this.weightFactors;
}
