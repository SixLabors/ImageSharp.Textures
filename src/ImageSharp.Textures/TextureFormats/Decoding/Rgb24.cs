// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture format with pixels which use 8 bits for each channel without an alpha channel (pixel format is Rgb24).
/// </summary>
internal struct Rgb24 : IBlock<Rgb24>
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
        return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public readonly byte[] Decompress(byte[] blockData, int width, int height) => blockData;
}
