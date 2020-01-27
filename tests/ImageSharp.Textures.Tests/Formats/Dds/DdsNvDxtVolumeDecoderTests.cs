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
    public class DdsNvDxtVolumeDecoderTests
    {
        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Volume, TestTextureTool.NvDxt, "volume has-mips.dds")]
        public void DdsDecoder_CanDecode_Volume_NvDxt_Has_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.CompareTextures(texture);
            provider.SaveTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Volume, TestTextureTool.NvDxt, "volume no-mips.dds")]
        public void DdsDecoder_CanDecode_Volume_NvDxt_No_Mips(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.CompareTextures(texture);
            provider.SaveTextures(texture);
        }
    }
}
