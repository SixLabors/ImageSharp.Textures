// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Common.Helpers
{
    internal static class FloatHelper
    {
        public static float UnpackFloat16ToFloat(ushort value)
        {
            uint result = (uint)(
                (value >> 15) << 31 |
                (((value >> 10) & 0x1f) - 15 + 127) << 23 |
                (value & 0x3ff) << 13);
            return Unsafe.As<uint, float>(ref result);
        }

        public static ushort PackFloatToFloat16(float value)
        {
            uint temp = Unsafe.As<float, uint>(ref value);
            return (ushort)(
                (temp >> 31) << 15 |
                (((temp >> 23) & 0xff) - 127 + 15) << 10 |
                (temp & 0x7fffff) >> 13);
        }

        public static float UnpackFloat10ToFloat(ushort value)
        {
            uint result = (uint)((
                ((value >> 5) & 0x1f) - 10 + 127) << 23 |
                (value & 0x1f) << 18);
            return Unsafe.As<uint, float>(ref result);
        }

        public static ushort PackFloatToFloat10(float value)
        {
            uint temp = Unsafe.As<float, uint>(ref value);
            return (ushort)((
                ((temp >> 23) & 0xff) - 127 + 10) << 5 |
                (temp & 0x7fffff) >> 18);
        }

        public static float UnpackFloat11ToFloat(ushort value)
        {
            uint result = (uint)((
                ((value >> 6) & 0x1f) - 11 + 127) << 23 |
                (value & 0x3f) << 17);
            return Unsafe.As<uint, float>(ref result);
        }

        public static ushort PackFloatToFloat11(float value)
        {
            uint temp = Unsafe.As<float, uint>(ref value);
            return (ushort)((
                ((temp >> 23) & 0xff) - 127 + 11) << 6 |
                (temp & 0x7fffff) >> 17);
        }
    }
}
