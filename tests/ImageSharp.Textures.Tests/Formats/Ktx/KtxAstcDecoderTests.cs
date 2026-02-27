// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

[Trait("Format", "Ktx")]
[Trait("Format", "Astc")]
public class KtxAstcDecoderTests
{
    private static readonly KtxDecoder KtxDecoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx.Astc.Rgb32_8x8)]
    public void KtxAstcDecoder_CanDecode_Rgba32_Blocksizes(TestTextureProvider provider)
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

        firstMipMapImage.CompareToReferenceOutput(provider, appendPixelTypeToFileName: false);
    }
}
