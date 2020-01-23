using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct Bgr16 : IBlock<Bgr16>
    {
        public int BitsPerPixel => 16;

        public ImageFormat Format => ImageFormat.Rgba16;

        public byte PixelDepthBytes => 2;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 2;

        public bool Compressed => false;

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            return blockData;
        }
    }
}
