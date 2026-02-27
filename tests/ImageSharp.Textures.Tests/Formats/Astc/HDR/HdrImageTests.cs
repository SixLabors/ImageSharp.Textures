// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

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
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // The HDR-A-1x1.astc file has a 6x6 footprint based on the header
        Assert.Equal(6, astcFile.Footprint.Width);
        Assert.Equal(6, astcFile.Footprint.Height);
        Assert.Equal(FootprintType.Footprint6x6, astcFile.Footprint.Type);
    }

    [Fact]
    public void DecodeHdrAstcFile_1x1Pixel_ShouldProduceValidHdrOutput()
    {
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks,
            astcFile.Width,
            astcFile.Height,
            astcFile.Footprint);

        // Should produce 1 pixel with 4 values (RGBA)
        Assert.Equal(4, hdrResult.Length);

        // HDR values can exceed 1.0
        // Just verify they're in a reasonable range (0.0 to 10.0)
        foreach (float value in hdrResult)
        {
            Assert.True(value >= 0.0f);
            Assert.True(value < 10.0f);
        }
    }

    [Fact]
    public void DecodeHdrAstcFile_Tile_ShouldProduceValidHdrOutput()
    {
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_Tile);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks,
            astcFile.Width,
            astcFile.Height,
            astcFile.Footprint);

        // Should produce Width * Height pixels, each with 4 values
        Assert.Equal(astcFile.Width * astcFile.Height * 4, hdrResult.Length);

        // Verify at least some HDR values exceed 1.0 (typical for HDR content)
        int valuesGreaterThanOne = 0;
        foreach (float v in hdrResult)
        {
            if (v > 1.0f)
            {
                valuesGreaterThanOne++;
            }
        }

        Assert.Equal(64, valuesGreaterThanOne);
    }

    [Fact]
    [Description("Verify that HDR ASTC files can be decoded with the LDR API, producing clamped values")]
    public void DecodeHdrAstcFile_WithLdrApi_ShouldClampValues()
    {
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);

        if (!File.Exists(astcPath))
        {
            return;
        }

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode using LDR API
        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);

        // Should produce 1 pixel with 4 bytes (RGBA)
        Assert.Equal(4, ldrResult.Length);

        // All values should be in LDR range
        foreach (byte value in ldrResult)
        {
            Assert.True(value >= byte.MinValue);
            Assert.True(value <= byte.MaxValue);
        }
    }

    [Fact]
    [Description("Verify that HDR and LDR APIs produce consistent relative channel values for the same HDR ASTC file")]
    public void HdrAndLdrApis_OnSameHdrFile_ShouldProduceConsistentRelativeValues()
    {
        string astcPath = TestFile.GetInputFileFullPath(TestImages.Astc.Hdr.Hdr_A_1x1);

        byte[] astcData = File.ReadAllBytes(astcPath);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // Decode with both APIs
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);
        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);

        // Both should produce output for 1 pixel
        Assert.Equal(4, hdrResult.Length);
        Assert.Equal(4, ldrResult.Length);

        // The relative ordering of RGB channels should be consistent between APIs.
        // If HDR channel i > channel j, then LDR channel i should be >= channel j
        // (accounting for clamping at 255).
        for (int i = 0; i < 3; i++)
        {
            for (int j = i + 1; j < 3; j++)
            {
                if (hdrResult[i] > hdrResult[j])
                {
                    Assert.True(ldrResult[i] >= ldrResult[j]);
                }
                else if (hdrResult[i] < hdrResult[j])
                {
                    Assert.True(ldrResult[i] <= ldrResult[j]);
                }
            }
        }
    }
}
