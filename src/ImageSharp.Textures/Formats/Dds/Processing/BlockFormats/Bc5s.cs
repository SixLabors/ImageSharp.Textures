// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    public struct Bc5s : IBlock<Bc5s>
    {
        /// <inheritdoc/>
        public int BitsPerPixel => 24;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 3;

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
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            IBlock self = this;

            return Helper.InMemoryDecode<Bc5s>(blockData, width, height, (byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride) =>
            {
                sbyte red0 = (sbyte)blockData[streamIndex++];
                sbyte red1 = (sbyte)blockData[streamIndex++];
                red0 = red0 == -128 ? (sbyte)-127 : red0;
                red1 = red1 == -128 ? (sbyte)-127 : red1;
                ulong rIndex = blockData[streamIndex++];
                rIndex |= (ulong)blockData[streamIndex++] << 8;
                rIndex |= (ulong)blockData[streamIndex++] << 16;
                rIndex |= (ulong)blockData[streamIndex++] << 24;
                rIndex |= (ulong)blockData[streamIndex++] << 32;
                rIndex |= (ulong)blockData[streamIndex++] << 40;

                sbyte green0 = (sbyte)blockData[streamIndex++];
                sbyte green1 = (sbyte)blockData[streamIndex++];
                green0 = green0 == -128 ? (sbyte)-127 : green0;
                green1 = green1 == -128 ? (sbyte)-127 : green1;
                ulong gIndex = blockData[streamIndex++];
                gIndex |= (ulong)blockData[streamIndex++] << 8;
                gIndex |= (ulong)blockData[streamIndex++] << 16;
                gIndex |= (ulong)blockData[streamIndex++] << 24;
                gIndex |= (ulong)blockData[streamIndex++] << 32;
                gIndex |= (ulong)blockData[streamIndex++] << 40;

                for (int i = 0; i < 16; ++i)
                {
                    byte rSel = (byte)((uint)(rIndex >> (3 * i)) & 0x07);
                    byte gSel = (byte)((uint)(gIndex >> (3 * i)) & 0x07);

                    data[dataIndex++] = 0; // skip blue
                    data[dataIndex++] = Bc4s.InterpolateColor(gSel, green0, green1);
                    data[dataIndex++] = Bc4s.InterpolateColor(rSel, red0, red1);

                    // Is mult 4?
                    if ((i + 1 & 0x3) == 0)
                    {
                        dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                    }
                }

                return streamIndex;
            });
        }
    }
}
