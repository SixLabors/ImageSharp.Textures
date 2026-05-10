// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// IEEE 754 half-precision (FP16) constants and helpers used by the HDR decoder.
/// </summary>
internal static class Fp16
{
    /// <summary>FP16 bit pattern for 1.0 (sign 0, exponent 15, mantissa 0).</summary>
    public const ushort One = 0x7800;

    /// <summary>FP16 bit pattern for the largest finite value (sign 0, exponent 30, mantissa all ones).</summary>
    public const ushort MaxFinite = 0x7BFF;

    /// <summary>
    /// Converts a 16-bit LNS (Log-Normalized Space) value to a 16-bit SF16 (FP16) bit pattern.
    /// </summary>
    /// <remarks>
    /// The LNS value encodes a 5-bit exponent in the upper bits and an 11-bit mantissa
    /// in the lower bits. The mantissa is transformed using a piecewise linear function
    /// before being combined with the exponent to form the FP16 result.
    /// </remarks>
    public static ushort FromLns(int lns)
    {
        int mantissaComponent = lns & 0x7FF;       // Lower 11 bits: mantissa component
        int exponentComponent = (lns >> 11) & 0x1F; // Upper 5 bits: exponent component

        int mantissaTransformed;
        if (mantissaComponent < 512)
        {
            mantissaTransformed = mantissaComponent * 3;
        }
        else if (mantissaComponent < 1536)
        {
            mantissaTransformed = (mantissaComponent * 4) - 512;
        }
        else
        {
            mantissaTransformed = (mantissaComponent * 5) - 2048;
        }

        int result = (exponentComponent << 10) | (mantissaTransformed >> 3);
        return (ushort)Math.Min(result, MaxFinite);
    }

    /// <summary>
    /// Decodes a 16-bit LNS value to a single-precision float by converting through FP16,
    /// per ASTC spec §C.2.15. The LNS value is passed through <see cref="FromLns"/>, reinterpreted
    /// as FP16 bits, and widened to <see cref="float"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LnsToFloat(int lns) => (float)BitConverter.UInt16BitsToHalf(FromLns(lns));

    /// <summary>
    /// Widens an FP16 bit pattern (already in SF16 form, no LNS conversion) to <see cref="float"/>.
    /// Used for HDR void-extent blocks (ASTC spec §C.2.23), whose channel values are stored as
    /// FP16 bit patterns directly rather than as LNS values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Fp16ToFloat(ushort fp16Bits) => (float)BitConverter.UInt16BitsToHalf(fp16Bits);
}
