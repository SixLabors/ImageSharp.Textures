// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Dds
{
    public class DdsNvDxtFlatDecoderTests
    {
        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips 3DC.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_3DC(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips 3DC.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_3DC(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips A8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_A8(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips A8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_A8(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips A8L8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_A8L8(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips A8L8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_A8L8(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips CXV8U8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_CXV8U8(TestTextureProvider provider)
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                using Texture texture = provider.GetTexture(new DdsDecoder());
            });
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips CXV8U8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_CXV8U8(TestTextureProvider provider)
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                using Texture texture = provider.GetTexture(new DdsDecoder());
            });
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips DXT1A.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_DXT1A(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips DXT1A.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_DXT1A(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips DXT1C.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_DXT1C(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips DXT1C.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_DXT1C(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips DXT3.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_DXT3(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips DXT3.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_DXT3(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips DXT5.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_DXT5(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips DXT5.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_DXT5(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips DXT5NM.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_DXT5NM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips DXT5NM.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_DXT5NM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips FP16X4.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_FP16X4(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips FP16X4.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_FP16X4(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips FP32.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_FP32(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips FP32.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_FP32(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips FP32X4.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_FP32X4(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips FP32X4.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_FP32X4(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips G16R16.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_G16R16(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips G16R16.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_G16R16(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips G16R16F.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_G16R16F(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips G16R16F.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_G16R16F(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips U1555.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_U1555(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips U1555.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_U1555(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips U4444.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_U4444(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips U4444.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_U4444(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips U555.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_U555(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips U555.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_U555(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips U565.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_U565(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips U565.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_U565(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips U888.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_U888(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips U888.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_U888(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips U8888.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_U8888(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips U8888.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_U8888(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat has-mips V8U8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_Has_Mips_V8U8(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.Dds, TestTextureType.Flat, TestTextureTool.NvDxt, "flat no-mips V8U8.dds")]
        public void DdsDecoder_CanDecode_Flat_NvDxt_No_Mips_V8U8(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }
    }
}
