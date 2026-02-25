// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.Core;

internal static class BitOperations
{
    /// <summary>
    /// Return the specified range as a <see cref="UInt128"/> (low bits in lower 64 bits)
    /// </summary>
    public static UInt128 GetBits(UInt128 value, int start, int length)
    {
        if (length <= 0)
            return UInt128.Zero;

        var shifted = value >> start;
        if (length >= 128)
            return shifted;

        if (length >= 64)
        {
            ulong lowMask = ~0UL;
            int highBits = length - 64;
            ulong highMask = (highBits == 64)
                ? ~0UL
                : ((1UL << highBits) - 1UL);

            return new UInt128(shifted.High() & highMask, shifted.Low() & lowMask);
        }
        else
        {
            ulong mask = (length == 64)
                ? ~0UL
                : ((1UL << length) - 1UL);

            return new UInt128(0, shifted.Low() & mask);
        }
    }

    /// <summary>
    /// Return the specified range as a ulong
    /// </summary>
    public static ulong GetBits(ulong value, int start, int length)
    {
        if (length <= 0)
            return 0UL;

        int totalBits = sizeof(ulong) * 8;
        ulong mask = length == totalBits
            ? ~0UL
            : ~0UL >> (totalBits - length);

        return (value >> start) & mask;
    }

    /// <summary>
    /// Transfers a few bits of precision from one value to another.
    /// </summary>
    /// <remarks>
    /// The 'bit_transfer_signed' function defined in Section C.2.14 of the ASTC specification
    /// </remarks>
    public static (int a, int b) TransferPrecision(int a, int b)
    {
        b >>= 1;
        b |= a & 0x80;
        a >>= 1;
        a &= 0x3F;

        if ((a & 0x20) != 0)
            a -= 0x40;

        return (a, b);
    }

    /// <summary>
    /// Takes two values, |a| in the range [-32, 31], and |b| in the range [0, 255],
    /// and returns the two values in [0, 255] that will reconstruct |a| and |b| when
    /// passed to the <see cref="TransferPrecision"/> function.
    /// </summary>
    public static (int a, int b) TransferPrecisionInverse(int a, int b)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(a, -32);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(a, 31);
        ArgumentOutOfRangeException.ThrowIfLessThan(b, byte.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(b, byte.MaxValue);

        if (a < 0)
            a += 0x40;

        a <<= 1;
        a |= b & 0x80;
        b <<= 1;
        b &= 0xff;

        return (a, b);
    }
}