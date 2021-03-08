// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    /// <summary>
    /// Texture for pixel data with 11 bits for red and green and 10 bits for the blue channel as float.
    /// </summary>
    internal struct R11G11B10Float : IBlock<R11G11B10Float>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 32;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 4;

        /// <inheritdoc/>
        public byte DivSize => 1;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 4;

        /// <inheritdoc/>
        public bool Compressed => false;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<R11G11B10f>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height) => blockData;
    }
}
