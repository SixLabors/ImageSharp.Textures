// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture compressed with RgbaAstc4x4.
/// </summary>
internal readonly struct RgbaAstc4X4 : IBlock<RgbaAstc4X4>
{
    // See https://developer.nvidia.com/astc-texture-compression-for-game-assets
    // https://chromium.googlesource.com/external/github.com/ARM-software/astc-encoder/+/HEAD/Docs/FormatOverview.md
    public static Size BlockSize => new(4, 4);

    /// <inheritdoc/>
    // The 2D block footprints in ASTC range from 4x4 texels up to 12x12 texels, which all compress into 128-bit output blocks.
    // By dividing 128 bits by the number of texels in the footprint, we derive the format bit rates which range from 8 bpt(128/(4*4)) down to 0.89 bpt(128/(12*12)).
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
    public byte[] Decompress(byte[] blockData, int width, int height) =>
        AstcDecoder.DecompressImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}
