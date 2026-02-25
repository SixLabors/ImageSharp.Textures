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
        var stream = new BitStream(0b1010101010101010UL, 32);

        stream.Bits.Should().Be(32);
    }

    [Fact]
    public void Constructor_WithoutParameters_ShouldInitializeEmpty()
    {
        var stream = default(BitStream);

        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void TryGetBits_WithSingleBitFromZero_ShouldReturnZero()
    {
        var stream = new BitStream(0UL, 1);

        var success = stream.TryGetBits<uint>(1, out var bits);

        success.Should().BeTrue();
        bits.Should().Be(0U);
    }

    [Fact]
    public void TryGetBits_StreamEnd_ShouldReturnFalse()
    {
        var stream = new BitStream(0UL, 1);
        stream.TryGetBits<uint>(1, out _);

        var success = stream.TryGetBits<uint>(1, out var _);

        success.Should().BeFalse();
    }

    [Fact]
    public void TryGetBits_WithAlternatingBitPattern_ShouldExtractCorrectly()
    {
        var stream = new BitStream(0b1010101010101010UL, 32);

        stream.TryGetBits<uint>(1, out var bits1).Should().BeTrue();
        bits1.Should().Be(0U);

        stream.TryGetBits<uint>(3, out var bits2).Should().BeTrue();
        bits2.Should().Be(0b101U);

        stream.TryGetBits<uint>(8, out var bits3).Should().BeTrue();
        bits3.Should().Be(0b10101010U);

        stream.Bits.Should().Be(20);

        stream.TryGetBits<uint>(20, out var bits4).Should().BeTrue();
        bits4.Should().Be(0b1010U);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void TryGetBits_With64BitsOfOnes_ShouldReturnAllOnes()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        var stream = new BitStream(allBits, 64);

        // Check initial state
        stream.Bits.Should().Be(64);

        var success = stream.TryGetBits<ulong>(64, out var bits);

        success.Should().BeTrue();
        bits.Should().Be(allBits);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void TryGetBits_With40BitsFromFullBits_ShouldReturnLower40Bits()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        var stream = new BitStream(allBits, 64);

        // Check initial state
        stream.Bits.Should().Be(64);

        var success = stream.TryGetBits<ulong>(40, out var bits);

        success.Should().BeTrue();
        bits.Should().Be(expected40Bits);
        stream.Bits.Should().Be(24);
    }

    [Fact]
    public void TryGetBits_WithZeroBits_ShouldReturnZeroAndNotConsume()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        var stream = new BitStream(allBits, 32);

        stream.TryGetBits<ulong>(0, out var bits1).Should().BeTrue();
        bits1.Should().Be(0UL);

        stream.TryGetBits<ulong>(32, out var bits2).Should().BeTrue();
        bits2.Should().Be(expected40Bits & 0xFFFFFFFFUL);

        stream.TryGetBits<ulong>(0, out var bits3).Should().BeTrue();
        bits3.Should().Be(0UL);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_WithSmallValues_ShouldAccumulateCorrectly()
    {
        var stream = default(BitStream);

        stream.PutBits(0U, 1);
        stream.PutBits(0b11U, 2);

        stream.Bits.Should().Be(3);
        stream.TryGetBits<uint>(3, out var bits).Should().BeTrue();
        bits.Should().Be(0b110U);
    }

    [Fact]
    public void PutBits_With64BitsOfOnes_ShouldStoreCorrectly()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        var stream = default(BitStream);

        stream.PutBits(allBits, 64);

        stream.Bits.Should().Be(64);
        stream.TryGetBits<ulong>(64, out var bits).Should().BeTrue();
        bits.Should().Be(allBits);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_With40BitsOfOnes_ShouldMaskTo40Bits()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        var stream = default(BitStream);

        stream.PutBits(allBits, 40);

        stream.TryGetBits<ulong>(40, out var bits).Should().BeTrue();
        bits.Should().Be(expected40Bits);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_WithZeroBitsInterspersed_ShouldReturnValue()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        var stream = default(BitStream);

        stream.PutBits(0U, 0);
        stream.PutBits((uint)(allBits & 0xFFFFFFFFUL), 32);
        stream.PutBits(0U, 0);

        stream.TryGetBits<ulong>(32, out var bits).Should().BeTrue();
        bits.Should().Be(expected40Bits & 0xFFFFFFFFUL);
        stream.Bits.Should().Be(0);
    }

    [Fact]
    public void PutBits_ThenGetBits_ShouldReturnValue()
    {
        var stream = default(BitStream);
        const uint value1 = 0b101;
        const uint value2 = 0b11001100;

        stream.PutBits(value1, 3);
        stream.PutBits(value2, 8);

        stream.TryGetBits<uint>(3, out var retrieved1).Should().BeTrue();
        retrieved1.Should().Be(value1);
        stream.TryGetBits<uint>(8, out var retrieved2).Should().BeTrue();
        retrieved2.Should().Be(value2);
    }
}
