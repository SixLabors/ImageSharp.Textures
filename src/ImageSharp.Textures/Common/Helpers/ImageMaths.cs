// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Common.Helpers
{
    /// <summary>
    /// Provides common image math methods.
    /// </summary>
    public static class ImageMaths
    {
        /// <summary>
        /// Scales a value from a 16 bit <see cref="ushort"/> to it's 8 bit <see cref="byte"/> equivalent.
        /// </summary>
        /// <param name="component">The 8 bit component value.</param>
        /// <returns>The <see cref="byte"/></returns>
        public static byte DownScaleFrom16BitTo8Bit(ushort component) => (byte)(component / 0x101);

        /// <summary>
        /// Scales a value from a 32 bit <see cref="ushort"/> to it's 8 bit <see cref="byte"/> equivalent.
        /// </summary>
        /// <param name="component">The 8 bit component value.</param>
        /// <returns>The <see cref="byte"/></returns>
        public static byte DownScaleFrom32BitTo8Bit(uint component) => (byte)(component / 0x1010101);

        /// <summary>
        /// Scales a value from a 64 bit <see cref="ushort"/> to it's 8 bit <see cref="byte"/> equivalent.
        /// </summary>
        /// <param name="component">The 8 bit component value.</param>
        /// <returns>The <see cref="byte"/></returns>
        public static byte DownScaleFrom64BitTo8Bit(ulong component) => (byte)(component / 0x101010101010101);

        /// <summary>
        /// Scales a value from a 32 bit <see cref="ushort"/> to it's 16 bit <see cref="ushort"/> equivalent.
        /// </summary>
        /// <param name="component">The 16 bit component value.</param>
        /// <returns>The <see cref="ushort"/></returns>
        public static ushort DownScaleFrom32BitTo16Bit(uint component) => (ushort)(component / 0x10001);

        /// <summary>
        /// Scales a value from a 64 bit <see cref="ushort"/> to it's 16 bit <see cref="ushort"/> equivalent.
        /// </summary>
        /// <param name="component">The 16 bit component value.</param>
        /// <returns>The <see cref="ushort"/></returns>
        public static ushort DownScaleFrom64BitTo16Bit(ulong component) => (ushort)(component / 0x1000100010001);

        /// <summary>
        /// Scales a value from a 64 bit <see cref="ulong"/> to it's 32 bit <see cref="uint"/> equivalent.
        /// </summary>
        /// <param name="component">The 32 bit component value.</param>
        /// <returns>The <see cref="uint"/></returns>
        public static uint DownScaleFrom64BitTo32Bit(ulong component) => (uint)(component / 0x100000001);

        public static ushort UpscaleFrom8BitTo16Bit(byte component) => (ushort)(component * 0x101);

        public static uint UpscaleFrom8BitTo32Bit(byte component) => (uint)(component * 0x1010101);

        public static ulong UpscaleFrom8BitTo64Bit(byte component) => (ulong)(component * 0x101010101010101);

        public static uint UpscaleFrom16BitTo32Bit(ushort component) => (uint)(component * 0x10001);

        public static ulong UpscaleFrom16BitTo64Bit(ushort component) => (ulong)(component * 0x1000100010001);

        public static ulong UpscaleFrom32BitTo64Bit(uint component) => (ulong)(component * 0x100000001);
    }
}
