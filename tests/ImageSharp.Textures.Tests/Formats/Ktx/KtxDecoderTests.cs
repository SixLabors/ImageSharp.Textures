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
    [GroupOutput("Ktx")]
    [Trait("Format", "Ktx")]
    public class KtxDecoderTests
    {
        private static readonly KtxDecoder KtxDecoder = new KtxDecoder();

        [Theory]
        [WithFile(TestTextureFormat.Ktx, TestTextureType.Flat, TestTextureTool.PvrTexToolCli, TestImages.Ktx.Rgba)]
        public void CanDecode_Rgba8888(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(KtxDecoder);
            provider.SaveTextures(texture);
            var flatTexture = texture as FlatTexture;

            Assert.NotNull(flatTexture?.MipMaps);
            Assert.Equal(8, flatTexture.MipMaps.Count);

            int[] expectedSizes = [200, 100, 50, 25, 12, 6, 3, 1];
            for (int i = 0; i < expectedSizes.Length; i++)
            {
                using Image mipImage = flatTexture.MipMaps[i].GetImage();
                Assert.Equal(expectedSizes[i], mipImage.Height);
                Assert.Equal(expectedSizes[i], mipImage.Width);
            }

            using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
            Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);
            var firstMipMapImage = firstMipMap as Image<Rgba32>;
            firstMipMapImage.CompareToReferenceOutput(provider, appendPixelTypeToFileName: false);
        }
    }
}
