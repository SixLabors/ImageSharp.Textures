// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit;

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Textures.Tests.Formats.Dds
{
    [Trait("Format", "Dds")]
    public class DdsDecoderCubemapTests
    {
        private static readonly DdsDecoder DdsDecoder = new DdsDecoder();

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Cubemap, TestTextureTool.NvDxt, "cubemap has-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_With_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(DdsDecoder);
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Cubemap, TestTextureTool.NvDxt, "cubemap no-mips.dds")]
        public void DdsDecoder_CanDecode_Cubemap_Without_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(DdsDecoder);
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }
    }
}
