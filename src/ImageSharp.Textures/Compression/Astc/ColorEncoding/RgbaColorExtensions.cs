// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using static SixLabors.ImageSharp.Textures.Compression.Astc.Core.Rgba32Extensions;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

internal static class RgbaColorExtensions
{
    /// <summary>
    /// Uses the value in the blue channel to tint the red and green
    /// </summary>
    /// <remarks>
    /// Applies the 'blue_contract' function defined in Section C.2.14 of the ASTC specification.
    /// </remarks>
    public static Rgba32 WithBlueContract(int red, int green, int blue, int alpha)
        => ClampedRgba32(
            r: (red + blue) >> 1,
            g: (green + blue) >> 1,
            b: blue,
            a: alpha);

    /// <summary>
    /// Uses the value in the blue channel to tint the red and green
    /// </summary>
    /// <remarks>
    /// Applies the 'blue_contract' function defined in Section C.2.14 of the ASTC specification.
    /// </remarks>
    public static Rgba32 WithBlueContract(this Rgba32 color)
        => WithBlueContract(color.R, color.G, color.B, color.A);

    /// <summary>
    /// The inverse of <see cref="WithBlueContract(Rgba32)"/>
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
