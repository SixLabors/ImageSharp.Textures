// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;

/// <summary>
/// Builds <see cref="QuantizationMap"/> instances for the pure-bit BISE encoding mode
/// (no trits/quints). Bit-replicates each quantized value up to <c>totalUnquantizedBits</c>
/// width to derive its unquantized form (ASTC spec §C.2.18).
/// </summary>
internal static class BitQuantizationMap
{
    /// <param name="range">Inclusive upper bound of the quantized slot index. <c>range + 1</c>
    /// must be a power of two.</param>
    /// <param name="totalUnquantizedBits">Bit width of the unquantized output: 8 for endpoint
    /// values, 6 for weights.</param>
    public static QuantizationMap Create(int range, int totalUnquantizedBits)
    {
        Guard.IsTrue(CountOnes(range + 1) == 1, nameof(range), "range + 1 must be a power of two.");

        int bitCount = QuantizationMap.Log2Floor(range + 1);
        List<int> unquantization = [];
        List<int> quantization = [];

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

            if (unquantizedBitCount != totalUnquantizedBits)
            {
                throw new InvalidOperationException();
            }

            unquantization.Add(unquantized);

            if (bits > 0)
            {
                int previousUnquantized = unquantization[bits - 1];
                while (quantization.Count <= (previousUnquantized + unquantized) / 2)
                {
                    quantization.Add(bits - 1);
                }
            }

            while (quantization.Count <= unquantized)
            {
                quantization.Add(bits);
            }
        }

        return new QuantizationMap([.. quantization], [.. unquantization]);
    }

    private static int CountOnes(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }
}
