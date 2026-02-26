// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture format for pixel data with 5 bit for red and blue and 6 bits for the green color channel.
/// </summary>
internal struct Rgb565 : IBlock<Rgb565>
{
    /// <inheritdoc/>
    public readonly int BitsPerPixel => 16;

    /// <inheritdoc/>
    public readonly byte PixelDepthBytes => 2;

    /// <inheritdoc/>
    public readonly byte DivSize => 1;

    /// <inheritdoc/>
    public readonly byte CompressedBytesPerBlock => 2;

    /// <inheritdoc/>
    public readonly bool Compressed => false;

    /// <inheritdoc/>
    public Image GetImage(byte[] blockData, int width, int height)
    {
        byte[] decompressedData = this.Decompress(blockData, width, height);
        return Image.LoadPixelData<Textures.PixelFormats.Rgb565>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public readonly byte[] Decompress(byte[] blockData, int width, int height) => blockData;
}
