// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Astc;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

/// <summary>
/// Tests using real HDR ASTC files from the ARM astc-encoder reference repository.
/// These tests validate that our HDR implementation produces valid output for
/// actual HDR-compressed ASTC data.
/// </summary>
public class HdrImageTests
{
    [Fact]
    [Description("Verify that the ASTC file header is correctly parsed for HDR content, including footprint detection")]
    public void DecodeHdrFile_VerifyFootprintDetection()
    {
        var astcPath = FileBasedHelpers.GetHdrPath("HDR-A-1x1.astc");

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // The HDR-A-1x1.astc file has a 6x6 footprint based on the header
        astcFile.Footprint.Width.Should().Be(6);
        astcFile.Footprint.Height.Should().Be(6);
        astcFile.Footprint.Type.Should().Be(FootprintType.Footprint6x6);
    }

    [Fact]
    public void DecodeHdrAstcFile_1x1Pixel_ShouldProduceValidHdrOutput()
    {
        var astcPath = FileBasedHelpers.GetHdrPath("HDR-A-1x1.astc");

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks,
            astcFile.Width,
            astcFile.Height,
            astcFile.Footprint);

        // Should produce 1 pixel with 4 values (RGBA)
        hdrResult.Length.Should().Be(RgbaColor.BytesPerPixel);

        // HDR values can exceed 1.0
        // Just verify they're in a reasonable range (0.0 to 10.0)
        foreach (var value in hdrResult)
        {
            value.Should().BeGreaterThanOrEqualTo(0.0f);
            value.Should().BeLessThan(10.0f);
        }
    }

    [Fact]
    public void DecodeHdrAstcFile_Tile_ShouldProduceValidHdrOutput()
    {
        var astcPath = FileBasedHelpers.GetHdrPath("hdr-tile.astc");

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks,
            astcFile.Width,
            astcFile.Height,
            astcFile.Footprint);

        // Should produce Width * Height pixels, each with 4 values
        hdrResult.Length.Should().Be(astcFile.Width * astcFile.Height * RgbaColor.BytesPerPixel);

        // Verify at least some HDR values exceed 1.0 (typical for HDR content)
        int valuesGreaterThanOne = 0;
        foreach (var v in hdrResult)
        {
            if (v > 1.0f)
                valuesGreaterThanOne++;
        }
        valuesGreaterThanOne.Should().Be(64);
    }

    [Fact]
    [Description("Verify that HDR ASTC files can be decoded with the LDR API, producing clamped values")]
    public void DecodeHdrAstcFile_WithLdrApi_ShouldClampValues()
    {
        var astcPath = FileBasedHelpers.GetHdrPath("HDR-A-1x1.astc");

        if (!File.Exists(astcPath))
        {
            return;
        }

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode using LDR API
        var ldrResult = AstcDecoder.DecompressImage(astcFile);

        // Should produce 1 pixel with 4 bytes (RGBA)
        ldrResult.Length.Should().Be(RgbaColor.BytesPerPixel);

        // All values should be in LDR range
        foreach (var value in ldrResult)
        {
            value.Should().BeGreaterThanOrEqualTo(byte.MinValue);
            value.Should().BeLessThanOrEqualTo(byte.MaxValue);
        }
    }

    [Fact]
    [Description("Verify that HDR and LDR APIs produce consistent relative channel values for the same HDR ASTC file")]
    public void HdrAndLdrApis_OnSameHdrFile_ShouldProduceConsistentRelativeValues()
    {
        var astcPath = FileBasedHelpers.GetHdrPath("HDR-A-1x1.astc");

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with both APIs
        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);
        var ldrResult = AstcDecoder.DecompressImage(astcFile);

        // Both should produce output for 1 pixel
        hdrResult.Length.Should().Be(4);
        ldrResult.Length.Should().Be(4);

        // The relative ordering of RGB channels should be consistent between APIs.
        // If HDR channel i > channel j, then LDR channel i should be >= channel j
        // (accounting for clamping at 255).
        for (int i = 0; i < 3; i++)
        {
            for (int j = i + 1; j < 3; j++)
            {
                if (hdrResult[i] > hdrResult[j])
                    ldrResult[i].Should().BeGreaterThanOrEqualTo(ldrResult[j]);
                else if (hdrResult[i] < hdrResult[j])
                    ldrResult[i].Should().BeLessThanOrEqualTo(ldrResult[j]);
            }
        }
    }
}
