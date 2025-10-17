// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx
{
    [Trait("Format", "Ktx")]
    public class KtxDecoderTests
    {
        private static readonly KtxDecoder KtxDecoder = new KtxDecoder();

        [Theory]
        [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.PvrTexToolCli, TestImages.Ktx.Rgba)]
        public void KtxDecoder_CanDecode_Rgba8888(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(KtxDecoder);
            provider.SaveTextures(texture);
            var flatTexture = texture as FlatTexture;

            Assert.NotNull(flatTexture?.MipMaps);
            Assert.Equal(8, flatTexture.MipMaps.Count);
            Assert.Equal(200, flatTexture.MipMaps[0].GetImage().Height);
            Assert.Equal(200, flatTexture.MipMaps[0].GetImage().Width);
            Assert.Equal(100, flatTexture.MipMaps[1].GetImage().Height);
            Assert.Equal(100, flatTexture.MipMaps[1].GetImage().Width);
            Assert.Equal(50, flatTexture.MipMaps[2].GetImage().Height);
            Assert.Equal(50, flatTexture.MipMaps[2].GetImage().Width);
            Assert.Equal(25, flatTexture.MipMaps[3].GetImage().Height);
            Assert.Equal(25, flatTexture.MipMaps[3].GetImage().Width);
            Assert.Equal(12, flatTexture.MipMaps[4].GetImage().Height);
            Assert.Equal(12, flatTexture.MipMaps[4].GetImage().Width);
            Assert.Equal(6, flatTexture.MipMaps[5].GetImage().Height);
            Assert.Equal(6, flatTexture.MipMaps[5].GetImage().Width);
            Assert.Equal(3, flatTexture.MipMaps[6].GetImage().Height);
            Assert.Equal(3, flatTexture.MipMaps[6].GetImage().Width);
            Assert.Equal(1, flatTexture.MipMaps[7].GetImage().Height);
            Assert.Equal(1, flatTexture.MipMaps[7].GetImage().Width);
            Image firstMipMap = flatTexture.MipMaps[0].GetImage();
            Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);
            var firstMipMapImage = firstMipMap as Image<Rgba32>;
            firstMipMapImage.CompareToReferenceOutput(provider, appendPixelTypeToFileName: false);
        }
    }
}
