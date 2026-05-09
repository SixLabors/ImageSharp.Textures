// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

// HDR ASTC block types for VK_FORMAT_ASTC_*_SFLOAT_BLOCK containers. These decode
// the same 128-bit ASTC blocks as the LDR variants but route through the HDR path
// and produce Rgba128Float (4 x float32) output rather than Rgba32.

internal readonly struct RgbaAstcHdr4X4 : IBlock<RgbaAstcHdr4X4>
{
    public static Size BlockSize => new(4, 4);
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

internal readonly struct RgbaAstcHdr5X4 : IBlock<RgbaAstcHdr5X4>
{
    public static Size BlockSize => new(5, 4);
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

internal readonly struct RgbaAstcHdr5X5 : IBlock<RgbaAstcHdr5X5>
{
    public static Size BlockSize => new(5, 5);
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

internal readonly struct RgbaAstcHdr6X5 : IBlock<RgbaAstcHdr6X5>
{
    public static Size BlockSize => new(6, 5);
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

internal readonly struct RgbaAstcHdr6X6 : IBlock<RgbaAstcHdr6X6>
{
    public static Size BlockSize => new(6, 6);
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

internal readonly struct RgbaAstcHdr8X5 : IBlock<RgbaAstcHdr8X5>
{
    public static Size BlockSize => new(8, 5);
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

internal readonly struct RgbaAstcHdr8X6 : IBlock<RgbaAstcHdr8X6>
{
    public static Size BlockSize => new(8, 6);
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

internal readonly struct RgbaAstcHdr8X8 : IBlock<RgbaAstcHdr8X8>
{
    public static Size BlockSize => new(8, 8);
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);
    public byte PixelDepthBytes => AstcDecoder.RgbaHdrPixelDepthBytes;
    public byte DivSize => 8;
    public byte CompressedBytesPerBlock => AstcDecoder.AstcBlockSize;
    public bool Compressed => true;

    public Image GetImage(byte[] blockData, int width, int height)
        => Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(this.Decompress(blockData, width, height), width, height);

    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressHdrImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}

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

internal readonly struct RgbaAstcHdr10X6 : IBlock<RgbaAstcHdr10X6>
{
    public static Size BlockSize => new(10, 6);
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

internal readonly struct RgbaAstcHdr10X8 : IBlock<RgbaAstcHdr10X8>
{
    public static Size BlockSize => new(10, 8);
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);
    public byte PixelDepthBytes => AstcDecoder.RgbaHdrPixelDepthBytes;
    public byte DivSize => 8;
    public byte CompressedBytesPerBlock => AstcDecoder.AstcBlockSize;
    public bool Compressed => true;

    public Image GetImage(byte[] blockData, int width, int height)
        => Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(this.Decompress(blockData, width, height), width, height);

    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressHdrImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}

internal readonly struct RgbaAstcHdr10X10 : IBlock<RgbaAstcHdr10X10>
{
    public static Size BlockSize => new(10, 10);
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);
    public byte PixelDepthBytes => AstcDecoder.RgbaHdrPixelDepthBytes;
    public byte DivSize => 10;
    public byte CompressedBytesPerBlock => AstcDecoder.AstcBlockSize;
    public bool Compressed => true;

    public Image GetImage(byte[] blockData, int width, int height)
        => Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(this.Decompress(blockData, width, height), width, height);

    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressHdrImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}

internal readonly struct RgbaAstcHdr12X10 : IBlock<RgbaAstcHdr12X10>
{
    public static Size BlockSize => new(12, 10);
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);
    public byte PixelDepthBytes => AstcDecoder.RgbaHdrPixelDepthBytes;
    public byte DivSize => 10;
    public byte CompressedBytesPerBlock => AstcDecoder.AstcBlockSize;
    public bool Compressed => true;

    public Image GetImage(byte[] blockData, int width, int height)
        => Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(this.Decompress(blockData, width, height), width, height);

    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressHdrImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}

internal readonly struct RgbaAstcHdr12X12 : IBlock<RgbaAstcHdr12X12>
{
    public static Size BlockSize => new(12, 12);
    public int BitsPerPixel => 128 / (BlockSize.Width * BlockSize.Height);
    public byte PixelDepthBytes => AstcDecoder.RgbaHdrPixelDepthBytes;
    public byte DivSize => 12;
    public byte CompressedBytesPerBlock => AstcDecoder.AstcBlockSize;
    public bool Compressed => true;

    public Image GetImage(byte[] blockData, int width, int height)
        => Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(this.Decompress(blockData, width, height), width, height);

    public byte[] Decompress(byte[] blockData, int width, int height)
        => AstcDecoder.DecompressHdrImage(blockData, width, height, BlockSize.Width, BlockSize.Height, this.CompressedBytesPerBlock);
}
