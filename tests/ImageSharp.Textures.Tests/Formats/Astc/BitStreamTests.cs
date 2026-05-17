// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

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

        bool success = stream.TryGetBits(1, out uint bits);

        Assert.True(success);
        Assert.Equal(0U, bits);
    }

    [Fact]
    public void TryGetBits_StreamEnd_ShouldReturnFalse()
    {
        BitStream stream = new(0UL, 1);
        stream.TryGetBits(1, out uint _);

        bool success = stream.TryGetBits(1, out uint _);

        Assert.False(success);
    }

    [Fact]
    public void TryGetBits_WithAlternatingBitPattern_ShouldExtractCorrectly()
    {
        BitStream stream = new(0b1010101010101010UL, 32);

        Assert.True(stream.TryGetBits(1, out uint bits1));
        Assert.Equal(0U, bits1);

        Assert.True(stream.TryGetBits(3, out uint bits2));
        Assert.Equal(0b101U, bits2);

        Assert.True(stream.TryGetBits(8, out uint bits3));
        Assert.Equal(0b10101010U, bits3);

        Assert.Equal(20u, stream.Bits);

        Assert.True(stream.TryGetBits(20, out uint bits4));
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

        bool success = stream.TryGetBits(64, out ulong bits);

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

        bool success = stream.TryGetBits(40, out ulong bits);

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

        Assert.True(stream.TryGetBits(0, out ulong bits1));
        Assert.Equal(0UL, bits1);

        Assert.True(stream.TryGetBits(32, out ulong bits2));
        Assert.Equal(expected40Bits & 0xFFFFFFFFUL, bits2);

        Assert.True(stream.TryGetBits(0, out ulong bits3));
        Assert.Equal(0UL, bits3);
        Assert.Equal(0u, stream.Bits);
    }

    // Regression: a zero-bit read used to leak the high half of the buffer into the low half
    // (`this.high << 64` masks to `<< 0`, so `low |= high`), corrupting all subsequent reads.
    [Fact]
    public void TryGetBits_WithZeroBits_ShouldNotCorruptLowFromHigh()
    {
        // Low half is all zeros, high half has a distinctive pattern.
        BitStream stream = new(new UInt128(0xAAAAAAAAAAAAAAAAUL, 0UL), dataSize: 128);

        Assert.True(stream.TryGetBits(0, out ulong zero));
        Assert.Equal(0UL, zero);

        // The next 64 bits should still be the original low half (0), not polluted by high.
        Assert.True(stream.TryGetBits(64, out ulong low));
        Assert.Equal(0UL, low);

        // And the remaining 64 bits should be the original high half untouched.
        Assert.True(stream.TryGetBits(64, out ulong high));
        Assert.Equal(0xAAAAAAAAAAAAAAAAUL, high);
        Assert.Equal(0u, stream.Bits);
    }

    // Regression: reading exactly 128 bits used to leave `low = high` instead of zeroing both halves
    // (`this.high >> 64` masks to `>> 0`). Only observable after writing new bits back.
    [Fact]
    public void TryGetBits_WithFullBuffer_ShouldZeroBothHalvesAfterRead()
    {
        BitStream stream = new(new UInt128(0xDEADBEEFDEADBEEFUL, 0xCAFEBABECAFEBABEUL), dataSize: 128);

        Assert.True(stream.TryGetBits(128, out UInt128 all));
        Assert.Equal(new UInt128(0xDEADBEEFDEADBEEFUL, 0xCAFEBABECAFEBABEUL), all);
        Assert.Equal(0u, stream.Bits);

        // Push 8 bits; read them back. Stale data in `low` would OR into the new value.
        stream.PutBits(0x3CU, 8);
        Assert.Equal(8u, stream.Bits);
        Assert.True(stream.TryGetBits(8, out uint roundTrip));
        Assert.Equal(0x3CU, roundTrip);
    }

    [Fact]
    public void PutBits_WithSmallValues_ShouldAccumulateCorrectly()
    {
        BitStream stream = default;

        stream.PutBits(0U, 1);
        stream.PutBits(0b11U, 2);

        Assert.Equal(3u, stream.Bits);
        Assert.True(stream.TryGetBits(3, out uint bits));
        Assert.Equal(0b110U, bits);
    }

    [Fact]
    public void PutBits_With64BitsOfOnes_ShouldStoreCorrectly()
    {
        const ulong allBits = 0xFFFFFFFFFFFFFFFFUL;
        BitStream stream = default;

        stream.PutBits(allBits, 64);

        Assert.Equal(64u, stream.Bits);
        Assert.True(stream.TryGetBits(64, out ulong bits));
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

        Assert.True(stream.TryGetBits(40, out ulong bits));
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

        Assert.True(stream.TryGetBits(32, out ulong bits));
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

        Assert.True(stream.TryGetBits(3, out uint retrieved1));
        Assert.Equal(value1, retrieved1);
        Assert.True(stream.TryGetBits(8, out uint retrieved2));
        Assert.Equal(value2, retrieved2);
    }
}
