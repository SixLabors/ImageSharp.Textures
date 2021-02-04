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

        /// <summary>
        /// Gets the pixel depth in bytes.
        /// </summary>
        byte PixelDepthBytes { get; }

        byte DivSize { get; }

        /// <summary>
        /// Gets the number of compressed bytes per block.
        /// </summary>
        byte CompressedBytesPerBlock { get; }

        /// <summary>
        /// Gets a value indicating whether this block is compressed.
        /// </summary>
        bool Compressed { get; }

        /// <summary>
        /// Gets the image from the (maybe compressed) block data.
        /// </summary>
        /// <param name="blockData">The block data bytes.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>The Image.</returns>
        Image GetImage(byte[] blockData, int width, int height);

        /// <summary>
        /// Gets the decompressed data.
        /// </summary>
        /// <param name="blockData">The block data bytes.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>The decompressed byte data of the image.</returns>
        byte[] Decompress(byte[] blockData, int width, int height);
    }
}
