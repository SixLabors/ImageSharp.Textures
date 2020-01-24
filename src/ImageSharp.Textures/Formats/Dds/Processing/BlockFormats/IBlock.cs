using System;
using System.Collections.Generic;
using System.Text;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{

    public interface IBlock<TSelf> : IBlock
        where TSelf : struct, IBlock<TSelf>
    {
    }


    public interface IBlock
    {
        int BitsPerPixel { get; }
        ImageFormat Format { get; }
        byte PixelDepthBytes { get; }
        byte DivSize { get; }
        byte CompressedBytesPerBlock { get; }
        bool Compressed { get; }

        byte[] Decompress(byte[] blockData, int width, int height);
    }
}
