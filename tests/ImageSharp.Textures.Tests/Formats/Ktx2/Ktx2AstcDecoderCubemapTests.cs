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

[GroupOutput("Ktx2")]
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
public class Ktx2AstcDecoderCubemapTests
{
    private static readonly Ktx2Decoder KtxDecoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Cubemap, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Cubemap.Rgb32_Srgb_6x6)]
    public void CanDecode_All_Faces(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);

        CubemapTexture cubemapTexture = texture as CubemapTexture;
        Assert.NotNull(cubemapTexture);

        cubemapTexture.CompareFacesToReferenceOutput<Rgba32>(ImageComparer.Exact, provider);
    }
}
