// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class PhysicalAstcBlockTests
{
    private static readonly UInt128 ErrorBlock = UInt128.Zero;

    [Fact]
    public void Create_WithUInt64_ShouldRoundTripBlockBits()
    {
        const ulong expectedLow = 0x0000000001FE000173UL;

        var block = PhysicalBlock.Create(expectedLow);

        block.BlockBits.Should().Be((UInt128)expectedLow);
    }

    [Fact]
    public void Create_WithUInt128_ShouldRoundTripBlockBits()
    {
        var expected = (UInt128)0x12345678ABCDEF00UL | ((UInt128)0xCAFEBABEDEADBEEFUL << 64);

        var block = PhysicalBlock.Create(expected);

        block.BlockBits.Should().Be(expected);
    }

    [Fact]
    public void Create_WithMatchingUInt64AndUInt128_ShouldProduceIdenticalBlocks()
    {
        const ulong value = 0x0000000001FE000173UL;

        var block1 = PhysicalBlock.Create(value);
        var block2 = PhysicalBlock.Create((UInt128)value);

        block1.BlockBits.Should().Be(block2.BlockBits);
    }

    [Fact]
    public void IsVoidExtent_WithKnownVoidExtentPattern_ShouldReturnTrue()
    {
        var block = PhysicalBlock.Create((UInt128)0xFFFFFFFFFFFFFDFCUL);

        block.IsVoidExtent.Should().BeTrue();
    }

    [Fact]
    public void IsVoidExtent_WithStandardBlock_ShouldReturnFalse()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        block.IsVoidExtent.Should().BeFalse();
    }

    [Fact]
    public void IsVoidExtent_WithErrorBlock_ShouldReturnFalse()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        block.IsVoidExtent.Should().BeFalse();
    }

    [Fact]
    public void GetVoidExtentCoordinates_WithValidVoidExtentBlock_ShouldReturnExpectedCoordinates()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var coords = block.GetVoidExtentCoordinates();

        coords.Should().NotBeNull();
        coords.Should().HaveCount(4);
        coords![0].Should().Be(0);
        coords[1].Should().Be(8191);
        coords[2].Should().Be(0);
        coords[3].Should().Be(8191);
    }

    [Fact]
    public void GetVoidExtentCoordinates_WithAllOnesPattern_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0xFFFFFFFFFFFFFDFCUL);

        var coords = block.GetVoidExtentCoordinates();

        block.IsVoidExtent.Should().BeTrue();
        coords.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidVoidExtentCoordinates_ShouldBeIllegalEncoding()
    {
        var block1 = PhysicalBlock.Create(0x0008004002001DFCUL);
        var block2 = PhysicalBlock.Create(0x0007FFC001FFFDFCUL);

        block1.IsIllegalEncoding.Should().BeTrue();
        block2.IsIllegalEncoding.Should().BeTrue();
    }

    [Fact]
    public void Create_WithModifiedHighBitsOnVoidExtent_ShouldStillBeValid()
    {
        var original = PhysicalBlock.Create(0xFFF8003FFE000DFCUL, 0UL);
        var modified = PhysicalBlock.Create(0xFFF8003FFE000DFCUL, 0xdeadbeefdeadbeef);

        original.IsIllegalEncoding.Should().BeFalse();
        original.IsVoidExtent.Should().BeTrue();
        modified.IsIllegalEncoding.Should().BeFalse();
        modified.IsVoidExtent.Should().BeTrue();
    }

    [Fact]
    public void GetWeightRange_WithValidBlock_ShouldReturn7()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var weightRange = block.GetWeightRange();

        weightRange.Should().HaveValue();
        weightRange.Should().Be(7);
    }

    [Fact]
    public void GetWeightRange_WithTooManyBits_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000373UL);

        var weightRange = block.GetWeightRange();

        weightRange.Should().BeNull();
    }

    [Fact]
    public void GetWeightRange_WithOneBitPerWeight_ShouldReturn1()
    {
        var block = PhysicalBlock.Create(0x4000000000800D44UL);

        var weightRange = block.GetWeightRange();

        weightRange.Should().HaveValue();
        weightRange.Should().Be(1);
    }

    [Fact]
    public void GetWeightRange_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var weightRange = block.GetWeightRange();

        weightRange.Should().BeNull();
    }

    [Fact]
    public void GetWeightGridDimensions_WithValidBlock_ShouldReturn6x5()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var dims = block.GetWeightGridDimensions();

        dims.Should().NotBeNull();
        dims!.Value.Width.Should().Be(6);
        dims.Value.Height.Should().Be(5);
    }

    [Fact]
    public void GetWeightGridDimensions_WithTooManyBitsForGrid_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000373UL);

        var dims = block.GetWeightGridDimensions();

        dims.Should().BeNull();
        var error = block.IdentifyInvalidEncodingIssues();
        error.Should().Contain("Invalid block encoding");
    }

    [Fact]
    public void GetWeightGridDimensions_WithDualPlaneBlock_ShouldReturn3x5()
    {
        var block = PhysicalBlock.Create(0x0000000001FE0005FFUL);

        var dims = block.GetWeightGridDimensions();

        dims.Should().NotBeNull();
        dims!.Value.Width.Should().Be(3);
        dims.Value.Height.Should().Be(5);
    }

    [Fact]
    public void GetWeightGridDimensions_WithNonSharedCEM_ShouldReturn8x8()
    {
        var block = PhysicalBlock.Create(0x4000000000800D44UL);

        var dims = block.GetWeightGridDimensions();

        dims.Should().NotBeNull();
        dims!.Value.Width.Should().Be(8);
        dims.Value.Height.Should().Be(8);
    }

    [Fact]
    public void GetWeightGridDimensions_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var dims = block.GetWeightGridDimensions();

        dims.Should().BeNull();
    }

    [Fact]
    public void IsDualPlane_WithSinglePlaneBlock_ShouldReturnFalse()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        block.IsDualPlane.Should().BeFalse();
    }

    [Fact]
    public void IsDualPlane_WithDualPlaneBlock_ShouldReturnTrue()
    {
        var block = PhysicalBlock.Create(0x0000000001FE0005FFUL);

        block.IsDualPlane.Should().BeTrue();
    }

    [Fact]
    public void IsDualPlane_WithErrorBlock_ShouldReturnFalse()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        block.IsDualPlane.Should().BeFalse();
    }

    [Fact]
    public void IsDualPlane_WithInvalidEncoding_ShouldReturnFalse()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000573UL);

        block.IsDualPlane.Should().BeFalse();
        block.GetWeightGridDimensions().Should().BeNull();
        block.IdentifyInvalidEncodingIssues().Should().Contain("Invalid block encoding");
    }

    [Fact]
    public void IsDualPlane_WithValidSinglePlaneBlock_ShouldHaveValidEncoding()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000108UL);

        block.IsDualPlane.Should().BeFalse();
        block.IsIllegalEncoding.Should().BeFalse();
    }

    [Fact]
    public void GetWeightBitCount_WithStandardBlock_ShouldReturn90()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var bitCount = block.GetWeightBitCount();

        bitCount.Should().Be(90);
    }

    [Fact]
    public void GetWeightBitCount_WithDualPlaneBlock_ShouldReturn90()
    {
        var block = PhysicalBlock.Create(0x0000000001FE0005FFUL);

        var bitCount = block.GetWeightBitCount();

        bitCount.Should().Be(90);
    }

    [Fact]
    public void GetWeightBitCount_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var bitCount = block.GetWeightBitCount();

        bitCount.Should().BeNull();
    }

    [Fact]
    public void GetWeightBitCount_WithVoidExtent_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var bitCount = block.GetWeightBitCount();

        bitCount.Should().BeNull();
    }

    [Fact]
    public void GetWeightBitCount_WithInvalidBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000573UL);

        var bitCount = block.GetWeightBitCount();

        bitCount.Should().BeNull();
    }

    [Fact]
    public void GetWeightStartBit_WithNonSharedCEM_ShouldReturn64()
    {
        var block = PhysicalBlock.Create(0x4000000000800D44UL);

        var startBit = block.GetWeightStartBit();

        startBit.Should().Be(64);
    }

    [Fact]
    public void GetWeightStartBit_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var startBit = block.GetWeightStartBit();

        startBit.Should().BeNull();
    }

    [Fact]
    public void GetWeightStartBit_WithVoidExtent_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var startBit = block.GetWeightStartBit();

        startBit.Should().BeNull();
    }

    [Fact]
    public void IsIllegalEncoding_WithValidBlocks_ShouldReturnFalse()
    {
        PhysicalBlock.Create(0x0000000001FE000173UL).IsIllegalEncoding.Should().BeFalse();
        PhysicalBlock.Create(0x0000000001FE0005FFUL).IsIllegalEncoding.Should().BeFalse();
        PhysicalBlock.Create(0x0000000001FE000108UL).IsIllegalEncoding.Should().BeFalse();
    }

    [Fact]
    public void IdentifyInvalidEncodingIssues_WithZeroBlock_ShouldReturnReservedBlockModeError()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var error = block.IdentifyInvalidEncodingIssues();

        error.Should().NotBeNull();
        error.Should().Contain("Invalid block encoding");
    }

    [Fact]
    public void IdentifyInvalidEncodingIssues_WithTooManyWeightBits_ShouldReturnError()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000573UL);

        var error = block.IdentifyInvalidEncodingIssues();

        error.Should().NotBeNull();
        error.Should().Contain("Invalid block encoding");
    }

    [Theory]
    [InlineData(0x0000000001FE0005A8UL)]
    [InlineData(0x0000000001FE000588UL)]
    [InlineData(0x0000000001FE00002UL)]
    public void IdentifyInvalidEncodingIssues_WithInvalidBlocks_ShouldReturnError(ulong blockBits)
    {
        var block = PhysicalBlock.Create(blockBits);

        var error = block.IdentifyInvalidEncodingIssues();

        error.Should().NotBeNull();
    }

    [Fact]
    public void IdentifyInvalidEncodingIssues_WithDualPlaneFourPartitions_ShouldReturnError()
    {
        var block = PhysicalBlock.Create(0x000000000000001D1FUL);

        var error = block.IdentifyInvalidEncodingIssues();

        block.GetPartitionsCount().Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("Invalid block encoding");
    }

    [Theory]
    [InlineData(0x000000000000000973UL)]
    [InlineData(0x000000000000001173UL)]
    [InlineData(0x000000000000001973UL)]
    public void GetPartitionsCount_WithInvalidPartitionConfig_ShouldReturnNull(ulong blockBits)
    {
        var block = PhysicalBlock.Create(blockBits);

        var partitions = block.GetPartitionsCount();

        partitions.Should().BeNull();
    }

    [Theory]
    [InlineData(0x0000000001FE000173UL, 1)]
    [InlineData(0x0000000001FE0005FFUL, 1)]
    [InlineData(0x0000000001FE000108UL, 1)]
    [InlineData(0x4000000000800D44UL, 2)]
    public void GetPartitionsCount_WithValidBlock_ShouldReturnExpectedCount(ulong blockBits, int expectedCount)
    {
        var block = PhysicalBlock.Create(blockBits);

        var count = block.GetPartitionsCount();

        count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(0x4000000000FFED44UL, 0x3FF)]
    [InlineData(0x4000000000AAAD44UL, 0x155)]
    public void GetPartitionId_WithValidMultiPartitionBlock_ShouldReturnExpectedId(ulong blockBits, int expectedId)
    {
        var block = PhysicalBlock.Create(blockBits);

        var partitionId = block.GetPartitionId();

        partitionId.Should().Be(expectedId);
    }

    [Fact]
    public void GetPartitionId_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var partitionId = block.GetPartitionId();

        partitionId.Should().BeNull();
    }

    [Fact]
    public void GetPartitionId_WithVoidExtent_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var partitionId = block.GetPartitionId();

        partitionId.Should().BeNull();
    }

    [Fact]
    public void GetEndpointMode_WithFourPartitionBlock_ShouldReturnSameModeForAll()
    {
        var block = PhysicalBlock.Create(0x000000000000001961UL);

        for (int i = 0; i < 4; ++i)
        {
            var mode = block.GetEndpointMode(i);
            mode.Should().Be(ColorEndpointMode.LdrLumaDirect);
        }
    }

    [Fact]
    public void GetEndpointMode_WithNonSharedCEM_ShouldReturnDifferentModes()
    {
        var block = PhysicalBlock.Create(0x4000000000800D44UL);

        var mode0 = block.GetEndpointMode(0);
        var mode1 = block.GetEndpointMode(1);

        mode0.Should().Be(ColorEndpointMode.LdrLumaDirect);
        mode1.Should().Be(ColorEndpointMode.LdrLumaBaseOffset);
    }

    [Fact]
    public void GetEndpointMode_WithVoidExtent_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var mode = block.GetEndpointMode(0);

        mode.Should().BeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(100)]
    public void GetEndpointMode_WithInvalidPartitionIndex_ShouldReturnNull(int index)
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var mode = block.GetEndpointMode(index);

        mode.Should().BeNull();
    }

    [Fact]
    public void GetColorValuesCount_WithStandardBlock_ShouldReturn2()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var count = block.GetColorValuesCount();

        count.Should().Be(2);
    }

    [Fact]
    public void GetColorValuesCount_WithVoidExtent_ShouldReturn4()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var count = block.GetColorValuesCount();

        count.Should().Be(4);
    }

    [Fact]
    public void GetColorValuesCount_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var count = block.GetColorValuesCount();

        count.Should().BeNull();
    }

    [Fact]
    public void GetColorBitCount_WithStandardBlock_ShouldReturn16()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var bitCount = block.GetColorBitCount();

        bitCount.Should().Be(16);
    }

    [Fact]
    public void GetColorBitCount_WithVoidExtent_ShouldReturn64()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var bitCount = block.GetColorBitCount();

        bitCount.Should().Be(64);
    }

    [Fact]
    public void GetColorBitCount_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var bitCount = block.GetColorBitCount();

        bitCount.Should().BeNull();
    }

    [Fact]
    public void GetColorValuesRange_WithStandardBlock_ShouldReturn255()
    {
        var block = PhysicalBlock.Create(0x0000000001FE000173UL);

        var range = block.GetColorValuesRange();

        range.Should().Be(255);
    }

    [Fact]
    public void GetColorValuesRange_WithVoidExtent_ShouldReturnMaxUInt16()
    {
        var block = PhysicalBlock.Create(0xFFF8003FFE000DFCUL);

        var range = block.GetColorValuesRange();

        range.Should().Be((1 << 16) - 1);
    }

    [Fact]
    public void GetColorValuesRange_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var range = block.GetColorValuesRange();

        range.Should().BeNull();
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
        var block = PhysicalBlock.Create(blockBits);

        var startBit = block.GetColorStartBit();

        startBit.Should().Be(expectedStartBit);
    }

    [Fact]
    public void GetColorStartBit_WithErrorBlock_ShouldReturnNull()
    {
        var block = PhysicalBlock.Create(ErrorBlock);

        var startBit = block.GetColorStartBit();

        startBit.Should().BeNull();
    }
}
