// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class BitStreamTests
{
    [Fact]
    public void Constructor_WithBitsAndLength_ShouldInitializeCorrectly()
    {
        BitStream stream = new(0b1010101010101010UL, 32);

        Assert.Equal(32u, stream.Bits);
    }

    [Fact]
    public void Constructor_WithoutParameters_ShouldInitializeEmpty()
    {
        BitStream stream = default;

        Assert.Equal(0u, stream.Bits);
    }

    [Fact]
    public void TryGetBits_WithSingleBitFromZero_ShouldReturnZero()
    {
        BitStream stream = new(0UL, 1);

        bool success = stream.TryGetBits<uint>(1, out uint bits);

        Assert.True(success);
        Assert.Equal(0U, bits);
    }

    [Fact]
    public void TryGetBits_StreamEnd_ShouldReturnFalse()
    {
        BitStream stream = new(0UL, 1);
        stream.TryGetBits<uint>(1, out _);

        bool success = stream.TryGetBits<uint>(1, out uint _);

        Assert.False(success);
    }

    [Fact]
    public void TryGetBits_WithAlternatingBitPattern_ShouldExtractCorrectly()
    {
        BitStream stream = new(0b1010101010101010UL, 32);

        Assert.True(stream.TryGetBits<uint>(1, out uint bits1));
        Assert.Equal(0U, bits1);

        Assert.True(stream.TryGetBits<uint>(3, out uint bits2));
        Assert.Equal(0b101U, bits2);

        Assert.True(stream.TryGetBits<uint>(8, out uint bits3));
        Assert.Equal(0b10101010U, bits3);

        Assert.Equal(20u, stream.Bits);

        Assert.True(stream.TryGetBits<uint>(20, out uint bits4));
        Assert.Equal(0b1010U, bits4);
        Assert.Equal(0u, stream.Bits);
    }

    [Fact]
    public void TryGetBits_With64BitsOfOnes_ShouldReturnAllOnes()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        BitStream stream = new(allBits, 64);

        // Check initial state
        Assert.Equal(64u, stream.Bits);

        bool success = stream.TryGetBits<ulong>(64, out ulong bits);

        Assert.True(success);
        Assert.Equal(allBits, bits);
        Assert.Equal(0u, stream.Bits);
    }

    [Fact]
    public void TryGetBits_With40BitsFromFullBits_ShouldReturnLower40Bits()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = new(allBits, 64);

        // Check initial state
        Assert.Equal(64u, stream.Bits);

        bool success = stream.TryGetBits<ulong>(40, out ulong bits);

        Assert.True(success);
        Assert.Equal(expected40Bits, bits);
        Assert.Equal(24u, stream.Bits);
    }

    [Fact]
    public void TryGetBits_WithZeroBits_ShouldReturnZeroAndNotConsume()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = new(allBits, 32);

        Assert.True(stream.TryGetBits<ulong>(0, out ulong bits1));
        Assert.Equal(0UL, bits1);

        Assert.True(stream.TryGetBits<ulong>(32, out ulong bits2));
        Assert.Equal(expected40Bits & 0xFFFFFFFFUL, bits2);

        Assert.True(stream.TryGetBits<ulong>(0, out ulong bits3));
        Assert.Equal(0UL, bits3);
        Assert.Equal(0u, stream.Bits);
    }

    [Fact]
    public void PutBits_WithSmallValues_ShouldAccumulateCorrectly()
    {
        BitStream stream = default;

        stream.PutBits(0U, 1);
        stream.PutBits(0b11U, 2);

        Assert.Equal(3u, stream.Bits);
        Assert.True(stream.TryGetBits<uint>(3, out uint bits));
        Assert.Equal(0b110U, bits);
    }

    [Fact]
    public void PutBits_With64BitsOfOnes_ShouldStoreCorrectly()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        BitStream stream = default;

        stream.PutBits(allBits, 64);

        Assert.Equal(64u, stream.Bits);
        Assert.True(stream.TryGetBits<ulong>(64, out ulong bits));
        Assert.Equal(allBits, bits);
        Assert.Equal(0u, stream.Bits);
    }

    [Fact]
    public void PutBits_With40BitsOfOnes_ShouldMaskTo40Bits()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        const ulong expected40Bits = 0x000000FFFFFFFFFFUL;
        BitStream stream = default;

        stream.PutBits(allBits, 40);

        Assert.True(stream.TryGetBits<ulong>(40, out ulong bits));
        Assert.Equal(expected40Bits, bits);
        Assert.Equal(0u, stream.Bits);
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

        Assert.True(stream.TryGetBits<ulong>(32, out ulong bits));
        Assert.Equal(expected40Bits & 0xFFFFFFFFUL, bits);
        Assert.Equal(0u, stream.Bits);
    }

    [Fact]
    public void PutBits_ThenGetBits_ShouldReturnValue()
    {
        BitStream stream = default;
        const uint value1 = 0b101;
        const uint value2 = 0b11001100;

        stream.PutBits(value1, 3);
        stream.PutBits(value2, 8);

        Assert.True(stream.TryGetBits<uint>(3, out uint retrieved1));
        Assert.Equal(value1, retrieved1);
        Assert.True(stream.TryGetBits<uint>(8, out uint retrieved2));
        Assert.Equal(value2, retrieved2);
    }
}
