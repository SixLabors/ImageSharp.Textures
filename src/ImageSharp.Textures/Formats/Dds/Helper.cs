// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    public static class Helper
    {
        public delegate int DecodeDelegate(byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride);

        public static int CalcBlocks(int pixels) => Math.Max(1, (pixels + 3) / 4);

        public static byte[] InMemoryDecode<TBlock>(byte[] memBuffer, int width, int height, DecodeDelegate decode)
            where TBlock : struct, IBlock
        {
            TBlock blockFormat = default;
            int heightBlocks = Helper.CalcBlocks(height);
            int widthBlocks = Helper.CalcBlocks(width);
            int StridePixels = widthBlocks * blockFormat.DivSize;
            int deflatedStrideBytes = StridePixels * blockFormat.PixelDepthBytes;
            int dataLen = heightBlocks * blockFormat.DivSize * deflatedStrideBytes;
            byte[] data = new byte[dataLen];

            int pixelsLeft = data.Length;
            int dataIndex = 0;
            int bIndex = 0;

            int stridePixels = widthBlocks * blockFormat.DivSize;
            int stride = stridePixels * blockFormat.PixelDepthBytes;
            int blocksPerStride = widthBlocks;
            int indexPixelsLeft = heightBlocks * blockFormat.DivSize * stride;

            while (indexPixelsLeft > 0)
            {
                int origDataIndex = dataIndex;

                for (int i = 0; i < blocksPerStride; i++)
                {
                    bIndex = decode.Invoke(memBuffer, data, bIndex, dataIndex, stridePixels);
                    dataIndex += blockFormat.DivSize * blockFormat.PixelDepthBytes;
                }

                int filled = stride * blockFormat.DivSize;
                pixelsLeft -= filled;
                indexPixelsLeft -= filled;

                // Jump down to the block that is exactly (divSize - 1)
                // below the current row we are on
                dataIndex = origDataIndex + filled;
            }

            return data;
        }
    }
}
