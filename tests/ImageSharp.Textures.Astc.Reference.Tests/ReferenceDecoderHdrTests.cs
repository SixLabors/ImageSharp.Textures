// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;
using SixLabors.ImageSharp.Textures.Astc.Reference.Tests.Utils;

namespace SixLabors.ImageSharp.Textures.Astc.Reference.Tests;

/// <summary>
/// HDR comparison tests between SixLabors.ImageSharp.Textures.Astc and the ARM reference ASTC decoder.
/// These validate that SixLabors.ImageSharp.Textures.Astc produces HDR output matching the official ARM implementation.
/// </summary>
public class ReferenceDecoderHdrTests
{
    public static TheoryData<FootprintType> AllFootprintTypes =>
        new()
        {
            FootprintType.Footprint4x4,
            FootprintType.Footprint5x4,
            FootprintType.Footprint5x5,
            FootprintType.Footprint6x5,
            FootprintType.Footprint6x6,
            FootprintType.Footprint8x5,
            FootprintType.Footprint8x6,
            FootprintType.Footprint8x8,
            FootprintType.Footprint10x5,
            FootprintType.Footprint10x6,
            FootprintType.Footprint10x8,
            FootprintType.Footprint10x10,
            FootprintType.Footprint12x10,
            FootprintType.Footprint12x12,
        };

    [Theory]
    [InlineData("HDR-A-1x1")]
    [InlineData("hdr-tile")]
    [InlineData("LDR-A-1x1")]
    [InlineData("ldr-tile")]
    public void DecompressHdr_WithHdrImage_ShouldMatch(string basename)
    {
        var filePath = Path.Combine("TestData", "HDR", basename + ".astc");

        var bytes = File.ReadAllBytes(filePath);
        var astcFile = AstcFile.FromMemory(bytes);
        var (blockX, blockY) = ReferenceDecoder.ToBlockDimensions(astcFile.Footprint.Type);

        var expected = ReferenceDecoder.DecompressHdr(
            astcFile.Blocks, astcFile.Width, astcFile.Height, blockX, blockY);
        var actual = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        CompareF16(actual, expected, astcFile.Width, astcFile.Height, basename);
    }

    [Theory]
    [InlineData("atlas_small_4x4")]
    [InlineData("atlas_small_5x5")]
    [InlineData("atlas_small_6x6")]
    [InlineData("atlas_small_8x8")]
    public void DecompressHdr_WithLdrImage_ShouldMatch(string basename)
    {
        var filePath = Path.Combine("TestData", "Input", basename + ".astc");
        var bytes = File.ReadAllBytes(filePath);
        var astcFile = AstcFile.FromMemory(bytes);
        var (blockX, blockY) = ReferenceDecoder.ToBlockDimensions(astcFile.Footprint.Type);

        var expected = ReferenceDecoder.DecompressHdr(
            astcFile.Blocks, astcFile.Width, astcFile.Height, blockX, blockY);
        var actual = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        CompareF16(actual, expected, astcFile.Width, astcFile.Height, basename);
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    public void DecompressHdr_SolidColor_ShouldMatch(FootprintType footprintType)
    {
        var (blockX, blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);
        int width = blockX;
        int height = blockY;

        // Single block: R=G=B=2.0, A=1.0 (above LDR range)
        var pixels = new Half[width * height * 4];
        for (int index = 0; index < width * height; index++)
        {
            pixels[index * 4 + 0] = (Half)2.0f;
            pixels[index * 4 + 1] = (Half)2.0f;
            pixels[index * 4 + 2] = (Half)2.0f;
            pixels[index * 4 + 3] = (Half)1.0f;
        }

        var compressed = ReferenceDecoder.CompressHdr(pixels, width, height, blockX, blockY);
        var footprint = Footprint.FromFootprintType(footprintType);

        var expected = ReferenceDecoder.DecompressHdr(compressed, width, height, blockX, blockY);
        var actual = AstcDecoder.DecompressHdrImage(compressed, width, height, footprint);

        CompareF16(actual, expected, width, height, $"BrightSolid_{footprintType}");
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    public void DecompressHdr_Gradient_ShouldMatch(FootprintType footprintType)
    {
        var (blockX, blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);
        // 2×2 blocks for HDR gradient
        int width = blockX * 2;
        int height = blockY * 2;

        // Gradient from 0.0 to 4.0
        var pixels = new Half[width * height * 4];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int idx = (row * width + col) * 4;
                float fraction = (float)(row * width + col) / (width * height - 1);
                float value = fraction * 4.0f;
                pixels[idx + 0] = (Half)value;
                pixels[idx + 1] = (Half)value;
                pixels[idx + 2] = (Half)value;
                pixels[idx + 3] = (Half)1.0f;
            }
        }

        var compressed = ReferenceDecoder.CompressHdr(pixels, width, height, blockX, blockY);
        var footprint = Footprint.FromFootprintType(footprintType);

        var expected = ReferenceDecoder.DecompressHdr(compressed, width, height, blockX, blockY);
        var actual = AstcDecoder.DecompressHdrImage(compressed, width, height, footprint);

        CompareF16(actual, expected, width, height, $"HdrGradient_{footprintType}");
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    [Description("In ASTC, the encoder picks the best endpoint mode per block. A single image can have some blocks" +
        " encoded with LDR modes and others with HDR modes, the encoder optimizes each block independently.")]
    public void DecompressHdr_MixedLdrHdr_ShouldMatch(FootprintType footprintType)
    {
        var (blockX, blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);
        // 2×2 blocks
        int width = blockX * 2;
        int height = blockY * 2;
        int halfWidth = width / 2;

        var pixels = new Half[width * height * 4];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int idx = (row * width + col) * 4;
                if (col < halfWidth)
                {
                    // LDR left half: values in 0.0-1.0
                    float fraction = (float)row / (height - 1);
                    pixels[idx + 0] = (Half)(fraction * 0.8f);
                    pixels[idx + 1] = (Half)(fraction * 0.5f);
                    pixels[idx + 2] = (Half)(fraction * 0.3f);
                }
                else
                {
                    // HDR right half: values above 1.0
                    float fraction = (float)row / (height - 1);
                    pixels[idx + 0] = (Half)(1.0f + fraction * 3.0f);
                    pixels[idx + 1] = (Half)(0.5f + fraction * 2.0f);
                    pixels[idx + 2] = (Half)(0.2f + fraction * 1.5f);
                }
                pixels[idx + 3] = (Half)1.0f;
            }
        }

        var compressed = ReferenceDecoder.CompressHdr(pixels, width, height, blockX, blockY);
        var footprint = Footprint.FromFootprintType(footprintType);

        var expected = ReferenceDecoder.DecompressHdr(compressed, width, height, blockX, blockY);
        var actual = AstcDecoder.DecompressHdrImage(compressed, width, height, footprint);

        CompareF16(actual, expected, width, height, $"MixedLdrHdr_{footprintType}");
    }

    /// <summary>
    /// Compare float output from SixLabors.ImageSharp.Textures.Astc against FP16 output from the ARM reference decoder.
    /// SixLabors.ImageSharp.Textures.Astc outputs float values (bit-cast from FP16 for HDR, normalized for LDR).
    /// The ARM reference outputs raw FP16 Half values which are converted to float for comparison.
    /// </summary>
    private static void CompareF16(Span<float> actual, Half[] expected, int width, int height, string label)
    {
        int channelCount = width * height * RgbaColor.BytesPerPixel;
        actual.Length.Should().Be(channelCount, because: $"actual float output size should match for {label}");
        expected.Length.Should().Be(channelCount, because: $"expected F16 output size should match for {label}");

        int mismatches = 0;
        float worstRelDiff = 0;
        int worstPixel = -1;
        int worstChannel = -1;

        for (int index = 0; index < channelCount; index++)
        {
            float actualValue = actual[index];
            float expectedValue = (float)expected[index];

            // Both NaN == match; one NaN == mismatch
            if (float.IsNaN(actualValue) && float.IsNaN(expectedValue))
                continue;
            if (float.IsNaN(actualValue) || float.IsNaN(expectedValue))
            {
                mismatches++;
                continue;
            }

            float absDiff = MathF.Abs(actualValue - expectedValue);
            float maxVal = MathF.Max(MathF.Abs(actualValue), MathF.Max(MathF.Abs(expectedValue), 1e-6f));
            float relDiff = absDiff / maxVal;

            // Use a relative tolerance of 0.1% plus absolute tolerance of one FP16 ULP (~0.001 for values near 1.0)
            if (absDiff > 0.001f && relDiff > 0.001f)
            {
                mismatches++;
                if (relDiff > worstRelDiff)
                {
                    worstRelDiff = relDiff;
                    worstPixel = index / 4;
                    worstChannel = index % 4;
                }
            }
        }

        if (mismatches > 0)
        {
            string channelName = worstChannel switch { 0 => "R", 1 => "G", 2 => "B", _ => "A" };
            int pixelX = worstPixel % width;
            int pixelY = worstPixel / width;
            Assert.Fail(
                $"[{label}] {mismatches}/{channelCount} F16 channel mismatches. " +
                $"Worst: pixel ({pixelX},{pixelY}) channel {channelName}, " +
                $"actual={actual[worstPixel * 4 + worstChannel]:G5} vs " +
                $"expected={(float)expected[worstPixel * 4 + worstChannel]:G5} " +
                $"(relDiff={worstRelDiff:P2}).");
        }
    }
}
