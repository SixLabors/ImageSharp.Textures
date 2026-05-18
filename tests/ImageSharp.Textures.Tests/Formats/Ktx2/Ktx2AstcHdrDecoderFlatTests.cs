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

    /// <summary>
    /// Exercises the ASTC code path where blocks use HDR endpoint modes (2/3/7/11/14/15)
    /// but the KTX2 container is tagged as <c>VK_FORMAT_ASTC_*_UNORM_BLOCK</c>. Per ASTC spec
    /// §C.2.19, §C.2.25 the LDR profile treats HDR-mode blocks as reserved and emits the
    /// error colour (magenta) for every texel.
    /// </summary>
    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Astc4x4_HdrInUnorm)]
    public void Decode_HdrBlocksInUnormContainer_EmitsErrorColor(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        FlatTexture flat = Assert.IsType<FlatTexture>(texture);

        using Image image = flat.MipMaps[0].GetImage();
        using Image<Rgba32> rgba = image.CloneAs<Rgba32>();

        // Spec §C.2.19 error colour: opaque magenta for every texel of an HDR-in-LDR block.
        Rgba32 magenta = new(0xFF, 0x00, 0xFF, 0xFF);
        rgba.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                foreach (Rgba32 pixel in accessor.GetRowSpan(y))
                {
                    Assert.Equal(magenta, pixel);
                }
            }
        });
    }
}
