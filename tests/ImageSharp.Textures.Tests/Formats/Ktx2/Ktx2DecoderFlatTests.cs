// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

[Trait("Format", "Ktx2")]
public class Ktx2DecoderFlatTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    [Theory]
    [Description("Ensure that a single mipmap level does not result in an empty mipmap collection")]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Rgb48UnormMips)]
    public void Ktx2Decoder_LevelCountZero_DecodesBaseLevelMipMap(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);

        FlatTexture flatTexture = texture as FlatTexture;
        Assert.NotNull(flatTexture);
        Assert.Single(flatTexture.MipMaps);
        using Image mipImage = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(256, mipImage.Width);
        Assert.Equal(256, mipImage.Height);
    }
}
