// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;

internal sealed class QuintQuantizationMap : QuantizationMap
{
    public QuintQuantizationMap(int range, Func<int, int, int, int> unquantFunc)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual((range + 1) % 5, 0);

        int bitsPowerOfTwo = (range + 1) / 5;
        int bitCount = bitsPowerOfTwo == 0 ? 0 : Log2Floor(bitsPowerOfTwo);

        for (int quint = 0; quint < 5; ++quint)
        {
            for (int bits = 0; bits < (1 << bitCount); ++bits)
            {
                this.UnquantizationMapBuilder.Add(unquantFunc(quint, bits, range));
            }
        }

        this.GenerateQuantizationMap();
        this.Freeze();
    }

    internal static int GetUnquantizedValue(int quint, int bits, int range)
    {
        int a = (bits & 1) != 0 ? 0x1FF : 0;
        var (b, c) = range switch
        {
            9 => (0, 113),
            19 => ((bits >> 1) & 0x1) is var x ? ((x << 2) | (x << 3) | (x << 8), 54) : default,
            39 => ((bits >> 1) & 0x3) is var x ? ((x >> 1) | (x << 1) | (x << 7), 26) : default,
            79 => ((bits >> 1) & 0x7) is var x ? ((x >> 1) | (x << 6), 13) : default,
            159 => ((bits >> 1) & 0xF) is var x ? ((x >> 3) | (x << 5), 6) : default,
            _ => throw new ArgumentException("Illegal quint encoding")
        };
        int t = (quint * c) + b;
        t ^= a;
        t = (a & 0x80) | (t >> 2);
        return t;
    }

    internal static int GetUnquantizedWeight(int quint, int bits, int range)
    {
        if (range == 4)
        {
            int[] weights = [0, 16, 32, 47, 63];
            return weights[quint];
        }

        int a = (bits & 1) != 0 ? 0x7F : 0;
        var (b, c) = range switch
        {
            9 => (0, 28),
            19 => ((bits >> 1) & 0x1) is var x ? ((x << 1) | (x << 6), 13) : default,
            _ => throw new ArgumentException("Illegal quint encoding")
        };
        int t = (quint * c) + b;
        t ^= a;
        t = (a & 0x20) | (t >> 2);
        return t;
    }
}
