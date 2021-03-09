// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Common.Extensions;
using SixLabors.ImageSharp.Textures.Formats.Dds.Emums;
using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;
using Fp32 = SixLabors.ImageSharp.Textures.TextureFormats.Decoding.Fp32;
using L32 = SixLabors.ImageSharp.Textures.TextureFormats.Decoding.L32;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    /// <summary>
    /// Decodes direct draw surfaces.
    /// </summary>
    internal class DdsProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DdsProcessor" /> class.
        /// </summary>
        /// <param name="ddsHeader">The DDS header.</param>
        /// <param name="ddsHeaderDxt10">The DDS header DXT10.</param>
        public DdsProcessor(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10)
        {
            this.DdsHeader = ddsHeader;
            this.DdsHeaderDxt10 = ddsHeaderDxt10;
        }

        /// <summary>
        /// Gets the dds header.
        /// </summary>
        public DdsHeader DdsHeader { get; }

        /// <summary>
        /// Gets the dxt 10 header.
        /// </summary>
        public DdsHeaderDxt10 DdsHeaderDxt10 { get; }

        /// <summary>
        /// Decodes the mipmaps of a DDS textures.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="count">The mipmap count.</param>
        /// <returns>The decoded mipmaps.</returns>
        public MipMap[] DecodeDds(Stream stream, int width, int height, int count)
        {
            Guard.MustBeGreaterThan(count, 0, nameof(count));

            switch (this.DdsHeader.PixelFormat.FourCC)
            {
                case DdsFourCc.None:
                case DdsFourCc.R16FLOAT:
                case DdsFourCc.R16G16FLOAT:
                case DdsFourCc.R16G16B16A16SNORM:
                case DdsFourCc.R16G16B16A16UNORM:
                case DdsFourCc.R16G16B16A16FLOAT:
                case DdsFourCc.R32FLOAT:
                case DdsFourCc.R32G32FLOAT:
                case DdsFourCc.R32G32B32A32FLOAT:
                case DdsFourCc.YUY2:
                case DdsFourCc.RGBG:
                case DdsFourCc.GRGB:
                    return this.ProcessUncompressed(stream, width, height, count);
                case DdsFourCc.DXT1:
                    return this.AllocateMipMaps<Dxt1>(stream, width, height, count);
                case DdsFourCc.DXT2:
                case DdsFourCc.DXT4:
                    throw new NotSupportedException("Due to patentsCan, DXT2 or DXT4 cannot be supported.");
                case DdsFourCc.DXT3:
                    return this.AllocateMipMaps<Dxt3>(stream, width, height, count);
                case DdsFourCc.DXT5:
                    return this.AllocateMipMaps<Dxt5>(stream, width, height, count);
                case DdsFourCc.DX10:
                    return this.GetDx10Dds(stream, width, height, count);
                case DdsFourCc.ATI1:
                case DdsFourCc.BC4U:
                    return this.AllocateMipMaps<Bc4>(stream, width, height, count);
                case DdsFourCc.BC4S:
                    return this.AllocateMipMaps<Bc4s>(stream, width, height, count);
                case DdsFourCc.ATI2:
                case DdsFourCc.BC5U:
                    return this.AllocateMipMaps<Bc5>(stream, width, height, count);
                case DdsFourCc.BC5S:
                    return this.AllocateMipMaps<Bc5s>(stream, width, height, count);
                default:
                    throw new NotSupportedException($"FourCC: {this.DdsHeader.PixelFormat.FourCC.FourCcToString()} not supported.");
            }
        }

        public MipMap[] ProcessUncompressed(Stream stream, int width, int height, int count)
        {
            uint bitsPerPixel = this.DdsHeader.PixelFormat.RGBBitCount;
            switch (bitsPerPixel)
            {
                case 8:
                    return this.EightBitImageFormat(stream, width, height, count);
                case 16:
                    return this.SixteenBitImageFormat(stream, width, height, count);
                case 24:
                    return this.TwentyFourBitImageFormat(stream, width, height, count);
                case 32:
                    return this.ThirtyTwoBitImageFormat(stream, width, height, count);
                default:
                    // For unknown reason some formats do not have the bitsPerPixel set in the header (its zero).
                    switch (this.DdsHeader.PixelFormat.FourCC)
                    {
                        case DdsFourCc.R16FLOAT:
                            return this.SixteenBitImageFormat(stream, width, height, count);
                        case DdsFourCc.R32FLOAT:
                        case DdsFourCc.R16G16FLOAT:
                        case DdsFourCc.YUY2:
                        case DdsFourCc.RGBG:
                        case DdsFourCc.GRGB:
                            return this.ThirtyTwoBitImageFormat(stream, width, height, count);
                        case DdsFourCc.R16G16B16A16SNORM:
                        case DdsFourCc.R16G16B16A16UNORM:
                        case DdsFourCc.R16G16B16A16FLOAT:
                        case DdsFourCc.R32G32FLOAT:
                            return this.SixtyFourBitImageFormat(stream, width, height, count);
                        case DdsFourCc.R32G32B32A32FLOAT:
                            return this.HundredTwentyEightBitImageFormat(stream, width, height, count);
                    }

                    throw new Exception($"Unrecognized rgb bit count: {this.DdsHeader.PixelFormat.RGBBitCount}");
            }
        }

        /// <summary>
        /// Allocates and decodes all mipmap levels of a DDS texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="count">The mipmap count.</param>
        /// <returns>The decoded mipmaps.</returns>
        private MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, int count)
            where TBlock : struct, IBlock<TBlock>
        {
            var blockFormat = default(TBlock);

            var mipMaps = new MipMap<TBlock>[count];

            for (int i = 0; i < count; i++)
            {
                int widthBlocks = blockFormat.Compressed ? Helper.CalcBlocks(width) : width;
                int heightBlocks = blockFormat.Compressed ? Helper.CalcBlocks(height) : height;
                int bytesToRead = heightBlocks * widthBlocks * blockFormat.CompressedBytesPerBlock;

                // Special case for yuv formats with a single pixel.
                if (bytesToRead < blockFormat.BitsPerPixel / 8)
                {
                    bytesToRead = blockFormat.BitsPerPixel / 8;
                }

                byte[] mipData = new byte[bytesToRead];
                int read = stream.Read(mipData, 0, bytesToRead);
                if (read != bytesToRead)
                {
                    throw new TextureFormatException("could not read enough texture data from the stream");
                }

                mipMaps[i] = new MipMap<TBlock>(blockFormat, mipData, width, height);

                width >>= 1;
                height >>= 1;
            }

            return mipMaps;
        }

        private MipMap[] EightBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            bool hasAlpha = pixelFormat.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels);

            if (pixelFormat.RBitMask == 0x0 && pixelFormat.GBitMask == 0x0 && pixelFormat.BBitMask == 0x0)
            {
                return this.AllocateMipMaps<A8>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xFF && pixelFormat.GBitMask == 0x0 && pixelFormat.BBitMask == 0x0)
            {
                return this.AllocateMipMaps<L8>(stream, width, height, count);
            }

            throw new Exception("Unsupported 8 bit format");
        }

        private MipMap[] SixteenBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            bool hasAlpha = pixelFormat.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels);

            if (hasAlpha && pixelFormat.RBitMask == 0xF00 && pixelFormat.GBitMask == 0xF0 && pixelFormat.BBitMask == 0xF)
            {
                return this.AllocateMipMaps<Bgra16>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0x7C00 && pixelFormat.GBitMask == 0x3E0 && pixelFormat.BBitMask == 0x1F)
            {
                return this.AllocateMipMaps<Bgr555>(stream, width, height, count);
            }

            if (hasAlpha && pixelFormat.RBitMask == 0x7C00 && pixelFormat.GBitMask == 0x3E0 && pixelFormat.BBitMask == 0x1F)
            {
                return this.AllocateMipMaps<Bgra5551>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xF800 && pixelFormat.GBitMask == 0x7E0 && pixelFormat.BBitMask == 0x1F)
            {
                return this.AllocateMipMaps<Bgr565>(stream, width, height, count);
            }

            if (hasAlpha && pixelFormat.RBitMask == 0xFF && pixelFormat.GBitMask == 0x0 && pixelFormat.BBitMask == 0x0)
            {
                return this.AllocateMipMaps<La16>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xFFFF && pixelFormat.GBitMask == 0x0 && pixelFormat.BBitMask == 0x0)
            {
                return this.AllocateMipMaps<La16>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xFF && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0x0)
            {
                return this.AllocateMipMaps<Rg16>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.R16FLOAT)
            {
                return this.AllocateMipMaps<R16Float>(stream, width, height, count);
            }

            throw new Exception("Unsupported 16 bit format");
        }

        private MipMap[] TwentyFourBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            bool hasAlpha = pixelFormat.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels);

            if (!hasAlpha && pixelFormat.RBitMask == 0xFF0000 && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0xFF)
            {
                return this.AllocateMipMaps<Bgr24>(stream, width, height, count);
            }

            throw new Exception("Unsupported 24 bit format");
        }

        private MipMap[] ThirtyTwoBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            bool hasAlpha = pixelFormat.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels);

            if (hasAlpha && pixelFormat.RBitMask == 0xFF0000 && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0xFF)
            {
                return this.AllocateMipMaps<Bgra32>(stream, width, height, count);
            }

            if (hasAlpha && pixelFormat.RBitMask == 0xFF && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0xFF0000)
            {
                return this.AllocateMipMaps<Rgba32>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xFF0000 && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0xFF)
            {
                return this.AllocateMipMaps<Bgr32>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xFF && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0xFF0000)
            {
                return this.AllocateMipMaps<Rgb32>(stream, width, height, count);
            }

            if (!hasAlpha && pixelFormat.RBitMask == 0xFFFF && pixelFormat.GBitMask == 0xFFFF0000 && pixelFormat.BBitMask == 0x0)
            {
                return this.AllocateMipMaps<Rg32>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.R32FLOAT)
            {
                return this.AllocateMipMaps<Fp32>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.R16G16FLOAT)
            {
                return this.AllocateMipMaps<R16G16Float>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.YUY2)
            {
                return this.AllocateMipMaps<Yuy2>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.RGBG)
            {
                return this.AllocateMipMaps<R8G8B8G8>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.GRGB)
            {
                return this.AllocateMipMaps<G8R8G8B8>(stream, width, height, count);
            }

            throw new Exception("Unsupported 32 bit format");
        }

        private MipMap[] SixtyFourBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            if (pixelFormat.FourCC == DdsFourCc.R16G16B16A16SNORM || pixelFormat.FourCC == DdsFourCc.R16G16B16A16UNORM)
            {
                return this.AllocateMipMaps<Rgba64>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.R32G32FLOAT)
            {
                return this.AllocateMipMaps<R32G32Float>(stream, width, height, count);
            }

            if (pixelFormat.FourCC == DdsFourCc.R16G16B16A16FLOAT)
            {
                return this.AllocateMipMaps<R16G16B16A16Float>(stream, width, height, count);
            }

            throw new Exception("Unsupported 64 bit format");
        }

        private MipMap[] HundredTwentyEightBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            if (pixelFormat.FourCC == DdsFourCc.R32G32B32A32FLOAT || pixelFormat.FourCC == DdsFourCc.R32FLOAT)
            {
                return this.AllocateMipMaps<R32G32B32A32Float>(stream, width, height, count);
            }

            throw new Exception("Unsupported 128 bit format");
        }

        /*
         https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide
         https://docs.microsoft.com/en-us/windows/win32/api/dxgiformat/ne-dxgiformat-dxgi_format
        */

        private MipMap[] GetDx10Dds(Stream stream, int width, int height, int count)
        {
            switch (this.DdsHeaderDxt10.DxgiFormat)
            {
                case DxgiFormat.BC1_Typeless:
                case DxgiFormat.BC1_UNorm_SRGB:
                case DxgiFormat.BC1_UNorm:
                    return this.AllocateMipMaps<Dxt1>(stream, width, height, count);
                case DxgiFormat.BC2_Typeless:
                case DxgiFormat.BC2_UNorm:
                case DxgiFormat.BC2_UNorm_SRGB:
                    return this.AllocateMipMaps<Dxt3>(stream, width, height, count);
                case DxgiFormat.BC3_Typeless:
                case DxgiFormat.BC3_UNorm:
                case DxgiFormat.BC3_UNorm_SRGB:
                    return this.AllocateMipMaps<Dxt5>(stream, width, height, count);
                case DxgiFormat.BC4_Typeless:
                case DxgiFormat.BC4_UNorm:
                    return this.AllocateMipMaps<Bc4>(stream, width, height, count);
                case DxgiFormat.BC4_SNorm:
                    return this.AllocateMipMaps<Bc4s>(stream, width, height, count);
                case DxgiFormat.BC5_Typeless:
                case DxgiFormat.BC5_UNorm:
                    return this.AllocateMipMaps<Bc5>(stream, width, height, count);
                case DxgiFormat.BC5_SNorm:
                    return this.AllocateMipMaps<Bc5s>(stream, width, height, count);
                case DxgiFormat.BC6H_Typeless:
                case DxgiFormat.BC6H_UF16:
                    return this.AllocateMipMaps<Bc6h>(stream, width, height, count);
                case DxgiFormat.BC6H_SF16:
                    return this.AllocateMipMaps<Bc6hs>(stream, width, height, count);
                case DxgiFormat.BC7_Typeless:
                case DxgiFormat.BC7_UNorm:
                case DxgiFormat.BC7_UNorm_SRGB:
                    return this.AllocateMipMaps<Bc7>(stream, width, height, count);
                case DxgiFormat.R8G8B8A8_Typeless:
                case DxgiFormat.R8G8B8A8_UNorm:
                case DxgiFormat.R8G8B8A8_UNorm_SRGB:
                case DxgiFormat.R8G8B8A8_UInt:
                case DxgiFormat.R8G8B8A8_SNorm:
                case DxgiFormat.R8G8B8A8_SInt:
                case DxgiFormat.B8G8R8X8_Typeless:
                case DxgiFormat.B8G8R8X8_UNorm:
                case DxgiFormat.B8G8R8X8_UNorm_SRGB:
                    return this.AllocateMipMaps<Bgra32>(stream, width, height, count);
                case DxgiFormat.B8G8R8A8_Typeless:
                case DxgiFormat.B8G8R8A8_UNorm:
                case DxgiFormat.B8G8R8A8_UNorm_SRGB:
                    return this.AllocateMipMaps<Bgra32>(stream, width, height, count);
                case DxgiFormat.R32G32B32A32_Float:
                    return this.AllocateMipMaps<R32G32B32A32Float>(stream, width, height, count);
                case DxgiFormat.R32G32B32A32_Typeless:
                case DxgiFormat.R32G32B32A32_UInt:
                case DxgiFormat.R32G32B32A32_SInt:
                    return this.AllocateMipMaps<R32G32B32A32>(stream, width, height, count);
                case DxgiFormat.R32G32B32_Float:
                    return this.AllocateMipMaps<R32G32B32Float>(stream, width, height, count);
                case DxgiFormat.R32G32B32_Typeless:
                case DxgiFormat.R32G32B32_UInt:
                case DxgiFormat.R32G32B32_SInt:
                    return this.AllocateMipMaps<R32G32B32>(stream, width, height, count);
                case DxgiFormat.R16G16B16A16_Typeless:
                case DxgiFormat.R16G16B16A16_Float:
                case DxgiFormat.R16G16B16A16_UNorm:
                case DxgiFormat.R16G16B16A16_UInt:
                case DxgiFormat.R16G16B16A16_SNorm:
                case DxgiFormat.R16G16B16A16_SInt:
                    return this.AllocateMipMaps<Rgba64>(stream, width, height, count);
                case DxgiFormat.R32G32_Float:
                    return this.AllocateMipMaps<R32G32Float>(stream, width, height, count);
                case DxgiFormat.R32G32_Typeless:
                case DxgiFormat.R32G32_UInt:
                case DxgiFormat.R32G32_SInt:
                    return this.AllocateMipMaps<R32G32>(stream, width, height, count);
                case DxgiFormat.R10G10B10A2_Typeless:
                case DxgiFormat.R10G10B10A2_UNorm:
                case DxgiFormat.R10G10B10A2_UInt:
                    return this.AllocateMipMaps<Rgba1010102>(stream, width, height, count);
                case DxgiFormat.R16G16_Float:
                    return this.AllocateMipMaps<R16G16Float>(stream, width, height, count);
                case DxgiFormat.R16G16_Typeless:
                case DxgiFormat.R16G16_UNorm:
                case DxgiFormat.R16G16_UInt:
                case DxgiFormat.R16G16_SNorm:
                case DxgiFormat.R16G16_SInt:
                    return this.AllocateMipMaps<Rg32>(stream, width, height, count);
                case DxgiFormat.R32_Float:
                    return this.AllocateMipMaps<Fp32>(stream, width, height, count);
                case DxgiFormat.R32_Typeless:
                case DxgiFormat.R32_UInt:
                case DxgiFormat.R32_SInt:
                    // Treating single channel format as 32 bit gray image.
                    return this.AllocateMipMaps<L32>(stream, width, height, count);
                case DxgiFormat.R8G8_Typeless:
                case DxgiFormat.R8G8_UNorm:
                case DxgiFormat.R8G8_UInt:
                case DxgiFormat.R8G8_SNorm:
                case DxgiFormat.R8G8_SInt:
                    return this.AllocateMipMaps<Rg16>(stream, width, height, count);
                case DxgiFormat.R16_Float:
                    return this.AllocateMipMaps<R16Float>(stream, width, height, count);
                case DxgiFormat.R16_Typeless:
                case DxgiFormat.R16_UNorm:
                case DxgiFormat.R16_UInt:
                case DxgiFormat.R16_SNorm:
                case DxgiFormat.R16_SInt:
                    // Treating single channel format as 16 bit gray image.
                    return this.AllocateMipMaps<L16>(stream, width, height, count);
                case DxgiFormat.R8_Typeless:
                case DxgiFormat.R8_UNorm:
                case DxgiFormat.R8_UInt:
                case DxgiFormat.R8_SNorm:
                case DxgiFormat.R8_SInt:
                    // Treating single channel format as 8 bit gray image.
                    return this.AllocateMipMaps<L8>(stream, width, height, count);
                case DxgiFormat.A8_UNorm:
                    return this.AllocateMipMaps<A8>(stream, width, height, count);
                case DxgiFormat.R1_UNorm:
                    throw new Exception("not implemented");
                case DxgiFormat.R11G11B10_Float:
                    return this.AllocateMipMaps<R11G11B10Float>(stream, width, height, count);
                case DxgiFormat.Y410:
                    return this.AllocateMipMaps<Y410>(stream, width, height, count);
                case DxgiFormat.Y416:
                    return this.AllocateMipMaps<Y416>(stream, width, height, count);
                case DxgiFormat.Y210:
                    return this.AllocateMipMaps<Y210>(stream, width, height, count);
                case DxgiFormat.Y216:
                    return this.AllocateMipMaps<Y216>(stream, width, height, count);
                case DxgiFormat.AYUV:
                    return this.AllocateMipMaps<Ayuv>(stream, width, height, count);
                case DxgiFormat.YUY2:
                    return this.AllocateMipMaps<Yuy2>(stream, width, height, count);
                case DxgiFormat.R8G8_B8G8_UNorm:
                    return this.AllocateMipMaps<R8G8B8G8>(stream, width, height, count);
                case DxgiFormat.G8R8_G8B8_UNorm:
                    return this.AllocateMipMaps<G8R8G8B8>(stream, width, height, count);
                case DxgiFormat.R32G8X24_Typeless:
                case DxgiFormat.D32_Float_S8X24_UInt:
                case DxgiFormat.R32_Float_X8X24_Typeless:
                case DxgiFormat.X32_Typeless_G8X24_UInt:
                case DxgiFormat.D32_Float:
                case DxgiFormat.R24G8_Typeless:
                case DxgiFormat.D24_UNorm_S8_UInt:
                case DxgiFormat.R24_UNorm_X8_Typeless:
                case DxgiFormat.X24_Typeless_G8_UInt:
                case DxgiFormat.D16_UNorm:
                case DxgiFormat.R9G9B9E5_SharedExp:
                case DxgiFormat.R10G10B10_XR_BIAS_A2_UNorm:
                case DxgiFormat.NV12:
                case DxgiFormat.P010:
                case DxgiFormat.P016:
                case DxgiFormat.Opaque_420:
                case DxgiFormat.NV11:
                case DxgiFormat.AI44:
                case DxgiFormat.IA44:
                case DxgiFormat.P8:
                case DxgiFormat.A8P8:
                case DxgiFormat.B4G4R4A4_UNorm:
                case DxgiFormat.P208:
                case DxgiFormat.V208:
                case DxgiFormat.V408:
                case DxgiFormat.Unknown:
                default:
                    throw new NotSupportedException($"Unsupported format {this.DdsHeaderDxt10.DxgiFormat}");
            }
        }
    }
}
