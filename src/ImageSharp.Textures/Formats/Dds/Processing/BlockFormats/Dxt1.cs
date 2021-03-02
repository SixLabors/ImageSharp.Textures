// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    /// <summary>
    /// Texture compressed with DXT1.
    /// </summary>
    public struct Dxt1 : IBlock<Dxt1>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 24;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 3;

        /// <inheritdoc/>
        public byte DivSize => 4;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 8;

        /// <inheritdoc/>
        public bool Compressed => true;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            IBlock self = this;
            var colors = new ImageSharp.PixelFormats.Rgb24[4];

            return Helper.InMemoryDecode<Dxt1>(blockData, width, height, (stream, data, streamIndex, dataIndex, stride) =>
            {
                ushort color0 = blockData[streamIndex++];
                color0 |= (ushort)(blockData[streamIndex++] << 8);

                ushort color1 = blockData[streamIndex++];
                color1 |= (ushort)(blockData[streamIndex++] << 8);

                // Extract R5G6B5 (in that order)
                colors[0].R = (byte)(color0 & 0x1f);
                colors[0].G = (byte)((color0 & 0x7E0) >> 5);
                colors[0].B = (byte)((color0 & 0xF800) >> 11);
                colors[0].R = (byte)((colors[0].R << 3) | (colors[0].R >> 2));
                colors[0].G = (byte)((colors[0].G << 2) | (colors[0].G >> 3));
                colors[0].B = (byte)((colors[0].B << 3) | (colors[0].B >> 2));

                colors[1].R = (byte)(color1 & 0x1f);
                colors[1].G = (byte)((color1 & 0x7E0) >> 5);
                colors[1].B = (byte)((color1 & 0xF800) >> 11);
                colors[1].R = (byte)((colors[1].R << 3) | (colors[1].R >> 2));
                colors[1].G = (byte)((colors[1].G << 2) | (colors[1].G >> 3));
                colors[1].B = (byte)((colors[1].B << 3) | (colors[1].B >> 2));

                // Used the two extracted colors to create two new colors that are
                // slightly different.
                if (color0 > color1)
                {
                    colors[2].R = (byte)(((2 * colors[0].R) + colors[1].R) / 3);
                    colors[2].G = (byte)(((2 * colors[0].G) + colors[1].G) / 3);
                    colors[2].B = (byte)(((2 * colors[0].B) + colors[1].B) / 3);

                    colors[3].R = (byte)((colors[0].R + (2 * colors[1].R)) / 3);
                    colors[3].G = (byte)((colors[0].G + (2 * colors[1].G)) / 3);
                    colors[3].B = (byte)((colors[0].B + (2 * colors[1].B)) / 3);
                }
                else
                {
                    colors[2].R = (byte)((colors[0].R + colors[1].R) / 2);
                    colors[2].G = (byte)((colors[0].G + colors[1].G) / 2);
                    colors[2].B = (byte)((colors[0].B + colors[1].B) / 2);

                    colors[3].R = 0;
                    colors[3].G = 0;
                    colors[3].B = 0;
                }

                for (int i = 0; i < 4; i++)
                {
                    // Every 2 bit is a code [0-3] and represent what color the
                    // current pixel is

                    // Read in a byte and thus 4 colors
                    byte rowVal = blockData[streamIndex++];
                    for (int j = 0; j < 8; j += 2)
                    {
                        // Extract code by shifting the row byte so that we can
                        // AND it with 3 and get a value [0-3]
                        ImageSharp.PixelFormats.Rgb24 col = colors[(rowVal >> j) & 0x03];
                        data[dataIndex++] = col.R;
                        data[dataIndex++] = col.G;
                        data[dataIndex++] = col.B;
                    }

                    // Jump down a row and start at the beginning of the row
                    dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                }

                return streamIndex;
            });
        }
    }
}
