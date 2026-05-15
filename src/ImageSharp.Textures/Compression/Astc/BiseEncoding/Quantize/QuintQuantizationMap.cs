// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;

/// <summary>
/// Builds <see cref="QuantizationMap"/> instances for the quint BISE encoding mode plus the
/// per-quint unquantization tables for endpoint values and weights (ASTC spec §C.2.18).
/// </summary>
internal static class QuintQuantizationMap
{
    /// <param name="range">Inclusive upper bound of the quantized slot index. <c>range + 1</c>
    /// must be divisible by 5.</param>
    /// <param name="unquantFunc">Per-quint unquantization function — typically
    /// <see cref="GetUnquantizedValue"/> or <see cref="GetUnquantizedWeight"/>.</param>
    public static QuantizationMap Create(int range, Func<int, int, int, int> unquantFunc)
    {
        Guard.IsTrue((range + 1) % 5 == 0, nameof(range), "range + 1 must be a multiple of 5.");

        int bitsPowerOfTwo = (range + 1) / 5;
        int bitCount = bitsPowerOfTwo == 0 ? 0 : QuantizationMap.Log2Floor(bitsPowerOfTwo);

        int[] unquantization = new int[5 * (1 << bitCount)];
        int idx = 0;
        for (int quint = 0; quint < 5; ++quint)
        {
            for (int bits = 0; bits < (1 << bitCount); ++bits)
            {
                unquantization[idx++] = unquantFunc(quint, bits, range);
            }
        }

        int[] quantization = QuantizationMap.BuildQuantizationMapFromUnquantized(unquantization);
        return new QuantizationMap(quantization, unquantization);
    }

    internal static int GetUnquantizedValue(int quint, int bits, int range)
    {
        int a = (bits & 1) != 0 ? 0x1FF : 0;
        (int b, int c) = range switch
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
        (int b, int c) = range switch
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
