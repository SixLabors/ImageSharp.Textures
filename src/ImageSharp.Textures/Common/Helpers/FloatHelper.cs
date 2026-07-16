// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Common.Helpers
{
    internal static class FloatHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float UnpackFloat32ToFloat(uint value) => Unsafe.As<uint, float>(ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackFloatToFloat32(float value) => Unsafe.As<float, uint>(ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float UnpackFloat16ToFloat(uint value) => (float)BitConverter.UInt16BitsToHalf((ushort)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackFloatToFloat16(float value) => BitConverter.HalfToUInt16Bits((Half)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float UnpackFloat10ToFloat(uint value, uint bias = 10)
        {
            uint e = (value >> 5) & 0x1Fu;
            uint m = value & 0x1Fu;

            return e switch
            {
                // Zero
                0 when m == 0 => 0f,

                // Denormalized
                0 => m * BitConverter.UInt32BitsToSingle((128u - bias - 5u) << 23),

                // Inf/NaN
                31 => BitConverter.UInt32BitsToSingle((0xFFu << 23) | (m << 18)),

                // Normalized
                _ => BitConverter.UInt32BitsToSingle(((e + 127u - bias) << 23) | (m << 18)),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackFloatToFloat10(float value)
        {
            uint temp = Unsafe.As<float, uint>(ref value);
            return
                ((((temp >> 23) & 0xff) - 127 + 10) << 5) |
                ((temp & 0x7fffff) >> 18);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float UnpackFloat11ToFloat(uint value, uint bias = 11)
        {
            uint e = (value >> 6) & 0x1Fu;
            uint m = value & 0x3Fu;

            if (e == 0)
            {
                if (m == 0)
                {
                    return 0f;
                }

                // Denormalized: m * 2^(1 - bias - 6)
                return m * BitConverter.UInt32BitsToSingle((128u - bias - 6u) << 23);
            }

            if (e == 31)
            {
                uint ieee = (0xFFu << 23) | (m << 17);
                return BitConverter.UInt32BitsToSingle(ieee);
            }

            return BitConverter.UInt32BitsToSingle(((e + 127u - bias) << 23) | (m << 17));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackFloatToFloat11(float value)
        {
            uint temp = Unsafe.As<float, uint>(ref value);
            return
                ((((temp >> 23) & 0xff) - 127 + 11) << 6) |
                ((temp & 0x7fffff) >> 17);
        }
    }
}
