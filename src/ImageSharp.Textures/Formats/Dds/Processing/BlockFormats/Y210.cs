// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    public struct Y210 : IBlock<Y210>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 64;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 3;

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
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            int totalPixels = width * height;
            byte[] decompressed = new byte[totalPixels * 3];
            Span<ImageSharp.PixelFormats.Rgb24> rgb24Span = MemoryMarshal.Cast<byte, ImageSharp.PixelFormats.Rgb24>(decompressed);

            // Same as Y216 with least significant 6 bits set to zero.
            var pixel = default(ImageSharp.PixelFormats.Rgb24);
            int pixelIdx = 0;
            for (int i = 0; i < blockData.Length; i += 8)
            {
                uint y0 = BitConverter.ToUInt16(blockData, i);
                uint u = BitConverter.ToUInt16(blockData, i + 2);
                uint y1 = BitConverter.ToUInt16(blockData, i + 4);
                uint v = BitConverter.ToUInt16(blockData, i + 6);

                y0 = (y0 >> 6) - 64;
                u = (u >> 6) - 512;
                y1 = (y1 >> 6) - 64;
                v = (v >> 6) - 512;

                Vector4 rgbVec = ColorSpaceConversion.YuvToRgba10Bit(y0, u, v);
                pixel.FromVector4(rgbVec);
                rgb24Span[pixelIdx++] = pixel;
                if (pixelIdx >= totalPixels)
                {
                    break;
                }

                rgbVec = ColorSpaceConversion.YuvToRgba10Bit(y1, u, v);
                pixel.FromVector4(rgbVec);
                rgb24Span[pixelIdx++] = pixel;
            }

            return decompressed;
        }
    }
}
