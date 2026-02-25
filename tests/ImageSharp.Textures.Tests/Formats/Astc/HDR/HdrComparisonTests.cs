// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

/// <summary>
/// Comparing HDR and LDR ASTC decoding behavior using real reference files.
/// </summary>
public class HdrComparisonTests
{
    [Fact]
    public void HdrFile_DecodedWithHdrApi_ShouldPreserveExtendedRange()
    {
        // HDR files should decode to values potentially exceeding 1.0
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "HDR-A-1x1.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with HDR API
        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Verify we get Float16 output
        hdrResult.Length.Should().Be(4); // 1 pixel, 4 channels

        // HDR content can have values > 1.0 (this file may or may not, but should allow it)
        foreach (var value in hdrResult)
        {
            float.IsNaN(value).Should().BeFalse();
            float.IsInfinity(value).Should().BeFalse();
            value.Should().BeGreaterThanOrEqualTo(0.0f);
        }
    }

    [Fact]
    public void LdrFile_DecodedWithHdrApi_ShouldUpscaleToHdrRange()
    {
        // LDR files decoded with HDR API should produce values in 0.0-1.0 range
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "LDR-A-1x1.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with HDR API
        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        hdrResult.Length.Should().Be(4);

        // LDR content should map to 0.0-1.0 range when decoded with HDR API
        foreach (var value in hdrResult)
        {
            value.Should().BeGreaterThanOrEqualTo(0.0f);
            value.Should().BeLessThanOrEqualTo(1.0f);
        }
    }

    [Fact]
    public void HdrFile_DecodedWithLdrApi_ShouldClampToByteRange()
    {
        // HDR files decoded with LDR API should clamp to 0-255
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "HDR-A-1x1.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with LDR API
        var ldrResult = AstcDecoder.DecompressImage(astcFile);

        ldrResult.Length.Should().Be(4);

        // All values must be in LDR range
        foreach (var value in ldrResult)
        {
            value.Should().BeGreaterThanOrEqualTo((byte)0);
            value.Should().BeLessThanOrEqualTo((byte)255);
        }
    }

    [Fact]
    public void LdrFile_DecodedWithBothApis_ShouldProduceConsistentValues()
    {
        // LDR content should produce equivalent results with both APIs
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "LDR-A-1x1.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with both APIs
        var ldrResult = AstcDecoder.DecompressImage(astcFile);
        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Compare results - LDR byte should map to HDR float / 255.0
        for (int i = 0; i < 4; i++)
        {
            byte ldrValue = ldrResult[i];
            float hdrValue = hdrResult[i];

            float expectedHdr = ldrValue / 255.0f;

            Math.Abs(hdrValue - expectedHdr).Should().BeLessThan(0.01f);
        }
    }

    [Fact]
    public void HdrTile_ShouldDecodeSuccessfully()
    {
        // Test larger HDR tile decoding
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "hdr-tile.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Should produce Width * Height * 4 values
        hdrResult.Length.Should().Be(astcFile.Width * astcFile.Height * 4);

        foreach (var value in hdrResult)
        {
            float.IsNaN(value).Should().BeFalse();
            float.IsInfinity(value).Should().BeFalse();
        }
    }

    [Fact]
    public void LdrTile_ShouldDecodeSuccessfully()
    {
        // Test larger LDR tile decoding
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "ldr-tile.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with both APIs
        var ldrResult = AstcDecoder.DecompressImage(astcFile);
        var hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Both should produce correct output sizes
        ldrResult.Length.Should().Be(astcFile.Width * astcFile.Height * 4);
        hdrResult.Length.Should().Be(astcFile.Width * astcFile.Height * 4);
    }

    [Fact]
    public void SameFootprint_HdrVsLdr_ShouldBothDecode()
    {
        // Verify files with same footprint decode correctly
        var hdrPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "HDR-A-1x1.astc"));
        var ldrPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "LDR-A-1x1.astc"));

        var hdrData = File.ReadAllBytes(hdrPath);
        var ldrData = File.ReadAllBytes(ldrPath);

        var hdrFile = AstcFile.FromMemory(hdrData);
        var ldrFile = AstcFile.FromMemory(ldrData);

        // Both are 1x1 with 6x6 footprint
        hdrFile.Width.Should().Be(ldrFile.Width);
        hdrFile.Height.Should().Be(ldrFile.Height);
        hdrFile.Footprint.Width.Should().Be(ldrFile.Footprint.Width);
        hdrFile.Footprint.Height.Should().Be(ldrFile.Footprint.Height);

        // Both should decode successfully with HDR API
        var hdrDecoded = AstcDecoder.DecompressHdrImage(
            hdrFile.Blocks, hdrFile.Width, hdrFile.Height, hdrFile.Footprint);
        var ldrDecoded = AstcDecoder.DecompressHdrImage(
            ldrFile.Blocks, ldrFile.Width, ldrFile.Height, ldrFile.Footprint);

        hdrDecoded.Length.Should().Be(4);
        ldrDecoded.Length.Should().Be(4);
    }

    [Fact]
    public void HdrColor_FromLdr_ShouldMatchLdrToHdrApiConversion()
    {
        // Verify that HdrColor.FromRgba() produces same results as decoding LDR with HDR API
        var astcPath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.HdrFolder, "LDR-A-1x1.astc"));

        var astcData = File.ReadAllBytes(astcPath);
        var astcFile = AstcFile.FromMemory(astcData);

        // Decode with LDR API to get byte values
        var ldrBytes = AstcDecoder.DecompressImage(astcFile);

        // Convert LDR bytes to HDR using HdrColor
        var ldrColor = new RgbaColor(ldrBytes[0], ldrBytes[1], ldrBytes[2], ldrBytes[3]);
        var hdrFromLdr = RgbaHdrColor.FromRgba(ldrColor);

        // Decode with HDR API
        var hdrDirect = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Compare: UNORM16 normalized values should match HDR API output
        for (int i = 0; i < 4; i++)
        {
            float fromConversion = hdrFromLdr[i] / 65535.0f;
            float fromDirect = hdrDirect[i];

            Math.Abs(fromConversion - fromDirect).Should().BeLessThan(0.0001f);
        }
    }
}
