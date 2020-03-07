using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct Rg16 : IBlock<Rg16>
    {
        public int BitsPerPixel => 16;

        public byte PixelDepthBytes => 4;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 4;

        public bool Compressed => false;

        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.Textures.PixelFormats.Rg16>(decompressedData, width, height);
        }

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            return blockData;
        }
    }
}
