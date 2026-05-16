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

/// <summary>
/// Tests for VK_FORMAT_ASTC_*_SFLOAT_BLOCK (HDR ASTC) flat decoding in KTX2 files.
/// </summary>
[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
[Trait("Format", "Hdr")]
public class Ktx2AstcHdrDecoderFlatTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    // HDR ASTC compression loss stacks on top of the half-float -> 8-bit PNG round-trip.
    private static readonly ImageComparer Comparer = ImageComparer.TolerantPercentage(0.15f);

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba64_Sfloat_4x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba64_Sfloat_6x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba64_Sfloat_10x5)]
    public void CanDecode_Astc_Sfloat_Blocksizes(TestTextureProvider provider)
    {
        string blockSize = BlockSizeExtractor.FromFileName(provider.InputFile);

        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.True(flatTexture.MipMaps.Count > 0);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();

        Image<Rgba128Float> firstMipMapImage = firstMipMap as Image<Rgba128Float>;
        Assert.NotNull(firstMipMapImage);

        firstMipMapImage.CompareToReferenceOutput(Comparer, provider, testOutputDetails: blockSize);
    }
}
