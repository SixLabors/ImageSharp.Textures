// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Extensions;

    /// <summary>
    /// Class representing decoding compressed direct draw surfaces
    /// </summary>
    internal abstract class DdsCompressed
    {
        private MipMap[] mipMaps = new MipMap[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="DdsCompressed"/> class.
        /// </summary>
        /// <param name="ddsHeader"><see cref="DdsHeader" /></param>
        /// <param name="ddsHeaderDxt10"><see cref="DdsHeaderDxt10" /></param>
        protected DdsCompressed(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10)
        {
        }

        /// <summary>
        /// Gets the mip map offset data.
        /// </summary>
        public MipMap[] MipMaps => this.mipMaps;





        /// <summary>
        /// Decompress a given block.
        /// </summary>
        protected abstract int Decode(Span<byte> stream, Span<byte> data, int streamIndex, int dataIndex, int stride);


        /// <inheritdoc/>
        //public int Stride => this.DeflatedStrideBytes;

        //private int BytesPerStride => this.WidthBlocks * this.BlockInfo.CompressedBytesPerBlock;

        //private int WidthBlocks => this.CalcBlocks((int)this.DdsHeader.Width);

        //private int HeightBlocks => this.CalcBlocks((int)this.DdsHeader.Height);

        //private int StridePixels => this.WidthBlocks * this.BlockInfo.DivSize;

        //private int DeflatedStrideBytes => this.StridePixels * this.BlockInfo.PixelDepthBytes;

        private int CalcBlocks(int pixels) => Math.Max(1, (pixels + 3) / 4);

        /// <summary>
        /// Creates data buffer for mip maps.
        /// </summary>
        /// <returns>The allocated size.</returns>
        private void AllocateMipMaps(Stream stream)
        {
            //int len = this.HeightBlocks * this.BlockInfo.DivSize * this.DeflatedStrideBytes;

            //if (this.DdsHeader.TextureCount() <= 1)
            //{
            //    int width = (int)this.DdsHeader.Width;
            //    int height = (int)this.DdsHeader.Height;
            //    int widthBlocks = this.CalcBlocks(width);

            //    int stridePixels = widthBlocks * this.BlockInfo.DivSize;
            //    int stride = stridePixels * this.BlockInfo.PixelDepthBytes;

            //    var mipData = new byte[len];
            //    stream.Read(mipData, 0, len);

            //    this.mipMaps = new[] { new MipMap(this.BlockInfo, mipData, true, width, height, widthBlocks) };
            //    return;
            //}

            //this.mipMaps = new MipMap[this.DdsHeader.TextureCount() - 1];
            //for (int i = 1; i < this.DdsHeader.TextureCount(); i++)
            //{
            //    int width = (int)(this.DdsHeader.Width / Math.Pow(2, i));
            //    int height = (int)(this.DdsHeader.Height / Math.Pow(2, i));
            //    int widthBlocks = this.CalcBlocks(width);
            //    int heightBlocks = this.CalcBlocks(height);

            //    int stridePixels = widthBlocks * this.BlockInfo.DivSize;
            //    int stride = stridePixels * this.BlockInfo.PixelDepthBytes;

            //    len = heightBlocks * this.BlockInfo.DivSize * stride;

            //    var mipData = new byte[len];
            //    stream.Read(mipData, 0, len);

            //    this.mipMaps[i - 1] = new MipMap(this.BlockInfo, mipData, true, width, height, widthBlocks);
            //}
        }

        //public static int Translate(Stream str, byte[] buf, int bufLen, int bufIndex)
        //{
        //    Buffer.BlockCopy(buf, bufIndex, buf, 0, bufLen - bufIndex);
        //    int result = str.Read(buf, bufLen - bufIndex, bufIndex);
        //    return result + bufLen - bufIndex;
        //}

        //private byte[] InMemoryDecode(MipMap mipMap)
        //{
        //    Decode(memBuffer, data, bIndex, (uint)dataIndex, mipMap.Stride);

        //}

        /// <summary>
        /// Decompresses encoded data.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="array">encoded data buffer</param>
        /// <returns>The <see cref="Image{TPixel}"/>.</returns>
        protected void Decode(Stream stream)
        {
            AllocateMipMaps(stream);
            //var heightBlockAligned = HeightBlocks;
            //long totalSize = WidthBlocks * CompressedBytesPerBlock * heightBlockAligned;

            //for (int i = 1; i < this.DdsHeader.TextureCount(); i++)
            //{
            //    var width = (int)(this.DdsHeader.Width / Math.Pow(2, i));
            //    var height = (int)(this.DdsHeader.Height / Math.Pow(2, i));
            //    var widthBlocks = CalcBlocks(width);
            //    var heightBlocks = CalcBlocks(height);
            //    totalSize += widthBlocks * heightBlocks * CompressedBytesPerBlock;
            //}

            //DataLen = (int)totalSize;
            //Data = config.Allocator.Rent((int)totalSize);
            //_compressed = true;
            //Util.Fill(stream, Data, DataLen, config.BufferSize);


            //    var totalLen = AllocateMipMaps();
            //    byte[] data = new byte[totalLen];
            //    var pixelsLeft = totalLen;
            //    int dataIndex = 0;

            //    int imageIndex = 0;
            //    int divSize = DivSize;
            //    int stride = DeflatedStrideBytes;
            //    int blocksPerStride = WidthBlocks;
            //    int indexPixelsLeft = HeightBlocks * DivSize * stride;
            //    var stridePixels = StridePixels;
            //    int bytesPerStride = BytesPerStride;

            //    int bufferSize;
            //    byte[] streamBuffer = new byte[0x8000];

            //    do
            //    {
            //        int workingSize;
            //        bufferSize = workingSize = stream.Read(streamBuffer, 0, 0x8000);
            //        int bIndex = 0;
            //        while (workingSize > 0 && indexPixelsLeft > 0)
            //        {
            //            // If there is not enough of the buffer to fill the next
            //            // set of 16 square pixels Get the next buffer
            //            if (workingSize < bytesPerStride)
            //            {
            //                bufferSize = workingSize = Translate(stream, streamBuffer, 0x8000, bIndex);
            //                bIndex = 0;
            //            }

            //            var origDataIndex = dataIndex;

            //            // Now that we have enough pixels to fill a stride (and
            //            // this includes the normally 4 pixels below the stride)
            //            for (uint i = 0; i < blocksPerStride; i++)
            //            {
            //                bIndex = Decode(streamBuffer, data, bIndex, dataIndex, stridePixels);

            //                // Advance to the next block, which is (pixel depth *
            //                // divSize) bytes away
            //                dataIndex += divSize * PixelDepthBytes;
            //            }

            //            // Each decoded block is divSize by divSize so pixels left
            //            // is Width * multiplied by block height
            //            workingSize -= bytesPerStride;

            //            var filled = stride * divSize;
            //            pixelsLeft -= filled;
            //            indexPixelsLeft -= filled;

            //            // Jump down to the block that is exactly (divSize - 1)
            //            // below the current row we are on
            //            dataIndex = origDataIndex + filled;

            //            if (indexPixelsLeft <= 0 && imageIndex < MipMaps.Length)
            //            {
            //                var mip = MipMaps[imageIndex];
            //                var widthBlocks = CalcBlocks(mip.Width);
            //                var heightBlocks = CalcBlocks(mip.Height);
            //                stridePixels = widthBlocks * DivSize;
            //                stride = stridePixels * PixelDepthBytes;
            //                blocksPerStride = widthBlocks;
            //                indexPixelsLeft = heightBlocks * DivSize * stride;
            //                bytesPerStride = widthBlocks * CompressedBytesPerBlock;
            //                imageIndex++;
            //            }
            //        }
            //    } while (bufferSize != 0 && pixelsLeft > 0);
        }
    }
}
