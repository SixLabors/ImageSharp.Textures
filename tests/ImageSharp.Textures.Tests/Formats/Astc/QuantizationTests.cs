// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class QuantizationTests
{
    [Fact]
    public void QuantizeCEValueToRange_WithMaxValue_ShouldNotExceedRange()
    {
        for (int range = Quantization.EndpointRangeMinValue; range <= byte.MaxValue; range++)
        {
            Quantization.QuantizeCEValueToRange(byte.MaxValue, range).Should().BeLessThanOrEqualTo(range);
        }
    }

    [Fact]
    public void QuantizeWeightToRange_WithMaxValue_ShouldNotExceedRange()
    {
        for (int range = 1; range < Quantization.WeightRangeMaxValue; range++)
        {
            Quantization.QuantizeWeightToRange(64, range).Should().BeLessThanOrEqualTo(range);
        }
    }

    [Fact]
    public void QuantizeCEValueToRange_WithVariousValues_ShouldNotExceedRange()
    {
        int[] ranges = BoundedIntegerSequenceCodec.MaxRanges;
        int[] testValues = [0, 4, 15, 22, 66, 91, 126];

        foreach (int range in ranges.Where(r => r >= Quantization.EndpointRangeMinValue))
        {
            foreach (int value in testValues)
            {
                Quantization.QuantizeCEValueToRange(value, range).Should().BeLessThanOrEqualTo(range);
            }
        }
    }

    [Fact]
    public void QuantizeWeightToRange_WithVariousValues_ShouldNotExceedRange()
    {
        int[] ranges = BoundedIntegerSequenceCodec.MaxRanges;
        int[] testValues = [0, 4, 15, 22];

        foreach (int range in ranges.Where(r => r <= Quantization.WeightRangeMaxValue))
        {
            foreach (int value in testValues)
            {
                Quantization.QuantizeWeightToRange(value, range).Should().BeLessThanOrEqualTo(range);
            }
        }
    }

    [Fact]
    public void QuantizeWeight_ThenUnquantize_ShouldReturnOriginalQuantizedValue()
    {
        int[] ranges = BoundedIntegerSequenceCodec.MaxRanges;

        foreach (int range in ranges.Where(r => r <= Quantization.WeightRangeMaxValue))
        {
            for (int quantizedValue = 0; quantizedValue <= range; ++quantizedValue)
            {
                int unquantized = Quantization.UnquantizeWeightFromRange(quantizedValue, range);
                int requantized = Quantization.QuantizeWeightToRange(unquantized, range);

                requantized.Should().Be(quantizedValue);
            }
        }
    }

    [Fact]
    public void QuantizeCEValue_ThenUnquantize_ShouldReturnOriginalQuantizedValue()
    {
        int[] ranges = BoundedIntegerSequenceCodec.MaxRanges;

        foreach (int range in ranges.Where(r => r >= Quantization.EndpointRangeMinValue))
        {
            for (int quantizedValue = 0; quantizedValue <= range; ++quantizedValue)
            {
                int unquantized = Quantization.UnquantizeCEValueFromRange(quantizedValue, range);
                int requantized = Quantization.QuantizeCEValueToRange(unquantized, range);

                requantized.Should().Be(quantizedValue);
            }
        }
    }

    [Theory]
    [InlineData(2, 7)]
    [InlineData(7, 7)]
    [InlineData(39, 63)]
    [InlineData(66, 79)]
    [InlineData(91, 191)]
    [InlineData(126, 255)]
    [InlineData(255, 255)]
    public void UnquantizeCEValueFromRange_ShouldProduceValidByteValue(int quantizedValue, int range)
    {
        int result = Quantization.UnquantizeCEValueFromRange(quantizedValue, range);

        result.Should().BeLessThan(256);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 7)]
    [InlineData(7, 7)]
    [InlineData(29, 31)]
    public void UnquantizeWeightFromRange_ShouldNotExceed64(int quantizedValue, int range)
    {
        int result = Quantization.UnquantizeWeightFromRange(quantizedValue, range);

        result.Should().BeLessThanOrEqualTo(64);
    }

    [Fact]
    public void Quantize_WithDesiredRange_ShouldMatchExpectedRangeOutput()
    {
        int[] ranges = BoundedIntegerSequenceCodec.MaxRanges;
        int rangeIndex = 0;

        for (int desiredRange = 1; desiredRange <= byte.MaxValue; ++desiredRange)
        {
            while (rangeIndex + 1 < ranges.Length && ranges[rangeIndex + 1] <= desiredRange)
            {
                ++rangeIndex;
            }

            int expectedRange = ranges[rangeIndex];

            // Test CE values
            if (desiredRange >= Quantization.EndpointRangeMinValue)
            {
                int[] testValues = [0, 13, 173, 208, 255];
                foreach (int value in testValues)
                {
                    Quantization.QuantizeCEValueToRange(value, desiredRange)
                        .Should().Be(Quantization.QuantizeCEValueToRange(value, expectedRange));
                }
            }

            // Test weight values
            if (desiredRange <= Quantization.WeightRangeMaxValue)
            {
                int[] testValues = [0, 12, 23, 63];
                foreach (int value in testValues)
                {
                    Quantization.QuantizeWeightToRange(value, desiredRange)
                        .Should().Be(Quantization.QuantizeWeightToRange(value, expectedRange));
                }
            }
        }

        rangeIndex.Should().Be(ranges.Length - 1);
    }

    [Fact]
    public void QuantizeCEValueToRange_WithRangeByteMax_ShouldBeIdentity()
    {
        for (int value = byte.MinValue; value <= byte.MaxValue; value++)
        {
            Quantization.QuantizeCEValueToRange(value, byte.MaxValue).Should().Be(value);
        }
    }

    [Fact]
    public void QuantizeCEValueToRange_ShouldBeMonotonicIncreasing()
    {
        for (int numBits = 3; numBits < 8; numBits++)
        {
            int range = (1 << numBits) - 1;
            int lastQuantizedValue = -1;

            for (int value = byte.MinValue; value <= byte.MaxValue; value++)
            {
                int quantizedValue = Quantization.QuantizeCEValueToRange(value, range);

                quantizedValue.Should().BeGreaterThanOrEqualTo(lastQuantizedValue);
                lastQuantizedValue = quantizedValue;
            }

            lastQuantizedValue.Should().Be(range);
        }
    }

    [Fact]
    public void QuantizeWeightToRange_ShouldBeMonotonicallyIncreasing()
    {
        for (int numBits = 3; numBits < 8; ++numBits)
        {
            int range = (1 << numBits) - 1;

            if (range > Quantization.WeightRangeMaxValue)
            {
                continue;
            }

            int lastQuantizedValue = -1;

            for (int value = 0; value <= 64; ++value)
            {
                int quantizedValue = Quantization.QuantizeWeightToRange(value, range);

                quantizedValue.Should().BeGreaterThanOrEqualTo(lastQuantizedValue);
                lastQuantizedValue = quantizedValue;
            }

            lastQuantizedValue.Should().Be(range);
        }
    }

    [Fact]
    public void QuantizeCEValueToRange_WithSmallBitRanges_ShouldQuantizeLowValuesToZero()
    {
        for (int numBits = 1; numBits <= 8; ++numBits)
        {
            int range = (1 << numBits) - 1;

            if (range < Quantization.EndpointRangeMinValue)
            {
                continue;
            }

            const int cevBits = 8;
            int halfMaxQuantBits = Math.Max(0, cevBits - numBits - 1);
            int largestCevToZero = (1 << halfMaxQuantBits) - 1;

            Quantization.QuantizeCEValueToRange(largestCevToZero, range).Should().Be(0);
        }
    }

    [Fact]
    public void QuantizeWeightToRange_WithSmallBitRanges_ShouldQuantizeLowValuesToZero()
    {
        for (int numBits = 1; numBits <= 8; numBits++)
        {
            int range = (1 << numBits) - 1;

            if (range > Quantization.WeightRangeMaxValue)
            {
                continue;
            }

            const int weightBits = 6;
            int halfMaxQuantBits = Math.Max(0, weightBits - numBits - 1);
            int largestWeightToZero = (1 << halfMaxQuantBits) - 1;

            Quantization.QuantizeWeightToRange(largestWeightToZero, range).Should().Be(0);
        }
    }

    [Fact]
    public void UnquantizeWeightFromRange_WithQuintRange_ShouldMatchExpected()
    {
        List<int> values = [4, 6, 4, 6, 7, 5, 7, 5];
        List<int> quintExpected = [14, 21, 14, 21, 43, 50, 43, 50];

        List<int> quantized = [.. values.Select(v => Quantization.UnquantizeWeightFromRange(v, 9))];

        quantized.Should().Equal(quintExpected);
    }

    [Fact]
    public void UnquantizeWeightFromRange_WithTritRange_ShouldMatchExpected()
    {
        List<int> values = [4, 6, 4, 6, 7, 5, 7, 5];
        List<int> tritExpected = [5, 23, 5, 23, 41, 59, 41, 59];

        List<int> quantized = [.. values.Select(v => Quantization.UnquantizeWeightFromRange(v, 11))];

        quantized.Should().Equal(tritExpected);
    }

    [Fact]
    public void QuantizeCEValueToRange_WithInvalidMinRange_ShouldThrowArgumentOutOfRangeException()
    {
        for (int range = 0; range < Quantization.EndpointRangeMinValue; range++)
        {
            Action action = () => Quantization.QuantizeCEValueToRange(0, range);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    [Fact]
    public void UnquantizeCEValueFromRange_WithInvalidMinRange_ShouldThrowArgumentOutOfRangeException()
    {
        for (int range = 0; range < Quantization.EndpointRangeMinValue; range++)
        {
            Action action = () => Quantization.UnquantizeCEValueFromRange(0, range);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    [Fact]
    public void QuantizeWeightToRange_WithZeroRange_ShouldThrowArgumentOutOfRangeException()
    {
        Action action = () => Quantization.QuantizeWeightToRange(0, 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UnquantizeWeightFromRange_WithZeroRange_ShouldThrowArgumentOutOfRangeException()
    {
        Action action = () => Quantization.UnquantizeWeightFromRange(0, 0);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(256, 7)]
    [InlineData(10000, 17)]
    public void QuantizeCEValueToRange_WithInvalidValue_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.QuantizeCEValueToRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(8, 7)]
    [InlineData(-1000, 17)]
    public void UnquantizeCEValueFromRange_WithInvalidValue_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.UnquantizeCEValueFromRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, -7)]
    [InlineData(0, 257)]
    public void QuantizeCEValueToRange_WithInvalidRange_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.QuantizeCEValueToRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, -17)]
    [InlineData(0, 256)]
    public void UnquantizeCEValueFromRange_WithInvalidRange_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.UnquantizeCEValueFromRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(256, 7)]
    [InlineData(10000, 17)]
    public void QuantizeWeightToRange_WithInvalidValue_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.QuantizeWeightToRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(8, 7)]
    [InlineData(-1000, 17)]
    public void UnquantizeWeightFromRange_WithInvalidValue_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.UnquantizeWeightFromRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, -7)]
    [InlineData(0, 32)]
    public void QuantizeWeightToRange_WithInvalidRange_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.QuantizeWeightToRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, -17)]
    [InlineData(0, 64)]
    public void UnquantizeWeightFromRange_WithInvalidRange_ShouldThrowArgumentOutOfRangeException(int value, int range)
    {
        Action action = () => Quantization.UnquantizeWeightFromRange(value, range);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}
