// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Dds
{
    using static TestTextures.Dds;

    public class DdsDecoderTests
    {
        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Cubemap, "cubemap has-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Cubemap, "cubemap no-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_NoMips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, "flat-pot-no-alpha DXT5.dds")]
        public void DdsDecoder_CanDecode_Flat_DXT5(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Volume, "volume has-mips.dds")]
        public void DdsDecoder_CanDecode_Volume_With_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Volume, "volume no-mips.dds")]
        public void DdsDecoder_CanDecode_Volume_With_No_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, "flat-*.*", true)]
        public void DdsDecoder_CanDecode_Flat_WildCard(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, "flat-pot-alpha A8_UNORM.dds")]
        public void DdsDecoder_CanDecode_Uncompressed(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            this.CompareTextures(texture, provider);
            this.SaveTextures(texture, provider);
        }

        private void CompareMipMaps(MipMap[] mipMaps, TestTextureProvider testTextureProvider, string name)
        {
            string filename;

            if (testTextureProvider.TextureType == TestTextureType.Flat)
            {
                string[] fileParts = Path.GetFileName(testTextureProvider.InputFile).Split(' ');
                filename = fileParts[0];
            }
            else
            {
                filename = testTextureProvider.TextureType.ToString().ToLower();
                if (!string.IsNullOrEmpty(name))
                {
                    filename = $"{filename}-{name}";
                }
            }

            filename = $"{filename}.png";

            string baselinePath = Path.Combine(TestEnvironment.BaselineDirectoryFullPath, testTextureProvider.TextureType.ToString(), filename);

            using var imageExpected = Image.Load<Rgba32>(baselinePath);
            using Image testImage = mipMaps[0].GetImage();
            using Image<Rgba32> imageActual = testImage.CloneAs<Rgba32>();

            var comparer = ImageComparer.Tolerant(1F);
            comparer.VerifySimilarity(imageExpected, imageActual);
        }

        private void CompareTextures(Texture texture, TestTextureProvider testTextureProvider)
        {
            if (texture is CubemapTexture cubemapTexture)
            {
                this.CompareMipMaps(cubemapTexture.PositiveX.MipMaps.ToArray(), testTextureProvider, "positive-x");
                this.CompareMipMaps(cubemapTexture.NegativeX.MipMaps.ToArray(), testTextureProvider, "negative-x");
                this.CompareMipMaps(cubemapTexture.PositiveY.MipMaps.ToArray(), testTextureProvider, "positive-y");
                this.CompareMipMaps(cubemapTexture.NegativeY.MipMaps.ToArray(), testTextureProvider, "negative-y");
                this.CompareMipMaps(cubemapTexture.PositiveZ.MipMaps.ToArray(), testTextureProvider, "positive-z");
                this.CompareMipMaps(cubemapTexture.NegativeZ.MipMaps.ToArray(), testTextureProvider, "negative-z");
            }

            if (texture is FlatTexture flatTexture)
            {
                this.CompareMipMaps(flatTexture.MipMaps.ToArray(), testTextureProvider, null);
            }

            if (texture is VolumeTexture volumeTexture)
            {
                for (int i = 0; i < volumeTexture.Slices.Count; i++)
                {
                    this.CompareMipMaps(volumeTexture.Slices[i].MipMaps.ToArray(), testTextureProvider, $"slice-{i + 1}");
                }
            }
        }

        private void SaveMipMaps(MipMap[] mipMaps, TestTextureProvider testTextureProvider, string name)
        {
            string path = Path.Combine(TestEnvironment.ActualOutputDirectoryFullPath, testTextureProvider.TextureFormat.ToString(), testTextureProvider.TextureType.ToString(), testTextureProvider.MethodName, Path.GetFileNameWithoutExtension(testTextureProvider.InputFile));

            Directory.CreateDirectory(path);

            for (int i = 0; i < mipMaps.Length; i++)
            {
                string filename = $"mipmap-{i + 1}";
                if (!string.IsNullOrEmpty(name))
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
                this.SaveMipMaps(cubemapTexture.PositiveX.MipMaps.ToArray(), testTextureProvider, "positive-x");
                this.SaveMipMaps(cubemapTexture.NegativeX.MipMaps.ToArray(), testTextureProvider, "negative-x");
                this.SaveMipMaps(cubemapTexture.PositiveY.MipMaps.ToArray(), testTextureProvider, "positive-y");
                this.SaveMipMaps(cubemapTexture.NegativeY.MipMaps.ToArray(), testTextureProvider, "negative-y");
                this.SaveMipMaps(cubemapTexture.PositiveZ.MipMaps.ToArray(), testTextureProvider, "positive-z");
                this.SaveMipMaps(cubemapTexture.NegativeZ.MipMaps.ToArray(), testTextureProvider, "negative-z");
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
