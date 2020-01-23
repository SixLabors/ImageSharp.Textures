using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    public static class Helper
    {
        public static int CalcBlocks(int pixels) => Math.Max(1, (pixels + 3) / 4);

        public delegate int DecodeDelegate(byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride);

        public static byte[] InMemoryDecode<TBlock>(byte[] memBuffer, int width, int height, DecodeDelegate decode) where TBlock : struct, IBlock
        {
            TBlock blockFormat = default;
            int HeightBlocks = Helper.CalcBlocks(height);
            int WidthBlocks = Helper.CalcBlocks(width);
            int StridePixels = WidthBlocks * blockFormat.DivSize;
            int DeflatedStrideBytes = StridePixels * blockFormat.PixelDepthBytes;
            int DataLen = HeightBlocks * blockFormat.DivSize * DeflatedStrideBytes;
            byte[] data = new byte[DataLen];

            int pixelsLeft = (int)data.Length;
            int dataIndex = 0;
            int bIndex = 0;


            int stridePixels = WidthBlocks * blockFormat.DivSize;
            int stride = stridePixels * blockFormat.PixelDepthBytes;
            int blocksPerStride = WidthBlocks;
            int indexPixelsLeft = HeightBlocks * blockFormat.DivSize * stride;


            while (indexPixelsLeft > 0)
            {
                int origDataIndex = dataIndex;

                for (int i = 0; i < blocksPerStride; i++)
                {
                    bIndex = decode.Invoke(memBuffer, data, bIndex, (int)dataIndex, (int)stridePixels);
                    dataIndex += (int)(blockFormat.DivSize * blockFormat.PixelDepthBytes);
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
