// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Textures;
    using SixLabors.ImageSharp.Textures.Formats.Dds;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Emums;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Extensions;

    /// <summary>
    /// A DirectDraw Surface that is not compressed.  
    /// Thus what is in the input stream gets directly translated to the image buffer.
    /// </summary>
    internal class DdsUncompressed 
    {
        private readonly bool _rgbSwapped;
        private MipMap[] mipMaps = new MipMap[0];

        //internal DdsUncompressed(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10, uint bitsPerPixel, bool rgbSwapped)
        //    : base(ddsHeader, ddsHeaderDxt10)
        //{
        //    _rgbSwapped = rgbSwapped;

        //    BlockInfo = GetBlockInfo((int)bitsPerPixel);
        //}

        //internal DdsUncompressed(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10)
        //    : base(ddsHeader, ddsHeaderDxt10)
        //{
        //    _rgbSwapped = this.DdsHeader.PixelFormat.RBitMask < this.DdsHeader.PixelFormat.GBitMask;

        //    BlockInfo = GetBlockInfo((int)ddsHeader.PixelFormat.RGBBitCount);
        //}

        //public override MipMap[] MipMaps => mipMaps;


        //protected override void Decode(Stream stream)
        //{
        //    AllocateMipMaps(stream);
        //}

        //public ImageFormat GetImageFormat(int bitsPerPixel)
        //{

        //    switch (bitsPerPixel)
        //    {
        //        case 8:
        //            return ImageFormat.Rgb8;
        //        case 16:
        //            return this.SixteenBitImageFormat();
        //        case 24:
        //            return ImageFormat.Rgb24;
        //        case 32:
        //            return ImageFormat.Rgba32;
        //        default:
        //            throw new Exception($"Unrecognized rgb bit count: {bitsPerPixel}");
        //    }
        //}

        //public BlockInfo GetBlockInfo(int bitsPerPixel)
        //{
        //    //div, block, depth
        //    switch (bitsPerPixel)
        //    {
        //        //case 8:
        //        //    return new BlockInfo
        //        //    {
        //        //        BlockFormat = BlockFormat.Uncompressed,
        //        //        BitsPerPixel = 8,
        //        //        Format = ImageFormat.Rgb8,
        //        //        PixelDepthBytes = 1,
        //        //        DivSize = 1,
        //        //        CompressedBytesPerBlock = 8
        //        //    };
        //        //case 16:
        //        //    ImageFormat format = this.SixteenBitImageFormat();
        //        //    return new BlockInfo
        //        //    {
        //        //        BlockFormat = BlockFormat.Uncompressed,
        //        //        BitsPerPixel = 16,
        //        //        Format = format,
        //        //        PixelDepthBytes = 2,
        //        //        DivSize = 1,
        //        //        CompressedBytesPerBlock = 16
        //        //    };
        //        //case 24:
        //        //    return new BlockInfo
        //        //    {
        //        //        BlockFormat = BlockFormat.Uncompressed,
        //        //        BitsPerPixel = 24,
        //        //        Format = ImageFormat.Rgb24,
        //        //        PixelDepthBytes = 3,
        //        //        DivSize = 1,
        //        //        CompressedBytesPerBlock = 24
        //        //    };
        //        //case 32:
        //        //    return new BlockInfo
        //        //    {
        //        //        BlockFormat = BlockFormat.Uncompressed,
        //        //        BitsPerPixel = 32,
        //        //        Format = ImageFormat.Rgba32,
        //        //        PixelDepthBytes = 4,
        //        //        DivSize = 1,
        //        //        CompressedBytesPerBlock = 32
        //        //    };
        //        default:
        //            throw new Exception($"Unrecognized rgb bit count: {this.DdsHeader.PixelFormat.RGBBitCount}");
        //    }
        //}



        //private ImageFormat SixteenBitImageFormat()
        //{
        //    var pf = this.DdsHeader.PixelFormat;

        //    if (pf.ABitMask == 0xF000 && pf.RBitMask == 0xF00 && pf.GBitMask == 0xF0 && pf.BBitMask == 0xF)
        //    {
        //        return ImageFormat.Rgba16;
        //    }

        //    if (pf.Flags.HasFlag(DdsPixelFormatFlags.AlphaPixels))
        //    {
        //        return ImageFormat.R5g5b5a1;
        //    }

        //    return pf.GBitMask == 0x7e0 ? ImageFormat.R5g6b5 : ImageFormat.R5g5b5;
        //}

        //private void Swap(MipMap mipMap)
        //{
        //    // Swap the R and B channels
        //    if (_rgbSwapped)
        //    {
        //        switch (BlockInfo.Format)
        //        {
        //            case ImageFormat.Rgba32:
        //                for (int i = 0; i < mipMap.BlockData.Length; i += 4)
        //                {
        //                    byte temp = mipMap.BlockData[i];
        //                    mipMap.BlockData[i] = mipMap.BlockData[i + 2];
        //                    mipMap.BlockData[i + 2] = temp;
        //                }
        //                break;
        //            case ImageFormat.Rgba16:
        //                for (int i = 0; i < mipMap.BlockData.Length; i += 2)
        //                {
        //                    byte temp = (byte)(mipMap.BlockData[i] & 0xF);
        //                    mipMap.BlockData[i] = (byte)((mipMap.BlockData[i] & 0xF0) + (mipMap.BlockData[i + 1] & 0XF));
        //                    mipMap.BlockData[i + 1] = (byte)((mipMap.BlockData[i + 1] & 0xF0) + temp);

        //                }
        //                break;
        //            default:
        //                throw new Exception($"Do not know how to swap {BlockInfo.Format}");
        //        }
        //    }
        //}

        //private void AllocateMipMaps(Stream stream)
        //{
        //    if (this.DdsHeader.TextureCount() <= 1)
        //    {
        //        int width = (int)Math.Max(BlockInfo.DivSize, (int)this.DdsHeader.Width);
        //        int height = (int)Math.Max(BlockInfo.DivSize, this.DdsHeader.Height);
        //        int bytesPerPixel = (BlockInfo.BitsPerPixel + 7) / 8;
        //        int stride = CalcStride(width, BlockInfo.BitsPerPixel);
        //        int len = stride * height;

        //        var mipData = new byte[len];
        //        stream.Read(mipData, 0, len);

        //        var mipMap = new MipMap(BlockInfo, mipData, false, width, height, stride / bytesPerPixel);
        //        Swap(mipMap);
        //        this.mipMaps = new[] { mipMap };
        //        return;
        //    }

        //    mipMaps = new MipMap[this.DdsHeader.TextureCount() - 1];

        //    for (int i = 0; i < this.DdsHeader.TextureCount() - 1; i++)
        //    {
        //        int width = (int)Math.Max(BlockInfo.DivSize, (int)(this.DdsHeader.Width / Math.Pow(2, i + 1)));
        //        int height = (int)Math.Max(BlockInfo.DivSize, this.DdsHeader.Height / Math.Pow(2, i + 1));

        //        int bytesPerPixel = (BlockInfo.BitsPerPixel + 7) / 8;
        //        int stride = CalcStride(width, BlockInfo.BitsPerPixel);
        //        int len = stride * height;

        //        var mipData = new byte[len];
        //        stream.Read(mipData, 0, len);

        //        var mipMap = new MipMap(BlockInfo, mipData, false, width, height, stride / bytesPerPixel);
        //        Swap(mipMap);
        //        mipMaps[i] = mipMap;
        //    }
        //}
    }
}
