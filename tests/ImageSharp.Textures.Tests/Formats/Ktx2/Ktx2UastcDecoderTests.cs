// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

/// <summary>
/// Decoding of UASTC content in KTX2 containers
/// </summary>
[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
public class Ktx2UastcDecoderTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Uastc.Rgba32_Unorm_4x4)]
    public void CanDecode_Uastc_Ldr(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgba32> firstMipMapImage = firstMipMap as Image<Rgba32>;
        Assert.NotNull(firstMipMapImage);

        firstMipMapImage.CompareToReferenceOutput(ImageComparer.Exact, provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Uastc.Rgba32_Srgb_4x4)]
    [Description("sRGB-tagged UASTC decodes with sRGB endpoint expansion")]
    public void CanDecode_Uastc_Srgb(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgba32> firstMipMapImage = firstMipMap as Image<Rgba32>;
        Assert.NotNull(firstMipMapImage);

        firstMipMapImage.CompareToReferenceOutput(ImageComparer.Exact, provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Uastc.Cubemap.Rgba32_Unorm_4x4)]
    public void CanDecode_Uastc_Ldr_Cubemap(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        CubemapTexture cubemapTexture = texture as CubemapTexture;
        Assert.NotNull(cubemapTexture);

        cubemapTexture.CompareFacesToReferenceOutput<Rgba32>(ImageComparer.Exact, provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Uastc.Hdr.Rgba64_Sfloat_4x4)]
    [Description("UASTC HDR 4x4 is standard ASTC HDR block data, so it decodes through the existing ASTC HDR path")]
    public void CanDecode_UastcHdr_4x4(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(128, firstMipMap.PixelType.BitsPerPixel);

        using Image<Rgba32> rgba = firstMipMap.CloneAs<Rgba32>();
        rgba.CompareToReferenceOutput(ImageComparer.Exact, provider);
    }

    [Theory]
    [Description("UASTC HDR 6x6 intermediate is an unsupported custom supercompressed format")]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Uastc.Hdr.Rgba64_Sfloat_6x6i)]
    public void Decode_UastcHdr_6x6i_Throws(TestTextureProvider provider)
        => Assert.Throws<NotSupportedException>(() => provider.GetTexture(Ktx2Decoder));
}
