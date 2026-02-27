// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
public class Ktx2AstcDecoderCubemapTests
{
    private static readonly Ktx2Decoder KtxDecoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Srgb_6x6_Cube)]
    public void Ktx2AstcDecoder_CanDecode_All_Faces(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        CubemapTexture cubemapTexture = texture as CubemapTexture;

        using Image posXImage = cubemapTexture.PositiveX.MipMaps[0].GetImage();
        (posXImage as Image<Rgba32>).CompareToReferenceOutput(
            ImageComparer.Exact,
            provider,
            testOutputDetails: "posX");

        using Image negXImage = cubemapTexture.NegativeX.MipMaps[0].GetImage();
        (negXImage as Image<Rgba32>).CompareToReferenceOutput(
            ImageComparer.Exact,
            provider,
            testOutputDetails: "negX");

        using Image posYImage = cubemapTexture.PositiveY.MipMaps[0].GetImage();
        (posYImage as Image<Rgba32>).CompareToReferenceOutput(
            ImageComparer.Exact,
            provider,
            testOutputDetails: "posY");

        using Image negYImage = cubemapTexture.NegativeY.MipMaps[0].GetImage();
        (negYImage as Image<Rgba32>).CompareToReferenceOutput(
            ImageComparer.TolerantPercentage(3.0f),
            provider,
            testOutputDetails: "negY");

        using Image posZImage = cubemapTexture.PositiveZ.MipMaps[0].GetImage();
        (posZImage as Image<Rgba32>).CompareToReferenceOutput(
            ImageComparer.Exact,
            provider,
            testOutputDetails: "posZ");

        using Image negZImage = cubemapTexture.NegativeZ.MipMaps[0].GetImage();
        (negZImage as Image<Rgba32>).CompareToReferenceOutput(
            ImageComparer.Exact,
            provider,
            testOutputDetails: "negZ");
    }
}
