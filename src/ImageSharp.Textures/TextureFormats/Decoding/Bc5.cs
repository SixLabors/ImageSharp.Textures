// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Texture compressed with BC5 with two color channels, red and green.
    /// </summary>
    internal struct Bc5 : IBlock<Bc5>
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

            // Should RG format be used instead RGB24?
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgb24>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            IBlock self = this;

            byte[] firstGradient = new byte[8];
            byte[] secondGradient = new byte[8];

            return Helper.InMemoryDecode<Bc5>(blockData, width, height, (stream, data, streamIndex, dataIndex, stride) =>
            {
                streamIndex = ExtractGradient(firstGradient, blockData, streamIndex);
                ulong firstCodes = blockData[streamIndex++];
                firstCodes |= (ulong)blockData[streamIndex++] << 8;
                firstCodes |= (ulong)blockData[streamIndex++] << 16;
                firstCodes |= (ulong)blockData[streamIndex++] << 24;
                firstCodes |= (ulong)blockData[streamIndex++] << 32;
                firstCodes |= (ulong)blockData[streamIndex++] << 40;

                streamIndex = ExtractGradient(secondGradient, blockData, streamIndex);
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
                        // 3 bits determine alpha index to use.
                        byte firstIndex = (byte)((firstCodes >> (alphaShift + (3 * j))) & 0x07);
                        byte secondIndex = (byte)((secondCodes >> (alphaShift + (3 * j))) & 0x07);
                        data[dataIndex++] = firstGradient[firstIndex];
                        data[dataIndex++] = secondGradient[secondIndex];
                        data[dataIndex++] = 0; // Skip blue.
                    }

                    dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                }

                return streamIndex;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ExtractGradient(Span<byte> gradient, Span<byte> stream, int bIndex)
        {
            byte endpoint0;
            byte endpoint1;
            gradient[0] = endpoint0 = stream[bIndex++];
            gradient[1] = endpoint1 = stream[bIndex++];

            if (endpoint0 > endpoint1)
            {
                for (int i = 1; i < 7; i++)
                {
                    gradient[1 + i] = (byte)((((7 - i) * endpoint0) + (i * endpoint1)) / 7);
                }
            }
            else
            {
                for (int i = 1; i < 5; ++i)
                {
                    gradient[1 + i] = (byte)((((5 - i) * endpoint0) + (i * endpoint1)) / 5);
                }

                gradient[6] = 0;
                gradient[7] = 255;
            }

            return bIndex;
        }
    }
}
