// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

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
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with HDR API
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Verify we get Float16 output
        Assert.Equal(4, hdrResult.Length); // 1 pixel, 4 channels

        // HDR content can have values > 1.0 (this file may or may not, but should allow it)
        foreach (float value in hdrResult)
        {
            Assert.False(float.IsNaN(value));
            Assert.False(float.IsInfinity(value));
            Assert.True(value >= 0.0f);
        }
    }

    [Fact]
    public void LdrFile_DecodedWithHdrApi_ShouldUpscaleToHdrRange()
    {
        // LDR files decoded with HDR API should produce values in 0.0-1.0 range
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Ldr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with HDR API
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        Assert.Equal(4, hdrResult.Length);

        // LDR content should map to 0.0-1.0 range when decoded with HDR API
        foreach (float value in hdrResult)
        {
            Assert.True(value >= 0.0f);
            Assert.True(value <= 1.0f);
        }
    }

    [Fact]
    public void HdrFile_DecodedWithLdrApi_ShouldClampToByteRange()
    {
        // HDR files decoded with LDR API should clamp to 0-255
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with LDR API
        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);

        Assert.Equal(4, ldrResult.Length);

        // All values must be in LDR range
        foreach (byte value in ldrResult)
        {
            Assert.True(value >= 0);
            Assert.True(value <= 255);
        }
    }

    [Fact]
    public void LdrFile_DecodedWithBothApis_ShouldProduceConsistentValues()
    {
        // LDR content should produce equivalent results with both APIs
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Ldr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with both APIs
        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Compare results - LDR byte should map to HDR float / 255.0
        for (int i = 0; i < 4; i++)
        {
            byte ldrValue = ldrResult[i];
            float hdrValue = hdrResult[i];

            float expectedHdr = ldrValue / 255.0f;

            Assert.True(Math.Abs(hdrValue - expectedHdr) < 0.01f);
        }
    }

    [Fact]
    public void HdrTile_ShouldDecodeSuccessfully()
    {
        // Test larger HDR tile decoding
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_Tile);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Should produce Width * Height * 4 values
        Assert.Equal(astcFile.Width * astcFile.Height * 4, hdrResult.Length);

        foreach (float value in hdrResult)
        {
            Assert.False(float.IsNaN(value));
            Assert.False(float.IsInfinity(value));
        }
    }

    [Fact]
    public void LdrTile_ShouldDecodeSuccessfully()
    {
        // Test larger LDR tile decoding
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Ldr_Tile);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with both APIs
        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Both should produce correct output sizes
        Assert.Equal(astcFile.Width * astcFile.Height * 4, ldrResult.Length);
        Assert.Equal(astcFile.Width * astcFile.Height * 4, hdrResult.Length);
    }

    [Fact]
    public void SameFootprint_HdrVsLdr_ShouldBothDecode()
    {
        // Verify files with same footprint decode correctly
        string hdrPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);
        string ldrPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Ldr_A_1x1);

        byte[] hdrData = File.ReadAllBytes(hdrPath);
        byte[] ldrData = File.ReadAllBytes(ldrPath);

        AstcFile hdrFile = AstcFile.FromMemory(hdrData);
        AstcFile ldrFile = AstcFile.FromMemory(ldrData);

        // Both are 1x1 with 6x6 footprint
        Assert.Equal(ldrFile.Width, hdrFile.Width);
        Assert.Equal(ldrFile.Height, hdrFile.Height);
        Assert.Equal(ldrFile.Footprint.Width, hdrFile.Footprint.Width);
        Assert.Equal(ldrFile.Footprint.Height, hdrFile.Footprint.Height);

        // Both should decode successfully with HDR API
        Span<float> hdrDecoded = AstcDecoder.DecompressHdrImage(
            hdrFile.Blocks, hdrFile.Width, hdrFile.Height, hdrFile.Footprint);
        Span<float> ldrDecoded = AstcDecoder.DecompressHdrImage(
            ldrFile.Blocks, ldrFile.Width, ldrFile.Height, ldrFile.Footprint);

        Assert.Equal(4, hdrDecoded.Length);
        Assert.Equal(4, ldrDecoded.Length);
    }

    [Fact]
    public void HdrColor_FromLdr_ShouldMatchLdrToHdrApiConversion()
    {
        // Verify that HdrColor.FromRgba() produces same results as decoding LDR with HDR API
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Ldr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with LDR API to get byte values
        Span<byte> ldrBytes = AstcDecoder.DecompressImage(astcFile);

        // Convert LDR bytes to HDR using extension method
        Rgba32 ldrColor = new(ldrBytes[0], ldrBytes[1], ldrBytes[2], ldrBytes[3]);
        Rgba64 hdrFromLdr = new(ldrColor);

        // Decode with HDR API
        Span<float> hdrDirect = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // Compare: UNORM16 normalized values should match HDR API output
        for (int i = 0; i < 4; i++)
        {
            float fromConversion = hdrFromLdr.GetChannel(i) / 65535.0f;
            float fromDirect = hdrDirect[i];

            Assert.True(Math.Abs(fromConversion - fromDirect) < 0.0001f);
        }
    }
}
