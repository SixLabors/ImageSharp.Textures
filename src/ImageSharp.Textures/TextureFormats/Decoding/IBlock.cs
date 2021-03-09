// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Interface for a block texture.
    /// </summary>
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

        /// <summary>
        /// Gets the div size.
        /// </summary>
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
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The Image.</returns>
        Image GetImage(byte[] blockData, int width, int height);

        /// <summary>
        /// Gets the decompressed data.
        /// </summary>
        /// <param name="blockData">The block data bytes.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The decompressed byte data of the texture.</returns>
        byte[] Decompress(byte[] blockData, int width, int height);
    }
}
