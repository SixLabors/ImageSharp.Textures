// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

// Thin wrapper tests for PhysicalBlock. The structural block-mode classification is covered
// directly against BlockInfo in BlockInfoTests — these tests only assert the wrapper's
// factory round-trip and its delegation to BlockInfo for validity/void-extent flags.
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
    public void IsIllegalEncoding_WithValidBlocks_ShouldReturnFalse()
    {
        Assert.False(PhysicalBlock.Create(0x0000000001FE000173UL).IsIllegalEncoding);
        Assert.False(PhysicalBlock.Create(0x0000000001FE0005FFUL).IsIllegalEncoding);
        Assert.False(PhysicalBlock.Create(0x0000000001FE000108UL).IsIllegalEncoding);
    }

    [Fact]
    public void IsIllegalEncoding_WithZeroBlock_ShouldReturnTrue()
    {
        PhysicalBlock block = PhysicalBlock.Create(ErrorBlock);

        Assert.True(block.IsIllegalEncoding);
    }

    [Fact]
    public void IsIllegalEncoding_WithTooManyWeightBits_ShouldReturnTrue()
    {
        PhysicalBlock block = PhysicalBlock.Create(0x0000000001FE000573UL);

        Assert.True(block.IsIllegalEncoding);
    }

    [Theory]
    [InlineData(0x0000000001FE0005A8UL)]
    [InlineData(0x0000000001FE000588UL)]
    [InlineData(0x0000000001FE00002UL)]
    public void IsIllegalEncoding_WithInvalidBlocks_ShouldReturnTrue(ulong blockBits)
    {
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        Assert.True(block.IsIllegalEncoding);
    }

    [Fact]
    public void IsIllegalEncoding_WithDualPlaneFourPartitions_ShouldReturnTrue()
    {
        // 4 partitions + dual plane is explicitly illegal per ASTC spec §C.2.11.
        PhysicalBlock block = PhysicalBlock.Create(0x000000000000001D1FUL);

        Assert.True(block.IsIllegalEncoding);
    }
}
