// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

[GroupOutput("Ktx")]
[Trait("Format", "Ktx")]
[Trait("Format", "Astc")]
public class KtxAstcDecoderFlatTests
{
    private static readonly KtxDecoder KtxDecoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_8x8)]
    public void CanDecode_Rgba32_Blocksizes(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(256, firstMipMap.Width);
        Assert.Equal(256, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgba32> firstMipMapImage = firstMipMap as Image<Rgba32>;

        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_4x4)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_5x4)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_5x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_6x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_6x6)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_8x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_8x6)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_8x8)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_10x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_10x6)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_10x8)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_10x10)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_12x10)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_Unorm_12x12)]
    public void CanDecode_Rgba32_Unorm(TestTextureProvider provider)
    {
        string blockSize = BlockSizeExtractor.FromFileName(provider.InputFile);
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        (firstMipMap as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: $"{blockSize}");
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_4x4)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_5x4)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_5x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_6x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_6x6)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_8x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_8x6)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_8x8)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_10x5)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_10x6)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_10x8)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_10x10)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_12x10)]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_sRgb_12x12)]
    public void CanDecode_Rgba32_Srgb(TestTextureProvider provider)
    {
        string blockSize = BlockSizeExtractor.FromFileName(provider.InputFile);
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        (firstMipMap as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: $"{blockSize}");
    }
}
