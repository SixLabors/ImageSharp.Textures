// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.IO;

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

    public static AstcFileHeader FromMemory(Span<byte> data)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, SizeInBytes);

        // ASTC header is 16 bytes:
        // - magic (4),
        // - blockdim (3),
        // - xsize,y,z (each 3 little-endian bytes)
        uint magic = BitConverter.ToUInt32(data);
        ArgumentOutOfRangeException.ThrowIfNotEqual(magic, Magic);

        return new AstcFileHeader(
            BlockWidth: data[4],
            BlockHeight: data[5],
            BlockDepth: data[6],
            ImageWidth: data[7] | (data[8] << 8) | (data[9] << 16),
            ImageHeight: data[10] | (data[11] << 8) | (data[12] << 16),
            ImageDepth: data[13] | (data[14] << 8) | (data[15] << 16));
    }
}
