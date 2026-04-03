// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Texture for data with three channels (RGB) as 16 bit floats (48 bits total).
    /// </summary>
    internal struct Rgb48Float : IBlock<Rgb48Float>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 48;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 6;

        /// <inheritdoc/>
        public byte DivSize => 1;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 6;

        /// <inheritdoc/>
        public bool Compressed => false;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<Textures.PixelFormats.Rgb48Float>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height) => blockData;
    }
}
