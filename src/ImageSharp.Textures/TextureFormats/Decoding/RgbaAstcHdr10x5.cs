// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

internal readonly struct RgbaAstcHdr10X5 : IBlock<RgbaAstcHdr10X5>
{
    public static Size BlockSize => new(10, 5);

    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);

    public byte PixelDepthBytes => AstcDecoder.RgbaHdrPixelDepthBytes;

    public byte DivSize => 4;

    public byte CompressedBytesPerBlock => AstcDecoder.AstcBlockSize;

    public bool Compressed => true;

    public Image GetImage(byte[] blockData, int width, int height)
        => Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(this.Decompress(blockData, width, height), width, height);

    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressHdrImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}
