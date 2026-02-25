// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class CodecTests
{
    [Fact]
    public void ASTCDecompressToRGBA_WithZeroWidth_ShouldReturnEmpty()
    {
        var data = new byte[256];
        const int height = 16;

        var result = AstcDecoder.DecompressImage(data, 0, height, FootprintType.Footprint4x4);

        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void ASTCDecompressToRGBA_WithZeroHeight_ShouldReturnEmpty()
    {
        var data = new byte[256];
        const int width = 16;

        var result = AstcDecoder.DecompressImage(data, width, 0, FootprintType.Footprint4x4);

        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void ASTCDecompressToRGBA_WithDataSizeNotMultipleOfBlockSize_ShouldReturnEmpty()
    {
        var data = new byte[256];
        const int width = 16;
        const int height = 16;
        var invalidData = data.AsSpan(0, data.Length - 1).ToArray();

        var result = AstcDecoder.DecompressImage(invalidData, width, height, FootprintType.Footprint4x4);

        result.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void ASTCDecompressToRGBA_WithMismatchedBlockCount_ShouldReturnEmpty()
    {
        var data = new byte[256];
        const int width = 16;
        const int height = 16;
        var mismatchedData = data.AsSpan(0, data.Length - PhysicalBlock.SizeInBytes).ToArray();

        var result = AstcDecoder.DecompressImage(mismatchedData, width, height, FootprintType.Footprint4x4);

        result.ToArray().Should().BeEmpty();
    }

    [Theory]
    [InlineData("atlas_small_4x4", FootprintType.Footprint4x4, 256, 256)]
    [InlineData("atlas_small_5x5", FootprintType.Footprint5x5, 256, 256)]
    [InlineData("atlas_small_6x6", FootprintType.Footprint6x6, 256, 256)]
    [InlineData("atlas_small_8x8", FootprintType.Footprint8x8, 256, 256)]
    public void ASTCDecompressToRGBA_WithValidData_ShouldMatchExpected(
        string imageName,
        FootprintType footprintType,
        int width,
        int height)
    {
        var astcData = FileBasedHelpers.LoadASTCFile(imageName);
        var footprint = Footprint.FromFootprintType(footprintType);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int expectedBlockCount = blocksWide * blocksHigh;

        // Check ASTC data structure
        (astcData.Length % PhysicalBlock.SizeInBytes).Should().Be(0, "astc byte length must be multiple of block size");
        (astcData.Length / PhysicalBlock.SizeInBytes).Should().Be(expectedBlockCount, $"ASTC block count should match expected");

        // Verify all blocks can be unpacked
        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            var block = astcData.AsSpan(i, PhysicalBlock.SizeInBytes).ToArray();
            var bits = new UInt128(BitConverter.ToUInt64(block, 8), BitConverter.ToUInt64(block, 0));
            var info = BlockInfo.Decode(bits);
            var logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, bits, in info);

            logicalBlock.Should().NotBeNull("all blocks should unpack successfully");
        }

        var decodedPixels = AstcDecoder.DecompressImage(astcData, width, height, footprintType);
        var actualImage = new ImageBuffer(decodedPixels.ToArray(), width, height, 4);

        var expectedImagePath = FileBasedHelpers.GetExpectedPath(imageName + ".bmp");
        var expectedImage = FileBasedHelpers.LoadExpectedImage(expectedImagePath);
        ImageUtils.CompareSumOfSquaredDifferences(expectedImage, actualImage, 0.1);
    }

    [Theory]
    [InlineData("atlas_small_4x4", FootprintType.Footprint4x4, 256, 256)]
    [InlineData("atlas_small_5x5", FootprintType.Footprint5x5, 256, 256)]
    [InlineData("atlas_small_6x6", FootprintType.Footprint6x6, 256, 256)]
    [InlineData("atlas_small_8x8", FootprintType.Footprint8x8, 256, 256)]
    public void DecompressToImage_WithAstcFile_ShouldMatchExpected(
        string imageName,
        FootprintType footprint,
        int width,
        int height)
    {
        var astcPath = FileBasedHelpers.GetInputPath(imageName + ".astc");
        var astcBytes = File.ReadAllBytes(astcPath);
        var file = AstcFile.FromMemory(astcBytes);

        // Check file header
        file.Footprint.Type.Should().Be(footprint);
        file.Width.Should().Be(width);
        file.Height.Should().Be(height);

        var decodedPixels = AstcDecoder.DecompressImage(file);
        var actualImage = new ImageBuffer(decodedPixels.ToArray(), width, height, 4);

        var expectedImagePath = FileBasedHelpers.GetExpectedPath(imageName + ".bmp");
        var expectedImage = FileBasedHelpers.LoadExpectedImage(expectedImagePath);
        ImageUtils.CompareSumOfSquaredDifferences(expectedImage, actualImage, 0.1);
    }

}
