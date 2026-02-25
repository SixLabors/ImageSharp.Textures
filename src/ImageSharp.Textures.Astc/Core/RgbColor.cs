// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.Core;

internal readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static int BytesPerPixel => 3;

    public static RgbColor Empty => default;

    /// <summary>
    /// The rounded arithmetic mean of the R, G, and B channels
    /// </summary>
    public byte Average
    {
        get
        {
            var sum = R + G + B;
            return (byte)((sum * 256 + 384) / 768);
        }
    }

    public RgbColor(int r, int g, int b) : this(
        (byte)Math.Clamp(r, byte.MinValue, byte.MaxValue),
        (byte)Math.Clamp(g, byte.MinValue, byte.MaxValue),
        (byte)Math.Clamp(b, byte.MinValue, byte.MaxValue))
    {
    }

    public int this[int i]
        => i switch
        {
            0 => R,
            1 => G,
            2 => B,
            _ => throw new ArgumentOutOfRangeException(nameof(i), $"Index must be between 0 and {BytesPerPixel - 1}. Actual value: {i}.")
        };

    public static int SquaredError(RgbColor a, RgbColor b)
    {
        int result = 0;
        for (int i = 0; i < BytesPerPixel; i++)
        {
            int diff = a[i] - b[i];
            result += diff * diff;
        }
        return result;
    }

    /// <summary>
    /// Computes the squared error comparing only the RGB channels of two RgbaColors.
    /// </summary>
    public static int SquaredError(RgbaColor a, RgbaColor b)
    {
        int dr = a.R - b.R;
        int dg = a.G - b.G;
        int db = a.B - b.B;
        return dr * dr + dg * dg + db * db;
    }
}
