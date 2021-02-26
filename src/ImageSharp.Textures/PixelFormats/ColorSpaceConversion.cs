// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    internal static class ColorSpaceConversion
    {
        private const float Max8Bit = 256F;

        private const float Max10Bit = 1023F;

        private const float Max16Bit = 65535F;

        private static readonly Vector4 Multiplier16Bit = new Vector4(Max16Bit, Max16Bit, Max16Bit, Max16Bit);

        private static readonly Vector4 Multiplier10Bit = new Vector4(Max10Bit, Max10Bit, Max10Bit, 3F);

        private static readonly Vector4 Multiplier8Bit = new Vector4(Max8Bit, Max8Bit, Max8Bit, Max8Bit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 YuvToRgba16Bit(uint y, uint u, uint v, uint a = ushort.MaxValue)
        {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/bb970578.aspx
            // R = 1.1689Y' + 1.6023Cr'
            // G = 1.1689Y' - 0.3933Cb' - 0.8160Cr'
            // B = 1.1689Y'+ 2.0251Cb'
            uint r = ((76607 * y) + (105006 * v) + 32768) >> 16;
            uint g = ((76607 * y) - (25772 * u) - (53477 * v) + 32768) >> 16;
            uint b = ((76607 * y) + (132718 * u) + 32768) >> 16;

            return new Vector4(Clamp(r, 0, ushort.MaxValue), Clamp(g, 0, ushort.MaxValue), Clamp(b, 0, ushort.MaxValue), Clamp(a, 0, ushort.MaxValue)) / Multiplier16Bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 YuvToRgba10Bit(uint y, uint u, uint v, uint a = 1023)
        {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/bb970578.aspx
            // R = 1.1678Y' + 1.6007Cr'
            // G = 1.1678Y' - 0.3929Cb' - 0.8152Cr'
            // B = 1.1678Y' + 2.0232Cb'
            uint r = ((76533 * y) + (104905 * v) + 32768) >> 16;
            uint g = ((76533 * y) - (25747 * u) - (53425 * v) + 32768) >> 16;
            uint b = ((76533 * y) + (132590 * u) + 32768) >> 16;

            return new Vector4(Clamp(r, 0, 1023), Clamp(g, 0, 1023), Clamp(b, 0, 1023), Clamp(a, 0, 1023)) / Multiplier10Bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 YuvToRgba8Bit(int y, int u, int v, int a = 256)
        {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/bb970578.aspx
            // R = 1.1644Y' + 1.5960Cr'
            // G = 1.1644Y' - 0.3917Cb' - 0.8128Cr'
            // B = 1.1644Y' + 2.0172Cb'
            int r = ((298 * y) + (409 * v) + 128) >> 8;
            int g = ((298 * y) - (100 * u) - (208 * v) + 128) >> 8;
            int b = ((298 * y) + (516 * u) + 128) >> 8;

            return new Vector4(Clamp(r, 0, 256), Clamp(g, 0, 256), Clamp(b, 0, 256), Clamp(a, 0, 256)) / Multiplier8Bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Clamp(uint val, uint min, uint max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Clamp(int val, int min, int max)
        {
            return Math.Min(Math.Max(val, min), max);
        }
    }
}
