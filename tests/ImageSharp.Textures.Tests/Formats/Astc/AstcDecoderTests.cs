// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

#nullable enable
public class AstcDecoderTests
{
    [Fact]
    public void DecompressImage_WithZeroWidth_ShouldReturnEmpty()
    {
        byte[] data = new byte[256];
        const int height = 16;

        Span<byte> result = AstcDecoder.DecompressImage(data, 0, height, FootprintType.Footprint4x4);

        Assert.Empty(result.ToArray());
    }

    [Fact]
    public void DecompressImage_WithZeroHeight_ShouldReturnEmpty()
    {
        byte[] data = new byte[256];
        const int width = 16;

        Span<byte> result = AstcDecoder.DecompressImage(data, width, 0, FootprintType.Footprint4x4);

        Assert.Empty(result.ToArray());
    }

    [Fact]
    public void DecompressImage_WithDataSizeNotMultipleOfBlockSize_ShouldReturnEmpty()
    {
        byte[] data = new byte[256];
        const int width = 16;
        const int height = 16;
        byte[] invalidData = data.AsSpan(0, data.Length - 1).ToArray();

        Span<byte> result = AstcDecoder.DecompressImage(invalidData, width, height, FootprintType.Footprint4x4);

        Assert.Empty(result.ToArray());
    }

    [Fact]
    public void DecompressImage_WithMismatchedBlockCount_ShouldReturnEmpty()
    {
        byte[] data = new byte[256];
        const int width = 16;
        const int height = 16;
        byte[] mismatchedData = data.AsSpan(0, data.Length - PhysicalBlock.SizeInBytes).ToArray();

        Span<byte> result = AstcDecoder.DecompressImage(mismatchedData, width, height, FootprintType.Footprint4x4);

        Assert.Empty(result.ToArray());
    }

    [Theory]
    [InlineData(TestImages.Astc.Atlas_Small_4x4)]
    [InlineData(TestImages.Astc.Atlas_Small_5x5)]
    [InlineData(TestImages.Astc.Atlas_Small_6x6)]
    [InlineData(TestImages.Astc.Atlas_Small_8x8)]
    [InlineData(TestImages.Astc.Checkerboard)]
    [InlineData(TestImages.Astc.Checkered_4)]
    [InlineData(TestImages.Astc.Checkered_5)]
    [InlineData(TestImages.Astc.Checkered_6)]
    [InlineData(TestImages.Astc.Checkered_7)]
    [InlineData(TestImages.Astc.Checkered_8)]
    [InlineData(TestImages.Astc.Checkered_9)]
    [InlineData(TestImages.Astc.Checkered_10)]
    [InlineData(TestImages.Astc.Checkered_11)]
    [InlineData(TestImages.Astc.Checkered_12)]
    [InlineData(TestImages.Astc.Footprint_4x4)]
    [InlineData(TestImages.Astc.Footprint_5x4)]
    [InlineData(TestImages.Astc.Footprint_5x5)]
    [InlineData(TestImages.Astc.Footprint_6x5)]
    [InlineData(TestImages.Astc.Footprint_6x6)]
    [InlineData(TestImages.Astc.Footprint_8x5)]
    [InlineData(TestImages.Astc.Footprint_8x6)]
    [InlineData(TestImages.Astc.Footprint_8x8)]
    [InlineData(TestImages.Astc.Footprint_10x5)]
    [InlineData(TestImages.Astc.Footprint_10x6)]
    [InlineData(TestImages.Astc.Footprint_10x8)]
    [InlineData(TestImages.Astc.Footprint_10x10)]
    [InlineData(TestImages.Astc.Footprint_12x10)]
    [InlineData(TestImages.Astc.Footprint_12x12)]
    [InlineData(TestImages.Astc.Rgb_4x4)]
    [InlineData(TestImages.Astc.Rgb_5x4)]
    [InlineData(TestImages.Astc.Rgb_6x6)]
    [InlineData(TestImages.Astc.Rgb_8x8)]
    [InlineData(TestImages.Astc.Rgb_12x12)]
    public void DecompressImage_WithTestdataFile_ShouldReturnExpectedByteCount(string inputFile)
    {
        string filePath = TestFile.GetInputFileFullPath(inputFile);
        byte[] bytes = File.ReadAllBytes(filePath);
        AstcFile astc = AstcFile.FromMemory(bytes);

        Span<byte> result = AstcDecoder.DecompressImage(astc);

        Assert.Equal(astc.Width * astc.Height * 4, result.Length);
    }

    [Theory]
    [InlineData(TestImages.Astc.Atlas_Small_4x4, FootprintType.Footprint4x4, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_5x5, FootprintType.Footprint5x5, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_6x6, FootprintType.Footprint6x6, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_8x8, FootprintType.Footprint8x8, 256, 256)]
    public void DecompressImage_WithValidData_ShouldDecodeAllBlocks(
        string inputFile,
        FootprintType footprintType,
        int width,
        int height)
    {
        byte[] astcData = TestFile.Create(inputFile).Bytes[16..];
        Footprint footprint = Footprint.FromFootprintType(footprintType);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int expectedBlockCount = blocksWide * blocksHigh;

        // Check ASTC data structure
        Assert.Equal(0, astcData.Length % PhysicalBlock.SizeInBytes);
        Assert.Equal(expectedBlockCount, astcData.Length / PhysicalBlock.SizeInBytes);

        // Verify all blocks can be unpacked
        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            byte[] block = astcData.AsSpan(i, PhysicalBlock.SizeInBytes).ToArray();
            UInt128 bits = new(BitConverter.ToUInt64(block, 8), BitConverter.ToUInt64(block, 0));
            BlockInfo info = BlockInfo.Decode(bits);
            LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, bits, in info);

            Assert.NotNull(logicalBlock);
        }
    }

    [Theory]
    [InlineData(TestImages.Astc.Atlas_Small_4x4, TestImages.Astc.Expected.Atlas_Small_4x4, FootprintType.Footprint4x4, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_5x5, TestImages.Astc.Expected.Atlas_Small_5x5, FootprintType.Footprint5x5, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_6x6, TestImages.Astc.Expected.Atlas_Small_6x6, FootprintType.Footprint6x6, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_8x8, TestImages.Astc.Expected.Atlas_Small_8x8, FootprintType.Footprint8x8, 256, 256)]
    public void DecompressImage_WithAstcFile_ShouldMatchExpected(
        string inputFile,
        string expectedFile,
        FootprintType footprint,
        int width,
        int height)
    {
        string astcPath = TestFile.GetInputFileFullPath(inputFile);
        byte[] astcBytes = File.ReadAllBytes(astcPath);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        // Check file header
        Assert.Equal(footprint, file.Footprint.Type);
        Assert.Equal(width, file.Width);
        Assert.Equal(height, file.Height);

        byte[] decodedPixels = AstcDecoder.DecompressImage(file).ToArray();
        using Image<Rgba32> actualImage = Image.LoadPixelData<Rgba32>(decodedPixels, width, height);
        actualImage.Mutate(x => x.Flip(FlipMode.Vertical));

        string expectedImagePath = TestFile.GetInputFileFullPath(expectedFile);
        using Image<Rgba32> expectedImage = Image.Load<Rgba32>(expectedImagePath);
        ImageComparer.TolerantPercentage(0.1f).VerifySimilarity(expectedImage, actualImage);
    }
}
