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

/// <summary>
/// Tests for KTX2 ASTC array textures (<c>layerCount &gt; 0</c>).
/// </summary>
/// <remarks>
/// Full array-texture support is not yet implemented in <see cref="Ktx2Decoder"/>: the decoder
/// reads the level-index table for each mip and produces a <see cref="FlatTexture"/> from
/// whatever bytes appear there, without separating the per-layer slices.
/// </remarks>
[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
[Trait("Format", "Array")]
public class Ktx2AstcArrayDecoderTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    /// <summary>
    /// Decodes a 7-layer × 5-mip ASTC 6×6 sRGB array texture. The current decoder treats
    /// the file as a flat texture and produces one image per declared mip level.
    /// This captures this best-effort behaviour; they will need regenerating when proper
    /// array-layer support lands.
    /// </summary>
    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Array, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Array.Rgb32_Srgb_6x6_MipMap)]
    public void CanDecode_MipMaps(TestTextureProvider provider)
    {
        int mipMapLevel = 0;

        using Texture texture = provider.GetTexture(Ktx2Decoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture);

        foreach (MipMap mipMap in flatTexture.MipMaps)
        {
            using Image image = mipMap.GetImage();
            (image as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: $"{mipMapLevel++}");
        }
    }
}
