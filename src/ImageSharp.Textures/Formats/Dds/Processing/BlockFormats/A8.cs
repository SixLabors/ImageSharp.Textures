using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct A8 : IBlock<A8>
    {
        public int BitsPerPixel => 8;

        public byte PixelDepthBytes => 1;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 1;

        public bool Compressed => false;

        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.A8>(decompressedData, width, height);
        }

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            return blockData;
        }
    }
}
