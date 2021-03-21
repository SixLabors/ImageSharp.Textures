// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

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
    /// See ktx spec: https://www.khronos.org/registry/DataFormat/specs/1.3/dataformat.1.3.html#ETC1
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

        public static void DecodeEtc1Block(Span<byte> payload, Span<byte> decodedPixelSpan)
        {
            var red = payload[0];
            var green = payload[1];
            var blue = payload[2];
            var codeWordsWithFlags = payload[3];

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

            var pixelIndexWord = BinaryPrimitives.ReadUInt32BigEndian(payload.Slice(4, 4));

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FiveToEightBit(int color)
        {
            var c0r = color | ((color & 0xE0) >> 5);
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
            var pixelIndex = ((pixelIndexWord & (1 << i)) >> i) | ((pixelIndexWord & (0x10000 << i)) >> (16 + i - 1));
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
