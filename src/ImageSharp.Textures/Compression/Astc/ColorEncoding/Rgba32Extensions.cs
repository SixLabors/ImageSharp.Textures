// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

/// <summary>
/// ASTC-specific extension methods and helpers for <see cref="Rgba32"/>, including the
/// blue-contract transform used by several LDR endpoint modes (ASTC spec §C.2.14).
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
    /// <remarks>
    /// Reads the sequential [R, G, B, A] byte layout of <see cref="Rgba32"/> directly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetChannel(this in Rgba32 color, int i)
    {
        if ((uint)i >= 4)
        {
            throw new ArgumentOutOfRangeException(nameof(i), $"Index must be between 0 and 3. Actual value: {i}.");
        }

        return Unsafe.Add(ref Unsafe.As<Rgba32, byte>(ref Unsafe.AsRef(in color)), i);
    }

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

    /// <summary>
    /// Uses the value in the blue channel to tint the red and green
    /// (the 'blue_contract' function defined in ASTC spec §C.2.14).
    /// </summary>
    public static Rgba32 WithBlueContract(int red, int green, int blue, int alpha)
        => ClampedRgba32(
            r: (red + blue) >> 1,
            g: (green + blue) >> 1,
            b: blue,
            a: alpha);

    /// <summary>
    /// Uses the value in the blue channel to tint the red and green
    /// (the 'blue_contract' function defined in ASTC spec §C.2.14).
    /// </summary>
    public static Rgba32 WithBlueContract(this Rgba32 color)
        => WithBlueContract(color.R, color.G, color.B, color.A);

    /// <summary>
    /// The inverse of <see cref="WithBlueContract(Rgba32)"/>.
    /// </summary>
    public static Rgba32 WithInvertedBlueContract(this Rgba32 color)
        => ClampedRgba32(
            r: (2 * color.R) - color.B,
            g: (2 * color.G) - color.B,
            b: color.B,
            a: color.A);

    public static Rgba32 AsOffsetFrom(this Rgba32 color, Rgba32 baseColor)
    {
        int[] offset = [color.R, color.G, color.B, color.A];

        for (int i = 0; i < 4; ++i)
        {
            (int a, int b) = BitOperations.TransferPrecision(offset[i], baseColor.GetChannel(i));
            offset[i] = Math.Clamp(baseColor.GetChannel(i) + a, byte.MinValue, byte.MaxValue);
        }

        return ClampedRgba32(offset[0], offset[1], offset[2], offset[3]);
    }
}
