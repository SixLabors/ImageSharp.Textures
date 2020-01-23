using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct Rgba : IBlock<Rgba>
    {
        public int BitsPerPixel => 32;

        public ImageFormat Format => ImageFormat.Rgba32;

        public byte PixelDepthBytes => 4;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 4;

        public bool Compressed => false;

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            //for (int i = 0; i < blockData.Length; i += 4)
            //{
            //    byte temp = mipMap.BlockData[i];
            //    mipMap.BlockData[i] = mipMap.BlockData[i + 2];
            //    mipMap.BlockData[i + 2] = temp;
            //}
            return blockData;
        }
    }
}
