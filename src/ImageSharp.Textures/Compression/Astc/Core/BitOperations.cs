// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

internal static class BitOperations
{
    /// <summary>
    /// Return the specified range as a <see cref="UInt128"/> (low bits in lower 64 bits)
    /// </summary>
    public static UInt128 GetBits(UInt128 value, int start, int length) => length switch
    {
        <= 0 => UInt128.Zero,
        >= 128 => value >> start,
        _ => (value >> start) & (UInt128.MaxValue >> (128 - length))
    };

    /// <summary>
    /// Return the specified range as a ulong
    /// </summary>
    public static ulong GetBits(ulong value, int start, int length) => length switch
    {
        <= 0 => 0UL,
        >= 64 => value >> start,
        _ => (value >> start) & (ulong.MaxValue >> (64 - length))
    };

    /// <summary>
    /// Transfers a few bits of precision from one value to another.
    /// </summary>
    /// <remarks>
    /// The 'bit_transfer_signed' function defined in Section C.2.14 of the ASTC specification
    /// </remarks>
    public static (int A, int B) TransferPrecision(int a, int b)
    {
        b >>= 1;
        b |= a & 0x80;
        a >>= 1;
        a &= 0x3F;

        if ((a & 0x20) != 0)
        {
            a -= 0x40;
        }

        return (a, b);
    }
}
