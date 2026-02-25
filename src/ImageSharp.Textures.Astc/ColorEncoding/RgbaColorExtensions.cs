// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.ColorEncoding;

internal static class RgbaColorExtensions
{
    /// <summary>
    /// Uses the value in the blue channel to tint the red and green
    /// </summary>
    /// <remarks>
    /// Applies the 'blue_contract' function defined in Section C.2.14 of the ASTC specification.
    /// </remarks>
    public static RgbaColor WithBlueContract(int red, int green, int blue, int alpha)
        => new(
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
    public static RgbaColor WithBlueContract(this RgbaColor color)
        => WithBlueContract(color.R, color.G, color.B, color.A);

    /// <summary>
    /// The inverse of <see cref="WithBlueContract(RgbaColor)"/>
    /// </summary>
    public static RgbaColor WithInvertedBlueContract(this RgbaColor color)
        => new(
            r: (2 * color.R) - color.B,
            g: (2 * color.G) - color.B,
            b: color.B,
            a: color.A);

    public static RgbaColor AsOffsetFrom(this RgbaColor color, RgbaColor baseColor)
    {
        int[] offset = [color.R, color.G, color.B, color.A];

        for (int i = 0; i < RgbaColor.BytesPerPixel; ++i)
        {
            var (a, b) = BitOperations.TransferPrecision(offset[i], baseColor[i]);
            offset[i] = Math.Clamp(baseColor[i] + a, byte.MinValue, byte.MaxValue);
        }

        return new RgbaColor(offset[0], offset[1], offset[2], offset[3]);
    }
}
