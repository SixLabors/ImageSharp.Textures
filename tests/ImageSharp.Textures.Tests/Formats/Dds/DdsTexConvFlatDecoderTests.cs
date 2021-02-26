// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Dds
{
    public class DdsTexConvFlatDecoderTests
    {
        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat A8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_A8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat AYUV.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_AYUV(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B4G4R4A4_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B4G4R4A4_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B5G5R5A1_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B5G5R5A1_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B5G6R5_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B5G6R5_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B8G8R8A8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B8G8R8A8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B8G8R8A8_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B8G8R8A8_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B8G8R8X8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B8G8R8X8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat B8G8R8X8_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_B8G8R8X8_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC1_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC1_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC1_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC1_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC2_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC2_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC2_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC2_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC3_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC3_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC3_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC3_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC4_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC4_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC4_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC4_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC5_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC5_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC5_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC5_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC6H_SF16.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC6H_SF16(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC6H_UF16.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC6H_UF16(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC7_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC7_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BC7_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BC7_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BGRA.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BGRA(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BPTC.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BPTC(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat BPTC_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_BPTC_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat DXT1.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_DXT1(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat DXT2.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_DXT2(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat DXT3.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_DXT3(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat DXT4.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_DXT4(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat DXT5.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_DXT5(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat FP16.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_FP16(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat FP32.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_FP32(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat G8R8_G8B8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_G8R8_G8B8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R10G10B10_XR_BIAS_A2_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R10G10B10_XR_BIAS_A2_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R10G10B10A2_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R10G10B10A2_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R10G10B10A2_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R10G10B10A2_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R11G11B10_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R11G11B10_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16B16A16_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16B16A16_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16B16A16_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16B16A16_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16B16A16_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16B16A16_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16B16A16_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16B16A16_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R16G16B16A16_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R16G16B16A16_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32B32_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32B32_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32B32_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32B32_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32B32_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32B32_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32B32A32_FLOAT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32B32A32_FLOAT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32B32A32_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32B32A32_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R32G32B32A32_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R32G32B32A32_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8_B8G8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8_B8G8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8B8A8_SINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8B8A8_SINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8B8A8_SNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8B8A8_SNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8B8A8_UINT.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8B8A8_UINT(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8B8A8_UNORM.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8B8A8_UNORM(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R8G8B8A8_UNORM_SRGB.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R8G8B8A8_UNORM_SRGB(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat R9G9B9E5_SHAREDEXP.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_R9G9B9E5_SHAREDEXP(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat RGBA.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_RGBA(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat Y210.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_Y210(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat Y216.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_Y216(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat Y410.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_Y410(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat Y416.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_Y416(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }

        [Theory]
        [WithFile(TestTextureFormat.DDS, TestTextureType.Flat, TestTextureTool.TexConv, "flat YUY2.DDS")]
        public void DdsDecoder_CanDecode_Flat_TexConv_YUY2(TestTextureProvider provider)
        {
            using Texture texture = provider.GetTexture(new DdsDecoder());
            provider.SaveTextures(texture);
            provider.CompareTextures(texture);
        }
    }
}
