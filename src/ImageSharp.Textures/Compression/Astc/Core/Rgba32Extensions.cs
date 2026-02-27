// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// ASTC-specific extension methods and helpers for <see cref="Rgba32"/>.
/// </summary>
internal static class Rgba32Extensions
{
    /// <summary>
    /// Creates an <see cref="Rgba32"/> from integer values, clamping each channel to [0, 255].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgba32 ClampedRgba32(int r, int g, int b, int a = byte.MaxValue)
        => new(
            (byte)Math.Clamp(r, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(g, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(b, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(a, byte.MinValue, byte.MaxValue));

    /// <summary>
    /// Gets the rounded arithmetic mean of the R, G, and B channels.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetAverage(this Rgba32 color)
    {
        int sum = color.R + color.G + color.B;
        return (byte)(((sum * 256) + 384) / 768);
    }

    /// <summary>
    /// Gets the channel value at the specified index: 0=R, 1=G, 2=B, 3=A.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetChannel(this Rgba32 color, int i)
        => i switch
        {
            0 => color.R,
            1 => color.G,
            2 => color.B,
            3 => color.A,
            _ => throw new ArgumentOutOfRangeException(nameof(i), $"Index must be between 0 and 3. Actual value: {i}.")
        };

    /// <summary>
    /// Computes the sum of squared per-channel differences across all four RGBA channels.
    /// </summary>
    public static int SquaredError(Rgba32 a, Rgba32 b)
    {
        int dr = a.R - b.R;
        int dg = a.G - b.G;
        int db = a.B - b.B;
        int da = a.A - b.A;
        return (dr * dr) + (dg * dg) + (db * db) + (da * da);
    }

    /// <summary>
    /// Computes the sum of squared per-channel differences for the RGB channels only, ignoring alpha.
    /// </summary>
    public static int SquaredErrorRgb(Rgba32 a, Rgba32 b)
    {
        int dr = a.R - b.R;
        int dg = a.G - b.G;
        int db = a.B - b.B;
        return (dr * dr) + (dg * dg) + (db * db);
    }

    /// <summary>
    /// Returns true if all four channels are within the specified tolerance of the other color.
    /// </summary>
    public static bool IsCloseTo(this Rgba32 color, Rgba32 other, int tolerance)
        => Math.Abs(color.R - other.R) <= tolerance &&
           Math.Abs(color.G - other.G) <= tolerance &&
           Math.Abs(color.B - other.B) <= tolerance &&
           Math.Abs(color.A - other.A) <= tolerance;
}
