// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.IO;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx2.Enums;
using SixLabors.ImageSharp.Textures.TextureFormats;
using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Decodes ktx textures.
    /// </summary>
    internal class Ktx2Processor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ktx2Processor" /> class.
        /// </summary>
        /// <param name="ktxHeader">The KTX header.</param>
        public Ktx2Processor(Ktx2Header ktxHeader) => this.KtxHeader = ktxHeader;

        /// <summary>
        /// Gets the KTX header.
        /// </summary>
        public Ktx2Header KtxHeader { get; }

        /// <summary>
        /// Decodes the mipmaps of a KTX2 textures.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="levelIndices">The start offsets and byte length of each texture.</param>
        /// <returns>The decoded mipmaps.</returns>
        public MipMap[] DecodeMipMaps(Stream stream, int width, int height, LevelIndex[] levelIndices)
        {
            DebugGuard.MustBeGreaterThan(width, 0, nameof(width));
            DebugGuard.MustBeGreaterThan(height, 0, nameof(height));
            DebugGuard.MustBeGreaterThan(levelIndices.Length, 0, nameof(levelIndices.Length));

            var allMipMapBytes = ReadAllMipMapBytes(stream, levelIndices);
            using var memoryStream = new MemoryStream(allMipMapBytes);

            switch (this.KtxHeader.VkFormat)
            {
                case VkFormat.VK_FORMAT_R8_UNORM:
                case VkFormat.VK_FORMAT_R8_SNORM:
                case VkFormat.VK_FORMAT_R8_UINT:
                case VkFormat.VK_FORMAT_R8_SINT:
                case VkFormat.VK_FORMAT_R8_SRGB:
                    // Single channel textures will be decoded to luminance image.
                    return AllocateMipMaps<L8>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16_UNORM:
                case VkFormat.VK_FORMAT_R16_SNORM:
                case VkFormat.VK_FORMAT_R16_UINT:
                case VkFormat.VK_FORMAT_R16_SINT:
                    // Single channel textures will be decoded to luminance image.
                    return AllocateMipMaps<L16>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16_SFLOAT:
                    return AllocateMipMaps<R16Float>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8_UNORM:
                case VkFormat.VK_FORMAT_R8G8_SNORM:
                case VkFormat.VK_FORMAT_R8G8_UINT:
                case VkFormat.VK_FORMAT_R8G8_SINT:
                case VkFormat.VK_FORMAT_R8G8_SRGB:
                    return AllocateMipMaps<Rg16>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16_UNORM:
                case VkFormat.VK_FORMAT_R16G16_SNORM:
                case VkFormat.VK_FORMAT_R16G16_UINT:
                case VkFormat.VK_FORMAT_R16G16_SINT:
                    return AllocateMipMaps<Rg32>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32_UINT:
                case VkFormat.VK_FORMAT_R32G32_SINT:
                    return AllocateMipMaps<Rg64>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32_SFLOAT:
                    return AllocateMipMaps<Rg64Float>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16B16_UNORM:
                case VkFormat.VK_FORMAT_R16G16B16_SNORM:
                case VkFormat.VK_FORMAT_R16G16B16_UINT:
                case VkFormat.VK_FORMAT_R16G16B16_SINT:
                    return AllocateMipMaps<Rgb48>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16B16A16_UNORM:
                case VkFormat.VK_FORMAT_R16G16B16A16_SNORM:
                case VkFormat.VK_FORMAT_R16G16B16A16_UINT:
                case VkFormat.VK_FORMAT_R16G16B16A16_SINT:
                    return AllocateMipMaps<Rgba64>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16B16A16_SFLOAT:
                    return AllocateMipMaps<Rg32Float>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32_UINT:
                case VkFormat.VK_FORMAT_R32G32B32_SINT:
                    return AllocateMipMaps<Rgb96>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32_SFLOAT:
                    return AllocateMipMaps<Rgb96Float>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32A32_UINT:
                case VkFormat.VK_FORMAT_R32G32B32A32_SINT:
                    return AllocateMipMaps<Rgba128>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32A32_SFLOAT:
                    return AllocateMipMaps<Rgba128Float>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B8G8R8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8_SRGB:
                    return AllocateMipMaps<Bgr24>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8B8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8_SRGB:
                    return AllocateMipMaps<Rgb24>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R5G6B5_UNORM_PACK16:
                    return AllocateMipMaps<Rgb565>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R4G4B4A4_UNORM_PACK16:
                    return AllocateMipMaps<Rgba4444>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B8G8R8A8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SRGB:
                    return AllocateMipMaps<Bgra32>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B5G5R5A1_UNORM_PACK16:
                    return AllocateMipMaps<Bgra5551>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B5G6R5_UNORM_PACK16:
                    return AllocateMipMaps<Bgr565>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B4G4R4A4_UNORM_PACK16:
                    return AllocateMipMaps<Bgra4444>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8B8A8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SRGB:
                    return AllocateMipMaps<Rgba32>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R5G5B5A1_UNORM_PACK16:
                    return AllocateMipMaps<Rgba5551>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC1_RGB_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_BC1_RGBA_UNORM_BLOCK:
                    return AllocateMipMaps<Dxt1>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC2_UNORM_BLOCK:
                    return AllocateMipMaps<Dxt3>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC3_UNORM_BLOCK:
                    return AllocateMipMaps<Dxt5>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC4_UNORM_BLOCK:
                    return AllocateMipMaps<Bc4>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC4_SNORM_BLOCK:
                    return AllocateMipMaps<Bc4s>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC5_UNORM_BLOCK:
                    return AllocateMipMaps<Bc5>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC5_SNORM_BLOCK:
                    return AllocateMipMaps<Bc5s>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC6H_UFLOAT_BLOCK:
                    return AllocateMipMaps<Bc6h>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC6H_SFLOAT_BLOCK:
                    return AllocateMipMaps<Bc6hs>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC7_UNORM_BLOCK:
                    return AllocateMipMaps<Bc7>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK:
                    return AllocateMipMaps<Etc1>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_4x4_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_4x4_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc4X4>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_5x4_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_5x4_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc5X4>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_5x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_5x5_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc5X5>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_6x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_6x5_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc6X5>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_6x6_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_6x6_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc6X6>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_8x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_8x5_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc8X5>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_8x6_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_8x6_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc8X6>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_8x8_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_8x8_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc8X8>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x5_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc10X5>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x6_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x6_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc10X6>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x8_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x8_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc10X8>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x10_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x10_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc10X10>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_12x10_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_12x10_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc12X10>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_12x12_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_12x12_SRGB_BLOCK:
                    return AllocateMipMaps<RgbaAstc12X12>(memoryStream, width, height, levelIndices);
            }

            throw new NotSupportedException("The pixel format is not supported");
        }

        /// <summary>
        /// Allocates and decodes the a KTX2 cube map texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of a texture face.</param>
        /// <param name="height">The height of a texture face.</param>
        /// <param name="levelIndices">The start offsets and byte length of each texture.</param>
        /// <returns>A decoded cubemap texture.</returns>
        /// <exception cref="NotSupportedException">The pixel format is not supported</exception>
        public CubemapTexture DecodeCubeMap(Stream stream, int width, int height, LevelIndex[] levelIndices)
        {
            DebugGuard.MustBeGreaterThan(width, 0, nameof(width));
            DebugGuard.MustBeGreaterThan(height, 0, nameof(height));

            switch (this.KtxHeader.VkFormat)
            {
                case VkFormat.VK_FORMAT_R8_UNORM:
                case VkFormat.VK_FORMAT_R8_SNORM:
                case VkFormat.VK_FORMAT_R8_UINT:
                case VkFormat.VK_FORMAT_R8_SINT:
                case VkFormat.VK_FORMAT_R8_SRGB:
                    return AllocateCubeMap<L8>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16_UNORM:
                case VkFormat.VK_FORMAT_R16_SNORM:
                case VkFormat.VK_FORMAT_R16_UINT:
                case VkFormat.VK_FORMAT_R16_SINT:
                    return AllocateCubeMap<L16>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8_UNORM:
                case VkFormat.VK_FORMAT_R8G8_SNORM:
                case VkFormat.VK_FORMAT_R8G8_UINT:
                case VkFormat.VK_FORMAT_R8G8_SINT:
                case VkFormat.VK_FORMAT_R8G8_SRGB:
                    return AllocateCubeMap<Rg16>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16_UNORM:
                case VkFormat.VK_FORMAT_R16G16_SNORM:
                case VkFormat.VK_FORMAT_R16G16_UINT:
                case VkFormat.VK_FORMAT_R16G16_SINT:
                    return AllocateCubeMap<Rg32>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16B16_UNORM:
                case VkFormat.VK_FORMAT_R16G16B16_SNORM:
                case VkFormat.VK_FORMAT_R16G16B16_UINT:
                case VkFormat.VK_FORMAT_R16G16B16_SINT:
                    return AllocateCubeMap<Rgb48>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16B16A16_UNORM:
                case VkFormat.VK_FORMAT_R16G16B16A16_SNORM:
                case VkFormat.VK_FORMAT_R16G16B16A16_UINT:
                case VkFormat.VK_FORMAT_R16G16B16A16_SINT:
                    return AllocateCubeMap<Rgba64>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16_SFLOAT:
                    return AllocateCubeMap<R16Float>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R16G16B16A16_SFLOAT:
                    return AllocateCubeMap<Rg32Float>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32_UINT:
                case VkFormat.VK_FORMAT_R32G32_SINT:
                    return AllocateCubeMap<Rg64>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32_SFLOAT:
                    return AllocateCubeMap<Rg64Float>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32_UINT:
                case VkFormat.VK_FORMAT_R32G32B32_SINT:
                    return AllocateCubeMap<Rgb96>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32_SFLOAT:
                    return AllocateCubeMap<Rgb96Float>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32A32_UINT:
                case VkFormat.VK_FORMAT_R32G32B32A32_SINT:
                    return AllocateCubeMap<Rgba128>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R32G32B32A32_SFLOAT:
                    return AllocateCubeMap<Rgba128Float>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B8G8R8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8_SRGB:
                    return AllocateCubeMap<Bgr24>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8B8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8_SRGB:
                    return AllocateCubeMap<Rgb24>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R5G6B5_UNORM_PACK16:
                    return AllocateCubeMap<Rgb565>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R4G4B4A4_UNORM_PACK16:
                    return AllocateCubeMap<Rgba4444>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B8G8R8A8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SRGB:
                    return AllocateCubeMap<Bgra32>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B5G5R5A1_UNORM_PACK16:
                    return AllocateCubeMap<Bgra5551>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B5G6R5_UNORM_PACK16:
                    return AllocateCubeMap<Bgr565>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_B4G4R4A4_UNORM_PACK16:
                    return AllocateCubeMap<Bgra4444>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8B8A8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SRGB:
                    return AllocateCubeMap<Rgba32>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R5G5B5A1_UNORM_PACK16:
                    return AllocateCubeMap<Rgba5551>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC1_RGB_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_BC1_RGBA_UNORM_BLOCK:
                    return AllocateCubeMap<Dxt1>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC2_UNORM_BLOCK:
                    return AllocateCubeMap<Dxt3>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC4_UNORM_BLOCK:
                    return AllocateCubeMap<Bc4>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC4_SNORM_BLOCK:
                    return AllocateCubeMap<Bc4s>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC5_UNORM_BLOCK:
                    return AllocateCubeMap<Bc5>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC5_SNORM_BLOCK:
                    return AllocateCubeMap<Bc5s>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC6H_UFLOAT_BLOCK:
                    return AllocateCubeMap<Bc6h>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC6H_SFLOAT_BLOCK:
                    return AllocateCubeMap<Bc6hs>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_BC7_UNORM_BLOCK:
                    return AllocateCubeMap<Bc7>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK:
                    return AllocateCubeMap<Etc1>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_4x4_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_4x4_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc4X4>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_5x4_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_5x4_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc5X4>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_5x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_5x5_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc5X5>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_6x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_6x5_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc6X5>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_6x6_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_6x6_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc6X6>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_8x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_8x5_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc8X5>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_8x6_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_8x6_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc8X6>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_8x8_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_8x8_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc8X8>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x5_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x5_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc10X5>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x6_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x6_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc10X6>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x8_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x8_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc10X8>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_10x10_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_10x10_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc10X10>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_12x10_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_12x10_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc12X10>(stream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_ASTC_12x12_UNORM_BLOCK:
                case VkFormat.VK_FORMAT_ASTC_12x12_SRGB_BLOCK:
                    return AllocateCubeMap<RgbaAstc12X12>(stream, width, height, levelIndices);
            }

            throw new NotSupportedException("The pixel format is not supported");
        }

        /// <summary>
        /// Allocates and decodes all mipmap levels of a ktx texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="levelIndices">The start offsets and byte length of each texture.</param>
        /// <returns>The decoded mipmaps.</returns>
        private static MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, LevelIndex[] levelIndices)
            where TBlock : struct, IBlock<TBlock>
        {
            var blockFormat = default(TBlock);
            var mipMaps = new MipMap<TBlock>[levelIndices.Length];
            for (int i = 0; i < levelIndices.Length; i++)
            {
                var pixelDataSize = levelIndices[i].UncompressedByteLength;
                byte[] mipMapData = new byte[pixelDataSize];
                ReadTextureData(stream, mipMapData);
                mipMaps[i] = new MipMap<TBlock>(blockFormat, mipMapData, width, height);

                width >>= 1;
                height >>= 1;
            }

            return mipMaps;
        }

        private static CubemapTexture AllocateCubeMap<TBlock>(Stream stream, int width, int height, LevelIndex[] levelIndices)
            where TBlock : struct, IBlock<TBlock>
        {
            var numberOfMipMaps = levelIndices.Length;

            var cubeMapTexture = new CubemapTexture();
            var blockFormat = default(TBlock);
            for (int i = 0; i < numberOfMipMaps; i++)
            {
                stream.Position = (long)levelIndices[i].ByteOffset;
                var uncompressedDataLength = levelIndices[i].UncompressedByteLength;
                var dataForEachFace = (uint)uncompressedDataLength / 6;

                cubeMapTexture.PositiveX.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.NegativeX.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.PositiveY.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.NegativeY.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.PositiveZ.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));
                cubeMapTexture.NegativeZ.MipMaps.Add(ReadFaceTexture(stream, width, height, blockFormat, dataForEachFace));

                width >>= 1;
                height >>= 1;
            }

            return cubeMapTexture;
        }

        /// <summary>
        /// Read all mip maps and store them in a byte array so the level 0 mipmap will be at the beginning
        /// followed by all other mip map levels.
        /// </summary>
        /// <param name="stream">The stream to read the mipmap data from.</param>
        /// <param name="levelIndices">The level indices.</param>
        /// <returns>A byte array containing all the mipmaps.</returns>
        private static byte[] ReadAllMipMapBytes(Stream stream, LevelIndex[] levelIndices)
        {
            ulong totalBytes = 0;
            for (int i = 0; i < levelIndices.Length; i++)
            {
                totalBytes += levelIndices[i].UncompressedByteLength;
            }

            byte[] allMipMapBytes = new byte[totalBytes];
            var idx = 0;
            for (int i = 0; i < levelIndices.Length; i++)
            {
                stream.Position = (long)levelIndices[i].ByteOffset;
                stream.Read(allMipMapBytes, idx, (int)levelIndices[i].UncompressedByteLength);
                idx += (int)levelIndices[i].UncompressedByteLength;
            }

            return allMipMapBytes;
        }

        private static MipMap<TBlock> ReadFaceTexture<TBlock>(Stream stream, int width, int height, TBlock blockFormat, uint dataForEachFace)
            where TBlock : struct, IBlock<TBlock>
        {
            byte[] faceData = new byte[dataForEachFace];
            ReadTextureData(stream, faceData);
            return new MipMap<TBlock>(blockFormat, faceData, width, height);
        }

        private static void ReadTextureData(Stream stream, byte[] mipMapData)
        {
            int bytesRead = stream.Read(mipMapData, 0, mipMapData.Length);
            if (bytesRead != mipMapData.Length)
            {
                throw new TextureFormatException("could not read enough texture data from the stream");
            }
        }
    }
}
