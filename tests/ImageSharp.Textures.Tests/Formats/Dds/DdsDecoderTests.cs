// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using Xunit;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Dds
{
    using static TestTextures.Dds;

    public class DdsDecoderTests
    {
        [Theory]
        [WithFile(TextureType.Cubemap, "cubemap-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_Mips(TestTextureProvider provider)
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                this.SaveTextures(texture, provider.InputFile);
                //image.DebugSave(provider);, provider.InputFileName
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }

        [Theory]
        [WithFile(TextureType.Cubemap, "cubemap-no-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_NoMips(TestTextureProvider provider)
        {
            using (Texture texture = provider.GetTexture(new DdsDecoder()))
            {
                this.SaveTextures(texture, provider.InputFile);
                //image.DebugSave(provider);, provider.InputFileName
                //DdsTestUtils.CompareWithReferenceDecoder(provider, image);
            }
        }


        private void SaveTextures(Texture texture, string inputName)
        { 
            if (!TestEnvironment.RunsOnCI)
            {
                return;
            }
            for (int i = 0; i < texture.Images.Length; i++)
            {
                for (int j = 0; j < texture.Images[i].Length; j++)
                {
                    texture.Images[i][j].Save($"d:\\{Path.GetFileNameWithoutExtension(inputName)}-depth{i}-mip{j}.png");
                }
            }
        }
    }
}
