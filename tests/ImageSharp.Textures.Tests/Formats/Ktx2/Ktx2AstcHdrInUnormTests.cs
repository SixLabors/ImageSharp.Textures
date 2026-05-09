// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Common.Exceptions;
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
/// Per the ASTC spec §C.2.19 the LDR (decode_unorm8) profile cannot decode HDR-mode
/// blocks. ARM astcenc returns <c>ASTCENC_ERR_BAD_DECODE_MODE</c> on this mismatch;
/// this codebase throws <see cref="TextureFormatException"/>.
/// </remarks>
[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
public class Ktx2AstcHdrInUnormTests
{
    private static readonly Ktx2Decoder Ktx2Decoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Astc4x4_HdrInUnorm)]
    public void Decode_HdrBlocksInUnormContainer_ShouldThrow(TestTextureProvider provider)
    {
        // KTX2 decode is lazy — the throw fires when MipMap.GetImage() actually decompresses.
        using Texture texture = provider.GetTexture(Ktx2Decoder);
        FlatTexture flat = Assert.IsType<FlatTexture>(texture);

        Assert.Throws<TextureFormatException>(() => flat.MipMaps[0].GetImage());
    }
}
