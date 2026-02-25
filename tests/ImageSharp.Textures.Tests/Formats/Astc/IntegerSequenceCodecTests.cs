// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class IntegerSequenceCodecTests
{
    [Fact]
    [Description("1 to 31 are the densest packing of valid encodings and those supported by the codec.")]
    public void GetPackingModeBitCount_ForValidRange_ShouldNotReturnUnknownMode()
    {
        for (int i = 1; i < 32; ++i)
        {
            var (mode, _) = BoundedIntegerSequenceCodec.GetPackingModeBitCount(i);
            mode.Should().NotBe(BiseEncodingMode.Unknown, $"Range {i} should not yield Unknown encoding mode");
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
            var (mode, bitCount) = BoundedIntegerSequenceCodec.GetPackingModeBitCount(i);
            var (expectedMode, expectedBitCount) = expected[i - 1];

            mode.Should().Be(expectedMode, $"range {i} mode should match");
            bitCount.Should().Be(expectedBitCount, $"range {i} bit count should match");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    public void GetPackingModeBitCount_WithInvalidRange_ShouldThrowArgumentOutOfRangeException(int range)
    {
        var action = () => BoundedIntegerSequenceCodec.GetPackingModeBitCount(range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(32)]
    [InlineData(63)]
    public void GetBitCount_WithBitEncodingMode1Bit_ShouldReturnValueCount(int valueCount)
    {
        var bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.BitEncoding, valueCount, 1);
        var bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 1);

        bitCount.Should().Be(valueCount);
        bitCountForRange.Should().Be(valueCount);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    [InlineData(32, 64)]
    public void GetBitCount_WithBitEncodingMode2Bits_ShouldReturnTwiceValueCount(int valueCount, int expected)
    {
        var bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.BitEncoding, valueCount, 2);
        var bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 3);

        bitCount.Should().Be(expected);
        bitCountForRange.Should().Be(expected);
    }

    [Fact]
    public void GetBitCount_WithTritEncoding15Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 15;
        const int bits = 3;
        int expectedBitCount = 8 * 3 + 15 * 3; // 69 bits

        var bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.TritEncoding, valueCount, bits);
        var bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 23);

        bitCount.Should().Be(expectedBitCount);
        bitCountForRange.Should().Be(bitCount);
    }

    [Fact]
    public void GetBitCount_WithTritEncoding13Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 13;
        const int bits = 2;
        const int expectedBitCount = 47;

        var bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.TritEncoding, valueCount, bits);
        var bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 11);

        bitCount.Should().Be(expectedBitCount);
        bitCountForRange.Should().Be(bitCount);
    }

    [Fact]
    public void GetBitCount_WithQuintEncoding6Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 6;
        const int bits = 4;
        int expectedBitCount = 7 * 2 + 6 * 4;  // 38 bits

        var bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.QuintEncoding, valueCount, bits);
        var bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, 79);

        bitCount.Should().Be(expectedBitCount);
        bitCountForRange.Should().Be(bitCount);
    }

    [Fact]
    public void GetBitCount_WithQuintEncoding7Values_ShouldReturnExpectedBitCount()
    {
        const int valueCount = 7;
        const int bits = 3;
        int expectedBitCount = 7 * 2 + // First two quint blocks
                       6 * 3 + // First two blocks of bits
                       3 + // Last quint block without high order four bits
                       3;       // Last block with one set of three bits

        var bitCount = BoundedIntegerSequenceCodec.GetBitCount(BiseEncodingMode.QuintEncoding, valueCount, bits);

        bitCount.Should().Be(expectedBitCount);
    }

    [Fact]
    public void EncodeDecode_WithQuintValues_ShouldEncodeAndDecodeExpectedValues()
    {
        const int valueRange = 79;
        var encoder = new BoundedIntegerSequenceEncoder(valueRange);
        var values = new[] { 3, 79, 37 };

        foreach (var value in values)
            encoder.AddValue(value);

        // Encode
        var bitSink = default(BitStream);
        encoder.Encode(ref bitSink);

        // Verify encoded data
        bitSink.Bits.Should().Be(19);
        bitSink.TryGetBits<ulong>(19, out var encoded).Should().BeTrue();
        encoded.Should().Be(0x4A7D3UL);

        // Decode
        var bitSrc = new BitStream(encoded, 19);
        var decoder = new BoundedIntegerSequenceDecoder(valueRange);
        var decoded = decoder.Decode(3, ref bitSrc);

        decoded.Should().Equal(values);
    }

    [Fact]
    public void DecodeThenEncode_WithQuintValues_ShouldPreserveEncoding()
    {
        var expectedValues = new[] { 16, 18, 17, 4, 7, 14, 10, 0 };
        const ulong encoding = 0x2b9c83dc;
        const int range = 19;

        // Decode
        var bitSrc = new BitStream(encoding, 64);
        var decoder = new BoundedIntegerSequenceDecoder(range);
        var decoded = decoder.Decode(expectedValues.Length, ref bitSrc);

        // Check decoded values
        decoded.Should().HaveCount(expectedValues.Length);
        decoded.Should().Equal(expectedValues);

        // Re-encode
        var bitSink = default(BitStream);
        var encoder = new BoundedIntegerSequenceEncoder(range);
        foreach (var value in expectedValues)
            encoder.AddValue(value);
        encoder.Encode(ref bitSink);

        // Re-encoded should match original
        bitSink.Bits.Should().Be(35);
        bitSink.TryGetBits<ulong>(35, out var reencoded).Should().BeTrue();
        reencoded.Should().Be(encoding);
    }

    [Fact]
    public void EncodeDecode_WithTritValues_ShouldEncodeAndDecodeExpectedValues()
    {
        const int valueRange = 11;
        var encoder = new BoundedIntegerSequenceEncoder(valueRange);
        var values = new[] { 7, 5, 3, 6, 10 };

        foreach (var value in values)
            encoder.AddValue(value);

        // Encode
        var bitSink = default(BitStream);
        encoder.Encode(ref bitSink);

        // Verify encoded data
        bitSink.Bits.Should().Be(18);
        bitSink.TryGetBits<ulong>(18, out var encoded).Should().BeTrue();
        encoded.Should().Be(0x37357UL);

        // Decode
        var bitSrc = new BitStream(encoded, 19);
        var decoder = new BoundedIntegerSequenceDecoder(valueRange);
        var decoded = decoder.Decode(5, ref bitSrc);

        decoded.Should().Equal(values);
    }

    [Fact]
    public void DecodeThenEncode_WithTritValues_ShouldPreserveEncoding()
    {
        var expectedValues = new[] { 6, 0, 0, 2, 0, 0, 0, 0, 8, 0, 0, 0, 0, 8, 8, 0 };
        const ulong encoding = 0x0004c0100001006UL;
        const int range = 11;

        // Decode
        var bitSrc = new BitStream(encoding, 64);
        var decoder = new BoundedIntegerSequenceDecoder(range);
        var decoded = decoder.Decode(expectedValues.Length, ref bitSrc);

        // Check decoded values
        decoded.Should().HaveCount(expectedValues.Length);
        decoded.Should().Equal(expectedValues);

        // Re-encode
        var bitSink = default(BitStream);
        var encoder = new BoundedIntegerSequenceEncoder(range);
        foreach (var value in expectedValues)
            encoder.AddValue(value);
        encoder.Encode(ref bitSink);

        // Assert re-encoded matches original
        bitSink.Bits.Should().Be(58);
        bitSink.TryGetBits<ulong>(58, out var reencoded).Should().BeTrue();
        reencoded.Should().Be(encoding);
    }

    [Fact]
    public void EncodeDecode_WithRandomValues_ShouldAlwaysRoundTripCorrectly()
    {
        var random = new Random(unchecked(0xbad7357));
        const int testCount = 1600;

        for (int test = 0; test < testCount; test++)
        {
            int valueCount = 4 + random.Next(0, 256) % 44;
            int range = 1 + random.Next(0, 256) % 63;

            int bitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(valueCount, range);
            if (bitCount >= 64)
                continue;

            // Generate random values
            var generated = new List<int>(valueCount);
            for (int i = 0; i < valueCount; i++)
                generated.Add(random.Next(range + 1));

            // Encode
            var bitSink = default(BitStream);
            var encoder = new BoundedIntegerSequenceEncoder(range);
            foreach (var value in generated)
                encoder.AddValue(value);

            encoder.Encode(ref bitSink);

            bitSink.TryGetBits<ulong>((int)bitSink.Bits, out var encoded).Should().BeTrue();

            // Decode
            var bitSrc = new BitStream(encoded, 64);
            var decoder = new BoundedIntegerSequenceDecoder(range);
            var decoded = decoder.Decode(valueCount, ref bitSrc);

            decoded.Should().HaveCount(generated.Count);
            decoded.Should().Equal(generated);
        }
    }
}
