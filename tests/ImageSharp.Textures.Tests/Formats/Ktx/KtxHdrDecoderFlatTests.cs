// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Formats.Ktx;
using SixLabors.ImageSharp.Textures.PixelFormats;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

/// <summary>
/// Tests for HDR (High Dynamic Range) formats in KTX files.
/// </summary>
[GroupOutput("Ktx")]
[Trait("Format", "Ktx")]
[Trait("Format", "Hdr")]
public class KtxHdrDecoderFlatTests
{
    private static readonly KtxDecoder KtxDecoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.R16)]
    public void CanDecode_R16F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(16, firstMipMap.PixelType.BitsPerPixel);

        Image<R16Float> firstMipMapImage = firstMipMap as Image<R16Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.R32)]
    public void CanDecode_R32F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        Image<Fp32> firstMipMapImage = firstMipMap as Image<Fp32>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.Rg32)]
    public void CanDecode_RG32F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        Image<Rg32Float> firstMipMapImage = firstMipMap as Image<Rg32Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.Rg64)]
    public void CanDecode_RG64F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(64, firstMipMap.PixelType.BitsPerPixel);

        Image<Rg64Float> firstMipMapImage = firstMipMap as Image<Rg64Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.Rgb48)]
    public void CanDecode_RGB48F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(48, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgb48Float> firstMipMapImage = firstMipMap as Image<Rgb48Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.Rgb96)]
    public void CanDecode_RGB96F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(96, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgb96Float> firstMipMapImage = firstMipMap as Image<Rgb96Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    // TODO: This test is failing because the decoded image has 0 alpha, but the png is saved with 1 alpha.
    // Not sure if this is an issue with the decoder, or the way the reference image was saved.
    // The RGBA32F image has 1 alpha
    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.Rgba64)]
    public void CanDecode_RGBA64F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(64, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgba64Float> firstMipMapImage = firstMipMap as Image<Rgba64Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Hdr.Rgba128)]
    public void CanDecode_RGBA128F(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Assert.NotNull(firstMipMap);
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(128, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgba128Float> firstMipMapImage = firstMipMap as Image<Rgba128Float>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }
}
