// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    using System;
    using System.IO;
    using SixLabors.ImageSharp.Textures.Formats.Dds;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Emums;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Extensions;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;
    using SixLabors.ImageSharp.Textures.TextureFormats;

    /// <summary>
    /// Class that represents direct draw surfaces
    /// </summary>
    internal class DdsProcessor
    {
        public DdsHeader DdsHeader { get; }

        public DdsHeaderDxt10 DdsHeaderDxt10 { get; }

        public DdsProcessor(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10)
        {
            this.DdsHeader = ddsHeader;
            this.DdsHeaderDxt10 = ddsHeaderDxt10;
        }

        private MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, int count)
            where TBlock : struct, IBlock<TBlock>
        {
            var blockFormat = default(TBlock);

            var mipMaps = new MipMap<TBlock>[count];

            for (int i = 0; i < count; i++)
            {
                int widthBlocks = Helper.CalcBlocks(width);
                int heightBlocks = Helper.CalcBlocks(height);
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
                case DdsFourCC.DXT1:
                    return this.AllocateMipMaps<Dxt1>(stream, width, height, count);
                case DdsFourCC.DXT2:
                case DdsFourCC.DXT4:
                    throw new ArgumentException("Cannot support DXT2 or DXT4");
                case DdsFourCC.DXT3:
                    return this.AllocateMipMaps<Dxt3>(stream, width, height, count);
                case DdsFourCC.DXT5:
                    return this.AllocateMipMaps<Dxt5>(stream, width, height, count);
                case DdsFourCC.None:
                    //dds = new DdsUncompressed(ddsHeader, ddsHeaderDxt10)
                    throw new ArgumentException($"FourCC: {this.DdsHeader.PixelFormat.FourCC} not supported.");
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
                    throw new ArgumentException($"FourCC: {this.DdsHeader.PixelFormat.FourCC} not supported.");
            }
        }

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
                    return this.AllocateMipMaps<Rgba>(stream, width, height, count);
                case DxgiFormat.B8G8R8A8_Typeless:
                case DxgiFormat.B8G8R8A8_UNorm:
                case DxgiFormat.B8G8R8A8_UNorm_SRGB:
                    return this.AllocateMipMaps<Bgra32>(stream, width, height, count);
                case DxgiFormat.Unknown:
                case DxgiFormat.R32G32B32A32_Typeless:
                case DxgiFormat.R32G32B32A32_Float:
                case DxgiFormat.R32G32B32A32_UInt:
                case DxgiFormat.R32G32B32A32_SInt:
                case DxgiFormat.R32G32B32_Typeless:
                case DxgiFormat.R32G32B32_Float:
                case DxgiFormat.R32G32B32_UInt:
                case DxgiFormat.R32G32B32_SInt:
                case DxgiFormat.R16G16B16A16_Typeless:
                case DxgiFormat.R16G16B16A16_Float:
                case DxgiFormat.R16G16B16A16_UNorm:
                case DxgiFormat.R16G16B16A16_UInt:
                case DxgiFormat.R16G16B16A16_SNorm:
                case DxgiFormat.R16G16B16A16_SInt:
                case DxgiFormat.R32G32_Typeless:
                case DxgiFormat.R32G32_Float:
                case DxgiFormat.R32G32_UInt:
                case DxgiFormat.R32G32_SInt:
                case DxgiFormat.R32G8X24_Typeless:
                case DxgiFormat.D32_Float_S8X24_UInt:
                case DxgiFormat.R32_Float_X8X24_Typeless:
                case DxgiFormat.X32_Typeless_G8X24_UInt:
                case DxgiFormat.R10G10B10A2_Typeless:
                case DxgiFormat.R10G10B10A2_UNorm:
                case DxgiFormat.R10G10B10A2_UInt:
                case DxgiFormat.R11G11B10_Float:
                case DxgiFormat.R16G16_Typeless:
                case DxgiFormat.R16G16_Float:
                case DxgiFormat.R16G16_UNorm:
                case DxgiFormat.R16G16_UInt:
                case DxgiFormat.R16G16_SNorm:
                case DxgiFormat.R16G16_SInt:
                case DxgiFormat.R32_Typeless:
                case DxgiFormat.D32_Float:
                case DxgiFormat.R32_Float:
                case DxgiFormat.R32_UInt:
                case DxgiFormat.R32_SInt:
                case DxgiFormat.R24G8_Typeless:
                case DxgiFormat.D24_UNorm_S8_UInt:
                case DxgiFormat.R24_UNorm_X8_Typeless:
                case DxgiFormat.X24_Typeless_G8_UInt:
                case DxgiFormat.R8G8_Typeless:
                case DxgiFormat.R8G8_UNorm:
                case DxgiFormat.R8G8_UInt:
                case DxgiFormat.R8G8_SNorm:
                case DxgiFormat.R8G8_SInt:
                case DxgiFormat.R16_Typeless:
                case DxgiFormat.R16_Float:
                case DxgiFormat.D16_UNorm:
                case DxgiFormat.R16_UNorm:
                case DxgiFormat.R16_UInt:
                case DxgiFormat.R16_SNorm:
                case DxgiFormat.R16_SInt:
                case DxgiFormat.R8_Typeless:
                case DxgiFormat.R8_UNorm:
                case DxgiFormat.R8_UInt:
                case DxgiFormat.R8_SNorm:
                case DxgiFormat.R8_SInt:
                case DxgiFormat.A8_UNorm:
                case DxgiFormat.R1_UNorm:
                case DxgiFormat.R9G9B9E5_SharedExp:
                case DxgiFormat.R8G8_B8G8_UNorm:
                case DxgiFormat.G8R8_G8B8_UNorm:
                case DxgiFormat.B8G8R8X8_UNorm:
                case DxgiFormat.R10G10B10_XR_BIAS_A2_UNorm:
                case DxgiFormat.B8G8R8X8_Typeless:
                case DxgiFormat.B8G8R8X8_UNorm_SRGB:
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
