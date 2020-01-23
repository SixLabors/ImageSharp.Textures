using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct Rgb8 : IBlock<Rgb8>
    {
        public int BitsPerPixel => 8;

        public ImageFormat Format => ImageFormat.Rgb8;

        public byte PixelDepthBytes => 1;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 1;

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
