// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Uastc;

/// <summary>
/// Static per-mode metadata for the 19 UASTC LDR modes (plus the reserved mode 19).
/// UASTC is a constrained subset of LDR ASTC 4x4. Each mode fixes the subset count,
/// color endpoint mode, weight grid range, and plane count.
/// </summary>
internal static class UastcMode
{
    /// <summary>
    /// The number of defined UASTC modes (0-18). Mode 19 is reserved.
    /// </summary>
    public const int TotalModes = 19;

    /// <summary>
    /// The mode index used for solid-color (void-extent style) blocks.
    /// </summary>
    public const int SolidColorModeIndex = 8;

    /// <summary>
    /// Per-mode Huffman code (value, bit length) for the leading mode prefix. Index 19 is the
    /// reserved future-expansion code. Used for the bit length (how many bits the mode prefix
    /// consumes), decoding uses the <see cref="HuffModes"/> accelerator.
    /// </summary>
    public static readonly (byte Code, byte Length)[] HuffCodes =
    [
        (0x1, 4), (0x35, 6), (0x1D, 5), (0x3, 5),
        (0x13, 5), (0xB, 5), (0x1B, 5), (0x7, 5),
        (0x17, 5), (0xF, 5), (0x2, 3), (0x0, 2),
        (0x6, 3), (0x1F, 5), (0xD, 5), (0x5, 7),
        (0x15, 6), (0x25, 6), (0x9, 4), (0x45, 7),
    ];

    /// <summary>
    /// ASTC BISE range table: {bits, trits, quints} per range index (ASTC spec §C.2.12).
    /// </summary>
    public static readonly (byte Bits, byte Trits, byte Quints)[] BiseRangeTable =
    [
        (1, 0, 0), (0, 1, 0), (2, 0, 0), (0, 0, 1),
        (1, 1, 0), (3, 0, 0), (1, 0, 1), (2, 1, 0),
        (4, 0, 0), (2, 0, 1), (3, 1, 0), (5, 0, 0),
        (3, 0, 1), (4, 1, 0), (6, 0, 0), (4, 0, 1),
        (5, 1, 0), (7, 0, 0), (5, 0, 1), (6, 1, 0),
        (8, 0, 0),
    ];

    /// <summary>
    /// Gets the bits per weight per texel.
    /// </summary>
    public static ReadOnlySpan<byte> WeightBits => [4, 2, 3, 2, 2, 3, 2, 2, 0, 2, 4, 2, 3, 1, 2, 4, 2, 2, 5];

    /// <summary>
    /// Gets the ASTC BISE range index used for weight quantization.
    /// </summary>
    public static ReadOnlySpan<byte> WeightRanges => [8, 2, 5, 2, 2, 5, 2, 2, 0, 2, 8, 2, 5, 0, 2, 8, 2, 2, 11];

    /// <summary>
    /// Gets the ASTC BISE range index used for endpoint quantization.
    /// </summary>
    public static ReadOnlySpan<byte> EndpointRanges => [19, 20, 8, 7, 12, 20, 18, 12, 0, 8, 13, 13, 19, 20, 20, 20, 20, 20, 11];

    /// <summary>
    /// Gets the number of subsets (partitions): 1, 2, or 3.
    /// </summary>
    public static ReadOnlySpan<byte> Subsets => [1, 1, 2, 3, 2, 1, 1, 2, 0, 2, 1, 1, 1, 1, 1, 1, 2, 1, 1];

    /// <summary>
    /// Gets the weight plane count: 1 (single plane) or 2 (dual plane).
    /// </summary>
    public static ReadOnlySpan<byte> Planes => [1, 1, 1, 1, 1, 1, 2, 1, 0, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1];

    /// <summary>
    /// Gets the number of color components: 3 (RGB), 4 (RGBA), or 2 (LA).
    /// </summary>
    public static ReadOnlySpan<byte> Comps => [3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 2, 2, 2, 3];

    /// <summary>
    /// Gets the ASTC color endpoint mode (CEM): 8=RGB direct, 12=RGBA direct, 4=LA direct, 0=void.
    /// </summary>
    public static ReadOnlySpan<byte> Cem => [8, 8, 8, 8, 8, 8, 8, 8, 0, 12, 12, 12, 12, 12, 12, 4, 4, 4, 8];

    /// <summary>
    /// Gets a per-mode flag indicating whether the block uses the luminance-alpha (LA) layout
    /// (output swizzled to LLLA).
    /// </summary>
    public static ReadOnlySpan<byte> IsLa => [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0];

    /// <summary>
    /// Gets the total hint-field bits per mode, used to skip hints when not reading them.
    /// </summary>
    public static ReadOnlySpan<byte> TotalHintBits => [15, 15, 15, 15, 15, 15, 15, 15, 0, 23, 17, 17, 17, 23, 23, 23, 23, 23, 15];

    /// <summary>
    /// Gets a lookup mapping the low 7 bits of the block's first byte to its UASTC mode (0-19);
    /// 19 is reserved. Equivalent to expanding <see cref="HuffCodes"/> over all 128 prefixes.
    /// </summary>
    public static ReadOnlySpan<byte> HuffModes =>
    [
        11, 0, 10, 3, 11, 15, 12, 7, 11, 18, 10, 5, 11, 14, 12, 9, 11, 0, 10, 4, 11, 16, 12, 8, 11, 18, 10, 6, 11, 2, 12, 13,
        11, 0, 10, 3, 11, 17, 12, 7, 11, 18, 10, 5, 11, 14, 12, 9, 11, 0, 10, 4, 11, 1, 12, 8, 11, 18, 10, 6, 11, 2, 12, 13,
        11, 0, 10, 3, 11, 19, 12, 7, 11, 18, 10, 5, 11, 14, 12, 9, 11, 0, 10, 4, 11, 16, 12, 8, 11, 18, 10, 6, 11, 2, 12, 13,
        11, 0, 10, 3, 11, 17, 12, 7, 11, 18, 10, 5, 11, 14, 12, 9, 11, 0, 10, 4, 11, 1, 12, 8, 11, 18, 10, 6, 11, 2, 12, 13,
    ];

    /// <summary>
    /// Decodes the UASTC mode from the first block byte. Returns false for the reserved mode 19.
    /// </summary>
    /// <param name="firstByte">The first byte of the 16-byte UASTC block.</param>
    /// <param name="mode">The decoded mode index (0-18) on success.</param>
    /// <returns>True if a valid mode was decoded; false for reserved/invalid.</returns>
    public static bool TryDecodeMode(byte firstByte, out int mode)
    {
        mode = HuffModes[firstByte & 0x7F];
        return mode < TotalModes;
    }
}
