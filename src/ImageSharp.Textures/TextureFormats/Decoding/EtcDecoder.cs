// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Decoder for ETC (Ericsson Texture Compression) compressed textures.
    /// Based on https://github.com/hglm/detex.git
    /// </summary>
    /// <remarks>
    /// See ktx specification: https://www.khronos.org/registry/DataFormat/specs/1.3/dataformat.1.3.html#ETC1
    /// </remarks>
    internal static class EtcDecoder
    {
        private static readonly int[,] ModifierTable =
        {
            { 2, 8, -2, -8 },
            { 5, 17, -5, -17 },
            { 9, 29, -9, -29 },
            { 13, 42, -13, -42 },
            { 18, 60, -18, -60 },
            { 24, 80, -24, -80 },
            { 33, 106, -33, -106 },
            { 47, 183, -47, -183 }
        };

        private static readonly int[] Complement3BitShiftedTable =
        {
            0, 8, 16, 24, -32, -24, -16, -8
        };

        private static readonly int[] Etc2DistanceTable = { 3, 6, 11, 16, 23, 32, 41, 64 };

        public static void DecodeEtc1Block(Span<byte> payload, Span<byte> decodedPixelSpan)
        {
            byte red = payload[0];
            byte green = payload[1];
            byte blue = payload[2];
            byte codeWordsWithFlags = payload[3];

            bool diffFlag = (codeWordsWithFlags & 2) != 0;
            bool flipFlag = (codeWordsWithFlags & 1) != 0;
            byte codeword1 = (byte)((codeWordsWithFlags & 224) >> 5);
            byte codeword2 = (byte)((codeWordsWithFlags & 28) >> 2);

            int c0r, c0g, c0b;
            int c1r, c1g, c1b;

            if (diffFlag)
            {
                int c0r5 = red & 0xF8;
                c0r = FiveToEightBit(c0r5);
                int c0g5 = green & 0xF8;
                c0g = FiveToEightBit(c0g5);
                int c0b5 = blue & 0xF8;
                c0b = FiveToEightBit(c0b5);

                int rd3 = Complement3BitShifted(red & 0x7);
                int gd3 = Complement3BitShifted(green & 0x7);
                int bd3 = Complement3BitShifted(blue & 0x7);
                c1r = FiveToEightBit(c0r5 + rd3);
                c1g = FiveToEightBit(c0g5 + gd3);
                c1b = FiveToEightBit(c0b5 + bd3);
            }
            else
            {
                int c0r4 = red & 0xF0;
                c0r = c0r4 | (c0r4 >> 4);
                int c0g4 = green & 0xF0;
                c0g = c0g4 | (c0g4 >> 4);
                int c0b4 = blue & 0xF0;
                c0b = c0b4 | (c0b4 >> 4);

                int c1r4 = red & 0x0F;
                c1r = c1r4 | (c1r4 << 4);
                int c1g4 = green & 0x0F;
                c1g = c1g4 | (c1g4 << 4);
                int c1b4 = blue & 0x0F;
                c1b = c1b4 | (c1b4 << 4);
            }

            uint pixelIndexWord = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(4, 4));

            // Check if the sub-blocks are horizontal or vertical.
            if (!flipFlag)
            {
                // Flip bit indicates horizontal sub-blocks.
                // 0000
                // 0000
                // 1111
                // 1111
                // Iterate over the pixels in each sub-block and set their final values in the image data.
                DecompressEtc1BlockHorizontal(pixelIndexWord, codeword1, codeword2, (byte)c0r, (byte)c0g, (byte)c0b, (byte)c1r, (byte)c1g, (byte)c1b, decodedPixelSpan);
            }
            else
            {
                // Flip bit indicates vertical sub-blocks.
                // 0011
                // 0011
                // 0011
                // 0011
                DecompressEtc1BlockVertical(pixelIndexWord, codeword1, codeword2, (byte)c0r, (byte)c0g, (byte)c0b, (byte)c1r, (byte)c1g, (byte)c1b, decodedPixelSpan);
            }
        }

        private static void DecompressEtc1BlockHorizontal(
            uint pixelIndexWord,
            uint tableCodeword1,
            uint tableCodeword2,
            byte redBaseColorSubBlock1,
            byte greenBaseColorSubBlock1,
            byte blueBaseColorSubBlock1,
            byte redBaseColorSubBlock2,
            byte greenBaseColorSubBlock2,
            byte blueBaseColorSubBlock2,
            Span<byte> pixelBuffer)
        {
            ProcessPixelEtc1(0, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(0, 3));
            ProcessPixelEtc1(1, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(3, 3));
            ProcessPixelEtc1(2, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(6, 3));
            ProcessPixelEtc1(3, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(9, 3));
            ProcessPixelEtc1(4, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(12, 3));
            ProcessPixelEtc1(5, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(15, 3));
            ProcessPixelEtc1(6, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(18, 3));
            ProcessPixelEtc1(7, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(21, 3));

            ProcessPixelEtc1(8, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(24, 3));
            ProcessPixelEtc1(9, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(27, 3));
            ProcessPixelEtc1(10, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(30, 3));
            ProcessPixelEtc1(11, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(33, 3));
            ProcessPixelEtc1(12, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(36, 3));
            ProcessPixelEtc1(13, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(39, 3));
            ProcessPixelEtc1(14, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(42, 3));
            ProcessPixelEtc1(15, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(45, 3));
        }

        private static void DecompressEtc1BlockVertical(
            uint pixelIndexWord,
            uint tableCodeword1,
            uint tableCodeword2,
            byte redBaseColorSubBlock1,
            byte greenBaseColorSubBlock1,
            byte blueBaseColorSubBlock1,
            byte redBaseColorSubBlock2,
            byte greenBaseColorSubBlock2,
            byte blueBaseColorSubBlock2,
            Span<byte> pixelBuffer)
        {
            ProcessPixelEtc1(0, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(0, 3));
            ProcessPixelEtc1(1, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(3, 3));

            ProcessPixelEtc1(2, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(6, 3));
            ProcessPixelEtc1(3, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(9, 3));

            ProcessPixelEtc1(4, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(12, 3));
            ProcessPixelEtc1(5, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(15, 3));

            ProcessPixelEtc1(6, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(18, 3));
            ProcessPixelEtc1(7, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(21, 3));

            ProcessPixelEtc1(8, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(24, 3));
            ProcessPixelEtc1(9, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(27, 3));

            ProcessPixelEtc1(10, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(30, 3));
            ProcessPixelEtc1(11, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(33, 3));

            ProcessPixelEtc1(12, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(36, 3));
            ProcessPixelEtc1(13, pixelIndexWord, tableCodeword1, redBaseColorSubBlock1, greenBaseColorSubBlock1, blueBaseColorSubBlock1, pixelBuffer.Slice(39, 3));

            ProcessPixelEtc1(14, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(42, 3));
            ProcessPixelEtc1(15, pixelIndexWord, tableCodeword2, redBaseColorSubBlock2, greenBaseColorSubBlock2, blueBaseColorSubBlock2, pixelBuffer.Slice(45, 3));
        }

        public static void DecodeEtc2Block(Span<byte> payload, Span<byte> decodedPixelSpan)
        {
            // Figure out the mode.
            if ((payload[3] & 2) == 0)
            {
                // Individual mode.
                DecodeEtc1Block(payload, decodedPixelSpan);
                return;
            }

            int r = payload[0] & 0xF8;
            r += Complement3BitShifted(payload[0] & 7);
            int g = payload[1] & 0xF8;
            g += Complement3BitShifted(payload[1] & 7);
            int b = payload[2] & 0xF8;
            b += Complement3BitShifted(payload[2] & 7);

            decodedPixelSpan.Clear();
            if ((r & 0xFF07) != 0)
            {
                ProcessBlockEtc2TMode(payload, decodedPixelSpan);
                return;
            }

            if ((g & 0xFF07) != 0)
            {
                ProcessBlockEtc2HMode(payload, decodedPixelSpan);
                return;
            }

            if ((b & 0xFF07) != 0)
            {
                // Planar mode.
                ProcessBlockEtc2PlanarMode(payload, decodedPixelSpan);
                return;
            }

            // Differential mode.
            DecodeEtc1Block(payload, decodedPixelSpan);
        }

        private static void ProcessBlockEtc2PlanarMode(Span<byte> payload, Span<byte> decodedPixelSpan)
        {
            // Each color O, H and V is in 6-7-6 format.
            int ro = (payload[0] & 0x7E) >> 1;
            int go = ((payload[0] & 0x1) << 6) | ((payload[1] & 0x7E) >> 1);
            int bo = ((payload[1] & 0x1) << 5) | (payload[2] & 0x18) | ((payload[2] & 0x03) << 1) | ((payload[3] & 0x80) >> 7);
            int rh = ((payload[3] & 0x7C) >> 1) | (payload[3] & 0x1);
            int gh = (payload[4] & 0xFE) >> 1;
            int bh = ((payload[4] & 0x1) << 5) | ((payload[5] & 0xF8) >> 3);
            int rv = ((payload[5] & 0x7) << 3) | ((payload[6] & 0xE0) >> 5);
            int gv = ((payload[6] & 0x1F) << 2) | ((payload[7] & 0xC0) >> 6);
            int bv = payload[7] & 0x3F;

            // Replicate bits.
            ro = (ro << 2) | ((ro & 0x30) >> 4);
            go = (go << 1) | ((go & 0x40) >> 6);
            bo = (bo << 2) | ((bo & 0x30) >> 4);
            rh = (rh << 2) | ((rh & 0x30) >> 4);
            gh = (gh << 1) | ((gh & 0x40) >> 6);
            bh = (bh << 2) | ((bh & 0x30) >> 4);
            rv = (rv << 2) | ((rv & 0x30) >> 4);
            gv = (gv << 1) | ((gv & 0x40) >> 6);
            bv = (bv << 2) | ((bv & 0x30) >> 4);

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    byte r = (byte)Helper.Clamp(((x * (rh - ro)) + (y * (rv - ro)) + (4 * ro) + 2) >> 2, 0, 255);
                    byte g = (byte)Helper.Clamp(((x * (gh - go)) + (y * (gv - go)) + (4 * go) + 2) >> 2, 0, 255);
                    byte b = (byte)Helper.Clamp(((x * (bh - bo)) + (y * (bv - bo)) + (4 * bo) + 2) >> 2, 0, 255);
                    int pixelIdx = ((y * 4) + x) * 3;
                    decodedPixelSpan[pixelIdx] = r;
                    decodedPixelSpan[pixelIdx + 1] = g;
                    decodedPixelSpan[pixelIdx + 2] = b;
                }
            }
        }

        private static void ProcessBlockEtc2TMode(Span<byte> payload, Span<byte> decodedPixelSpan)
        {
            int[] paintColorR = new int[4];
            int[] paintColorG = new int[4];
            int[] paintColorB = new int[4];

            int c0r = ((payload[0] & 0x18) >> 1) | (payload[0] & 0x3);
            c0r |= c0r << 4;
            int c0g = payload[1] & 0xF0;
            c0g |= c0g >> 4;
            int c0b = payload[1] & 0x0F;
            c0b |= c0b << 4;
            int c1r = payload[2] & 0xF0;
            c1r |= c1r >> 4;
            int c1g = payload[2] & 0x0F;
            c1g |= c1g << 4;
            int c1b = payload[3] & 0xF0;
            c1b |= c1b >> 4;

            int distance = Etc2DistanceTable[((payload[3] & 0x0C) >> 1) | (payload[3] & 0x1)];
            paintColorR[0] = c0r;
            paintColorG[0] = c0g;
            paintColorB[0] = c0b;
            paintColorR[2] = c1r;
            paintColorG[2] = c1g;
            paintColorB[2] = c1b;
            paintColorR[1] = Helper.Clamp(c1r + distance, 0, 255);
            paintColorG[1] = Helper.Clamp(c1g + distance, 0, 255);
            paintColorB[1] = Helper.Clamp(c1b + distance, 0, 255);
            paintColorR[3] = Helper.Clamp(c1r - distance, 0, 255);
            paintColorG[3] = Helper.Clamp(c1g - distance, 0, 255);
            paintColorB[3] = Helper.Clamp(c1b - distance, 0, 255);

            uint pixel_index_word = (uint)((payload[4] << 24) | (payload[5] << 16) | (payload[6] << 8) | payload[7]);
            int decodedPixelIdx = 0;
            for (int i = 0; i < 16; i++)
            {
                uint pixel_index = (uint)(((pixel_index_word & (1 << i)) >> i) | ((pixel_index_word & (0x10000 << i)) >> (16 + i - 1)));
                int r = paintColorR[pixel_index];
                int g = paintColorG[pixel_index];
                int b = paintColorB[pixel_index];
                decodedPixelSpan[decodedPixelIdx++] = (byte)r;
                decodedPixelSpan[decodedPixelIdx++] = (byte)g;
                decodedPixelSpan[decodedPixelIdx++] = (byte)b;
            }
        }

        private static void ProcessBlockEtc2HMode(Span<byte> payload, Span<byte> decodedPixelSpan)
        {
            int[] paintColorR = new int[4];
            int[] paintColorG = new int[4];
            int[] paintColorB = new int[4];

            int c0r = (payload[0] & 0x78) >> 3;
            c0r |= c0r << 4;
            int c0g = ((payload[0] & 0x07) << 1) | ((payload[1] & 0x10) >> 4);
            c0g |= c0g << 4;
            int c0b = (payload[1] & 0x08) | ((payload[1] & 0x03) << 1) | ((payload[2] & 0x80) >> 7);
            c0b |= c0b << 4;
            int c1r = (payload[2] & 0x78) >> 3;
            c1r |= c1r << 4;
            int c1g = ((payload[2] & 0x07) << 1) | ((payload[3] & 0x80) >> 7);
            c1g |= c1g << 4;
            int c1b = (payload[3] & 0x78) >> 3;
            c1b |= c1b << 4;

            int baseColor0Value = (c0r << 16) + (c0g << 8) + c0b;
            int baseColor1Value = (c1r << 16) + (c1g << 8) + c1b;
            int bit = baseColor0Value >= baseColor1Value ? 1 : 0;

            int distance = Etc2DistanceTable[(payload[3] & 0x04) | ((payload[3] & 0x01) << 1) | bit];
            paintColorR[0] = Helper.Clamp(c0r + distance, 0, 255);
            paintColorG[0] = Helper.Clamp(c0g + distance, 0, 255);
            paintColorB[0] = Helper.Clamp(c0b + distance, 0, 255);
            paintColorR[1] = Helper.Clamp(c0r - distance, 0, 255);
            paintColorG[1] = Helper.Clamp(c0g - distance, 0, 255);
            paintColorB[1] = Helper.Clamp(c0b - distance, 0, 255);
            paintColorR[2] = Helper.Clamp(c1r + distance, 0, 255);
            paintColorG[2] = Helper.Clamp(c1g + distance, 0, 255);
            paintColorB[2] = Helper.Clamp(c1b + distance, 0, 255);
            paintColorR[3] = Helper.Clamp(c1r - distance, 0, 255);
            paintColorG[3] = Helper.Clamp(c1g - distance, 0, 255);
            paintColorB[3] = Helper.Clamp(c1b - distance, 0, 255);

            uint pixel_index_word = (uint)((payload[4] << 24) | (payload[5] << 16) | (payload[6] << 8) | payload[7]);
            int decodedPixelIdx = 0;
            for (int i = 0; i < 16; i++)
            {
                uint pixel_index = (uint)(((pixel_index_word & (1 << i)) >> i) | ((pixel_index_word & (0x10000 << i)) >> (16 + i - 1)));
                int r = paintColorR[pixel_index];
                int g = paintColorG[pixel_index];
                int b = paintColorB[pixel_index];
                decodedPixelSpan[decodedPixelIdx++] = (byte)r;
                decodedPixelSpan[decodedPixelIdx++] = (byte)g;
                decodedPixelSpan[decodedPixelIdx++] = (byte)b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FiveToEightBit(int color)
        {
            int c0r = color | ((color & 0xE0) >> 5);
            return c0r;
        }

        /// <summary>
        /// This function calculates the 3-bit complement value in the range -4 to 3 of a three bit representation.
        /// The result is arithmetically shifted 3 places to the left before returning.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <returns>3-bit complement value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Complement3BitShifted(int x) => Complement3BitShiftedTable[x];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessPixelEtc1(int i, uint pixelIndexWord, uint tableCodeword, byte redBaseColorSubBlock, byte greenBaseColorSubBlock, byte blueBaseColorSubBlock, Span<byte> pixelBuffer)
        {
            long pixelIndex = ((pixelIndexWord & (1 << i)) >> i) | ((pixelIndexWord & (0x10000 << i)) >> (16 + i - 1));
            int modifier = ModifierTable[tableCodeword, pixelIndex];
            byte red = (byte)Helper.Clamp(0, redBaseColorSubBlock + modifier, 255);
            byte green = (byte)Helper.Clamp(0, greenBaseColorSubBlock + modifier, 255);
            byte blue = (byte)Helper.Clamp(0, blueBaseColorSubBlock + modifier, 255);

            pixelBuffer[0] = red;
            pixelBuffer[1] = green;
            pixelBuffer[2] = blue;
        }
    }
}
