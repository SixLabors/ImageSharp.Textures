// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture format for pixels which have only the red and green channel and use 16 bit for each.
/// </summary>
internal struct Rg32 : IBlock<Rg32>
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
        return Image.LoadPixelData<ImageSharp.PixelFormats.Rg32>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public readonly byte[] Decompress(byte[] blockData, int width, int height) => blockData;
}
