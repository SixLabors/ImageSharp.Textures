// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class BitOperationsTests
{
    [Fact]
    public void GetBits_UInt128WithLowBits_ShouldExtractCorrectly()
    {
        UInt128 value = new(0x1234567890ABCDEF, 0xFEDCBA0987654321);

        UInt128 result = BitOperations.GetBits(value, 0, 8);

        Assert.Equal(0x21UL, result.Low());
    }

    [Fact]
    public void GetBits_UInt128WithZeroLength_ShouldReturnZero()
    {
        UInt128 value = new(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

        UInt128 result = BitOperations.GetBits(value, 0, 0);

        Assert.Equal(UInt128.Zero, result);
    }

    [Fact]
    public void GetBits_ULongWithLowBits_ShouldExtractCorrectly()
    {
        ulong value = 0xFEDCBA0987654321;

        ulong result = BitOperations.GetBits(value, 0, 8);

        Assert.Equal(0x21UL, result);
    }

    [Fact]
    public void GetBits_ULongWithZeroLength_ShouldReturnZero()
    {
        ulong value = 0xFFFFFFFFFFFFFFFF;

        ulong result = BitOperations.GetBits(value, 0, 0);

        Assert.Equal(0UL, result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 20)]
    [InlineData(128, 255)]
    [InlineData(255, 128)]
    [InlineData(64, 64)]
    public void TransferPrecision_WithSameInput_ShouldBeDeterministic(int inputA, int inputB)
    {
        (int a1, int b1) = BitOperations.TransferPrecision(inputA, inputB);
        (int a2, int b2) = BitOperations.TransferPrecision(inputA, inputB);

        Assert.Equal(a2, a1);
        Assert.Equal(b2, b1);
    }

    [Fact]
    public void TransferPrecision_WithAllValidByteInputs_ShouldNotThrow()
    {
        for (int a = byte.MinValue; a <= byte.MaxValue; a++)
        {
            for (int b = byte.MinValue; b <= byte.MaxValue; b++)
            {
                BitOperations.TransferPrecision(a, b);
            }
        }
    }

}
