// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

#nullable enable
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
        Footprint footprint = Footprint.FromFootprintType(footprintType);
        LogicalBlock logicalBlock = new(footprint);

        Assert.Equal(footprint, logicalBlock.GetFootprint());
        Assert.Equal(footprintType, logicalBlock.GetFootprint().Type);
    }

    [Fact]
    public void GetFootprint_AfterConstruction_ShouldReturnOriginalFootprint()
    {
        Footprint footprint = Footprint.Get8x8();
        LogicalBlock logicalBlock = new(footprint);

        Footprint result = logicalBlock.GetFootprint();

        Assert.Equal(footprint, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(64)]
    public void SetWeightAt_WithValidWeight_ShouldStoreCorrectly(int weight)
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        logicalBlock.SetWeightAt(1, 1, weight);

        Assert.Equal(weight, logicalBlock.WeightAt(1, 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(65)]
    [InlineData(100)]
    public void SetWeightAt_WithInvalidWeight_ShouldThrowArgumentOutOfRangeException(int weight)
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        Action action = () => logicalBlock.SetWeightAt(0, 0, weight);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void WeightAt_WithDefaultWeights_ShouldReturnZero()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        int weight = logicalBlock.WeightAt(2, 2);

        Assert.Equal(0, weight);
    }

    [Fact]
    public void IsDualPlane_ByDefault_ShouldBeFalse()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        bool result = logicalBlock.IsDualPlane();

        Assert.False(result);
    }

    [Fact]
    public void SetDualPlaneChannel_WithValidChannel_ShouldEnableDualPlane()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        logicalBlock.SetDualPlaneChannel(0);

        Assert.True(logicalBlock.IsDualPlane());
    }

    [Fact]
    public void SetDualPlaneChannel_WithNegativeValue_ShouldDisableDualPlane()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        logicalBlock.SetDualPlaneChannel(0);

        logicalBlock.SetDualPlaneChannel(-1);

        Assert.False(logicalBlock.IsDualPlane());
    }

    [Fact]
    public void SetDualPlaneWeightAt_WhenNotDualPlane_ShouldThrowInvalidOperationException()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        Action action = () => logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        var ex = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("Not a dual plane block", ex.Message);
    }

    [Fact]
    public void SetDualPlaneWeightAt_AfterEnablingDualPlane_ShouldPreserveOriginalWeight()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 2);
        logicalBlock.SetDualPlaneChannel(0);

        logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        Assert.Equal(2, logicalBlock.WeightAt(2, 3));
        Assert.Equal(1, logicalBlock.DualPlaneWeightAt(0, 2, 3));
    }

    [Fact]
    public void DualPlaneWeightAt_ForNonDualPlaneChannel_ShouldReturnOriginalWeight()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 2);
        logicalBlock.SetDualPlaneChannel(0);
        logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        for (int i = 1; i < 4; ++i)
        {
            Assert.Equal(2, logicalBlock.DualPlaneWeightAt(i, 2, 3));
        }
    }

    [Fact]
    public void DualPlaneWeightAt_WhenNotDualPlane_ShouldReturnWeightAt()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 42);

        int result = logicalBlock.DualPlaneWeightAt(0, 2, 3);

        Assert.Equal(42, result);
    }

    [Fact]
    public void SetDualPlaneWeightAt_ThenDisableDualPlane_ShouldResetToOriginalWeight()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        logicalBlock.SetWeightAt(2, 3, 2);
        logicalBlock.SetDualPlaneChannel(0);
        logicalBlock.SetDualPlaneWeightAt(0, 2, 3, 1);

        logicalBlock.SetDualPlaneChannel(-1);

        Assert.False(logicalBlock.IsDualPlane());
        Assert.Equal(2, logicalBlock.WeightAt(2, 3));
        for (int i = 0; i < 4; ++i)
        {
            Assert.Equal(2, logicalBlock.DualPlaneWeightAt(i, 2, 3));
        }
    }

    [Fact]
    public void SetEndpoints_WithValidColors_ShouldStoreCorrectly()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        Rgba32 color1 = new(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        Rgba32 color2 = new(byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue);

        logicalBlock.SetEndpoints(color1, color2, 0);

        // No direct getter, but we can verify through ColorAt
        logicalBlock.SetWeightAt(0, 0, 0);
        logicalBlock.SetWeightAt(1, 1, 64);

        Rgba32 colorAtMinWeight = logicalBlock.ColorAt(0, 0);
        Rgba32 colorAtMaxWeight = logicalBlock.ColorAt(1, 1);

        Assert.Equal(color1.R, colorAtMinWeight.R);
        Assert.True(Math.Abs(colorAtMaxWeight.R - color2.R) <= 1);
    }

    [Fact]
    public void ColorAt_WithCheckerboardWeights_ShouldInterpolateCorrectly()
    {
        LogicalBlock logicalBlock = new(Footprint.Get8x8());

        // Create checkerboard weight pattern
        for (int j = 0; j < 8; ++j)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (((i ^ j) & 1) == 1)
                {
                    logicalBlock.SetWeightAt(i, j, 0);
                }
                else
                {
                    logicalBlock.SetWeightAt(i, j, 64);
                }
            }
        }

        Rgba32 endpointA = new(123, 45, 67, 89);
        Rgba32 endpointB = new(101, 121, 31, 41);
        logicalBlock.SetEndpoints(endpointA, endpointB, 0);

        for (int j = 0; j < 8; ++j)
        {
            for (int i = 0; i < 8; ++i)
            {
                Rgba32 color = logicalBlock.ColorAt(i, j);
                if (((i ^ j) & 1) == 1)
                {
                    // Weight 0 = first endpoint
                    Assert.Equal(endpointA.R, color.R);
                    Assert.Equal(endpointA.G, color.G);
                    Assert.Equal(endpointA.B, color.B);
                    Assert.Equal(endpointA.A, color.A);
                }
                else
                {
                    // Weight 64 = second endpoint
                    Assert.Equal(endpointB.R, color.R);
                    Assert.Equal(endpointB.G, color.G);
                    Assert.Equal(endpointB.B, color.B);
                    Assert.Equal(endpointB.A, color.A);
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
        LogicalBlock logicalBlock = new(Footprint.Get4x4());

        Action action = () => logicalBlock.ColorAt(x, y);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void SetPartition_WithValidPartition_ShouldUpdateCorrectly()
    {
        Footprint footprint = Footprint.Get8x8();
        LogicalBlock logicalBlock = new(footprint);

        // Create partition with 2 subsets, all pixels assigned to subset 0
        Partition newPartition = new(footprint, 2, 5)
        {
            Assignment = new int[footprint.PixelCount]
        };

        logicalBlock.SetPartition(newPartition);

        // Should be able to set endpoints for both valid partitions (0 and 1)
        Rgba32 redEndpoint = new(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        Rgba32 blackEndpoint = new(byte.MinValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        Rgba32 greenEndpoint = new(byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue);

        Action setEndpoint0 = () => logicalBlock.SetEndpoints(redEndpoint, blackEndpoint, 0);
        Action setEndpoint1 = () => logicalBlock.SetEndpoints(greenEndpoint, blackEndpoint, 1);

        setEndpoint0();
        setEndpoint1();

        // Should not be able to set endpoints for non-existent partition 2
        Action setEndpoint2 = () => logicalBlock.SetEndpoints(redEndpoint, blackEndpoint, 2);
        Assert.Throws<ArgumentOutOfRangeException>(setEndpoint2);
    }

    [Fact]
    public void SetPartition_WithDifferentFootprint_ShouldThrowInvalidOperationException()
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        Partition wrongPartition = new(Footprint.Get8x8(), 1, 0)
        {
            Assignment = new int[64]
        };

        Action action = () => logicalBlock.SetPartition(wrongPartition);

        var ex = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("New partitions may not be for a different footprint", ex.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void SetEndpoints_WithInvalidSubset_ShouldThrowArgumentOutOfRangeException(int subset)
    {
        LogicalBlock logicalBlock = new(Footprint.Get4x4());
        Rgba32 color1 = new(byte.MaxValue, byte.MinValue, byte.MinValue, byte.MaxValue);
        Rgba32 color2 = new(byte.MinValue, byte.MaxValue, byte.MinValue, byte.MaxValue);

        Action action = () => logicalBlock.SetEndpoints(color1, color2, subset);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void UnpackLogicalBlock_WithErrorBlock_ShouldReturnNull()
    {
        UInt128 bits = UInt128.Zero;
        BlockInfo info = BlockInfo.Decode(bits);

        LogicalBlock? result = LogicalBlock.UnpackLogicalBlock(Footprint.Get8x8(), bits, in info);

        Assert.Null(result);
    }

    [Fact]
    public void UnpackLogicalBlock_WithVoidExtentBlock_ShouldReturnLogicalBlock()
    {
        UInt128 bits = (UInt128)0xFFFFFFFFFFFFFDFCUL;
        BlockInfo info = BlockInfo.Decode(bits);

        LogicalBlock? result = LogicalBlock.UnpackLogicalBlock(Footprint.Get8x8(), bits, in info);

        Assert.NotNull(result);
        Assert.Equal(Footprint.Get8x8(), result!.GetFootprint());
    }

    [Fact]
    public void UnpackLogicalBlock_WithStandardBlock_ShouldReturnLogicalBlock()
    {
        UInt128 bits = (UInt128)0x0000000001FE000173UL;
        BlockInfo info = BlockInfo.Decode(bits);

        LogicalBlock? result = LogicalBlock.UnpackLogicalBlock(Footprint.Get6x5(), bits, in info);

        Assert.NotNull(result);
        Assert.Equal(Footprint.Get6x5(), result!.GetFootprint());
    }

    [Theory]

    // Synthetic test images
    [InlineData(TestImages.Astc.Footprint_4x4, TestImages.Astc.Expected.Footprint_4x4, FootprintType.Footprint4x4, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_5x4, TestImages.Astc.Expected.Footprint_5x4, FootprintType.Footprint5x4, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_5x5, TestImages.Astc.Expected.Footprint_5x5, FootprintType.Footprint5x5, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_6x5, TestImages.Astc.Expected.Footprint_6x5, FootprintType.Footprint6x5, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_6x6, TestImages.Astc.Expected.Footprint_6x6, FootprintType.Footprint6x6, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_8x5, TestImages.Astc.Expected.Footprint_8x5, FootprintType.Footprint8x5, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_8x6, TestImages.Astc.Expected.Footprint_8x6, FootprintType.Footprint8x6, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_8x8, TestImages.Astc.Expected.Footprint_8x8, FootprintType.Footprint8x8, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_10x5, TestImages.Astc.Expected.Footprint_10x5, FootprintType.Footprint10x5, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_10x6, TestImages.Astc.Expected.Footprint_10x6, FootprintType.Footprint10x6, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_10x8, TestImages.Astc.Expected.Footprint_10x8, FootprintType.Footprint10x8, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_10x10, TestImages.Astc.Expected.Footprint_10x10, FootprintType.Footprint10x10, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_12x10, TestImages.Astc.Expected.Footprint_12x10, FootprintType.Footprint12x10, 32, 32)]
    [InlineData(TestImages.Astc.Footprint_12x12, TestImages.Astc.Expected.Footprint_12x12, FootprintType.Footprint12x12, 32, 32)]

    // RGB without alpha images
    [InlineData(TestImages.Astc.Rgb_4x4, TestImages.Astc.Expected.Rgb_4x4, FootprintType.Footprint4x4, 224, 288)]
    [InlineData(TestImages.Astc.Rgb_5x4, TestImages.Astc.Expected.Rgb_5x4, FootprintType.Footprint5x4, 224, 288)]
    [InlineData(TestImages.Astc.Rgb_6x6, TestImages.Astc.Expected.Rgb_6x6, FootprintType.Footprint6x6, 224, 288)]
    [InlineData(TestImages.Astc.Rgb_8x8, TestImages.Astc.Expected.Rgb_8x8, FootprintType.Footprint8x8, 224, 288)]
    [InlineData(TestImages.Astc.Rgb_12x12, TestImages.Astc.Expected.Rgb_12x12, FootprintType.Footprint12x12, 224, 288)]

    // RGB with alpha images
    [InlineData(TestImages.Astc.Atlas_Small_4x4, TestImages.Astc.Expected.Atlas_Small_4x4, FootprintType.Footprint4x4, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_5x5, TestImages.Astc.Expected.Atlas_Small_5x5, FootprintType.Footprint5x5, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_6x6, TestImages.Astc.Expected.Atlas_Small_6x6, FootprintType.Footprint6x6, 256, 256)]
    [InlineData(TestImages.Astc.Atlas_Small_8x8, TestImages.Astc.Expected.Atlas_Small_8x8, FootprintType.Footprint8x8, 256, 256)]
    public void UnpackLogicalBlock_FromImage_ShouldDecodeCorrectly(
        string inputFile,
        string expectedFile,
        FootprintType footprintType,
        int width,
        int height)
    {
        Footprint footprint = Footprint.FromFootprintType(footprintType);
        byte[] astcData = TestFile.Create(inputFile).Bytes[16..];

        using Image<Rgba32> decodedImage = DecodeAstcBlocksToImage(footprint, astcData, width, height);

        string expectedPath = TestFile.GetInputFileFullPath(expectedFile);
        using Image<Rgba32> expectedImage = Image.Load<Rgba32>(expectedPath);
        ImageComparer.TolerantPercentage(1.0f).VerifySimilarity(expectedImage, decodedImage);
    }

    private static Image<Rgba32> DecodeAstcBlocksToImage(Footprint footprint, byte[] astcData, int width, int height)
    {
        // ASTC uses x/y ordering, so we flip Y to match ImageSharp's row/column origin.
        Image<Rgba32> image = new(width, height);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;

        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            int blockIndex = i / PhysicalBlock.SizeInBytes;
            int blockX = blockIndex % blocksWide;
            int blockY = blockIndex / blocksWide;

            byte[] blockSpan = astcData.AsSpan(i, PhysicalBlock.SizeInBytes).ToArray();
            UInt128 bits = new(
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

                    Rgba32 decoded = logicalBlock!.ColorAt(x, y);
                    image[px, height - 1 - py] = decoded;
                }
            }
        }

        return image;
    }
}
