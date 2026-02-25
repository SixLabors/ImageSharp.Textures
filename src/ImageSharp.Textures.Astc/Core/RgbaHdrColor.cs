// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.Core;

/// <summary>
/// Represents an HDR (High Dynamic Range) color with 16-bit per-channel precision.
/// </summary>
/// <remarks>
/// HDR colors use ushort values (0-65535) for each channel, allowing representation
/// of values beyond the standard 0-255 LDR range. This enables encoding of High Dynamic
/// Range content that can represent brightness values exceeding the typical white point.
/// </remarks>
internal readonly record struct RgbaHdrColor(ushort R, ushort G, ushort B, ushort A)
{
    public static RgbaHdrColor Empty => default;

    /// <summary>
    /// Indexer to access channels by index: 0=R, 1=G, 2=B, 3=A
    /// </summary>
    public ushort this[int i] => i switch
    {
        0 => R,
        1 => G,
        2 => B,
        3 => A,
        _ => throw new ArgumentOutOfRangeException(nameof(i), $"Index must be between 0 and 3. Actual value: {i}.")
    };

    /// <summary>
    /// Converts an LDR color (0-255) to HDR range (0-65535).
    /// </summary>
    public static RgbaHdrColor FromRgba(RgbaColor ldr)
        => new((ushort)(ldr.R * 257), (ushort)(ldr.G * 257), (ushort)(ldr.B * 257), (ushort)(ldr.A * 257));

    /// <summary>
    /// Converts an HDR color (0-65535) to LDR range (0-255).
    /// </summary>
    /// <remarks>
    /// Values are clamped to 0-255 range, so HDR values exceeding
    /// the standard white point will be clipped.
    /// </remarks>
    public RgbaColor ToLowDynamicRange()
        => new((byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8), (byte)(A >> 8));

    public bool IsCloseTo(RgbaHdrColor other, int tolerance)
        => Math.Abs(R - other.R) <= tolerance &&
           Math.Abs(G - other.G) <= tolerance &&
           Math.Abs(B - other.B) <= tolerance &&
           Math.Abs(A - other.A) <= tolerance;
}
