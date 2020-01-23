// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    using System;
    using System.Runtime.CompilerServices;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

    public struct Bc4 : IBlock<Bc4>
    {

        public int BitsPerPixel => 8;

        public ImageFormat Format => ImageFormat.Rgb8;

        public byte PixelDepthBytes => 1;

        public byte DivSize => 4;

        public byte CompressedBytesPerBlock => 8;

        public bool Compressed => true;

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            IBlock self = this;

            return Helper.InMemoryDecode<Bc4>(blockData, width, height, (byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride) =>
            {

                byte red0 = blockData[streamIndex++];
                byte red1 = blockData[streamIndex++];
                ulong rIndex = blockData[streamIndex++];
                rIndex |= (ulong)blockData[streamIndex++] << 8;
                rIndex |= (ulong)blockData[streamIndex++] << 16;
                rIndex |= (ulong)blockData[streamIndex++] << 24;
                rIndex |= (ulong)blockData[streamIndex++] << 32;
                rIndex |= (ulong)blockData[streamIndex++] << 40;

                for (int i = 0; i < 16; ++i)
                {
                    byte index = (byte)((uint)(rIndex >> 3 * i) & 0x07);

                    data[dataIndex++] = InterpolateColor(index, red0, red1);

                    // Is mult 4?
                    if ((i + 1 & 0x3) == 0)
                    {
                        dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                    }
                }

                return streamIndex;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte InterpolateColor(byte index, byte red0, byte red1)
        {
            byte red;
            if (index == 0)
            {
                red = red0;
            }
            else if (index == 1)
            {
                red = red1;
            }
            else
            {
                if (red0 > red1)
                {
                    index -= 1;
                    red = (byte)((red0 * (7 - index) + red1 * index) / 7.0f + 0.5f);
                }
                else
                {
                    if (index == 6)
                    {
                        red = 0;
                    }
                    else if (index == 7)
                    {
                        red = 255;
                    }
                    else
                    {
                        index -= 1;
                        red = (byte)((red0 * (5 - index) + red1 * index) / 5.0f + 0.5f);
                    }
                }
            }
            return red;
        }

    }
}
