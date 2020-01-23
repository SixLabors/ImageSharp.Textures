// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Dds
{
    using static TestTextures.Dds;

    public class DdsDecoderTests
    {
        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Cubemap, "cubemap-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Cubemap, "cubemap-no-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_NoMips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, "flat-pot-no-alpha-DXT5.DDS")]
        public void DdsDecoder_CanDecode_Flat_DXT5(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Volume, "volume-mips.dds")]
        public void DdsDecoder_CanDecode_Volume_With_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Volume, "volume-no-mips.dds")]
        public void DdsDecoder_CanDecode_Volume_With_No_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.SaveTextures(texture, provider);
        }

        private void SaveMipMaps(MipMap[] mipMaps, TestTextureProvider testTextureProvider, string name)
        {
            string path = Path.Combine(TestEnvironment.ActualOutputDirectoryFullPath, testTextureProvider.TextureFormat.ToString(),  testTextureProvider.TextureType.ToString(), testTextureProvider.MethodName);

            Directory.CreateDirectory(path);

            for (int i = 0; i < mipMaps.Length; i++)
            {
                string filename = $"mipmap{i + 1}";
                if (name != null)
                {
                    filename = $"{filename}-{name}";
                }

                using Image image = mipMaps[i].GetImage();
                image.Save(Path.Combine(path, $"{filename}.png"));
            }
        }

        private void SaveTextures(Texture texture, TestTextureProvider testTextureProvider)
        {
            if (TestEnvironment.RunsOnCI)
            {
                return;
            }

            if (texture is CubemapTexture cubemapTexture)
            {
                this.SaveMipMaps(cubemapTexture.PositiveX.MipMaps.ToArray(), testTextureProvider, "positivex");
                this.SaveMipMaps(cubemapTexture.NegativeX.MipMaps.ToArray(), testTextureProvider, "negativex");
                this.SaveMipMaps(cubemapTexture.PositiveY.MipMaps.ToArray(), testTextureProvider, "positivey");
                this.SaveMipMaps(cubemapTexture.NegativeY.MipMaps.ToArray(), testTextureProvider, "negativey");
                this.SaveMipMaps(cubemapTexture.PositiveZ.MipMaps.ToArray(), testTextureProvider, "positivez");
                this.SaveMipMaps(cubemapTexture.NegativeZ.MipMaps.ToArray(), testTextureProvider, "negativez");
            }

            if (texture is FlatTexture flatTexture)
            {
                this.SaveMipMaps(flatTexture.MipMaps.ToArray(), testTextureProvider, null);
            }

            if (texture is VolumeTexture volumeTexture)
            {
                for (int i = 0; i < volumeTexture.Slices.Count; i++)
                {
                    this.SaveMipMaps(volumeTexture.Slices[i].MipMaps.ToArray(), testTextureProvider, $"slice{i + 1}");
                }
            }
        }
    }
}
