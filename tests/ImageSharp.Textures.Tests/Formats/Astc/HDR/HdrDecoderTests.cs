// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

public class HdrDecoderTests
{
    [Fact]
    public void DecompressToFloat16_WithValidBlock_ShouldProduceCorrectOutputSize()
    {
        // Create a simple 4x4 block (16 bytes)
        var astcData = new byte[16];

        var footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        // Decompress using HDR API
        var hdrResult = AstcDecoder.DecompressHdrImage(astcData, 4, 4, footprint);

        // Verify output size: 4x4 pixels, 4 Half values (RGBA) per pixel
        hdrResult.Length.Should().Be(4 * 4 * 4); // 64 Half values total

        foreach (var value in hdrResult)
        {
            float.IsNaN(value).Should().BeFalse();
            float.IsInfinity(value).Should().BeFalse();

            // Values should be in reasonable range for normalized colors
            value.Should().BeGreaterThanOrEqualTo(0.0f);
            value.Should().BeLessThanOrEqualTo(1.1f); // Allow slight overshoot for HDR
        }
    }

    [Fact]
    public void DecompressToFloat16_WithDifferentFootprints_ShouldWork()
    {
        // Test that HDR API works with various footprint types
        var footprints = new[]
        {
            FootprintType.Footprint4x4,
            FootprintType.Footprint5x5,
            FootprintType.Footprint6x6,
            FootprintType.Footprint8x8
        };

        foreach (var footprint in footprints)
        {
            // Create a simple test: 1 block (footprint size) of zeros
            var fp = Footprint.FromFootprintType(footprint);
            var astcData = new byte[16]; // One ASTC block (all zeros = void extent block)

            var result = AstcDecoder.DecompressHdrImage(astcData, fp.Width, fp.Height, footprint);

            // Should produce footprint.Width * footprint.Height pixels, each with 4 Half values
            result.Length.Should().Be(fp.Width * fp.Height * 4);
        }
    }

    [Fact]
    public void ASTCDecompressToFloat16_WithInvalidData_ShouldReturnEmpty()
    {
        var emptyData = Array.Empty<byte>();

        var result = AstcDecoder.DecompressHdrImage(emptyData, 64, 64, FootprintType.Footprint4x4);

        result.Length.Should().Be(0);
    }

    [Fact]
    public void DecompressToFloat16_WithZeroDimensions_ShouldReturnEmpty()
    {
        var astcData = new byte[16];
        var footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        var result = AstcDecoder.DecompressHdrImage(astcData, 0, 0, footprint);

        result.Length.Should().Be(0);
    }

    [Fact]
    public void HdrColor_LdrRoundTrip_ShouldPreserveValues()
    {
        var ldrColor = new RgbaColor(50, 100, 150, 200);

        var hdrColor = RgbaHdrColor.FromRgba(ldrColor);
        var backToLdr = hdrColor.ToLowDynamicRange();

        backToLdr.R.Should().Be(ldrColor.R);
        backToLdr.G.Should().Be(ldrColor.G);
        backToLdr.B.Should().Be(ldrColor.B);
        backToLdr.A.Should().Be(ldrColor.A);
    }
}
