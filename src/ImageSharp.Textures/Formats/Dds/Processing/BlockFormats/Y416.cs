// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    public struct Y416 : IBlock<Y416>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 64;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 8;

        /// <inheritdoc/>
        public byte DivSize => 1;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 8;

        /// <inheritdoc/>
        public bool Compressed => false;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<Textures.PixelFormats.Y416>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            return blockData;
        }
    }
}
