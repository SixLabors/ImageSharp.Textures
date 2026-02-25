// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.Core;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class BitOperationsTests
{
    [Fact]
    public void GetBits_UInt128WithLowBits_ShouldExtractCorrectly()
    {
        UInt128 value = new UInt128(0x1234567890ABCDEF, 0xFEDCBA0987654321);

        var result = BitOperations.GetBits(value, 0, 8);

        result.Low().Should().Be(0x21UL);
    }

    [Fact]
    public void GetBits_UInt128WithZeroLength_ShouldReturnZero()
    {
        UInt128 value = new UInt128(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

        var result = BitOperations.GetBits(value, 0, 0);

        result.Should().Be(UInt128.Zero);
    }

    [Fact]
    public void GetBits_ULongWithLowBits_ShouldExtractCorrectly()
    {
        ulong value = 0xFEDCBA0987654321;

        var result = BitOperations.GetBits(value, 0, 8);

        result.Should().Be(0x21UL);
    }

    [Fact]
    public void GetBits_ULongWithZeroLength_ShouldReturnZero()
    {
        ulong value = 0xFFFFFFFFFFFFFFFF;

        var result = BitOperations.GetBits(value, 0, 0);

        result.Should().Be(0UL);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 20)]
    [InlineData(128, 255)]
    [InlineData(255, 128)]
    [InlineData(64, 64)]
    public void TransferPrecision_WithSameInput_ShouldBeDeterministic(int inputA, int inputB)
    {
        var (a1, b1) = BitOperations.TransferPrecision(inputA, inputB);
        var (a2, b2) = BitOperations.TransferPrecision(inputA, inputB);

        a1.Should().Be(a2);
        b1.Should().Be(b2);
    }

    [Fact]
    public void TransferPrecision_WithAllValidByteInputs_ShouldNotThrow()
    {
        for (int a = byte.MinValue; a <= byte.MaxValue; a++)
        {
            for (int b = byte.MinValue; b <= byte.MaxValue; b++)
            {
                var action = () => BitOperations.TransferPrecision(a, b);
                action.Should().NotThrow();
            }
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 10)]
    [InlineData(10, 255)]
    [InlineData(31, 128)]
    [InlineData(-32, 200)]
    [InlineData(-1, 100)]
    public void TransferPrecisionInverse_WithSameInput_ShouldBeDeterministic(int inputA, int inputB)
    {
        var (a1, b1) = BitOperations.TransferPrecisionInverse(inputA, inputB);
        var (a2, b2) = BitOperations.TransferPrecisionInverse(inputA, inputB);

        a1.Should().Be(a2);
        b1.Should().Be(b2);
    }

    [Theory]
    [InlineData(-33, 128)] // a too small
    [InlineData(32, 128)] // a too large
    [InlineData(0, -1)] // b too small
    [InlineData(0, 256)] // b too large
    public void TransferPrecisionInverse_WithInvalidInput_ShouldThrowArgumentOutOfRangeException(int a, int b)
    {
        var action = () => BitOperations.TransferPrecisionInverse(a, b);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 20)]
    [InlineData(31, 255)]
    [InlineData(-32, 128)]
    [InlineData(-1, 200)]
    public void TransferPrecision_AfterInverse_ShouldReturnOriginalValues(int originalA, int originalB)
    {
        var (encodedA, encodedB) = BitOperations.TransferPrecisionInverse(originalA, originalB);

        // Apply regular to decode
        var (decodedA, decodedB) = BitOperations.TransferPrecision(encodedA, encodedB);

        decodedA.Should().Be(originalA);
        decodedB.Should().Be(originalB);
    }
}
