// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

/// <summary>
/// ASTC-specific extension methods and helpers for <see cref="Rgba64"/>.
/// </summary>
internal static class Rgba64Extensions
{
    /// <summary>
    /// Gets the channel value at the specified index: 0=R, 1=G, 2=B, 3=A.
    /// </summary>
    /// <remarks>
    /// Reads the sequential [R, G, B, A] ushort layout of <see cref="Rgba64"/> directly.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetChannel(this in Rgba64 color, int i)
    {
        if ((uint)i >= 4)
        {
            throw new ArgumentOutOfRangeException(nameof(i), $"Index must be between 0 and 3. Actual value: {i}.");
        }

        return Unsafe.Add(ref Unsafe.As<Rgba64, ushort>(ref Unsafe.AsRef(in color)), i);
    }

    /// <summary>
    /// Returns true if all four channels are within the specified tolerance of the other color.
    /// </summary>
    public static bool IsCloseTo(this Rgba64 color, Rgba64 other, int tolerance)
        => Math.Abs(color.R - other.R) <= tolerance &&
           Math.Abs(color.G - other.G) <= tolerance &&
           Math.Abs(color.B - other.B) <= tolerance &&
           Math.Abs(color.A - other.A) <= tolerance;
}
