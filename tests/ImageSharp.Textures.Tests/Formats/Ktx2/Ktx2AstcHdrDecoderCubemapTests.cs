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
/// Tests for VK_FORMAT_ASTC_*_SFLOAT_BLOCK (HDR ASTC) cubemap decoding in KTX2 files.
/// </summary>
[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
[Trait("Format", "Hdr")]
public class Ktx2AstcHdrDecoderCubemapTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    // HDR ASTC compression loss stacks on top of the half-float -> 8-bit PNG round-trip.
    private static readonly ImageComparer Comparer = ImageComparer.TolerantPercentage(0.2f);

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Cubemap.Astc4x4_Sfloat)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Cubemap.Astc6x6_Sfloat)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Cubemap.Astc10x5_Sfloat)]
    public void CanDecode_Astc_Sfloat_Cubemap_Blocksizes(TestTextureProvider provider)
    {
        string blockSize = BlockSizeExtractor.FromFileName(provider.InputFile);

        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);

        CubemapTexture cubemapTexture = texture as CubemapTexture;
        Assert.NotNull(cubemapTexture);

        cubemapTexture.CompareFacesToReferenceOutput<Rgba128Float>(Comparer, provider, faceSuffix: blockSize);
    }
}
