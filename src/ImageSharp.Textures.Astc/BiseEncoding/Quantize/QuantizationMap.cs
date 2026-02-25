// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;

internal class QuantizationMap
{
    protected List<int> _quantizationMapBuilder = [];
    protected List<int> _unquantizationMapBuilder = [];

    // Flat arrays for O(1) lookup on the hot path (set by Freeze)
    private int[] _quantizationMap = [];
    private int[] _unquantizationMap = [];

    public int Quantize(int x)
        => (uint)x < (uint)_quantizationMap.Length
            ? _quantizationMap[x]
            : 0;

    public int Unquantize(int x)
        => (uint)x < (uint)_unquantizationMap.Length
            ? _unquantizationMap[x]
            : 0;

    internal static int Log2Floor(int value)
    {
        int result = 0;
        while ((1 << (result + 1)) <= value) result++;
        return result;
    }

    /// <summary>
    /// Converts builder lists to flat arrays. Called after construction is complete.
    /// </summary>
    protected void Freeze()
    {
        _unquantizationMap = [.. _unquantizationMapBuilder];
        _quantizationMap = [.. _quantizationMapBuilder];
        _unquantizationMapBuilder = [];
        _quantizationMapBuilder = [];
    }

    protected void GenerateQuantizationMap()
    {
        if (_unquantizationMapBuilder.Count <= 1) return;
        _quantizationMapBuilder.Clear();
        for (int i = 0; i < 256; ++i)
        {
            int bestIndex = 0;
            int bestScore = int.MaxValue;
            for (int index = 0; index < _unquantizationMapBuilder.Count; ++index)
            {
                int diff = i - _unquantizationMapBuilder[index];
                int score = diff * diff;
                if (score < bestScore) { bestIndex = index; bestScore = score; }
            }
            _quantizationMapBuilder.Add(bestIndex);
        }
    }
}
