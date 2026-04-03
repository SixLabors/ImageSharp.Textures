// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

internal static class UInt128Extensions
{
    /// <summary>
    /// The lower 64 bits of the <see cref="UInt128"/> value
    /// </summary>
    public static ulong Low(this UInt128 value)
        => (ulong)(value & 0xFFFFFFFFFFFFFFFFUL);

    /// <summary>
    /// The upper 64 bits of the <see cref="UInt128"/> value
    /// </summary>
    public static ulong High(this UInt128 value)
        => (ulong)(value >> 64);

    /// <summary>
    /// A mask with the lowest n bits set to 1
    /// </summary>
    public static UInt128 OnesMask(int n)
    {
        if (n <= 0)
        {
            return UInt128.Zero;
        }

        if (n >= 128)
        {
            return new UInt128(~0UL, ~0UL);
        }

        if (n <= 64)
        {
            ulong low = (n == 64)
                ? ~0UL
                : ((1UL << n) - 1UL);

            return new UInt128(0UL, low);
        }
        else
        {
            int highBits = n - 64;
            ulong low = ~0UL;
            ulong high = (highBits == 64)
                ? ~0UL
                : ((1UL << highBits) - 1UL);

            return new UInt128(high, low);
        }
    }

    /// <summary>
    /// Reverse bits across the full 128-bit value
    /// </summary>
    public static UInt128 ReverseBits(this UInt128 value)
    {
        ulong revLow = ReverseBits(value.Low());
        ulong revHigh = ReverseBits(value.High());

        return new UInt128(revLow, revHigh);
    }

    private static ulong ReverseBits(ulong x)
    {
        x = ((x >> 1) & 0x5555555555555555UL) | ((x & 0x5555555555555555UL) << 1);
        x = ((x >> 2) & 0x3333333333333333UL) | ((x & 0x3333333333333333UL) << 2);
        x = ((x >> 4) & 0x0F0F0F0F0F0F0F0FUL) | ((x & 0x0F0F0F0F0F0F0F0FUL) << 4);
        x = ((x >> 8) & 0x00FF00FF00FF00FFUL) | ((x & 0x00FF00FF00FF00FFUL) << 8);
        x = ((x >> 16) & 0x0000FFFF0000FFFFUL) | ((x & 0x0000FFFF0000FFFFUL) << 16);
        x = (x >> 32) | (x << 32);

        return x;
    }
}
