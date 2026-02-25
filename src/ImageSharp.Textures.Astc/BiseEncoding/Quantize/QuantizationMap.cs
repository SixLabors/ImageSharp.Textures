// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;

internal class QuantizationMap
{
    // Flat arrays for O(1) lookup on the hot path (set by Freeze)
    private int[] quantizationMap = [];
    private int[] unquantizationMap = [];

    protected List<int> QuantizationMapBuilder { get; set; } = [];

    protected List<int> UnquantizationMapBuilder { get; set; } = [];

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
    /// Converts builder lists to flat arrays. Called after construction is complete.
    /// </summary>
    protected void Freeze()
    {
        this.unquantizationMap = [.. this.UnquantizationMapBuilder];
        this.quantizationMap = [.. this.QuantizationMapBuilder];
        this.UnquantizationMapBuilder = [];
        this.QuantizationMapBuilder = [];
    }

    protected void GenerateQuantizationMap()
    {
        if (this.UnquantizationMapBuilder.Count <= 1)
        {
            return;
        }

        this.QuantizationMapBuilder.Clear();
        for (int i = 0; i < 256; ++i)
        {
            int bestIndex = 0;
            int bestScore = int.MaxValue;
            for (int index = 0; index < this.UnquantizationMapBuilder.Count; ++index)
            {
                int diff = i - this.UnquantizationMapBuilder[index];
                int score = diff * diff;
                if (score < bestScore)
                {
                    bestIndex = index;
                    bestScore = score;
                }
            }

            this.QuantizationMapBuilder.Add(bestIndex);
        }
    }
}
