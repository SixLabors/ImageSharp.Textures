using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests
{
    using static TestImages.Dds;

    public class DdsDecoderTests
    {
        [Theory]
        [WithFile(AtcRgb)]
        public void DdsDecoder_CanDecode_Atc_Rgb(TestImageProvider provider)
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                SaveTextures(texture, provider.FileName);
                //image.DebugSave(provider);
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }

        [Theory]
        [WithFile(Dxt1)]
        public void DdsDecoder_CanDecode_Dxt1_Rgb(TestImageProvider provider)
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                SaveTextures(texture, provider.FileName);
                //image.DebugSave(provider);
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }

        [Theory]
        [WithFile(Dxt3)]
        public void DdsDecoder_CanDecode_Dxt3_Rgb(TestImageProvider provider)
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                SaveTextures(texture, provider.FileName);
                //image.DebugSave(provider);
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }

        [Theory]
        [WithFile(Dxt5)]
        public void DdsDecoder_CanDecode_Dxt5_Rgb(TestImageProvider provider)
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                SaveTextures(texture, provider.FileName);
                //image.DebugSave(provider);
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }

        [Theory]
        [WithFile(Dxt1Cubemap)]
        public void DdsDecoder_CanDecode_Dxt1Cubemap_Rgb(TestImageProvider provider) 
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                SaveTextures(texture, provider.FileName);
                //image.DebugSave(provider);
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }

        private void SaveTextures(Texture texture, string name)
        {
            for (var i = 0; i < texture.Images.Length; i++)
            {
                for (var j = 0; j < texture.Images[i].Length; j++)
                {
                    texture.Images[i][j].Save($"d:\\{Path.GetFileNameWithoutExtension(name)}-depth{i}-mip{j}.png");
                }
            }
        }
    }
}
