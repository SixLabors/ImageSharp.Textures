// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Common.Helpers;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    /// <summary>
    /// Texture compressed with DXT3.
    /// </summary>
    public struct Dxt3 : IBlock<Dxt3>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 32;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 4;

        /// <inheritdoc/>
        public byte DivSize => 4;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 16;

        /// <inheritdoc/>
        public bool Compressed => true;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgba32>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            IBlock self = this;
            var colors = new ImageSharp.PixelFormats.Rgb24[4];

            return Helper.InMemoryDecode<Dxt3>(blockData, width, height, (stream, data, streamIndex, dataIndex, stride) =>
            {
                /*
                 * Strategy for decompression:
                 * -We're going to decode both alpha and color at the same time
                 * to save on space and time as we don't have to allocate an array
                 * to store values for later use.
                 */

            // Remember where the alpha data is stored so we can decode simultaneously.
            int alphaPtr = streamIndex;

            // Jump ahead to the color data.
            streamIndex += 8;

            // Colors are stored in a pair of 16 bits.
            ushort color0 = blockData[streamIndex++];
            color0 |= (ushort)(blockData[streamIndex++] << 8);

            ushort color1 = blockData[streamIndex++];
            color1 |= (ushort)(blockData[streamIndex++] << 8);

            // Extract R5G6B5.
            var r = (color0 & 0xF800) >> 11;
            var g = (color0 & 0x7E0) >> 5;
            var b = color0 & 0x1f;
            colors[0].R = PixelUtils.GetBytesFrom5BitValue(r);
            colors[0].G = PixelUtils.GetBytesFrom6BitValue(g);
            colors[0].B = PixelUtils.GetBytesFrom5BitValue(b);

            r = (color1 & 0xF800) >> 11;
            g = (color1 & 0x7E0) >> 5;
            b = color1 & 0x1f;
            colors[1].R = PixelUtils.GetBytesFrom5BitValue(r);
            colors[1].G = PixelUtils.GetBytesFrom6BitValue(g);
            colors[1].B = PixelUtils.GetBytesFrom5BitValue(b);

            // Used the two extracted colors to create two new colors
            // that are slightly different.
            colors[2].R = (byte)(((2 * colors[0].R) + colors[1].R) / 3);
            colors[2].G = (byte)(((2 * colors[0].G) + colors[1].G) / 3);
            colors[2].B = (byte)(((2 * colors[0].B) + colors[1].B) / 3);

            colors[3].R = (byte)((colors[0].R + (2 * colors[1].R)) / 3);
            colors[3].G = (byte)((colors[0].G + (2 * colors[1].G)) / 3);
            colors[3].B = (byte)((colors[0].B + (2 * colors[1].B)) / 3);

            for (int i = 0; i < 4; i++)
            {
                byte rowVal = blockData[streamIndex++];

                // Each row of rgb values have 4 alpha values that  are encoded in 4 bits.
                ushort rowAlpha = blockData[alphaPtr++];
                rowAlpha |= (ushort)(blockData[alphaPtr++] << 8);

                for (int j = 0; j < 8; j += 2)
                {
                    byte currentAlpha = (byte)((rowAlpha >> (j * 2)) & 0x0f);
                    currentAlpha |= (byte)(currentAlpha << 4);
                    ImageSharp.PixelFormats.Rgb24 col = colors[(rowVal >> j) & 0x03];
                    data[dataIndex++] = col.R;
                    data[dataIndex++] = col.G;
                    data[dataIndex++] = col.B;
                    data[dataIndex++] = currentAlpha;
                }

                dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
            }

            return streamIndex;
            });
        }
    }
}
