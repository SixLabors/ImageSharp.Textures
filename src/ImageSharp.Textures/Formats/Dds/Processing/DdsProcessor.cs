// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

using SixLabors.ImageSharp.Textures.Common.Extensions;
using SixLabors.ImageSharp.Textures.Formats.Dds.Emums;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    /// <summary>
    /// Class that represents direct draw surfaces
    /// </summary>
    internal class DdsProcessor
    {
        public DdsProcessor(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10)
        {
            this.DdsHeader = ddsHeader;
            this.DdsHeaderDxt10 = ddsHeaderDxt10;
        }

        public DdsHeader DdsHeader { get; }

        public DdsHeaderDxt10 DdsHeaderDxt10 { get; }

        /*
        private void AllocateMipMaps(Stream stream)
         {
            if (this.DdsHeader.TextureCount() <= 1)
            {
                int width = (int)Math.Max(BlockInfo.DivSize, (int)this.DdsHeader.Width);
                int height = (int)Math.Max(BlockInfo.DivSize, this.DdsHeader.Height);
                int bytesPerPixel = (BlockInfo.BitsPerPixel + 7) / 8;
                int stride = CalcStride(width, BlockInfo.BitsPerPixel);
                int len = stride * height;

                var mipData = new byte[len];
                stream.Read(mipData, 0, len);

                var mipMap = new MipMap(BlockInfo, mipData, false, width, height, stride / bytesPerPixel);
                Swap(mipMap);
                this.mipMaps = new[] { mipMap };
                return;
            }

            mipMaps = new MipMap[this.DdsHeader.TextureCount() - 1];

            for (int i = 0; i < this.DdsHeader.TextureCount() - 1; i++)
            {
                int width = (int)Math.Max(BlockInfo.DivSize, (int)(this.DdsHeader.Width / Math.Pow(2, i + 1)));
                int height = (int)Math.Max(BlockInfo.DivSize, this.DdsHeader.Height / Math.Pow(2, i + 1));

                int bytesPerPixel = (BlockInfo.BitsPerPixel + 7) / 8;
                int stride = CalcStride(width, BlockInfo.BitsPerPixel);
                int len = stride * height;

                var mipData = new byte[len];
                stream.Read(mipData, 0, len);

                var mipMap = new MipMap(BlockInfo, mipData, false, width, height, stride / bytesPerPixel);
                Swap(mipMap);
                mipMaps[i] = mipMap;
            }
         }
        */

        private MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, int count)
            where TBlock : struct, IBlock<TBlock>
        {
            var blockFormat = default(TBlock);

            var mipMaps = new MipMap<TBlock>[count];

            for (int i = 0; i < count; i++)
            {
                int widthBlocks = blockFormat.Compressed ? Helper.CalcBlocks(width) : width;
                int heightBlocks = blockFormat.Compressed ? Helper.CalcBlocks(height) : height;
                int len = heightBlocks * widthBlocks * blockFormat.CompressedBytesPerBlock;

                byte[] mipData = new byte[len];
                int read = stream.Read(mipData, 0, len);
                if (read != len)
                {
                    throw new InvalidDataException();
                }

                mipMaps[i] = new MipMap<TBlock>(blockFormat, mipData, width, height);

                width >>= 1;
                height >>= 1;
            }

            return mipMaps;
        }

        public MipMap[] DecodeDds(Stream stream, int width, int height, int count)
        {
            switch (this.DdsHeader.PixelFormat.FourCC)
            {
                case DdsFourCC.None:
                case DdsFourCC.R16G16B16A16_SNORM:
                case DdsFourCC.R16G16B16A16_UNORM:
                    return this.ProcessUncompressed(stream, width, height, count);
                case DdsFourCC.DXT1:
                    return this.AllocateMipMaps<Dxt1>(stream, width, height, count);
                case DdsFourCC.DXT2:
                case DdsFourCC.DXT4:
                    throw new ArgumentException("Can not support DXT2 or DXT4 due to patents.");
                case DdsFourCC.DXT3:
                    return this.AllocateMipMaps<Dxt3>(stream, width, height, count);
                case DdsFourCC.DXT5:
                    return this.AllocateMipMaps<Dxt5>(stream, width, height, count);
                case DdsFourCC.DX10:
                    return this.GetDx10Dds(stream, width, height, count);
                case DdsFourCC.ATI1:
                case DdsFourCC.BC4U:
                    return this.AllocateMipMaps<Bc4>(stream, width, height, count);
                case DdsFourCC.BC4S:
                    return this.AllocateMipMaps<Bc4s>(stream, width, height, count);
                case DdsFourCC.ATI2:
                case DdsFourCC.BC5U:
                    return this.AllocateMipMaps<Bc5>(stream, width, height, count);
                case DdsFourCC.BC5S:
                    return this.AllocateMipMaps<Bc5s>(stream, width, height, count);
                default:
                    throw new ArgumentException($"FourCC: {this.DdsHeader.PixelFormat.FourCC.FourCcToString()} not supported.");
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
                    // For some reason, some 64 bit format do not have the bitsPerPixel set in the header (its zero).
                    switch (this.DdsHeader.PixelFormat.FourCC)
                    {
                        case DdsFourCC.R16G16B16A16_SNORM:
                        case DdsFourCC.R16G16B16A16_UNORM:
                            return this.SixtyFourBitImageFormat(stream, width, height, count);
                    }

                    throw new Exception($"Unrecognized rgb bit count: {this.DdsHeader.PixelFormat.RGBBitCount}");
            }
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

            throw new Exception("Unsupported 16 bit format");
        }

        private MipMap[] TwentyFourBitImageFormat(Stream stream, int width, int height, int count)
        {
            DdsPixelFormat pixelFormat = this.DdsHeader.PixelFormat;

            bool hasAlpha = pixelFormat.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels);

            if (!hasAlpha && pixelFormat.RBitMask == 0xFF0000 && pixelFormat.GBitMask == 0xFF00 && pixelFormat.BBitMask == 0xFF)
            {
                return this.AllocateMipMaps<Rgb24>(stream, width, height, count);
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

            // R11G11B10_Float
            throw new Exception("Unsupported 32 bit format");
        }

        private MipMap[] SixtyFourBitImageFormat(Stream stream, int width, int height, int count)
        { 
            return this.AllocateMipMaps<Rgba64>(stream, width, height, count);
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
                    return this.AllocateMipMaps<Rgba32>(stream, width, height, count);
                case DxgiFormat.B8G8R8A8_Typeless:
                case DxgiFormat.B8G8R8A8_UNorm:
                case DxgiFormat.B8G8R8A8_UNorm_SRGB:
                    return this.AllocateMipMaps<Bgra32>(stream, width, height, count);
                case DxgiFormat.R32G32B32A32_Typeless:
                case DxgiFormat.R32G32B32A32_Float:
                case DxgiFormat.R32G32B32A32_UInt:
                case DxgiFormat.R32G32B32A32_SInt:
                    throw new Exception("not implemented");
                case DxgiFormat.R32G32B32_Typeless:
                case DxgiFormat.R32G32B32_Float:
                case DxgiFormat.R32G32B32_UInt:
                case DxgiFormat.R32G32B32_SInt:
                    throw new Exception("not implemented");
                case DxgiFormat.R16G16B16A16_Typeless:
                case DxgiFormat.R16G16B16A16_Float:
                case DxgiFormat.R16G16B16A16_UNorm:
                case DxgiFormat.R16G16B16A16_UInt:
                case DxgiFormat.R16G16B16A16_SNorm:
                case DxgiFormat.R16G16B16A16_SInt:
                    return this.AllocateMipMaps<Rgba64>(stream, width, height, count);
                case DxgiFormat.R32G32_Typeless:
                case DxgiFormat.R32G32_Float:
                case DxgiFormat.R32G32_UInt:
                case DxgiFormat.R32G32_SInt:
                    throw new Exception("not implemented");
                case DxgiFormat.R10G10B10A2_Typeless:
                case DxgiFormat.R10G10B10A2_UNorm:
                case DxgiFormat.R10G10B10A2_UInt:
                    return this.AllocateMipMaps<Rgba1010102>(stream, width, height, count);
                case DxgiFormat.R16G16_Float:
                    throw new Exception("not implemented");
                case DxgiFormat.R16G16_Typeless:
                case DxgiFormat.R16G16_UNorm:
                case DxgiFormat.R16G16_UInt:
                case DxgiFormat.R16G16_SNorm:
                case DxgiFormat.R16G16_SInt:
                    return this.AllocateMipMaps<Rg32>(stream, width, height, count);
                case DxgiFormat.R32_Float:
                    throw new Exception("not implemented");
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
                    throw new Exception("not implemented");
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
                case DxgiFormat.R32G8X24_Typeless:
                case DxgiFormat.D32_Float_S8X24_UInt:
                case DxgiFormat.R32_Float_X8X24_Typeless:
                case DxgiFormat.X32_Typeless_G8X24_UInt:
                case DxgiFormat.D32_Float:
                case DxgiFormat.R11G11B10_Float:
                case DxgiFormat.R24G8_Typeless:
                case DxgiFormat.D24_UNorm_S8_UInt:
                case DxgiFormat.R24_UNorm_X8_Typeless:
                case DxgiFormat.X24_Typeless_G8_UInt:
                case DxgiFormat.D16_UNorm:
                case DxgiFormat.R9G9B9E5_SharedExp:
                case DxgiFormat.R8G8_B8G8_UNorm:
                case DxgiFormat.G8R8_G8B8_UNorm:
                case DxgiFormat.R10G10B10_XR_BIAS_A2_UNorm:
                case DxgiFormat.NV12:
                case DxgiFormat.P010:
                case DxgiFormat.P016:
                case DxgiFormat.Opaque_420:
                case DxgiFormat.YUY2:
                case DxgiFormat.Y210:
                case DxgiFormat.Y216:
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
                    throw new Exception($"Unsupported format {this.DdsHeaderDxt10.DxgiFormat}");
            }
        }
    }
}
