// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture compressed with sRGB UASTC (Universal ASTC). Identical to <see cref="RgbaUastc4X4"/>
/// except colour endpoints are expanded with the sRGB rule (ASTC spec §C.2.19).
/// </summary>
internal readonly struct RgbaUastcSrgb4X4 : IBlock<RgbaUastcSrgb4X4>
{
    public static Size BlockSize => new(4, 4);

    /// <inheritdoc/>
    // UASTC always packs a 4x4 texel footprint into a 128-bit block, giving a fixed 8 bits per texel.
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);

    /// <inheritdoc/>
    public byte PixelDepthBytes => 4;

    /// <inheritdoc/>
    public byte DivSize => 4;

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
    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressUastcImage(blockData, width, height, LdrDecodeMode.Srgb);
}
