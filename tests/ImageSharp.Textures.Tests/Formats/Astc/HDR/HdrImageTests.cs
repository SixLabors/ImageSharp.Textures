// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Hdr;

/// <summary>
/// Tests using real HDR ASTC files from the ARM astc-encoder reference repository.
/// These tests validate that our HDR implementation produces valid output for
/// actual HDR-compressed ASTC data.
/// </summary>
[Trait("Format", "Astc")]
public class HdrImageTests
{
    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Hdr.Hdr_A_1x1)]
    [Description("Verify that the ASTC file header is correctly parsed for HDR content, including footprint detection")]
    public void DecodeHdrFile_VerifyFootprintDetection(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // The hdr-a-1x1.astc file has a 6x6 footprint based on the header
        Assert.Equal(6, astcFile.Footprint.Width);
        Assert.Equal(6, astcFile.Footprint.Height);
        Assert.Equal(FootprintType.Footprint6x6, astcFile.Footprint.Type);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Hdr.Hdr_A_1x1)]
    public void DecodeHdrAstcFile_1x1Pixel_ShouldProduceExpectedHdrValues(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks,
            astcFile.Width,
            astcFile.Height,
            astcFile.Footprint);

        Assert.Equal(4, hdrResult.Length);

        // HDR values exceed 1.0 for this file.
        Assert.Equal(1.625f, hdrResult[0], 0.001f);
        Assert.Equal(1.84375f, hdrResult[1], 0.001f);
        Assert.Equal(2.125f, hdrResult[2], 0.001f);
        Assert.Equal(1.0f, hdrResult[3], 0.001f);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Hdr.Hdr_Tile)]
    public void DecodeHdrAstcFile_Tile_ShouldProduceValidHdrOutput(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
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

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Hdr.Hdr_A_1x1)]
    [Description("LDR decoder emits the spec-mandated error colour (magenta) for HDR-encoded blocks per ASTC spec §C.2.19, §C.2.25.")]
    public void DecodeHdrAstcFile_WithLdrApi_EmitsErrorColor(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);

        // Spec §C.2.19 error colour: opaque magenta (0xFF, 0x00, 0xFF, 0xFF) per texel.
        for (int i = 0; i < ldrResult.Length; i += 4)
        {
            Assert.Equal(0xFF, ldrResult[i]);
            Assert.Equal(0x00, ldrResult[i + 1]);
            Assert.Equal(0xFF, ldrResult[i + 2]);
            Assert.Equal(0xFF, ldrResult[i + 3]);
        }
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Hdr.Hdr_A_1x1)]
    [Description("The HDR API decodes HDR content correctly; the LDR API emits magenta for it (spec §C.2.19).")]
    public void HdrApi_DecodesHdrContent_LdrApi_EmitsErrorColor(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // HDR values exceed 1.0; R < G < B ordering is preserved.
        Assert.Equal(1.625f, hdrResult[0], 0.001f);
        Assert.Equal(1.84375f, hdrResult[1], 0.001f);
        Assert.Equal(2.125f, hdrResult[2], 0.001f);
        Assert.True(hdrResult[0] < hdrResult[1]);
        Assert.True(hdrResult[1] < hdrResult[2]);

        // LDR API emits the spec-mandated error colour for the same HDR content.
        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);
        Assert.Equal(0xFF, ldrResult[0]);
        Assert.Equal(0x00, ldrResult[1]);
        Assert.Equal(0xFF, ldrResult[2]);
        Assert.Equal(0xFF, ldrResult[3]);
    }
}
