using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{ 
    public struct Bgr565 : IBlock<Bgr565>
    {
        public int BitsPerPixel => 16;

        public byte PixelDepthBytes => 2;

        public byte DivSize => 1;

        public byte CompressedBytesPerBlock => 2;

        public bool Compressed => false;

        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.Bgr565>(decompressedData, width, height);
        }

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            return blockData;
        }
    }
}
