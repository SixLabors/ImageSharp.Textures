// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    public struct Dxt5 : IBlock<Dxt5>
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
            byte[] alpha = new byte[8];
            var colors = new ImageSharp.PixelFormats.Rgb24[4];

            return Helper.InMemoryDecode<Dxt5>(blockData, width, height, (byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride) =>
            {
                streamIndex = Bc5.ExtractGradient(alpha, blockData, streamIndex);

                ulong alphaCodes = blockData[streamIndex++];
                alphaCodes |= (ulong)blockData[streamIndex++] << 8;
                alphaCodes |= (ulong)blockData[streamIndex++] << 16;
                alphaCodes |= (ulong)blockData[streamIndex++] << 24;
                alphaCodes |= (ulong)blockData[streamIndex++] << 32;
                alphaCodes |= (ulong)blockData[streamIndex++] << 40;

                // Colors are stored in a pair of 16 bits
                ushort color0 = blockData[streamIndex++];
                color0 |= (ushort)(blockData[streamIndex++] << 8);

                ushort color1 = blockData[streamIndex++];
                color1 |= (ushort)(blockData[streamIndex++] << 8);

                // Extract R5G6B5 (in that order)
                colors[0].R = (byte)(color0 & 0x1f);
                colors[0].G = (byte)((color0 & 0x7E0) >> 5);
                colors[0].B = (byte)((color0 & 0xF800) >> 11);
                colors[0].R = (byte)(colors[0].R << 3 | colors[0].R >> 2);
                colors[0].G = (byte)(colors[0].G << 2 | colors[0].G >> 3);
                colors[0].B = (byte)(colors[0].B << 3 | colors[0].B >> 2);

                colors[1].R = (byte)(color1 & 0x1f);
                colors[1].G = (byte)((color1 & 0x7E0) >> 5);
                colors[1].B = (byte)((color1 & 0xF800) >> 11);
                colors[1].R = (byte)(colors[1].R << 3 | colors[1].R >> 2);
                colors[1].G = (byte)(colors[1].G << 2 | colors[1].G >> 3);
                colors[1].B = (byte)(colors[1].B << 3 | colors[1].B >> 2);

                colors[2].R = (byte)(((2 * colors[0].R) + colors[1].R) / 3);
                colors[2].G = (byte)(((2 * colors[0].G) + colors[1].G) / 3);
                colors[2].B = (byte)(((2 * colors[0].B) + colors[1].B) / 3);

                colors[3].R = (byte)((colors[0].R + (2 * colors[1].R)) / 3);
                colors[3].G = (byte)((colors[0].G + (2 * colors[1].G)) / 3);
                colors[3].B = (byte)((colors[0].B + (2 * colors[1].B)) / 3);

                for (int alphaShift = 0; alphaShift < 48; alphaShift += 12)
                {
                    byte rowVal = blockData[streamIndex++];
                    for (int j = 0; j < 4; j++)
                    {
                        // 3 bits determine alpha index to use
                        byte alphaIndex = (byte)((alphaCodes >> (alphaShift + (3 * j))) & 0x07);
                        ImageSharp.PixelFormats.Rgb24 col = colors[(rowVal >> (j * 2)) & 0x03];
                        data[dataIndex++] = col.R;
                        data[dataIndex++] = col.G;
                        data[dataIndex++] = col.B;
                        data[dataIndex++] = alpha[alphaIndex];
                    }

                    dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                }

                return streamIndex;
            });
        }
    }
}
