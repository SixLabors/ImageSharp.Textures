// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

namespace SixLabors.ImageSharp.Textures.Tests.TextureFormats.Decoding;

[Trait("Format", "Astc")]
public class AstcDecoderTests
{
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

        Assert.Equal(width * height * AstcDecoder.RgbaPixelDepthBytes, result.Length);
    }

    [Fact]
    public void DecompressImage_WithNullBlockData_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() => AstcDecoder.DecompressImage(null, 256, 256, 4, 4, AstcDecoder.AstcBlockSize));

    [Theory]
    [InlineData(0, 256)]
    [InlineData(-256, 256)]
    [InlineData(256, 0)]
    [InlineData(256, -256)]
    public void DecompressImage_WithInvalidDimensions_ThrowsArgumentOutOfRangeException(int width, int height)
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, width, height, 4, 4, AstcDecoder.AstcBlockSize));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(32)]
    [InlineData(8)]
    public void DecompressImage_WithInvalidCompressedBytesPerBlock_ThrowsArgumentOutOfRangeException(byte invalidBytes)
    {
        byte[] blockData = new byte[invalidBytes * 64]; // 8x8 blocks for 256x256

        Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, invalidBytes));
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    [InlineData(7, 7)]
    [InlineData(16, 16)]
    public void DecompressImage_WithInvalidBlockDimensions_ThrowsArgumentOutOfRangeException(int blockWidth, int blockHeight)
    {
        byte[] blockData = new byte[AstcDecoder.AstcBlockSize];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, blockWidth, blockHeight, AstcDecoder.AstcBlockSize));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void DecompressImage_WithInsufficientBlockData_ThrowsArgumentOutOfRangeException(int dataSize)
    {
        byte[] blockData = new byte[dataSize];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, AstcDecoder.AstcBlockSize));
    }

    [Theory]
    [InlineData(1, 1, 4, 4)]
    [InlineData(2, 2, 4, 4)]
    [InlineData(3, 3, 4, 4)]
    [InlineData(100, 100, 4, 4)]
    [InlineData(255, 255, 4, 4)]
    [InlineData(256, 257, 4, 4)]
    [InlineData(257, 256, 4, 4)]
    [InlineData(4096, 4096, 4, 4)]
    [InlineData(2048, 2048, 8, 8)]
    public void DecompressImage_WithVariousSizes_ReturnsExpectedSize(int width, int height, int blockWidth, int blockHeight)
    {
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int totalBlocks = blocksWide * blocksHigh;
        byte[] blockData = new byte[totalBlocks * AstcDecoder.AstcBlockSize];

        byte[] result = AstcDecoder.DecompressImage(blockData, width, height, blockWidth, blockHeight, AstcDecoder.AstcBlockSize);

        Assert.Equal(width * height * AstcDecoder.RgbaPixelDepthBytes, result.Length);
    }

    [Fact]
    public void DecompressImage_WithExtraBlockData_ReturnsExpectedSize()
    {
        // More data than needed should work (extra data ignored)
        byte[] blockData = new byte[100000];

        byte[] result = AstcDecoder.DecompressImage(blockData, 256, 256, 4, 4, AstcDecoder.AstcBlockSize);

        Assert.Equal(256 * 256 * AstcDecoder.RgbaPixelDepthBytes, result.Length);
    }
}
