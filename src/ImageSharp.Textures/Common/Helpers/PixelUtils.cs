// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Common.Helpers
{
    /// <summary>
    /// Provides methods for calculating pixel values.
    /// </summary>
    public static class PixelUtils
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
    }
}
