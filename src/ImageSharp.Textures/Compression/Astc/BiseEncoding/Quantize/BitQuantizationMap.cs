// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;

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

            if (unquantizedBitCount != totalUnquantizedBits)
            {
                throw new InvalidOperationException();
            }

            this.UnquantizationMapBuilder.Add(unquantized);

            if (bits > 0)
            {
                int previousUnquantized = this.UnquantizationMapBuilder[bits - 1];
                while (this.QuantizationMapBuilder.Count <= (previousUnquantized + unquantized) / 2)
                {
                    this.QuantizationMapBuilder.Add(bits - 1);
                }
            }

            while (this.QuantizationMapBuilder.Count <= unquantized)
            {
                this.QuantizationMapBuilder.Add(bits);
            }
        }

        this.Freeze();
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
