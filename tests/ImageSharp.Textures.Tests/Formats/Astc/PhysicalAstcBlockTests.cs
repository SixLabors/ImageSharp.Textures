// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class PhysicalAstcBlockTests
{
    private static readonly UInt128 ErrorBlock = UInt128.Zero;

    [Fact]
    public void Create_WithUInt64_ShouldRoundTripBlockBits()
    {
        const ulong expectedLow = 0x0000000001FE000173UL;

        PhysicalBlock block = PhysicalBlock.Create(expectedLow);

        Assert.Equal((UInt128)expectedLow, block.BlockBits);
    }

    [Fact]
    public void Create_WithUInt128_ShouldRoundTripBlockBits()
    {
        UInt128 expected = (UInt128)0x12345678ABCDEF00UL | ((UInt128)0xCAFEBABEDEADBEEFUL << 64);

        PhysicalBlock block = PhysicalBlock.Create(expected);

        Assert.Equal(expected, block.BlockBits);
    }

    [Fact]
    public void Create_WithMatchingUInt64AndUInt128_ShouldProduceIdenticalBlocks()
    {
        const ulong value = 0x0000000001FE000173UL;

        PhysicalBlock block1 = PhysicalBlock.Create(value);
        PhysicalBlock block2 = PhysicalBlock.Create((UInt128)value);

        Assert.Equal(block2.BlockBits, block1.BlockBits);
    }

    [Fact]
    public void IsVoidExtent_WithKnownVoidExtentPattern_ShouldReturnTrue()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0xFFFFFFFFFFFFFDFCUL);

        Assert.True(block.IsVoidExtent);
    }

    [Fact]
    public void IsVoidExtent_WithStandardBlock_ShouldReturnFalse()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        Assert.False(block.IsVoidExtent);
    }

    [Fact]
    public void IsVoidExtent_WithErrorBlock_ShouldReturnFalse()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        Assert.False(block.IsVoidExtent);
    }

    [Fact]
    public void GetVoidExtentCoordinates_WithValidVoidExtentBlock_ShouldReturnExpectedCoordinates()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int[] coords = block.GetVoidExtentCoordinates();

        Assert.NotNull(coords);
        Assert.Equal(4, coords.Length);
        Assert.Equal(0, coords![0]);
        Assert.Equal(8191, coords[1]);
        Assert.Equal(0, coords[2]);
        Assert.Equal(8191, coords[3]);
    }

    [Fact]
    public void GetVoidExtentCoordinates_WithAllOnesPattern_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFFFFFFFFFFFFDFCUL);

        int[] coords = block.GetVoidExtentCoordinates();

        Assert.True(block.IsVoidExtent);
        Assert.Null(coords);
    }

    [Fact]
    public void Create_WithInvalidVoidExtentCoordinates_ShouldBeIllegalEncoding()
    {
        PhysicalBlock block1 = PhysicalBlock.Create(0x0008004002001DFCUL);
        PhysicalBlock block2 = PhysicalBlock.Create(0x0007FFC001FFFDFCUL);

        Assert.True(block1.IsIllegalEncoding);
        Assert.True(block2.IsIllegalEncoding);
    }

    [Fact]
    public void Create_WithModifiedHighBitsOnVoidExtent_ShouldStillBeValid()
    {
        PhysicalBlock original = PhysicalBlock.Create(0xFFF8003FFE000DFCUL, 0UL);
        PhysicalBlock modified = PhysicalBlock.Create(0xFFF8003FFE000DFCUL, 0xdeadbeefdeadbeef);

        Assert.False(original.IsIllegalEncoding);
        Assert.True(original.IsVoidExtent);
        Assert.False(modified.IsIllegalEncoding);
        Assert.True(modified.IsVoidExtent);
    }

    [Fact]
    public void GetWeightRange_WithValidBlock_ShouldReturn7()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        int? weightRange = block.GetWeightRange();

        Assert.NotNull(weightRange);
        Assert.Equal(7, weightRange);
    }

    [Fact]
    public void GetWeightRange_WithTooManyBits_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000373UL);

        int? weightRange = block.GetWeightRange();

        Assert.Null(weightRange);
    }

    [Fact]
    public void GetWeightRange_WithOneBitPerWeight_ShouldReturn1()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x4000000000800D44UL);

        int? weightRange = block.GetWeightRange();

        Assert.NotNull(weightRange);
        Assert.Equal(1, weightRange);
    }

    [Fact]
    public void GetWeightRange_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? weightRange = block.GetWeightRange();

        Assert.Null(weightRange);
    }

    [Fact]
    public void GetWeightGridDimensions_WithValidBlock_ShouldReturn6x5()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        (int Width, int Height)? dims = block.GetWeightGridDimensions();

        Assert.NotNull(dims);
        Assert.Equal(6, dims!.Value.Width);
        Assert.Equal(5, dims.Value.Height);
    }

    [Fact]
    public void GetWeightGridDimensions_WithTooManyBitsForGrid_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000373UL);

        (int Width, int Height)? dims = block.GetWeightGridDimensions();

        Assert.Null(dims);
        string error = block.IdentifyInvalidEncodingIssues();
        Assert.Contains("Invalid block encoding", error);
    }

    [Fact]
    public void GetWeightGridDimensions_WithDualPlaneBlock_ShouldReturn3x5()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE0005FFUL);

        (int Width, int Height)? dims = block.GetWeightGridDimensions();

        Assert.NotNull(dims);
        Assert.Equal(3, dims!.Value.Width);
        Assert.Equal(5, dims.Value.Height);
    }

    [Fact]
    public void GetWeightGridDimensions_WithNonSharedCEM_ShouldReturn8x8()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x4000000000800D44UL);

        (int Width, int Height)? dims = block.GetWeightGridDimensions();

        Assert.NotNull(dims);
        Assert.Equal(8, dims!.Value.Width);
        Assert.Equal(8, dims.Value.Height);
    }

    [Fact]
    public void GetWeightGridDimensions_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        (int Width, int Height)? dims = block.GetWeightGridDimensions();

        Assert.Null(dims);
    }

    [Fact]
    public void IsDualPlane_WithSinglePlaneBlock_ShouldReturnFalse()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        Assert.False(block.IsDualPlane);
    }

    [Fact]
    public void IsDualPlane_WithDualPlaneBlock_ShouldReturnTrue()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE0005FFUL);

        Assert.True(block.IsDualPlane);
    }

    [Fact]
    public void IsDualPlane_WithErrorBlock_ShouldReturnFalse()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        Assert.False(block.IsDualPlane);
    }

    [Fact]
    public void IsDualPlane_WithInvalidEncoding_ShouldReturnFalse()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000573UL);

        Assert.False(block.IsDualPlane);
        Assert.Null(block.GetWeightGridDimensions());
        Assert.Contains("Invalid block encoding", block.IdentifyInvalidEncodingIssues());
    }

    [Fact]
    public void IsDualPlane_WithValidSinglePlaneBlock_ShouldHaveValidEncoding()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000108UL);

        Assert.False(block.IsDualPlane);
        Assert.False(block.IsIllegalEncoding);
    }

    [Fact]
    public void GetWeightBitCount_WithStandardBlock_ShouldReturn90()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        int? bitCount = block.GetWeightBitCount();

        Assert.Equal(90, bitCount);
    }

    [Fact]
    public void GetWeightBitCount_WithDualPlaneBlock_ShouldReturn90()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE0005FFUL);

        int? bitCount = block.GetWeightBitCount();

        Assert.Equal(90, bitCount);
    }

    [Fact]
    public void GetWeightBitCount_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? bitCount = block.GetWeightBitCount();

        Assert.Null(bitCount);
    }

    [Fact]
    public void GetWeightBitCount_WithVoidExtent_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int? bitCount = block.GetWeightBitCount();

        Assert.Null(bitCount);
    }

    [Fact]
    public void GetWeightBitCount_WithInvalidBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000573UL);

        int? bitCount = block.GetWeightBitCount();

        Assert.Null(bitCount);
    }

    [Fact]
    public void GetWeightStartBit_WithNonSharedCEM_ShouldReturn64()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x4000000000800D44UL);

        int? startBit = block.GetWeightStartBit();

        Assert.Equal(64, startBit);
    }

    [Fact]
    public void GetWeightStartBit_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? startBit = block.GetWeightStartBit();

        Assert.Null(startBit);
    }

    [Fact]
    public void GetWeightStartBit_WithVoidExtent_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int? startBit = block.GetWeightStartBit();

        Assert.Null(startBit);
    }

    [Fact]
    public void IsIllegalEncoding_WithValidBlocks_ShouldReturnFalse()
    {
        Assert.False(PhysicalBlock.Create(0x0000000001FE000173UL).IsIllegalEncoding);
        Assert.False(PhysicalBlock.Create(0x0000000001FE0005FFUL).IsIllegalEncoding);
        Assert.False(PhysicalBlock.Create(0x0000000001FE000108UL).IsIllegalEncoding);
    }

    [Fact]
    public void IdentifyInvalidEncodingIssues_WithZeroBlock_ShouldReturnReservedBlockModeError()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        string error = block.IdentifyInvalidEncodingIssues();

        Assert.NotNull(error);
        Assert.Contains("Invalid block encoding", error);
    }

    [Fact]
    public void IdentifyInvalidEncodingIssues_WithTooManyWeightBits_ShouldReturnError()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000573UL);

        string error = block.IdentifyInvalidEncodingIssues();

        Assert.NotNull(error);
        Assert.Contains("Invalid block encoding", error);
    }

    [Theory]
    [InlineData(0x0000000001FE0005A8UL)]
    [InlineData(0x0000000001FE000588UL)]
    [InlineData(0x0000000001FE00002UL)]
    public void IdentifyInvalidEncodingIssues_WithInvalidBlocks_ShouldReturnError(ulong blockBits)
    {
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        string error = block.IdentifyInvalidEncodingIssues();

        Assert.NotNull(error);
    }

    [Fact]
    public void IdentifyInvalidEncodingIssues_WithDualPlaneFourPartitions_ShouldReturnError()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x000000000000001D1FUL);

        string error = block.IdentifyInvalidEncodingIssues();

        Assert.Null(block.GetPartitionsCount());
        Assert.NotNull(error);
        Assert.Contains("Invalid block encoding", error);
    }

    [Theory]
    [InlineData(0x000000000000000973UL)]
    [InlineData(0x000000000000001173UL)]
    [InlineData(0x000000000000001973UL)]
    public void GetPartitionsCount_WithInvalidPartitionConfig_ShouldReturnNull(ulong blockBits)
    {
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        int? partitions = block.GetPartitionsCount();

        Assert.Null(partitions);
    }

    [Theory]
    [InlineData(0x0000000001FE000173UL, 1)]
    [InlineData(0x0000000001FE0005FFUL, 1)]
    [InlineData(0x0000000001FE000108UL, 1)]
    [InlineData(0x4000000000800D44UL, 2)]
    public void GetPartitionsCount_WithValidBlock_ShouldReturnExpectedCount(ulong blockBits, int expectedCount)
    {
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        int? count = block.GetPartitionsCount();

        Assert.Equal(expectedCount, count);
    }

    [Theory]
    [InlineData(0x4000000000FFED44UL, 0x3FF)]
    [InlineData(0x4000000000AAAD44UL, 0x155)]
    public void GetPartitionId_WithValidMultiPartitionBlock_ShouldReturnExpectedId(ulong blockBits, int expectedId)
    {
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        int? partitionId = block.GetPartitionId();

        Assert.Equal(expectedId, partitionId);
    }

    [Fact]
    public void GetPartitionId_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? partitionId = block.GetPartitionId();

        Assert.Null(partitionId);
    }

    [Fact]
    public void GetPartitionId_WithVoidExtent_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int? partitionId = block.GetPartitionId();

        Assert.Null(partitionId);
    }

    [Fact]
    public void GetEndpointMode_WithFourPartitionBlock_ShouldReturnSameModeForAll()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x000000000000001961UL);

        for (int i = 0; i < 4; ++i)
        {
            ColorEndpointMode? mode = block.GetEndpointMode(i);
            Assert.Equal(ColorEndpointMode.LdrLumaDirect, mode);
        }
    }

    [Fact]
    public void GetEndpointMode_WithNonSharedCEM_ShouldReturnDifferentModes()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x4000000000800D44UL);

        ColorEndpointMode? mode0 = block.GetEndpointMode(0);
        ColorEndpointMode? mode1 = block.GetEndpointMode(1);

        Assert.Equal(ColorEndpointMode.LdrLumaDirect, mode0);
        Assert.Equal(ColorEndpointMode.LdrLumaBaseOffset, mode1);
    }

    [Fact]
    public void GetEndpointMode_WithVoidExtent_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        ColorEndpointMode? mode = block.GetEndpointMode(0);

        Assert.Null(mode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(100)]
    public void GetEndpointMode_WithInvalidPartitionIndex_ShouldReturnNull(int index)
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        ColorEndpointMode? mode = block.GetEndpointMode(index);

        Assert.Null(mode);
    }

    [Fact]
    public void GetColorValuesCount_WithStandardBlock_ShouldReturn2()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        int? count = block.GetColorValuesCount();

        Assert.Equal(2, count);
    }

    [Fact]
    public void GetColorValuesCount_WithVoidExtent_ShouldReturn4()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int? count = block.GetColorValuesCount();

        Assert.Equal(4, count);
    }

    [Fact]
    public void GetColorValuesCount_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? count = block.GetColorValuesCount();

        Assert.Null(count);
    }

    [Fact]
    public void GetColorBitCount_WithStandardBlock_ShouldReturn16()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        int? bitCount = block.GetColorBitCount();

        Assert.Equal(16, bitCount);
    }

    [Fact]
    public void GetColorBitCount_WithVoidExtent_ShouldReturn64()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int? bitCount = block.GetColorBitCount();

        Assert.Equal(64, bitCount);
    }

    [Fact]
    public void GetColorBitCount_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? bitCount = block.GetColorBitCount();

        Assert.Null(bitCount);
    }

    [Fact]
    public void GetColorValuesRange_WithStandardBlock_ShouldReturn255()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000173UL);

        int? range = block.GetColorValuesRange();

        Assert.Equal(255, range);
    }

    [Fact]
    public void GetColorValuesRange_WithVoidExtent_ShouldReturnMaxUInt16()
    {
        PhysicalBlock block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        int? range = block.GetColorValuesRange();

        Assert.Equal((1 << 16) - 1, range);
    }

    [Fact]
    public void GetColorValuesRange_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? range = block.GetColorValuesRange();

        Assert.Null(range);
    }

    [Theory]
    [InlineData(0x0000000001FE000173UL, 17)]
    [InlineData(0x0000000001FE0005FFUL, 17)]
    [InlineData(0x0000000001FE000108UL, 17)]
    [InlineData(0x4000000000FFED44UL, 29)]
    [InlineData(0x4000000000AAAD44UL, 29)]
    [InlineData(0xFFF8003FFE000DFCUL, 64)]
    public void GetColorStartBit_WithVariousBlocks_ShouldReturnExpectedValue(ulong blockBits, int expectedStartBit)
    {
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        int? startBit = block.GetColorStartBit();

        Assert.Equal(expectedStartBit, startBit);
    }

    [Fact]
    public void GetColorStartBit_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        int? startBit = block.GetColorStartBit();

        Assert.Null(startBit);
    }
}
