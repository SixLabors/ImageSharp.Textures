// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.PixelFormats;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Hdr")]
public class Ktx2HdrDecoderCubemapTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    // Half-float precision loss accumulates through the 8-bit PNG reference round-trip.
    private static readonly ImageComparer HalfFloatComparer = ImageComparer.TolerantPercentage(0.1f);

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.HdrCube.R32Sfloat)]
    public void CanDecode_R32_Sfloat_Cubemap(TestTextureProvider provider)
        => CompareAllFaces<Fp32>(provider, HalfFloatComparer);

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.HdrCube.Rg32Sfloat)]
    public void CanDecode_RG32_Sfloat_Cubemap(TestTextureProvider provider)
        => CompareAllFaces<Rg32Float>(provider, HalfFloatComparer);

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.HdrCube.Rgb48Sfloat)]
    public void CanDecode_RGB48_Sfloat_Cubemap(TestTextureProvider provider)
        => CompareAllFaces<Rgb48Float>(provider, HalfFloatComparer);

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.HdrCube.Rgba64Sfloat)]
    public void CanDecode_RGBA64_Sfloat_Cubemap(TestTextureProvider provider)
        => CompareAllFaces<Rgba64Float>(provider, HalfFloatComparer);

    private static void CompareAllFaces<TPixel>(TestTextureProvider provider, ImageComparer comparer)
        where TPixel : unmanaged, ImageSharp.PixelFormats.IPixel<TPixel>
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        CubemapTexture cubemapTexture = texture as CubemapTexture;

        Assert.NotNull(cubemapTexture);

        using Image posXImage = cubemapTexture.PositiveX.MipMaps[0].GetImage();
        (posXImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: "posX");

        using Image negXImage = cubemapTexture.NegativeX.MipMaps[0].GetImage();
        (negXImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: "negX");

        using Image posYImage = cubemapTexture.PositiveY.MipMaps[0].GetImage();
        (posYImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: "posY");

        using Image negYImage = cubemapTexture.NegativeY.MipMaps[0].GetImage();
        (negYImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: "negY");

        using Image posZImage = cubemapTexture.PositiveZ.MipMaps[0].GetImage();
        (posZImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: "posZ");

        using Image negZImage = cubemapTexture.NegativeZ.MipMaps[0].GetImage();
        (negZImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: "negZ");
    }
}
