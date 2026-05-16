// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class IntegerSequenceCodecTests
{
    [Fact]
    [Description("1 to 31 are the densest packing of valid encodings and those supported by the codec.")]
    public void GetPackingModeBitCount_ForValidRange_ShouldNotReturnUnknownMode()
    {
        for (int i = 1; i < 32; ++i)
        {
            (BiseEncodingMode mode, int _) = BoundedIntegerSequenceCodec.GetPackingModeBitCount(i);
            Assert.True(mode != BiseEncodingMode.Unknown, $"Range {i} should not yield Unknown encoding mode");
        }
    }

    [Fact]
    public void GetPackingModeBitCount_ForValidRange_ShouldMatchExpectedValues()
    {
        (BiseEncodingMode Mode, int BitCount)[] expected =
        [
            (BiseEncodingMode.BitEncoding, 1),    // Range 1
            (BiseEncodingMode.TritEncoding, 0),   // Range 2
            (BiseEncodingMode.BitEncoding, 2),    // Range 3
            (BiseEncodingMode.QuintEncoding, 0),  // Range 4
            (BiseEncodingMode.TritEncoding, 1),   // Range 5
            (BiseEncodingMode.BitEncoding, 3),    // Range 6
            (BiseEncodingMode.BitEncoding, 3),    // Range 7
            (BiseEncodingMode.QuintEncoding, 1),  // Range 8
            (BiseEncodingMode.QuintEncoding, 1),  // Range 9
            (BiseEncodingMode.TritEncoding, 2),   // Range 10
            (BiseEncodingMode.TritEncoding, 2),   // Range 11
            (BiseEncodingMode.BitEncoding, 4),    // Range 12
            (BiseEncodingMode.BitEncoding, 4),    // Range 13
            (BiseEncodingMode.BitEncoding, 4),    // Range 14
            (BiseEncodingMode.BitEncoding, 4),    // Range 15
            (BiseEncodingMode.QuintEncoding, 2),  // Range 16
            (BiseEncodingMode.QuintEncoding, 2),  // Range 17
            (BiseEncodingMode.QuintEncoding, 2),  // Range 18
            (BiseEncodingMode.QuintEncoding, 2),  // Range 19
            (BiseEncodingMode.TritEncoding, 3),   // Range 20
            (BiseEncodingMode.TritEncoding, 3),   // Range 21
            (BiseEncodingMode.TritEncoding, 3),   // Range 22
            (BiseEncodingMode.TritEncoding, 3),   // Range 23
            (BiseEncodingMode.BitEncoding, 5),    // Range 24
            (BiseEncodingMode.BitEncoding, 5),    // Range 25
            (BiseEncodingMode.BitEncoding, 5),    // Range 26
            (BiseEncodingMode.BitEncoding, 5),    // Range 27
            (BiseEncodingMode.BitEncoding, 5),    // Range 28
            (BiseEncodingMode.BitEncoding, 5),    // Range 29
            (BiseEncodingMode.BitEncoding, 5),    // Range 30
            (BiseEncodingMode.BitEncoding, 5) // Range 31
        ];

        for (int i = 1; i < 32; ++i)
        {
            (BiseEncodingMode mode, int bitCount) = BoundedIntegerSequenceCodec.GetPackingModeBitCount(i);
            (BiseEncodingMode expectedMode, int expectedBitCount) = expected[i - 1];

            Assert.True(mode == expectedMode, $"range {i} mode should match");
            Assert.True(bitCount == expectedBitCount, $"range {i} bit count should match");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    public void GetPackingModeBitCount_WithInvalidRange_ShouldThrowArgumentOutOfRangeException(int range)
    {
        Action action = () => BoundedIntegerSequenceCodec.GetPackingModeBitCount(range);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(63)]
    public void GetBitCount_WithBitEncodingMode1Bit_ShouldReturnValueCount(int valueCount)
    {
        int bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.BitEncoding, valueCount, 1);
        int bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 1);

        Assert.Equal(valueCount, bitCount);
        Assert.Equal(valueCount, bitCountForRange);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    [InlineData(32, 64)]
    public void GetBitCount_WithBitEncodingMode2Bits_ShouldReturnTwiceValueCount(int valueCount, int expected)
    {
        int bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.BitEncoding, valueCount, 2);
        int bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 3);

        Assert.Equal(expected, bitCount);
        Assert.Equal(expected, bitCountForRange);
    }

    [Fact]
    public void GetBitCount_WithTritEncoding15Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 15;
        const int bits = 3;
        int expectedBitCount = (8 * 3) + (15 * 3); // 69 bits

        int bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.TritEncoding, valueCount, bits);
        int bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 23);

        Assert.Equal(expectedBitCount, bitCount);
        Assert.Equal(bitCount, bitCountForRange);
    }

    [Fact]
    public void GetBitCount_WithTritEncoding13Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 13;
        const int bits = 2;
        const int expectedBitCount = 47;

        int bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.TritEncoding, valueCount, bits);
        int bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 11);

        Assert.Equal(expectedBitCount, bitCount);
        Assert.Equal(bitCount, bitCountForRange);
    }

    [Fact]
    public void GetBitCount_WithQuintEncoding6Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 6;
        const int bits = 4;
        int expectedBitCount = (7 * 2) + (6 * 4);  // 38 bits

        int bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.QuintEncoding, valueCount, bits);
        int bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 79);

        Assert.Equal(expectedBitCount, bitCount);
        Assert.Equal(bitCount, bitCountForRange);
    }

    [Fact]
    public void GetBitCount_WithQuintEncoding7Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 7;
        const int bits = 3;
        int expectedBitCount = (7 * 2) + // First two quint blocks
                       (6 * 3) + // First two blocks of bits
                       3 + // Last quint block without high order four bits
                       3;       // Last block with one set of three bits

        int bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.QuintEncoding, valueCount, bits);

        Assert.Equal(expectedBitCount, bitCount);
    }

    [Fact]
    public void Decode_WithKnownQuintEncoding_ShouldProduceExpectedValues()
    {
        const int valueRange = 79;
        const ulong encoding = 0x4A7D3UL;
        int[] expectedValues = [3, 79, 37];

        BitStream bitSrc = new(encoding, 19);
        int[] decoded = Decode(valueRange, expectedValues.Length, ref bitSrc);

        Assert.Equal(expectedValues, decoded);
    }

    [Fact]
    public void Decode_WithKnownQuintEncodingMultiBlock_ShouldProduceExpectedValues()
    {
        int[] expectedValues = [16, 18, 17, 4, 7, 14, 10, 0];
        const ulong encoding = 0x2b9c83dc;
        const int range = 19;

        BitStream bitSrc = new(encoding, 64);
        int[] decoded = Decode(range, expectedValues.Length, ref bitSrc);

        Assert.Equal(expectedValues.Length, decoded.Length);
        Assert.Equal(expectedValues, decoded);
    }

    [Fact]
    public void Decode_WithKnownTritEncoding_ShouldProduceExpectedValues()
    {
        const int valueRange = 11;
        const ulong encoding = 0x37357UL;
        int[] expectedValues = [7, 5, 3, 6, 10];

        BitStream bitSrc = new(encoding, 19);
        int[] decoded = Decode(valueRange, expectedValues.Length, ref bitSrc);

        Assert.Equal(expectedValues, decoded);
    }

    [Fact]
    public void Decode_WithKnownTritEncodingMultiBlock_ShouldProduceExpectedValues()
    {
        int[] expectedValues = [6, 0, 0, 2, 0, 0, 0, 0, 8, 0, 0, 0, 0, 8, 8, 0];
        const ulong encoding = 0x0004c0100001006UL;
        const int range = 11;

        BitStream bitSrc = new(encoding, 64);
        int[] decoded = Decode(range, expectedValues.Length, ref bitSrc);

        Assert.Equal(expectedValues.Length, decoded.Length);
        Assert.Equal(expectedValues, decoded);
    }

    /// <summary>
    /// Test helper: BISE-decodes a sequence by range, mirroring the convenience overload
    /// that production no longer needs. Production paths already have the BISE
    /// (encoding, bitCount) pair from <see cref="BoundedIntegerSequenceCodec.GetPackingModeBitCount"/>
    /// and call <see cref="BoundedIntegerSequenceDecoder.Decode"/> directly.
    /// </summary>
    private static int[] Decode(int range, int valuesCount, ref BitStream bitSource)
    {
        (BiseEncodingMode encoding, int bitCount) = BoundedIntegerSequenceCodec.GetPackingModeBitCount(range);
        int[] result = new int[valuesCount];
        BoundedIntegerSequenceDecoder.Decode(encoding, bitCount, valuesCount, ref bitSource, result);
        return result;
    }
}
