// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;

internal sealed class BitQuantizationMap : QuantizationMap
{
    // TotalUnquantizedBits is 8 for endpoint values and 6 for weights
    public BitQuantizationMap(int range, int totalUnquantizedBits)
    {
        // ensure range+1 is power of two
        ArgumentOutOfRangeException.ThrowIfNotEqual(CountOnes(range + 1), 1);

        int bitCount = Log2Floor(range + 1);

        for (int bits = 0; bits <= range; bits++)
        {
            int unquantized = bits;
            int unquantizedBitCount = bitCount;
            while (unquantizedBitCount < totalUnquantizedBits)
            {
                int destinationShiftUp = Math.Min(bitCount, totalUnquantizedBits - unquantizedBitCount);
                int sourceShiftDown = bitCount - destinationShiftUp;
                unquantized <<= destinationShiftUp;
                unquantized |= bits >> sourceShiftDown;
                unquantizedBitCount += destinationShiftUp;
            }
            if (unquantizedBitCount != totalUnquantizedBits) throw new InvalidOperationException();
            _unquantizationMapBuilder.Add(unquantized);

            if (bits > 0)
            {
                int previousUnquantized = _unquantizationMapBuilder[bits - 1];
                while (_quantizationMapBuilder.Count <= (previousUnquantized + unquantized) / 2)
                    _quantizationMapBuilder.Add(bits - 1);
            }
            while (_quantizationMapBuilder.Count <= unquantized) _quantizationMapBuilder.Add(bits);
        }

        Freeze();
    }

    private static int CountOnes(int value)
    {
        int count = 0;
        while (value != 0) { count += value & 1; value >>= 1; }
        return count;
    }
}
