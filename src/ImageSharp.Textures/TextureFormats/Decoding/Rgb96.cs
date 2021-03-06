// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Texture which has 32 bits per color channel without a alpha channel (pixel format R32G32B32).
    /// </summary>
    internal struct Rgb96 : IBlock<Rgb96>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 96;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 12;

        /// <inheritdoc/>
        public byte DivSize => 1;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 12;

        /// <inheritdoc/>
        public bool Compressed => false;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<Textures.PixelFormats.Rgb96>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height) => blockData;
    }
}
