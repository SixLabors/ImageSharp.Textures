// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;

internal sealed class TritQuantizationMap : QuantizationMap
{
    public TritQuantizationMap(int range, Func<int, int, int, int> unquantFunc)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual((range + 1) % 3, 0);

        int bitsPowerOfTwo = (range + 1) / 3;
        int bitCount = bitsPowerOfTwo == 0 ? 0 : Log2Floor(bitsPowerOfTwo);

        for (int trit = 0; trit < 3; ++trit)
        {
            for (int bits = 0; bits < (1 << bitCount); ++bits)
            {
                this.UnquantizationMapBuilder.Add(unquantFunc(trit, bits, range));
            }
        }

        this.GenerateQuantizationMap();
        this.Freeze();
    }

    internal static int GetUnquantizedValue(int trit, int bits, int range)
    {
        int a = (bits & 1) != 0 ? 0x1FF : 0;
        (int b, int c) = range switch
        {
            5 => (0, 204),
            11 => ((bits >> 1) & 0x1) is var x ? ((x << 1) | (x << 2) | (x << 4) | (x << 8), 93) : default,
            23 => ((bits >> 1) & 0x3) is var x ? (x | (x << 2) | (x << 7), 44) : default,
            47 => ((bits >> 1) & 0x7) is var x ? (x | (x << 6), 22) : default,
            95 => ((bits >> 1) & 0xF) is var x ? ((x >> 2) | (x << 5), 11) : default,
            191 => ((bits >> 1) & 0x1F) is var x ? ((x >> 4) | (x << 4), 5) : default,
            _ => throw new ArgumentException("Illegal trit encoding")
        };
        int t = (trit * c) + b;
        t ^= a;
        t = (a & 0x80) | (t >> 2);
        return t;
    }

    internal static int GetUnquantizedWeight(int trit, int bits, int range)
    {
        if (range == 2)
        {
            return trit switch
            {
                0 => 0,
                1 => 32,
                _ => 63
            };
        }

        int a = (bits & 1) != 0 ? 0x7F : 0;
        (int b, int c) = range switch
        {
            5 => (0, 50),
            11 => ((bits >> 1) & 1) is var x
                ? (x | (x << 2) | (x << 6), 23)
                : default,
            23 => ((bits >> 1) & 0x3) is var x
                ? (x | (x << 5), 11)
                : default,
            _ => throw new ArgumentException("Illegal trit encoding")
        };
        int t = (trit * c) + b;
        t ^= a;
        return (a & 0x20) | (t >> 2);
    }
}
