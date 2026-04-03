// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

public class HdrDecoderTests
{
    [Fact]
    public void DecompressToFloat16_WithValidBlock_ShouldProduceCorrectOutputSize()
    {
        // Create a simple 4x4 block (16 bytes)
        byte[] astcData = new byte[16];

        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        // Decompress using HDR API
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(astcData, 4, 4, footprint);

        // Verify output size: 4x4 pixels, 4 Half values (RGBA) per pixel
        Assert.Equal(4 * 4 * 4, hdrResult.Length); // 64 Half values total

        foreach (float value in hdrResult)
        {
            Assert.False(float.IsNaN(value));
            Assert.False(float.IsInfinity(value));

            // Values should be in reasonable range for normalized colors
            Assert.True(value >= 0.0f);
            Assert.True(value <= 1.1f); // Allow slight overshoot for HDR
        }
    }

    [Fact]
    public void DecompressToFloat16_WithDifferentFootprints_ShouldWork()
    {
        // Test that HDR API works with various footprint types
        FootprintType[] footprints =
        [
            FootprintType.Footprint4x4,
            FootprintType.Footprint5x5,
            FootprintType.Footprint6x6,
            FootprintType.Footprint8x8
        ];

        foreach (FootprintType footprint in footprints)
        {
            // Create a simple test: 1 block (footprint size) of zeros
            Footprint fp = Footprint.FromFootprintType(footprint);
            byte[] astcData = new byte[16]; // One ASTC block (all zeros = void extent block)

            Span<float> result = AstcDecoder.DecompressHdrImage(astcData, fp.Width, fp.Height, footprint);

            // Should produce footprint.Width * footprint.Height pixels, each with 4 Half values
            Assert.Equal(fp.Width * fp.Height * 4, result.Length);
        }
    }

    [Fact]
    public void ASTCDecompressToFloat16_WithInvalidData_ShouldReturnEmpty()
    {
        byte[] emptyData = [];

        Span<float> result = AstcDecoder.DecompressHdrImage(emptyData, 64, 64, FootprintType.Footprint4x4);

        Assert.Equal(0, result.Length);
    }

    [Fact]
    public void DecompressToFloat16_WithZeroDimensions_ShouldThrowArgumentOutOfRangeException()
    {
        byte[] astcData = new byte[16];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressHdrImage(astcData, 0, 0, footprint).ToArray());
    }

    [Fact]
    public void HdrColor_LdrRoundTrip_ShouldPreserveValues()
    {
        Rgba32 ldrColor = new(50, 100, 150, 200);

        Rgba64 hdrColor = new(ldrColor);
        Rgba32 backToLdr = hdrColor.ToRgba32();

        Assert.Equal(ldrColor.R, backToLdr.R);
        Assert.Equal(ldrColor.G, backToLdr.G);
        Assert.Equal(ldrColor.B, backToLdr.B);
        Assert.Equal(ldrColor.A, backToLdr.A);
    }
}
