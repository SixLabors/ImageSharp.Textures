// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.PixelFormats;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

/// <summary>
/// Tests for HDR (High Dynamic Range) formats in KTX2 files.
/// </summary>
[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Hdr")]
public class Ktx2HdrDecoderFlatTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.R16_Unorm)]
    public void CanDecode_R16_Unorm(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<L16> firstMipMapImage = firstMipMap as Image<L16>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rg32_Unorm)]
    public void CanDecode_RG32_Unorm(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<ImageSharp.PixelFormats.Rg32> firstMipMapImage = firstMipMap as Image<ImageSharp.PixelFormats.Rg32>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb48_Unorm)]
    public void CanDecode_RGB48_Unorm(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgb48> firstMipMapImage = firstMipMap as Image<Rgb48>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba64_Unorm)]
    public void CanDecode_RGBA64_Unorm(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgba64> firstMipMapImage = firstMipMap as Image<Rgba64>;
        firstMipMapImage.CompareToReferenceOutput(provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.R32_Sfloat)]
    public void CanDecode_R32_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Fp32> firstMipMapImage = firstMipMap as Image<Fp32>;

        // Float precision loss accumulates through the 8-bit PNG reference round-trip.
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.07f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rg32_Sfloat)]
    public void CanDecode_RG32_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rg32Float> firstMipMapImage = firstMipMap as Image<Rg32Float>;

        // Half-float precision loss accumulates through the 8-bit PNG reference round-trip.
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.07f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb48_Sfloat)]
    public void CanDecode_RGB48_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgb48Float> firstMipMapImage = firstMipMap as Image<Rgb48Float>;

        // Half-float precision loss accumulates through the 8-bit PNG reference round-trip.
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.07f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba64_Sfloat)]
    public void CanDecode_RGBA64_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgba64Float> firstMipMapImage = firstMipMap as Image<Rgba64Float>;

        // Half-float precision loss accumulates through the 8-bit PNG reference round-trip,
        // so we need to use a more tolerant comparison here.
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.07f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rg64_Sfloat)]
    public void CanDecode_RG48_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rg64Float> firstMipMapImage = firstMipMap as Image<Rg64Float>;
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.0003f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb96_Sfloat)]
    public void CanDecode_RGB96_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgb96Float> firstMipMapImage = firstMipMap as Image<Rgb96Float>;
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.0003f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba128_Sfloat)]
    public void CanDecode_RGBA128_Sfloat(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgba128Float> firstMipMapImage = firstMipMap as Image<Rgba128Float>;
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.0003f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb9e5_Ufloat)]
    public void CanDecode_Rgb9e5_Ufloat_Packed(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgba128Float> firstMipMapImage = firstMipMap as Image<Rgba128Float>;
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.0003f), provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.B10g11r11_Ufloat)]
    public void CanDecode_B10g11r11_Ufloat_Packed(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgba128Float> firstMipMapImage = firstMipMap as Image<Rgba128Float>;
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.0003f), provider);
    }
}
