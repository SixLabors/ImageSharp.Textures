// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class LogicalAstcBlockTests
{
    [Theory]
    [InlineData(FootprintType.Footprint4x4)]
    [InlineData(FootprintType.Footprint5x5)]
    [InlineData(FootprintType.Footprint8x8)]
    [InlineData(FootprintType.Footprint10x10)]
    [InlineData(FootprintType.Footprint12x12)]
    public void Constructor_WithValidFootprintType_ShoulReturnExpectedFootprint(FootprintType footprintType)
    {
        var footprint = Footprint.FromFootprintType(footprintType);
        var logicalBlock = new LogicalBlock(footprint);

        logicalBlock.GetFootprint().Should().Be(footprint);
        logicalBlock.GetFootprint().Type.Should().Be(footprintType);
    }

    [Fact]
    public void GetFootprint_AfterConstruction_ShouldReturnOriginalFootprint()
    {
        var footprint = Footprint.Get8x8();
        var logicalBlock = new LogicalBlock(footprint);

        var result = logicalBlock.GetFootprint();

        result.Should().Be(footprint);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(64)]
    public void SetWeightAt_WithValidWeight_ShouldStoreCorrectly(int weight)
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        logicalBlock.SetWeightAt(1, 1, weight);

        logicalBlock.WeightAt(1, 1).Should().Be(weight);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65)]
    [InlineData(100)]
    public void SetWeightAt_WithInvalidWeight_ShouldThrowArgumentOutOfRangeException(int weight)
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        var action = () => logicalBlock.SetWeightAt(0, 0, weight);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WeightAt_WithDefaultWeights_ShouldReturnZero()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        var weight = logicalBlock.WeightAt(2, 2);

        weight.Should().Be(0);
    }

    [Fact]
    public void IsDualPlane_ByDefault_ShouldBeFalse()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        var result = logicalBlock.IsDualPlane();

        result.Should().BeFalse();
    }

    [Fact]
    public void SetDualPlaneChannel_WithValidChannel_ShouldEnableDualPlane()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        logicalBlock.SetDualPlaneChannel(0);

        logicalBlock.IsDualPlane().Should().BeTrue();
    }

    [Fact]
    public void SetDualPlaneChannel_WithNegativeValue_ShouldDisableDualPlane()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        logicalBlock.SetDualPlaneChannel(0);

        logicalBlock.SetDualPlaneChannel(-1);

        logicalBlock.IsDualPlane().Should().BeFalse();
    }

    [Fact]
    public void SetDualPlaneWeightAt_WhenNotDualPlane_ShouldThrowInvalidOperationException()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        var action = () => logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Not a dual plane block");
    }

    [Fact]
    public void SetDualPlaneWeightAt_AfterEnablingDualPlane_ShouldPreserveOriginalWeight()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 2);
        logicalBlock.SetDualPlaneChannel(0);

        logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        logicalBlock.WeightAt(2, 3).Should().Be(2);
        logicalBlock.DualPlaneWeightAt(0, 2, 3).Should().Be(1);
    }

    [Fact]
    public void DualPlaneWeightAt_ForNonDualPlaneChannel_ShouldReturnOriginalWeight()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 2);
        logicalBlock.SetDualPlaneChannel(0);
        logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        for (int i = 1; i < 4; ++i)
        {
            logicalBlock.DualPlaneWeightAt(i, 2, 3).Should().Be(2);
        }
    }

    [Fact]
    public void DualPlaneWeightAt_WhenNotDualPlane_ShouldReturnWeightAt()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 42);

        var result = logicalBlock.DualPlaneWeightAt(0, 2, 3);

        result.Should().Be(42);
    }

    [Fact]
    public void SetDualPlaneWeightAt_ThenDisableDualPlane_ShouldResetToOriginalWeight()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 2);
        logicalBlock.SetDualPlaneChannel(0);
        logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        logicalBlock.SetDualPlaneChannel(-1);

        logicalBlock.IsDualPlane().Should().BeFalse();
        logicalBlock.WeightAt(2, 3).Should().Be(2);
        for (int i = 0; i < 4; ++i)
        {
            logicalBlock.DualPlaneWeightAt(i, 2, 3).Should().Be(2);
        }
    }

    [Fact]
    public void SetEndpoints_WithValidColors_ShouldStoreCorrectly()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        var color1 = new RgbaColor(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        var color2 = new RgbaColor(byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue);

        logicalBlock.SetEndpoints(color1, color2, 0);

        // No direct getter, but we can verify through ColorAt
        logicalBlock.SetWeightAt(0, 0, 0);
        logicalBlock.SetWeightAt(1, 1, 64);

        var colorAtMinWeight = logicalBlock.ColorAt(0, 0);
        var colorAtMaxWeight = logicalBlock.ColorAt(1, 1);

        colorAtMinWeight.R.Should().Be(color1.R);
        colorAtMaxWeight.R.Should().BeCloseTo(color2.R, 1);
    }

    [Fact]
    public void ColorAt_WithCheckerboardWeights_ShouldInterpolateCorrectly()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get8x8());

        // Create checkerboard weight pattern
        for (int j = 0; j < 8; ++j)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (((i ^ j) & 1) == 1)
                    logicalBlock.SetWeightAt(i, j, 0);
                else
                    logicalBlock.SetWeightAt(i, j, 64);
            }
        }

        var endpointA = new RgbaColor(123, 45, 67, 89);
        var endpointB = new RgbaColor(101, 121, 31, 41);
        logicalBlock.SetEndpoints(endpointA, endpointB, 0);

        for (int j = 0; j < 8; ++j)
        {
            for (int i = 0; i < 8; ++i)
            {
                var color = logicalBlock.ColorAt(i, j);
                if (((i ^ j) & 1) == 1)
                {
                    // Weight 0 = first endpoint
                    color.R.Should().Be(endpointA.R);
                    color.G.Should().Be(endpointA.G);
                    color.B.Should().Be(endpointA.B);
                    color.A.Should().Be(endpointA.A);
                }
                else
                {
                    // Weight 64 = second endpoint
                    color.R.Should().Be(endpointB.R);
                    color.G.Should().Be(endpointB.G);
                    color.B.Should().Be(endpointB.B);
                    color.A.Should().Be(endpointB.A);
                }
            }
        }
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(4, 0)]
    [InlineData(0, 4)]
    public void ColorAt_WithOutOfBoundsCoordinates_ShouldThrowArgumentOutOfRangeException(int x, int y)
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());

        var action = () => logicalBlock.ColorAt(x, y);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPartition_WithValidPartition_ShouldUpdateCorrectly()
    {
        var footprint = Footprint.Get8x8();
        var logicalBlock = new LogicalBlock(footprint);

        // Create partition with 2 subsets, all pixels assigned to subset 0
        var newPartition = new Partition(footprint, 2, 5)
        {
            Assignment = new int[footprint.PixelCount]
        };

        logicalBlock.SetPartition(newPartition);

        // Should be able to set endpoints for both valid partitions (0 and 1)
        var redEndpoint = new RgbaColor(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        var blackEndpoint = new RgbaColor(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        var greenEndpoint = new RgbaColor(byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue);

        var setEndpoint0 = () => logicalBlock.SetEndpoints(redEndpoint, blackEndpoint, 0);
        var setEndpoint1 = () => logicalBlock.SetEndpoints(greenEndpoint, blackEndpoint, 1);

        setEndpoint0.Should().NotThrow();
        setEndpoint1.Should().NotThrow();

        // Should not be able to set endpoints for non-existent partition 2
        var setEndpoint2 = () => logicalBlock.SetEndpoints(redEndpoint, blackEndpoint, 2);
        setEndpoint2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPartition_WithDifferentFootprint_ShouldThrowInvalidOperationException()
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        var wrongPartition = new Partition(Footprint.Get8x8(), 1, 0)
        {
            Assignment = new int[64]
        };

        var action = () => logicalBlock.SetPartition(wrongPartition);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("New partitions may not be for a different footprint");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void SetEndpoints_WithInvalidSubset_ShouldThrowArgumentOutOfRangeException(int subset)
    {
        var logicalBlock = new LogicalBlock(Footprint.Get4x4());
        var color1 = new RgbaColor(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        var color2 = new RgbaColor(byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue);

        var action = () => logicalBlock.SetEndpoints(color1, color2, subset);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UnpackLogicalBlock_WithErrorBlock_ShouldReturnNull()
    {
        var bits = UInt128.Zero;
        var info = BlockInfo.Decode(bits);

        var result = LogicalBlock.UnpackLogicalBlock(Footprint.Get8x8(), bits, in info);

        result.Should().BeNull();
    }

    [Fact]
    public void UnpackLogicalBlock_WithVoidExtentBlock_ShouldReturnLogicalBlock()
    {
        var bits = (UInt128)0xFFFFFFFFFFFFFDFCUL;
        var info = BlockInfo.Decode(bits);

        var result = LogicalBlock.UnpackLogicalBlock(Footprint.Get8x8(), bits, in info);

        result.Should().NotBeNull();
        result!.GetFootprint().Should().Be(Footprint.Get8x8());
    }

    [Fact]
    public void UnpackLogicalBlock_WithStandardBlock_ShouldReturnLogicalBlock()
    {
        var bits = (UInt128)0x0000000001FE000173UL;
        var info = BlockInfo.Decode(bits);

        var result = LogicalBlock.UnpackLogicalBlock(Footprint.Get6x5(), bits, in info);

        result.Should().NotBeNull();
        result!.GetFootprint().Should().Be(Footprint.Get6x5());
    }

    [Theory]
    // Synthetic test images
    [InlineData("footprint_4x4", false, FootprintType.Footprint4x4, 32, 32)]
    [InlineData("footprint_5x4", false, FootprintType.Footprint5x4, 32, 32)]
    [InlineData("footprint_5x5", false, FootprintType.Footprint5x5, 32, 32)]
    [InlineData("footprint_6x5", false, FootprintType.Footprint6x5, 32, 32)]
    [InlineData("footprint_6x6", false, FootprintType.Footprint6x6, 32, 32)]
    [InlineData("footprint_8x5", false, FootprintType.Footprint8x5, 32, 32)]
    [InlineData("footprint_8x6", false, FootprintType.Footprint8x6, 32, 32)]
    [InlineData("footprint_8x8", false, FootprintType.Footprint8x8, 32, 32)]
    [InlineData("footprint_10x5", false, FootprintType.Footprint10x5, 32, 32)]
    [InlineData("footprint_10x6", false, FootprintType.Footprint10x6, 32, 32)]
    [InlineData("footprint_10x8", false, FootprintType.Footprint10x8, 32, 32)]
    [InlineData("footprint_10x10", false, FootprintType.Footprint10x10, 32, 32)]
    [InlineData("footprint_12x10", false, FootprintType.Footprint12x10, 32, 32)]
    [InlineData("footprint_12x12", false, FootprintType.Footprint12x12, 32, 32)]
    // RGB without alpha images
    [InlineData("rgb_4x4", false, FootprintType.Footprint4x4, 224, 288)]
    [InlineData("rgb_5x4", false, FootprintType.Footprint5x4, 224, 288)]
    [InlineData("rgb_6x6", false, FootprintType.Footprint6x6, 224, 288)]
    [InlineData("rgb_8x8", false, FootprintType.Footprint8x8, 224, 288)]
    [InlineData("rgb_12x12", false, FootprintType.Footprint12x12, 224, 288)]
    // RGB with alpha images
    [InlineData("atlas_small_4x4", true, FootprintType.Footprint4x4, 256, 256)]
    [InlineData("atlas_small_5x5", true, FootprintType.Footprint5x5, 256, 256)]
    [InlineData("atlas_small_6x6", true, FootprintType.Footprint6x6, 256, 256)]
    [InlineData("atlas_small_8x8", true, FootprintType.Footprint8x8, 256, 256)]
    public void UnpackLogicalBlock_FromImage_ShouldDecodeCorrectly(
        string imageName,
        bool hasAlpha,
        FootprintType footprintType,
        int width,
        int height)
    {
        Footprint footprint = Footprint.FromFootprintType(footprintType);
        byte[] astcData = FileBasedHelpers.LoadASTCFile(imageName);

        using Image<Rgba32> decodedImage = DecodeAstcBlocksToImage(footprint, astcData, width, height);

        string expectedPath = FileBasedHelpers.GetExpectedPath(imageName + ".bmp");
        using Image<Rgba32> expectedImage = Image.Load<Rgba32>(expectedPath);
        ImageComparer.TolerantPercentage(1.0f).VerifySimilarity(expectedImage, decodedImage);
    }

    private static Image<Rgba32> DecodeAstcBlocksToImage(Footprint footprint, byte[] astcData, int width, int height)
    {
        // ASTC uses x/y ordering, so we flip Y to match ImageSharp's row/column origin.
        var image = new Image<Rgba32>(width, height);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;

        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            int blockIndex = i / PhysicalBlock.SizeInBytes;
            int blockX = blockIndex % blocksWide;
            int blockY = blockIndex / blocksWide;

            byte[] blockSpan = astcData.AsSpan(i, PhysicalBlock.SizeInBytes).ToArray();
            var bits = new UInt128(
                BitConverter.ToUInt64(blockSpan, 8),
                BitConverter.ToUInt64(blockSpan, 0));
            BlockInfo info = BlockInfo.Decode(bits);
            LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, bits, in info);
            Assert.NotNull(logicalBlock);

            for (int y = 0; y < blockHeight; ++y)
            {
                for (int x = 0; x < blockWidth; ++x)
                {
                    int px = (blockWidth * blockX) + x;
                    int py = (blockHeight * blockY) + y;
                    if (px >= width || py >= height)
                    {
                        continue;
                    }

                    RgbaColor decoded = logicalBlock!.ColorAt(x, y);
                    image[px, height - 1 - py] = new Rgba32(decoded.R, decoded.G, decoded.B, decoded.A);
                }
            }
        }

        return image;
    }
}
