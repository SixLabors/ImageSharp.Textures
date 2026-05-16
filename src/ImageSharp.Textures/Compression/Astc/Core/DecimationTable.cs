// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Caches pre-computed DecimationInfo tables and provides weight infill.
/// For each unique (footprint, gridX, gridY) combination, the bilinear interpolation
/// indices and factors are computed once and reused for every block with that configuration.
/// Uses a flat array indexed by (footprintType, gridX, gridY) for O(1) lookup.
/// </summary>
internal static class DecimationTable
{
    // Grid dimensions range from 2 to 12 inclusive
    private const int GridMin = 2;
    private const int GridRange = 11; // 12 - 2 + 1
    private const int FootprintCount = 14;
    private static readonly DecimationInfo?[] Table = new DecimationInfo?[FootprintCount * GridRange * GridRange];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DecimationInfo Get(Footprint footprint, int gridX, int gridY)
    {
        int index = ((int)footprint.Type * GridRange * GridRange) + ((gridX - GridMin) * GridRange) + (gridY - GridMin);

        // Volatile.Read pairs with the implicit release on CompareExchange to publish the
        // fully-constructed DecimationInfo. Entries are immutable, so losing the CAS race
        // is harmless — the caller discards its own instance and uses the winner.
        DecimationInfo? decimationInfo = Volatile.Read(ref Table[index]);
        if (decimationInfo is null)
        {
            DecimationInfo computed = Compute(footprint.Width, footprint.Height, gridX, gridY);
            decimationInfo = Interlocked.CompareExchange(ref Table[index], computed, null) ?? computed;
        }

        return decimationInfo;
    }

    /// <summary>
    /// Performs weight infill using pre-computed tables.
    /// Maps unquantized grid weights to per-texel weights via bilinear interpolation
    /// with pre-computed indices and factors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void InfillWeights(ReadOnlySpan<int> gridWeights, DecimationInfo decimationInfo, Span<int> result)
    {
        int texelCount = decimationInfo.TexelCount;
        ReadOnlySpan<int> weightIndices = decimationInfo.WeightIndices;
        ReadOnlySpan<int> weightFactors = decimationInfo.WeightFactors;
        int offset1 = texelCount, offset2 = texelCount * 2, offset3 = texelCount * 3;

        for (int i = 0; i < texelCount; i++)
        {
            result[i] = (8
                + (gridWeights[weightIndices[i]] * weightFactors[i])
                + (gridWeights[weightIndices[offset1 + i]] * weightFactors[offset1 + i])
                + (gridWeights[weightIndices[offset2 + i]] * weightFactors[offset2 + i])
                + (gridWeights[weightIndices[offset3 + i]] * weightFactors[offset3 + i])) >> 4;
        }
    }

    /// <summary>
    /// Scale factor for mapping texel index to grid position (ASTC spec §C.2.18)
    /// </summary>
    private static int GetScaleFactorD(int blockDimensions) => (1024 + (blockDimensions >> 1)) / (blockDimensions - 1);

    /// <summary>
    /// Builds the weight-infill lookup for one (footprint, weight-grid) combination.
    /// For each texel, computes the four surrounding weight-grid indices and bilinear
    /// interpolation factors (ASTC spec §C.2.18), storing them in parallel transposed
    /// arrays so that decode can iterate by contribution slot.
    /// </summary>
    private static DecimationInfo Compute(int footprintWidth, int footprintHeight, int gridWidth, int gridHeight)
    {
        int texelCount = footprintWidth * footprintHeight;
        int[] indices = new int[4 * texelCount];
        int[] factors = new int[4 * texelCount];

        int scaleHorizontal = GetScaleFactorD(footprintWidth);
        int scaleVertical = GetScaleFactorD(footprintHeight);
        int gridLimit = gridWidth * gridHeight;
        int maxGridX = gridWidth - 1;
        int maxGridY = gridHeight - 1;

        int texelIndex = 0;
        for (int texelY = 0; texelY < footprintHeight; ++texelY)
        {
            (int gridRowIndex, int fractionY) = MapTexelToGridAxis(texelY, scaleVertical, maxGridY);
            for (int texelX = 0; texelX < footprintWidth; ++texelX)
            {
                (int gridColIndex, int fractionX) = MapTexelToGridAxis(texelX, scaleHorizontal, maxGridX);
                StoreTexelContributions(texelIndex, texelCount, indices, factors, gridColIndex, gridRowIndex, fractionX, fractionY, gridWidth, gridLimit);
                texelIndex++;
            }
        }

        return new DecimationInfo(texelCount, indices, factors);
    }

    /// <summary>
    /// Maps a texel coordinate along one axis to the (gridIndex, fraction) pair used for
    /// bilinear interpolation. The grid index is in Q4 fixed-point (top bits) and the
    /// fraction occupies the low four bits.
    /// </summary>
    private static (int GridIndex, int Fraction) MapTexelToGridAxis(int texel, int scale, int maxGrid)
    {
        int scaled = scale * texel;
        int grid = ((scaled * maxGrid) + 32) >> 6;
        return (grid >> 4, grid & 0xF);
    }

    /// <summary>
    /// Computes the four (gridPoint, factor) contributions for one texel and writes them
    /// into the transposed output arrays. Each contribution slot has <paramref name="texelCount"/>
    /// entries so lookups at decode time touch contiguous memory per slot.
    /// Out-of-bounds grid points collapse to index 0 with a zero factor.
    /// </summary>
    private static void StoreTexelContributions(
        int texelIndex,
        int texelCount,
        int[] indices,
        int[] factors,
        int gridColIndex,
        int gridRowIndex,
        int fractionX,
        int fractionY,
        int gridWidth,
        int gridLimit)
    {
        int gridPoint0 = gridColIndex + (gridWidth * gridRowIndex);
        int gridPoint1 = gridPoint0 + 1;
        int gridPoint2 = gridColIndex + (gridWidth * (gridRowIndex + 1));
        int gridPoint3 = gridPoint2 + 1;

        int factor3 = ((fractionX * fractionY) + 8) >> 4;
        int factor2 = fractionY - factor3;
        int factor1 = fractionX - factor3;
        int factor0 = 16 - fractionX - fractionY + factor3;

        ClampGridPoint(ref gridPoint0, ref factor0, gridLimit);
        ClampGridPoint(ref gridPoint1, ref factor1, gridLimit);
        ClampGridPoint(ref gridPoint2, ref factor2, gridLimit);
        ClampGridPoint(ref gridPoint3, ref factor3, gridLimit);

        indices[texelIndex] = gridPoint0;
        indices[texelCount + texelIndex] = gridPoint1;
        indices[(2 * texelCount) + texelIndex] = gridPoint2;
        indices[(3 * texelCount) + texelIndex] = gridPoint3;

        factors[texelIndex] = factor0;
        factors[texelCount + texelIndex] = factor1;
        factors[(2 * texelCount) + texelIndex] = factor2;
        factors[(3 * texelCount) + texelIndex] = factor3;
    }

    /// <summary>
    /// Replaces an out-of-bounds grid point with a safe dummy index (0) and zeros its
    /// contribution factor so the corresponding term drops out of the bilinear blend.
    /// </summary>
    private static void ClampGridPoint(ref int gridPoint, ref int factor, int gridLimit)
    {
        if (gridPoint >= gridLimit)
        {
            factor = 0;
            gridPoint = 0;
        }
    }
}
