// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class EndpointCodecTests
{
    [Theory]
    [InlineData(EndpointEncodingMode.DirectLuma)]
    [InlineData(EndpointEncodingMode.DirectLumaAlpha)]
    [InlineData(EndpointEncodingMode.BaseScaleRgb)]
    [InlineData(EndpointEncodingMode.BaseScaleRgba)]
    [InlineData(EndpointEncodingMode.DirectRbg)]
    [InlineData(EndpointEncodingMode.DirectRgba)]
    internal void EncodeColorsForMode_WithVariousRanges_ShouldProduceValidQuantizedValues(EndpointEncodingMode mode)
    {
        var low = new RgbaColor(0, 0, 0, 0);
        var high = new RgbaColor(255, 255, 255, 255);

        for (int quantRange = 5; quantRange < 256; quantRange++)
        {
            var values = new List<int>();
            EndpointEncoder.EncodeColorsForMode(low, high, quantRange, mode, out var _, values);

            // Assert value count matches expected
            values.Should().HaveCount(mode.GetValuesCount());

            // Assert all values are within quantization range
            values.Should().AllSatisfy(v => v.Should().BeInRange(0, quantRange));
        }
    }

    [Theory]
    [InlineData(EndpointEncodingMode.DirectLuma)]
    [InlineData(EndpointEncodingMode.DirectLumaAlpha)]
    [InlineData(EndpointEncodingMode.BaseScaleRgb)]
    [InlineData(EndpointEncodingMode.BaseScaleRgba)]
    [InlineData(EndpointEncodingMode.DirectRbg)]
    [InlineData(EndpointEncodingMode.DirectRgba)]
    internal void EncodeDecodeColors_WithBlackAndWhite_ShouldPreserveColors(EndpointEncodingMode mode)
    {
        var white = new RgbaColor(255, 255, 255, 255);
        var black = new RgbaColor(0, 0, 0, 255);

        for (int quantRange = 5; quantRange < 256; ++quantRange)
        {
            var (low, high) = EncodeAndDecodeColors(white, black, quantRange, mode);

            (low == white).Should().BeTrue();
            (high == black).Should().BeTrue();
        }
    }

    [Fact]
    public void UsesBlueContract_WithDirectModes_ShouldDetectCorrectly()
    {
        var values = new List<int> { 132, 127, 116, 112, 183, 180, 31, 22 };

        EndpointEncoder.UsesBlueContract(255, ColorEndpointMode.LdrRgbDirect, values).Should().BeTrue();
        EndpointEncoder.UsesBlueContract(255, ColorEndpointMode.LdrRgbaDirect, values).Should().BeTrue();
    }

    [Fact]
    public void UsesBlueContract_WithOffsetModes_ShouldDetectBasedOnBitFlags()
    {
        var baseValues = new List<int> { 132, 127, 116, 112, 183, 180, 31, 22 };

        var valuesClearedBit6 = new List<int>(baseValues);
        valuesClearedBit6[1] &= 0xBF;
        valuesClearedBit6[3] &= 0xBF;
        valuesClearedBit6[5] &= 0xBF;
        valuesClearedBit6[7] &= 0xBF;

        EndpointEncoder.UsesBlueContract(255, ColorEndpointMode.LdrRgbBaseOffset, valuesClearedBit6).Should().BeFalse();
        EndpointEncoder.UsesBlueContract(255, ColorEndpointMode.LdrRgbaBaseOffset, valuesClearedBit6).Should().BeFalse();

        var valuesSetBit6 = new List<int>(baseValues);
        valuesSetBit6[1] |= 0x40;
        valuesSetBit6[3] |= 0x40;
        valuesSetBit6[5] |= 0x40;
        valuesSetBit6[7] |= 0x40;

        EndpointEncoder.UsesBlueContract(255, ColorEndpointMode.LdrRgbBaseOffset, valuesSetBit6).Should().BeTrue();
        EndpointEncoder.UsesBlueContract(255, ColorEndpointMode.LdrRgbaBaseOffset, valuesSetBit6).Should().BeTrue();
    }

    [Fact]
    public void EncodeColorsForMode_WithRgbDirectAndSpecificPairs_ShouldUseBlueContract()
    {
        var pairs = new[]
        {
            (new RgbaColor(22, 18, 30, 59), new RgbaColor(162, 148, 155, 59)),
            (new RgbaColor(22, 30, 27, 36), new RgbaColor(228, 221, 207, 36)),
            (new RgbaColor(54, 60, 55, 255), new RgbaColor(23, 30, 27, 255))
        };

        const int endpointRange = 31;

        foreach (var (low, high) in pairs)
        {
            var values = new List<int>();
            EndpointEncoder.EncodeColorsForMode(low, high, endpointRange, EndpointEncodingMode.DirectRbg, out var astcMode, values);

            EndpointEncoder.UsesBlueContract(endpointRange, astcMode, values).Should().BeTrue();
        }
    }

    [Fact]
    public void EncodeDecodeColors_WithLumaDirect_ShouldProduceLumaValues()
    {
        var mode = EndpointEncodingMode.DirectLuma;

        var result1 = EncodeAndDecodeColors(
            new RgbaColor(247, 248, 246, 255),
            new RgbaColor(2, 3, 1, 255),
            255, mode);

        (result1.Low == new RgbaColor(247, 247, 247, 255)).Should().BeTrue();
        (result1.High == new RgbaColor(2, 2, 2, 255)).Should().BeTrue();

        var result2 = EncodeAndDecodeColors(
            new RgbaColor(80, 80, 50, 255),
            new RgbaColor(99, 255, 6, 255),
            255, mode);

        (result2.Low == new RgbaColor(70, 70, 70, 255)).Should().BeTrue();
        (result2.High == new RgbaColor(120, 120, 120, 255)).Should().BeTrue();

        var result3 = EncodeAndDecodeColors(
            new RgbaColor(247, 248, 246, 255),
            new RgbaColor(2, 3, 1, 255),
            15, mode);

        (result3.Low == new RgbaColor(255, 255, 255, 255)).Should().BeTrue();
        (result3.High == new RgbaColor(0, 0, 0, 255)).Should().BeTrue();

        var result4 = EncodeAndDecodeColors(
            new RgbaColor(64, 127, 192, 255),
            new RgbaColor(0, 0, 0, 255),
            63, mode);

        (result4.Low == new RgbaColor(130, 130, 130, 255)).Should().BeTrue();
        (result4.High == new RgbaColor(0, 0, 0, 255)).Should().BeTrue();
    }

    [Fact]
    public void EncodeDecodeColors_WithLumaAlphaDirect_ShouldPreserveLumaAndAlpha()
    {
        var mode = EndpointEncodingMode.DirectLumaAlpha;

        // Grey with varying alpha
        var result1 = EncodeAndDecodeColors(
            new RgbaColor(64, 127, 192, 127),
            new RgbaColor(0, 0, 0, 20),
            63, mode);

        ((result1.Low == new RgbaColor(130, 130, 130, 125)) ||
            result1.Low.IsCloseTo(new RgbaColor(130, 130, 130, 125), 1)).Should().BeTrue();
        ((result1.High == new RgbaColor(0, 0, 0, 20)) ||
            result1.High.IsCloseTo(new RgbaColor(0, 0, 0, 20), 1)).Should().BeTrue();

        // Different alpha values
        var result2 = EncodeAndDecodeColors(
            new RgbaColor(247, 248, 246, 250),
            new RgbaColor(2, 3, 1, 172),
            255, mode);

        (result2.Low == new RgbaColor(247, 247, 247, 250)).Should().BeTrue();
        (result2.High == new RgbaColor(2, 2, 2, 172)).Should().BeTrue();
    }

    [Fact]
    public void EncodeDecodeColors_WithRgbDirectAndRandomColors_ShouldPreserveColors()
    {
        var mode = EndpointEncodingMode.DirectRbg;
        var random = new Random(unchecked((int)0xdeadbeef));

        for (int i = 0; i < 100; ++i)
        {
            var low = new RgbaColor(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256), 255);
            var high = new RgbaColor(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256), 255);
            var (Low, High) = EncodeAndDecodeColors(low, high, 255, mode);

            (Low == low).Should().BeTrue();
            (High == high).Should().BeTrue();
        }
    }

    [Fact]
    public void EncodeDecodeColors_WithRgbDirectAndSpecificColors_ShouldMatchExpected()
    {
        var mode = EndpointEncodingMode.DirectRbg;

        var result1 = EncodeAndDecodeColors(
            new RgbaColor(64, 127, 192, 255),
            new RgbaColor(0, 0, 0, 255),
            63, mode);

        (result1.Low == new RgbaColor(65, 125, 190, 255)).Should().BeTrue();
        (result1.High == new RgbaColor(0, 0, 0, 255)).Should().BeTrue();

        var result2 = EncodeAndDecodeColors(
            new RgbaColor(0, 0, 0, 255),
            new RgbaColor(64, 127, 192, 255),
            63, mode);

        (result2.Low == new RgbaColor(0, 0, 0, 255)).Should().BeTrue();
        (result2.High == new RgbaColor(65, 125, 190, 255)).Should().BeTrue();
    }

    [Fact]
    public void EncodeDecodeColors_WithRgbBaseScaleAndIdenticalColors_ShouldBeCloseToOriginal()
    {
        var mode = EndpointEncodingMode.BaseScaleRgb;
        var random = new Random(unchecked((int)0xdeadbeef));

        for (int i = 0; i < 100; ++i)
        {
            var color = new RgbaColor(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256), 255);
            var result = EncodeAndDecodeColors(color, color, 255, mode);

            result.Low.IsCloseTo(color, 1).Should().BeTrue();
            result.High.IsCloseTo(color, 1).Should().BeTrue();
        }
    }

    [Fact]
    public void EncodeDecodeColors_WithRgbBaseScaleAndDifferentColors_ShouldMatchExpected()
    {
        var mode = EndpointEncodingMode.BaseScaleRgb;
        var low = new RgbaColor(20, 4, 40, 255);
        var high = new RgbaColor(80, 16, 160, 255);

        var result1 = EncodeAndDecodeColors(low, high, 255, mode);
        result1.Low.IsCloseTo(low, 0).Should().BeTrue();
        result1.High.IsCloseTo(high, 0).Should().BeTrue();

        var result2 = EncodeAndDecodeColors(low, high, 127, mode);
        result2.Low.IsCloseTo(low, 1).Should().BeTrue();
        result2.High.IsCloseTo(high, 1).Should().BeTrue();
    }

    public static IEnumerable<object[]> RgbBaseOffsetColorPairs()
    {
        yield return new object[] { new RgbaColor(80, 16, 112, 255), new RgbaColor(87, 18, 132, 255) };
        yield return new object[] { new RgbaColor(80, 74, 82, 255), new RgbaColor(90, 92, 110, 255) };
        yield return new object[] { new RgbaColor(0, 0, 0, 255), new RgbaColor(2, 2, 2, 255) };
    }

    [Theory]
    [MemberData(nameof(RgbBaseOffsetColorPairs))]
    internal void DecodeColorsForMode_WithRgbBaseOffset_AndSpecificColorPairs_ShouldDecodeCorrectly(
        RgbaColor expectedLow, RgbaColor expectedHigh)
    {
        var values = EncodeRgbBaseOffset(expectedLow, expectedHigh);
        var (decLow, decHigh) = EndpointCodec.DecodeColorsForMode(values, 255, ColorEndpointMode.LdrRgbBaseOffset);

        (decLow == expectedLow).Should().BeTrue();
        (decHigh == expectedHigh).Should().BeTrue();
    }

    [Fact]
    public void DecodeColorsForMode_WithRgbBaseOffset_AndIdenticalColors_ShouldDecodeCorrectly()
    {
        var random = new Random(unchecked((int)0xdeadbeef));

        for (int i = 0; i < 100; ++i)
        {
            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);

            // Ensure even channels (reference test skips odd)
            if (((r | g | b) & 1) != 0) continue;

            var color = new RgbaColor(r, g, b, 255);
            var values = EncodeRgbBaseOffset(color, color);
            var (decLow, decHigh) = EndpointCodec.DecodeColorsForMode(values, 255, ColorEndpointMode.LdrRgbBaseOffset);

            (decLow == color).Should().BeTrue();
            (decHigh == color).Should().BeTrue();
        }
    }

    private static int[] EncodeRgbBaseOffset(RgbaColor low, RgbaColor high)
    {
        var values = new List<int>();
        for (int i = 0; i < 3; ++i)
        {
            bool isLarge = low[i] >= 128;
            values.Add((low[i] * 2) & 0xFF);
            int diff = (high[i] - low[i]) * 2;
            if (isLarge) diff |= 0x80;
            values.Add(diff);
        }
        return values.ToArray();
    }

    [Fact]
    public void DecodeCheckerboard_ShouldDecodeToGrayscaleEndpoints()
    {
        string astcFilePath = FileBasedHelpers.GetInputPath("checkerboard.astc");
        byte[] astcData = File.ReadAllBytes(astcFilePath);

        int blocksDecoded = 0;

        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            // Read block bytes
            UInt128 blockData = BinaryPrimitives.ReadUInt128LittleEndian(astcData.AsSpan(i, PhysicalBlock.SizeInBytes));
            var physicalBlock = PhysicalBlock.Create(blockData);

            // Unpack to intermediate block
            var intermediateBlock = IntermediateBlock.UnpackIntermediateBlock(physicalBlock);
            intermediateBlock.Should().NotBeNull("checkerboard blocks should not be void extent");
            var ib = intermediateBlock!.Value;

            // Verify endpoints exist
            ib.EndpointCount.Should().BeGreaterThan(0, "block should have endpoints");

            int colorRange = IntermediateBlock.EndpointRangeForBlock(ib);
            colorRange.Should().BeGreaterThan(0, "color range should be valid");

            // Check all endpoint pairs decode successfully to grayscale colors
            for (int ep = 0; ep < ib.EndpointCount; ep++)
            {
                var endpoints = ib.Endpoints[ep];
                ReadOnlySpan<int> colorSpan = ((ReadOnlySpan<int>)endpoints.Colors)[..endpoints.ColorCount];
                var (low, high) = EndpointCodec.DecodeColorsForMode(
                    colorSpan,
                    colorRange,
                    endpoints.Mode);

                // Assert - Checkerboard should produce grayscale colors (R == G == B)
                low.R.Should().Be(low.G, $"block {i} low endpoint should be grayscale");
                low.G.Should().Be(low.B, $"block {i} low endpoint should be grayscale");
                high.R.Should().Be(high.G, $"block {i} high endpoint should be grayscale");
                high.G.Should().Be(high.B, $"block {i} high endpoint should be grayscale");
            }

            blocksDecoded++;
        }

        // Verify we decoded a reasonable number of blocks
        blocksDecoded.Should().BeGreaterThan(0, "should have decoded at least one block");
    }

    private static (RgbaColor Low, RgbaColor High) EncodeAndDecodeColors(
        RgbaColor low,
        RgbaColor high,
        int quantRange,
        EndpointEncodingMode mode)
    {
        var values = new List<int>();
        var needsSwap = EndpointEncoder.EncodeColorsForMode(low, high, quantRange, mode, out var astcMode, values);
        var (decLow, decHigh) = EndpointCodec.DecodeColorsForMode(values.ToArray(), quantRange, astcMode);

        return needsSwap ? (decHigh, decLow) : (decLow, decHigh);
    }
}
