using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct R5g5b5a1 : IBlock<R5g5b5a1>
    {
        public int BitsPerPixel => 16;

        public ImageFormat Format => ImageFormat.R5g5b5a1;

        public byte PixelDepthBytes => 2;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 2;

        public bool Compressed => false;

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            //for (int i = 0; i < mipMap.BlockData.Length; i += 2)
            //{
            //    byte temp = (byte)(mipMap.BlockData[i] & 0xF);
            //    mipMap.BlockData[i] = (byte)((mipMap.BlockData[i] & 0xF0) + (mipMap.BlockData[i + 1] & 0XF));
            //    mipMap.BlockData[i + 1] = (byte)((mipMap.BlockData[i + 1] & 0xF0) + temp);

            //}
            return blockData;
        }
    }
}
