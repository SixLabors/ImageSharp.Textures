// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Texture for pixel data with the R8G8B8G8 format.
    /// </summary>
    internal struct Rgbg32 : IBlock<Rgbg32>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 32;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 3;

        /// <inheritdoc/>
        public byte DivSize => 1;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 2;

        /// <inheritdoc/>
        public bool Compressed => false;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            int totalPixels = width * height;
            byte[] decompressed = new byte[totalPixels * 3];
            Span<ImageSharp.PixelFormats.Rgb24> rgb24Span = MemoryMarshal.Cast<byte, ImageSharp.PixelFormats.Rgb24>(decompressed);

            var pixel = default(ImageSharp.PixelFormats.Rgb24);
            int pixelIdx = 0;
            for (int i = 0; i < blockData.Length; i += 4)
            {
                byte r = blockData[i];
                byte g0 = blockData[i + 1];
                byte b = blockData[i + 2];
                byte g1 = blockData[i + 3];

                pixel.FromRgb24(new ImageSharp.PixelFormats.Rgb24(r, g0, b));
                rgb24Span[pixelIdx++] = pixel;
                if (pixelIdx >= totalPixels)
                {
                    break;
                }

                pixel.FromRgb24(new ImageSharp.PixelFormats.Rgb24(r, g1, b));
                rgb24Span[pixelIdx++] = pixel;
            }

            return decompressed;
        }
    }
}
