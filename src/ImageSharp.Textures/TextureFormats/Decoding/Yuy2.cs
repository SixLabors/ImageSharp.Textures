// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.PixelFormats;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// A texture based on the YUV 4:2:2 video resource format. The pixel format will be decoded into Rgb24.
    /// </summary>
    internal struct Yuy2 : IBlock<Yuy2>
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
                int y0 = blockData[i];
                int u = blockData[i + 1];
                int y1 = blockData[i + 2];
                int v = blockData[i + 3];

                y0 -= 16;
                u -= 128;
                y1 -= 16;
                v -= 128;

                Vector4 rgbVec = ColorSpaceConversion.YuvToRgba8Bit(y0, u, v);
                pixel.FromVector4(rgbVec);
                rgb24Span[pixelIdx++] = pixel;
                if (pixelIdx >= totalPixels)
                {
                    break;
                }

                rgbVec = ColorSpaceConversion.YuvToRgba8Bit(y1, u, v);
                pixel.FromVector4(rgbVec);
                rgb24Span[pixelIdx++] = pixel;
            }

            return decompressed;
        }
    }
}
