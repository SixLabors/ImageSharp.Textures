// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    public interface IBlock
    {
        /// <summary>
        /// Gets the bits per pixel.
        /// </summary>
        int BitsPerPixel { get; }

        byte PixelDepthBytes { get; }

        byte DivSize { get; }

        byte CompressedBytesPerBlock { get; }

        bool Compressed { get; }

        Image GetImage(byte[] blockData, int width, int height);

        byte[] Decompress(byte[] blockData, int width, int height);
    }
}
