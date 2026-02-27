// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

namespace SixLabors.ImageSharp.Textures.Tests.TextureFormats.Decoding;

[Trait("Format", "Astc")]
public class AstcDecoderTests
{
    [Theory]
    [InlineData(4, 4)]
    [InlineData(5, 4)]
    [InlineData(5, 5)]
    [InlineData(6, 5)]
    [InlineData(6, 6)]
    [InlineData(8, 5)]
    [InlineData(8, 6)]
    [InlineData(8, 8)]
    [InlineData(10, 5)]
    [InlineData(10, 6)]
    [InlineData(10, 8)]
    [InlineData(10, 10)]
    [InlineData(12, 10)]
    [InlineData(12, 12)]
    public void DecodeBlock_WithValidBlockData_DoesNotThrow(int blockWidth, int blockHeight)
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize]; // ASTC blocks are always 16 bytes
        byte[] decodedPixels = new byte[blockWidth * blockHeight * 4];

        AstcDecoder.DecodeBlock(blockData, blockWidth, blockHeight, decodedPixels);

        Assert.Equal(blockWidth * blockHeight * 4, decodedPixels.Length);
    }

    [Fact]
    public void DecodeBlock_WithTooSmallBlockData_ThrowsArgumentException()
    {
        byte[] blockData = new byte[15]; // Too small
        byte[] decodedPixels = new byte[4 * 4 * 4];

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecodeBlock(blockData, 4, 4, decodedPixels));

        Assert.Contains("16 bytes", ex.Message);
        Assert.Contains("blockData", ex.ParamName);
    }

    [Fact]
    public void DecodeBlock_WithTooLargeBlockData_ThrowsArgumentException()
    {
        byte[] blockData = new byte[17]; // Too large
        byte[] decodedPixels = new byte[4 * 4 * 4];

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecodeBlock(blockData, 4, 4, decodedPixels));

        Assert.Contains("16 bytes", ex.Message);
        Assert.Contains("blockData", ex.ParamName);
    }

    [Fact]
    public void DecodeBlock_WithEmptyBlockData_ThrowsArgumentException()
    {
        byte[] blockData = [];
        byte[] decodedPixels = new byte[4 * 4 * 4];

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecodeBlock(blockData, 4, 4, decodedPixels));

        Assert.Contains("blockData", ex.ParamName);
    }

    [Fact]
    public void DecodeBlock_WithTooSmallOutputBuffer_ThrowsArgumentException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];
        byte[] decodedPixels = new byte[10]; // Too small for 4x4 block (needs 64 bytes)

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecodeBlock(blockData, 4, 4, decodedPixels));

        Assert.Contains("Output buffer", ex.Message);
        Assert.Contains("decodedPixels", ex.ParamName);
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    [InlineData(3, 4)]
    [InlineData(7, 7)]
    [InlineData(11, 11)]
    [InlineData(13, 13)]
    [InlineData(16, 16)]
    public void DecodeBlock_WithInvalidBlockDimensions_ThrowsArgumentOutOfRangeException(int blockWidth, int blockHeight)
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];
        byte[] decodedPixels = new byte[blockWidth * blockHeight * 4];

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecodeBlock(blockData, blockWidth, blockHeight, decodedPixels));

        Assert.Contains("Invalid ASTC block dimensions", ex.Message);
    }

    [Fact]
    public void DecodeBlock_WithZeroBlockWidth_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];
        byte[] decodedPixels = new byte[64];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecodeBlock(blockData, 0, 4, decodedPixels));
    }

    [Fact]
    public void DecodeBlock_WithNegativeBlockWidth_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];
        byte[] decodedPixels = new byte[64];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecodeBlock(blockData, -1, 4, decodedPixels));
    }

    [Fact]
    public void DecodeBlock_WithNegativeBlockHeight_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];
        byte[] decodedPixels = new byte[64];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecodeBlock(blockData, 4, -1, decodedPixels));
    }

    [Theory]
    [InlineData(256, 256, 4, 4)]
    [InlineData(512, 512, 8, 8)]
    [InlineData(128, 128, 6, 6)]
    [InlineData(200, 200, 10, 10)]
    public void DecompressImage_WithValidParameters_ReturnsCorrectSizedArray(int width, int height, int blockWidth, int blockHeight)
    {
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int totalBlocks = blocksWide * blocksHigh;
        byte[] blockData = new byte[totalBlocks * AstcDecoder.AstcBlockSize];

        byte[] result = AstcDecoder.DecompressImage(blockData, width, height, blockWidth, blockHeight, AstcDecoder.AstcBlockSize);

        Assert.Equal(width * height * 4, result.Length);
    }

    [Fact]
    public void DecompressImage_WithNullBlockData_ThrowsArgumentNullException()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            AstcDecoder.DecompressImage(null, 256, 256, 4, 4, 16));

        Assert.Equal("blockData", ex.ParamName);
    }

    [Fact]
    public void DecompressImage_WithZeroWidth_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 0, 256, 4, 4, 16));

        Assert.Equal("width", ex.ParamName);
    }

    [Fact]
    public void DecompressImage_WithNegativeWidth_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, -256, 256, 4, 4, 16));

        Assert.Equal("width", ex.ParamName);
    }

    [Fact]
    public void DecompressImage_WithZeroHeight_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 0, 4, 4, 16));

        Assert.Equal("height", ex.ParamName);
    }

    [Fact]
    public void DecompressImage_WithNegativeHeight_ThrowsArgumentOutOfRangeException()
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, -256, 4, 4, 16));

        Assert.Equal("height", ex.ParamName);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(32)]
    [InlineData(8)]
    public void DecompressImage_WithInvalidCompressedBytesPerBlock_ThrowsArgumentOutOfRangeException(byte invalidBytes)
    {
        byte[] blockData = new byte[invalidBytes * 64]; // 8x8 blocks for 256x256

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, invalidBytes));

        Assert.Equal("compressedBytesPerBlock", ex.ParamName);
        Assert.Contains("16 bytes", ex.Message);
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    [InlineData(7, 7)]
    [InlineData(16, 16)]
    public void DecompressImage_WithInvalidBlockDimensions_ThrowsArgumentOutOfRangeException(int blockWidth, int blockHeight)
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, blockWidth, blockHeight, 16));

        Assert.Contains("Invalid ASTC block dimensions", ex.Message);
    }

    [Fact]
    public void DecompressImage_WithTooSmallBlockData_ThrowsArgumentException()
    {
        // For 256x256 with 4x4 blocks, we need 64x64 = 4096 blocks * 16 bytes = 65536 bytes
        byte[] blockData = new byte[1000]; // Too small

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, 16));

        Assert.Equal("blockData", ex.ParamName);
        Assert.Contains("too small", ex.Message);
    }

    [Fact]
    public void DecompressImage_WithEmptyBlockData_ThrowsArgumentException()
    {
        byte[] blockData = [];

        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, 16));

        Assert.Equal("blockData", ex.ParamName);
    }

    [Theory]
    [InlineData(257, 256)] // Width not multiple of block size
    [InlineData(256, 257)] // Height not multiple of block size
    [InlineData(255, 255)] // Both not multiples
    [InlineData(100, 100)] // Different non-multiples
    public void DecompressImage_WithNonMultipleImageSizes_ReturnExpectedSize(int width, int height)
    {
        int blockWidth = 4;
        int blockHeight = 4;
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int totalBlocks = blocksWide * blocksHigh;
        byte[] blockData = new byte[totalBlocks * 16];

        byte[] result = AstcDecoder.DecompressImage(blockData, width, height, blockWidth, blockHeight, 16);

        Assert.Equal(width * height * 4, result.Length);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    public void DecompressImage_WithVerySmallImages_ReturnExpectedSize(int width, int height)
    {
        // Even tiny images need at least one block
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        byte[] result = AstcDecoder.DecompressImage(blockData, width, height, 4, 4, 16);

        Assert.Equal(width * height * 4, result.Length);
    }

    [Theory]
    [InlineData(4096, 4096, 4, 4)]
    [InlineData(2048, 2048, 8, 8)]
    public void DecompressImage_WithLargeImages_ReturnExpectedSize(int width, int height, int blockWidth, int blockHeight)
    {
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int totalBlocks = blocksWide * blocksHigh;
        byte[] blockData = new byte[totalBlocks * 16];

        byte[] result = AstcDecoder.DecompressImage(blockData, width, height, blockWidth, blockHeight, 16);

        Assert.Equal(width * height * 4, result.Length);
    }

    [Fact]
    public void DecompressImage_WithExactBlockDataSize_ReturnExpectedSize()
    {
        // 256x256 with 4x4 blocks = 64x64 blocks = 4096 blocks * 16 bytes = 65536 bytes
        byte[] blockData = new byte[65536];

        byte[] result = AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, 16);

        Assert.Equal(256 * 256 * 4, result.Length);
    }

    [Fact]
    public void DecompressImage_WithExtraBlockData_ReturnExpectedSize()
    {
        // More data than needed should work (extra data ignored)
        byte[] blockData = new byte[100000];

        byte[] result = AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, 16);

        Assert.Equal(256 * 256 * 4, result.Length);
    }

}
