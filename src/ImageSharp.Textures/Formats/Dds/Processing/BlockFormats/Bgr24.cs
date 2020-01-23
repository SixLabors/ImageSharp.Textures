using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct Bgr24 : IBlock<Bgr24>
    {
        public int BitsPerPixel => 24;

        public ImageFormat Format => ImageFormat.Rgb24;

        public byte PixelDepthBytes => 3;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 3;

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
