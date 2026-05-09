// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Named constants for the IEEE 754 half-precision (FP16) bit patterns used by the HDR decoder.
/// </summary>
internal static class Fp16
{
    /// <summary>FP16 bit pattern for 1.0 (sign 0, exponent 15, mantissa 0).</summary>
    public const ushort One = 0x7800;

    /// <summary>FP16 bit pattern for the largest finite value (sign 0, exponent 30, mantissa all ones).</summary>
    public const ushort MaxFinite = 0x7BFF;
}
