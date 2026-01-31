// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Common.Helpers
{
    /// <summary>
    /// Provides methods for calculating pixel values.
    /// </summary>
    internal static class PixelUtils
    {
        /// <summary>
        /// Performs final shifting from a 5bit value to an 8bit one.
        /// </summary>
        /// <param name="value">The masked and shifted value.</param>
        /// <returns>The <see cref="byte"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetBytesFrom5BitValue(int value) => (byte)((value << 3) | (value >> 2));

        /// <summary>
        /// Performs final shifting from a 6bit value to an 8bit one.
        /// </summary>
        /// <param name="value">The masked and shifted value.</param>
        /// <returns>The <see cref="byte"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetBytesFrom6BitValue(int value) => (byte)((value << 2) | (value >> 4));

        /// <summary>
        /// Extracts the R5G6B5 values from a packed ushort pixel in that order.
        /// </summary>
        /// <param name="color">The packed color.</param>
        /// <param name="dest">The extracted color.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtractR5G6B5(ushort color, ref Rgb24 dest)
        {
            var r = (color & 0xF800) >> 11;
            var g = (color & 0x7E0) >> 5;
            var b = color & 0x1f;
            dest.R = PixelUtils.GetBytesFrom5BitValue(r);
            dest.G = PixelUtils.GetBytesFrom6BitValue(g);
            dest.B = PixelUtils.GetBytesFrom5BitValue(b);
        }
    }
}
