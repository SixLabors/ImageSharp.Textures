// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

/// <summary>
/// Tests using real HDR ASTC files from the ARM astc-encoder reference repository.
/// These tests validate that our HDR implementation produces valid output for
/// actual HDR-compressed ASTC data.
/// </summary>
[Trait("Format", "Astc")]
public class HdrImageTests
{
    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_A_1x1)]
    [Description("Verify that the ASTC file header is correctly parsed for HDR content, including footprint detection")]
    public void DecodeHdrFile_VerifyFootprintDetection(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        // The HDR-A-1x1.astc file has a 6x6 footprint based on the header
        Assert.Equal(6, astcFile.Footprint.Width);
        Assert.Equal(6, astcFile.Footprint.Height);
        Assert.Equal(FootprintType.Footprint6x6, astcFile.Footprint.Type);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_A_1x1)]
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
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_Tile)]
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
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_A_1x1)]
    [Description("The LDR decoder must reject HDR-encoded blocks per ASTC spec §C.2.19 (matches astcenc's ASTCENC_ERR_BAD_DECODE_MODE).")]
    public void DecodeHdrAstcFile_WithLdrApi_ShouldThrow(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Assert.Throws<TextureFormatException>(() => AstcDecoder.DecompressImage(astcFile));
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_A_1x1)]
    [Description("The HDR API decodes HDR content correctly; the LDR API rejects it.")]
    public void HdrApi_DecodesHdrContent_LdrApi_Rejects(TestTextureProvider provider)
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

        // LDR API refuses the same HDR content.
        Assert.Throws<TextureFormatException>(() => AstcDecoder.DecompressImage(astcFile));
    }
}
