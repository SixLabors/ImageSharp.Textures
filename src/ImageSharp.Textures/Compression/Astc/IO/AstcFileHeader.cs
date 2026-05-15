// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.IO;

/// <summary>
/// The 16 byte ASTC file header
/// </summary>
/// <remarks>
/// ASTC block and decoded image dimensions in texels.
///
/// For 2D images the Z dimension must be set to 1.
///
/// Note that the image is not required to be an exact multiple of the compressed block
/// size; the compressed data may include padding that is discarded during decompression.
/// </remarks>
internal readonly record struct AstcFileHeader(byte BlockWidth, byte BlockHeight, byte BlockDepth, int ImageWidth, int ImageHeight, int ImageDepth)
{
    public const uint Magic = 0x5CA1AB13;
    public const int SizeInBytes = 16;

    // 2D footprints from the ASTC spec. 3D footprints are not supported.
    private static readonly (byte Width, byte Height)[] Valid2DFootprints =
    [
        (4, 4), (5, 4), (5, 5), (6, 5), (6, 6),
        (8, 5), (8, 6), (8, 8),
        (10, 5), (10, 6), (10, 8), (10, 10),
        (12, 10), (12, 12)
    ];

    public static AstcFileHeader FromMemory(Span<byte> data)
    {
        Guard.MustBeSizedAtLeast(data, SizeInBytes, nameof(data));

        // ASTC header is 16 bytes:
        // - magic (4),
        // - blockdim (3),
        // - xsize,y,z (each 3 little-endian bytes)
        uint magic = BinaryPrimitives.ReadUInt32LittleEndian(data);
        Guard.IsTrue(magic == Magic, nameof(data), $"Invalid ASTC file magic: expected 0x{Magic:X8}.");

        byte blockWidth = data[4];
        byte blockHeight = data[5];
        byte blockDepth = data[6];

        // Only 2D footprints are supported, so block depth must be 1.
        if (blockDepth != 1)
        {
            throw new NotSupportedException($"ASTC 3D block footprints are not supported (block depth = {blockDepth})");
        }

        if (!IsValid2DFootprint(blockWidth, blockHeight))
        {
            throw new NotSupportedException($"Unsupported ASTC block dimensions: {blockWidth}x{blockHeight}");
        }

        int imageWidth = data[7] | (data[8] << 8) | (data[9] << 16);
        int imageHeight = data[10] | (data[11] << 8) | (data[12] << 16);
        int imageDepth = data[13] | (data[14] << 8) | (data[15] << 16);

        Guard.MustBeGreaterThan(imageWidth, 0, nameof(imageWidth));
        Guard.MustBeGreaterThan(imageHeight, 0, nameof(imageHeight));
        Guard.MustBeGreaterThan(imageDepth, 0, nameof(imageDepth));

        // Guard against callers that compute a 4-byte-per-pixel RGBA8 output buffer.
        const int bytesPerPixel = 4;
        long totalPixels = (long)imageWidth * imageHeight;
        if (totalPixels > int.MaxValue / bytesPerPixel)
        {
            throw new ArgumentOutOfRangeException(nameof(data), "ASTC image dimensions exceed the maximum supported size");
        }

        return new AstcFileHeader(
            BlockWidth: blockWidth,
            BlockHeight: blockHeight,
            BlockDepth: blockDepth,
            ImageWidth: imageWidth,
            ImageHeight: imageHeight,
            ImageDepth: imageDepth);
    }

    private static bool IsValid2DFootprint(byte width, byte height)
    {
        foreach ((byte w, byte h) in Valid2DFootprints)
        {
            if (w == width && h == height)
            {
                return true;
            }
        }

        return false;
    }
}
