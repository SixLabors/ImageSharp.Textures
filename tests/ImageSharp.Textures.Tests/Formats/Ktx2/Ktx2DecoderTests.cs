// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.ComponentModel;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

[Trait("Format", "Ktx2")]
public class Ktx2DecoderTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    [Fact]
    [Description("Ensure that a single mipmap level does not result in an empty mipmap collection")]
    public void Ktx2Decoder_LevelCountZero_DecodesBaseLevelMipMap()
    {
        string path = Path.Combine(
            TestEnvironment.InputImagesDirectoryFullPath,
            "Ktx2",
            "Flat",
            "rgba32-srgb-mips.ktx2");

        using FileStream fileStream = File.OpenRead(path);
        using Texture texture = Ktx2Decoder.DecodeTexture(Configuration.Default, fileStream);

        FlatTexture flatTexture = texture as FlatTexture;
        Assert.NotNull(flatTexture);
        Assert.Single(flatTexture.MipMaps);
        Assert.Equal(256, flatTexture.MipMaps[0].GetImage().Width);
        Assert.Equal(256, flatTexture.MipMaps[0].GetImage().Height);
    }
}
