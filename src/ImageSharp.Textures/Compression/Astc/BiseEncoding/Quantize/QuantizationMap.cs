// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;

/// <summary>
/// Pre-computed quantize/unquantize lookup tables for a single ASTC quantization range.
/// Both arrays are constructed once and the instance is immutable thereafter, built via
/// <see cref="BitQuantizationMap"/>, <see cref="TritQuantizationMap"/>, or <see cref="QuintQuantizationMap"/>.
/// </summary>
internal sealed class QuantizationMap
{
    private readonly int[] quantizationMap;
    private readonly int[] unquantizationMap;

    /// <param name="quantizationMap">Length 256 (or shorter); maps an unquantized value to its
    /// nearest quantized slot.</param>
    /// <param name="unquantizationMap">Length <c>range + 1</c>; maps a quantized slot back to
    /// its unquantized value.</param>
    public QuantizationMap(int[] quantizationMap, int[] unquantizationMap)
    {
        this.quantizationMap = quantizationMap;
        this.unquantizationMap = unquantizationMap;
    }

    public int Quantize(int x)
        => (uint)x < (uint)this.quantizationMap.Length
            ? this.quantizationMap[x]
            : 0;

    public int Unquantize(int x)
        => (uint)x < (uint)this.unquantizationMap.Length
            ? this.unquantizationMap[x]
            : 0;

    internal static int Log2Floor(int value)
    {
        int result = 0;
        while ((1 << (result + 1)) <= value)
        {
            result++;
        }

        return result;
    }

    /// <summary>
    /// Builds a quantize-table from an already-populated unquantize-table by, for every
    /// unquantized value in [0, 255], picking the index in <paramref name="unquantized"/>
    /// whose value is closest. Used by <see cref="TritQuantizationMap"/> and
    /// <see cref="QuintQuantizationMap"/>; <see cref="BitQuantizationMap"/> builds its
    /// quantize table inline because the structure of bit-replication makes the closest
    /// match analytically derivable without a search.
    /// </summary>
    internal static int[] BuildQuantizationMapFromUnquantized(int[] unquantized)
    {
        int[] quantization = new int[256];
        for (int i = 0; i < 256; ++i)
        {
            int bestIndex = 0;
            int bestScore = int.MaxValue;
            for (int index = 0; index < unquantized.Length; ++index)
            {
                int diff = i - unquantized[index];
                int score = diff * diff;
                if (score < bestScore)
                {
                    bestIndex = index;
                    bestScore = score;
                }
            }

            quantization[i] = bestIndex;
        }

        return quantization;
    }
}
