// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    using System;
    using SixLabors.ImageSharp.Textures.Formats.Dds;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

    public struct Bc5 : IBlock<Bc5>
    {

        public int BitsPerPixel => 24;

        public byte PixelDepthBytes => 3;

        public byte DivSize => 4;

        public byte CompressedBytesPerBlock => 16;

        public bool Compressed => true;

        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
        }

        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            IBlock self = this;

            var _firstGradient = new byte[8];
            var _secondGradient = new byte[8];

            return Helper.InMemoryDecode<Bc5>(blockData, width, height, (byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride) =>
            {

                streamIndex = ExtractGradient(_firstGradient, blockData, streamIndex);
                ulong firstCodes = blockData[streamIndex++];
                firstCodes |= (ulong)blockData[streamIndex++] << 8;
                firstCodes |= (ulong)blockData[streamIndex++] << 16;
                firstCodes |= (ulong)blockData[streamIndex++] << 24;
                firstCodes |= (ulong)blockData[streamIndex++] << 32;
                firstCodes |= (ulong)blockData[streamIndex++] << 40;

                streamIndex = ExtractGradient(_secondGradient, blockData, streamIndex);
                ulong secondCodes = blockData[streamIndex++];
                secondCodes |= (ulong)blockData[streamIndex++] << 8;
                secondCodes |= (ulong)blockData[streamIndex++] << 16;
                secondCodes |= (ulong)blockData[streamIndex++] << 24;
                secondCodes |= (ulong)blockData[streamIndex++] << 32;
                secondCodes |= (ulong)blockData[streamIndex++] << 40;

                for (int alphaShift = 0; alphaShift < 48; alphaShift += 12)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        // 3 bits determine alpha index to use
                        byte firstIndex = (byte)(firstCodes >> alphaShift + 3 * j & 0x07);
                        byte secondIndex = (byte)(secondCodes >> alphaShift + 3 * j & 0x07);
                        data[dataIndex++] = 0; // skip blue
                        data[dataIndex++] = _secondGradient[secondIndex];
                        data[dataIndex++] = _firstGradient[firstIndex];
                    }
                    dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                }

                return streamIndex;

            });
        }

        internal static int ExtractGradient(Span<byte> gradient, Span<byte> stream, int bIndex)
        {
            byte endpoint0;
            byte endpoint1;
            gradient[0] = endpoint0 = stream[(int)bIndex++];
            gradient[1] = endpoint1 = stream[(int)bIndex++];

            if (endpoint0 > endpoint1)
            {
                for (int i = 1; i < 7; i++)
                    gradient[1 + i] = (byte)(((7 - i) * endpoint0 + i * endpoint1) / 7);
            }
            else
            {
                for (int i = 1; i < 5; ++i)
                    gradient[1 + i] = (byte)(((5 - i) * endpoint0 + i * endpoint1) / 5);
                gradient[6] = 0;
                gradient[7] = 255;
            }
            return bIndex;
        }
    }
}
