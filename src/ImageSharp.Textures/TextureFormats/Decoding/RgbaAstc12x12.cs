// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture compressed with RgbaAstc12x12.
/// </summary>
internal readonly struct RgbaAstc12X12 : IBlock<RgbaAstc12X12>
{
    public static Size BlockSize => new(12, 12);

    /// <inheritdoc/>
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);

    /// <inheritdoc/>
    public byte PixelDepthBytes => 4;

    /// <inheritdoc/>
    public byte DivSize => 12;

    /// <inheritdoc/>
    public byte CompressedBytesPerBlock => 16;

    /// <inheritdoc/>
    public bool Compressed => true;

    /// <inheritdoc/>
    public Image GetImage(byte[] blockData, int width, int height)
    {
        byte[] decompressedData = this.Decompress(blockData, width, height);
        return Image.LoadPixelData<ImageSharp.PixelFormats.Rgba32>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] blockData, int width, int height) =>
        AstcDecoder.DecompressImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}
