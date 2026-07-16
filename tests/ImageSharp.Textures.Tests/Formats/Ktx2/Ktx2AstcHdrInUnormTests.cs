// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

/// <summary>
/// Exercises the ASTC code path where blocks use HDR endpoint modes (2/3/7/11/14/15)
/// but the KTX2 container is tagged as <c>VK_FORMAT_ASTC_*_UNORM_BLOCK</c>.
/// </summary>
/// <remarks>
/// Per ASTC spec §C.2.19, §C.2.25 the LDR profile treats HDR-mode blocks as reserved and
/// emits the error colour (magenta) for every texel.
/// </remarks>
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
public class Ktx2AstcHdrInUnormTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

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
