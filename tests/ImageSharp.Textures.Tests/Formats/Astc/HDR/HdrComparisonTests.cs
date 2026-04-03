// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

/// <summary>
/// Comparing HDR and LDR ASTC decoding behavior using real reference files.
/// </summary>
[Trait("Format", "Astc")]
public class HdrComparisonTests
{
    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_A_1x1)]
    public void HdrFile_DecodedWithHdrApi_ShouldPreserveExtendedRange(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        Assert.Equal(4, hdrResult.Length);

        // All channels exceed 1.0, confirming HDR extended range.
        Assert.Equal(1.625f, hdrResult[0], 0.001f);
        Assert.Equal(1.84375f, hdrResult[1], 0.001f);
        Assert.Equal(2.125f, hdrResult[2], 0.001f);
        Assert.Equal(1.0f, hdrResult[3], 0.001f);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Ldr_A_1x1)]
    public void LdrFile_DecodedWithHdrApi_ShouldProduceExpectedNormalizedValues(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        Assert.Equal(4, hdrResult.Length);

        // LDR content maps to 0.0-1.0 range: values correspond to byte/255.
        Assert.Equal(43 / 255f, hdrResult[0], 0.001f);
        Assert.Equal(173 / 255f, hdrResult[1], 0.001f);
        Assert.Equal(0f, hdrResult[2], 0.001f);
        Assert.Equal(1.0f, hdrResult[3], 0.001f);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_A_1x1)]
    public void HdrFile_DecodedWithLdrApi_ShouldProduceExpectedClampedValues(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);

        Assert.Equal(4, ldrResult.Length);

        Assert.Equal(62, ldrResult[0]);
        Assert.Equal(63, ldrResult[1]);
        Assert.Equal(64, ldrResult[2]);
        Assert.Equal(60, ldrResult[3]);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Ldr_A_1x1)]
    public void LdrFile_DecodedWithBothApis_ShouldProduceConsistentValues(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(astcData);

        Span<byte> ldrResult = AstcDecoder.DecompressImage(astcFile);
        Span<float> hdrResult = AstcDecoder.DecompressHdrImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        // LDR: exact byte values.
        Assert.Equal(43, ldrResult[0]);
        Assert.Equal(173, ldrResult[1]);
        Assert.Equal(0, ldrResult[2]);
        Assert.Equal(255, ldrResult[3]);

        // HDR float should equal byte / 255.
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(ldrResult[i] / 255f, hdrResult[i], 0.001f);
        }
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Hdr_Tile)]
    public void HdrTile_ShouldDecodeSuccessfully(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
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

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Ldr_Tile)]
    public void LdrTile_ShouldDecodeSuccessfully(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
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
        string hdrPath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestImages.Astc.Hdr.Hdr_A_1x1));
        string ldrPath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestImages.Astc.Hdr.Ldr_A_1x1));

        AstcFile hdrFile = AstcFile.FromMemory(File.ReadAllBytes(hdrPath));
        AstcFile ldrFile = AstcFile.FromMemory(File.ReadAllBytes(ldrPath));

        // Both are 1x1 with 6x6 footprint.
        Assert.Equal(1, hdrFile.Width);
        Assert.Equal(1, hdrFile.Height);
        Assert.Equal(ldrFile.Width, hdrFile.Width);
        Assert.Equal(ldrFile.Height, hdrFile.Height);
        Assert.Equal(FootprintType.Footprint6x6, hdrFile.Footprint.Type);
        Assert.Equal(hdrFile.Footprint.Type, ldrFile.Footprint.Type);

        Span<float> hdrDecoded = AstcDecoder.DecompressHdrImage(
            hdrFile.Blocks, hdrFile.Width, hdrFile.Height, hdrFile.Footprint);
        Span<float> ldrDecoded = AstcDecoder.DecompressHdrImage(
            ldrFile.Blocks, ldrFile.Width, ldrFile.Height, ldrFile.Footprint);

        // HDR file has values > 1.0; LDR file stays in 0-1.
        Assert.Equal(1.625f, hdrDecoded[0], 0.001f);
        Assert.Equal(43 / 255f, ldrDecoded[0], 0.001f);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Hdr.Ldr_A_1x1)]
    public void HdrColor_FromLdr_ShouldMatchLdrToHdrApiConversion(TestTextureProvider provider)
    {
        byte[] astcData = File.ReadAllBytes(provider.InputFile);
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
