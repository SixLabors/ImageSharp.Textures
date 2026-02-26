// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class BitStreamTests
{
    [Fact]
    public void Constructor_WithBitsAndLength_ShouldInitializeCorrectly()
    {
        BitStream stream = new(0b1010101010101010UL, 32);

        stream.Bits.Should().Be(32);
    }

    [Fact]
    public void Constructor_WithoutParameters_ShouldInitializeEmpty()
    {
        BitStream stream = default;

        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void TryGetBits_WithSingleBitFromZero_ShouldReturnZero()
    {
        BitStream stream = new(0UL, 1);

        bool success = stream.TryGetBits<uint>(1, out uint bits);

        success.Should().BeTrue();
        bits.Should().Be(0U);
    }

    [Fact]
    public void TryGetBits_StreamEnd_ShouldReturnFalse()
    {
        BitStream stream = new(0UL, 1);
        stream.TryGetBits<uint>(1, out _);

        bool success = stream.TryGetBits<uint>(1, out uint _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryGetBits_WithAlternatingBitPattern_ShouldExtractCorrectly()
    {
        BitStream stream = new(0b1010101010101010UL, 32);

        stream.TryGetBits<uint>(1, out uint bits1).Should().BeTrue();
        bits1.Should().Be(0U);

        stream.TryGetBits<uint>(3, out uint bits2).Should().BeTrue();
        bits2.Should().Be(0b101U);

        stream.TryGetBits<uint>(8, out uint bits3).Should().BeTrue();
        bits3.Should().Be(0b10101010U);

        stream.Bits.Should().Be(20);

        stream.TryGetBits<uint>(20, out uint bits4).Should().BeTrue();
        bits4.Should().Be(0b1010U);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void TryGetBits_With64BitsOfOnes_ShouldReturnAllOnes()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        BitStream stream = new(allBits, 64);

        // Check initial state
        stream.Bits.Should().Be(64);

        bool success = stream.TryGetBits<ulong>(64, out ulong bits);

        success.Should().BeTrue();
        bits.Should().Be(allBits);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void TryGetBits_With40BitsFromFullBits_ShouldReturnLower40Bits()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = new(allBits, 64);

        // Check initial state
        stream.Bits.Should().Be(64);

        bool success = stream.TryGetBits<ulong>(40, out ulong bits);

        success.Should().BeTrue();
        bits.Should().Be(expected40Bits);
        stream.Bits.Should().Be(24);
    }

    [Fact]
    public void TryGetBits_WithZeroBits_ShouldReturnZeroAndNotConsume()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = new(allBits, 32);

        stream.TryGetBits<ulong>(0, out ulong bits1).Should().BeTrue();
        bits1.Should().Be(0UL);

        stream.TryGetBits<ulong>(32, out ulong bits2).Should().BeTrue();
        bits2.Should().Be(expected40Bits & 0xFFFFFFFFUL);

        stream.TryGetBits<ulong>(0, out ulong bits3).Should().BeTrue();
        bits3.Should().Be(0UL);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_WithSmallValues_ShouldAccumulateCorrectly()
    {
        BitStream stream = default;

        stream.PutBits(0U, 1);
        stream.PutBits(0b11U, 2);

        stream.Bits.Should().Be(3);
        stream.TryGetBits<uint>(3, out uint bits).Should().BeTrue();
        bits.Should().Be(0b110U);
    }

    [Fact]
    public void PutBits_With64BitsOfOnes_ShouldStoreCorrectly()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        BitStream stream = default;

        stream.PutBits(allBits, 64);

        stream.Bits.Should().Be(64);
        stream.TryGetBits<ulong>(64, out ulong bits).Should().BeTrue();
        bits.Should().Be(allBits);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_With40BitsOfOnes_ShouldMaskTo40Bits()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = default;

        stream.PutBits(allBits, 40);

        stream.TryGetBits<ulong>(40, out ulong bits).Should().BeTrue();
        bits.Should().Be(expected40Bits);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_WithZeroBitsInterspersed_ShouldReturnValue()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = default;

        stream.PutBits(0U, 0);
        stream.PutBits((uint)(allBits & 0xFFFFFFFFUL), 32);
        stream.PutBits(0U, 0);

        stream.TryGetBits<ulong>(32, out ulong bits).Should().BeTrue();
        bits.Should().Be(expected40Bits & 0xFFFFFFFFUL);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_ThenGetBits_ShouldReturnValue()
    {
        BitStream stream = default;
        const uint value1 = 0b101;
        const uint value2 = 0b11001100;

        stream.PutBits(value1, 3);
        stream.PutBits(value2, 8);

        stream.TryGetBits<uint>(3, out uint retrieved1).Should().BeTrue();
        retrieved1.Should().Be(value1);
        stream.TryGetBits<uint>(8, out uint retrieved2).Should().BeTrue();
        retrieved2.Should().Be(value2);
    }
}
