// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.Core;

internal readonly record struct RgbaColor(byte R, byte G, byte B, byte A)
{
    public static int BytesPerPixel => 4;

    public static RgbaColor Empty => default;

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

    public RgbaColor(int r, int g, int b, int a = byte.MaxValue) : this(
        (byte)Math.Clamp(r, byte.MinValue, byte.MaxValue),
        (byte)Math.Clamp(g, byte.MinValue, byte.MaxValue),
        (byte)Math.Clamp(b, byte.MinValue, byte.MaxValue),
        (byte)Math.Clamp(a, byte.MinValue, byte.MaxValue))
    {
    }

    public int this[int i]
        => i switch
        {
            0 => R,
            1 => G,
            2 => B,
            3 => A,
            _ => throw new ArgumentOutOfRangeException(nameof(i), $"Index must be between 0 and {BytesPerPixel - 1}. Actual value: {i}.")
        };

    public static int SquaredError(RgbaColor a, RgbaColor b)
    {
        int result = 0;
        for (int i = 0; i < BytesPerPixel; i++)
        {
            int diff = a[i] - b[i];
            result += diff * diff;
        }
        return result;
    }

    public bool IsCloseTo(RgbaColor other, int tolerance)
        => Math.Abs(R - other.R) <= tolerance &&
           Math.Abs(G - other.G) <= tolerance &&
           Math.Abs(B - other.B) <= tolerance &&
           Math.Abs(A - other.A) <= tolerance;
}
