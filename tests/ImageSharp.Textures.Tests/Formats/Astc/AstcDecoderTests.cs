// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

#nullable enable

[GroupOutput("Astc")]
[Trait("Format", "Astc")]
public class AstcDecoderTests
{
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
    [InlineData(TestImages.Astc.Rgba_4x4)]
    [InlineData(TestImages.Astc.Rgba_5x5)]
    [InlineData(TestImages.Astc.Rgba_6x6)]
    [InlineData(TestImages.Astc.Rgba_8x8)]
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
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", inputFile));
        byte[] bytes = File.ReadAllBytes(filePath);
        AstcFile astc = AstcFile.FromMemory(bytes);

        Span<byte> result = AstcDecoder.DecompressImage(astc);

        Assert.Equal(astc.Width * astc.Height * 4, result.Length);
    }

    [Theory]
    [InlineData(TestImages.Astc.Rgba_4x4, FootprintType.Footprint4x4, 256, 256)]
    [InlineData(TestImages.Astc.Rgba_5x5, FootprintType.Footprint5x5, 256, 256)]
    [InlineData(TestImages.Astc.Rgba_6x6, FootprintType.Footprint6x6, 256, 256)]
    [InlineData(TestImages.Astc.Rgba_8x8, FootprintType.Footprint8x8, 256, 256)]
    public void DecompressImage_WithValidData_ShouldDecodeAllBlocks(
        string inputFile,
        FootprintType footprintType,
        int width,
        int height)
    {
        byte[] astcData = TestFile.Create(Path.Combine("Astc", inputFile)).Bytes[16..];
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
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_5x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_8x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_12x12)]
    public void DecompressImage_WithAstcRgbFile_ShouldMatchExpected(TestTextureProvider provider)
    {
        byte[] astcBytes = File.ReadAllBytes(provider.InputFile);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        string blockSize = $"{file.Footprint.Width}x{file.Footprint.Height}";

        byte[] decodedPixels = AstcDecoder.DecompressImage(file).ToArray();
        using Image<Rgba32> actualImage = Image.LoadPixelData<Rgba32>(decodedPixels, file.Width, file.Height);
        actualImage.Mutate(x => x.Flip(FlipMode.Vertical));

        actualImage.CompareToReferenceOutput(
            ImageComparer.TolerantPercentage(0.03f),
            provider,
            testOutputDetails: blockSize);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_5x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_8x8)]
    public void DecompressImage_WithAstcRgbaFile_ShouldMatchExpected(TestTextureProvider provider)
    {
        byte[] astcBytes = File.ReadAllBytes(provider.InputFile);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        string blockSize = $"{file.Footprint.Width}x{file.Footprint.Height}";

        byte[] decodedPixels = AstcDecoder.DecompressImage(file).ToArray();
        using Image<Rgba32> actualImage = Image.LoadPixelData<Rgba32>(decodedPixels, file.Width, file.Height);
        actualImage.Mutate(x => x.Flip(FlipMode.Vertical));

        actualImage.CompareToReferenceOutput(
            ImageComparer.TolerantPercentage(0.03f),
            provider,
            testOutputDetails: blockSize);
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(4, -1)]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void DecompressImage_WithInvalidDimensions_ShouldThrowArgumentOutOfRangeException(int width, int height)
    {
        byte[] data = new byte[16];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(data, width, height, FootprintType.Footprint4x4).ToArray());
    }

    [Fact]
    public void DecompressImageToBuffer_WithNegativeWidth_ShouldThrowArgumentOutOfRangeException()
    {
        byte[] data = new byte[16];
        byte[] buffer = new byte[64];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(data, -1, 4, footprint, buffer));
    }

    [Fact]
    public void DecompressImageToBuffer_WithTooSmallBuffer_ShouldThrowArgumentOutOfRangeException()
    {
        // 4x4 image with 4x4 blocks = 1 block = 16 bytes input, needs 4*4*4=64 bytes output
        byte[] data = new byte[16];
        byte[] buffer = new byte[32]; // too small
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(data, 4, 4, footprint, buffer));
    }

    [Theory]
    [InlineData(8, 64)]
    [InlineData(16, 10)]
    public void DecompressBlock_WithInvalidBufferSizes_ShouldThrowArgumentOutOfRangeException(int dataSize, int bufferSize)
    {
        byte[] data = new byte[dataSize];
        byte[] buffer = new byte[bufferSize];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressBlock(data, footprint, buffer));
    }

    [Theory]
    [InlineData(8, 64)]
    [InlineData(16, 10)]
    public void DecompressHdrBlock_WithInvalidBufferSizes_ShouldThrowArgumentOutOfRangeException(int dataSize, int bufferSize)
    {
        byte[] data = new byte[dataSize];
        float[] buffer = new float[bufferSize];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressHdrBlock(data, footprint, buffer));
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(4, -1)]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void DecompressHdrImage_WithInvalidDimensions_ShouldThrowArgumentOutOfRangeException(int width, int height)
    {
        byte[] data = new byte[16];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressHdrImage(data, width, height, FootprintType.Footprint4x4).ToArray());
    }

    [Fact]
    public void DecompressHdrImageToBuffer_WithTooSmallBuffer_ShouldThrowArgumentOutOfRangeException()
    {
        byte[] data = new byte[16];
        float[] buffer = new float[32]; // too small for 4x4 image (needs 64)
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressHdrImage(data, 4, 4, footprint, buffer));
    }
}
