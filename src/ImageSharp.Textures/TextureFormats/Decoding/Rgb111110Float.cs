// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture for pixel data with 11 bits for red and green and 10 bits for the blue channel as float.
/// </summary>
internal struct Rgb111110Float : IBlock<Rgb111110Float>
{
    /// <inheritdoc/>
    public readonly int BitsPerPixel => 32;

    /// <inheritdoc/>
    public readonly byte PixelDepthBytes => 4;

    /// <inheritdoc/>
    public readonly byte DivSize => 1;

    /// <inheritdoc/>
    public readonly byte CompressedBytesPerBlock => 4;

    /// <inheritdoc/>
    public readonly bool Compressed => false;

    /// <inheritdoc/>
    public Image GetImage(byte[] blockData, int width, int height)
    {
        byte[] decompressedData = this.Decompress(blockData, width, height);
        return Image.LoadPixelData<Textures.PixelFormats.R11G11B10Float>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public readonly byte[] Decompress(byte[] blockData, int width, int height) => blockData;
}
