// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture for pixel with the BGR24 format.
/// </summary>
internal struct Bgr24 : IBlock<Bgr24>
{
    /// <inheritdoc/>
    public readonly int BitsPerPixel => 24;

    /// <inheritdoc/>
    public readonly byte PixelDepthBytes => 3;

    /// <inheritdoc/>
    public readonly byte DivSize => 1;

    /// <inheritdoc/>
    public readonly byte CompressedBytesPerBlock => 3;

    /// <inheritdoc/>
    public readonly bool Compressed => false;

    /// <inheritdoc/>
    public Image GetImage(byte[] blockData, int width, int height)
    {
        byte[] decompressedData = this.Decompress(blockData, width, height);
        return Image.LoadPixelData<ImageSharp.PixelFormats.Bgr24>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public readonly byte[] Decompress(byte[] blockData, int width, int height) => blockData;
}
