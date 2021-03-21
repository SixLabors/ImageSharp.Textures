// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Helper methods for decoding textures.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Delegate to decode a texture.
        /// </summary>
        /// <param name="stream">The stream containing the encoded texture.</param>
        /// <param name="data">The data array to decompress the texture data to.</param>
        /// <param name="streamIndex">Index of the stream.</param>
        /// <param name="dataIndex">Index of the data.</param>
        /// <param name="stride">The stride.</param>
        /// <returns>Stream position after decompression.</returns>
        public delegate int DecodeDelegate(byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride);

        /// <summary>
        /// Calculates the number of blocks.
        /// </summary>
        /// <param name="pixels">The number of pixels.</param>
        /// <returns>The number of blocks.</returns>
        public static int CalcBlocks(int pixels) => Math.Max(1, (pixels + 3) / 4);

        /// <summary>
        /// Decodes a block texture.
        /// </summary>
        /// <typeparam name="TBlock">The type of the block.</typeparam>
        /// <param name="memBuffer">The memory buffer with the encoded texture data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="decode">The decode delegate to use.</param>
        /// <returns>The decoded bytes of the texture.</returns>
        public static byte[] InMemoryDecode<TBlock>(byte[] memBuffer, int width, int height, DecodeDelegate decode)
            where TBlock : struct, IBlock
        {
            TBlock blockFormat = default;
            int heightBlocks = CalcBlocks(height);
            int widthBlocks = CalcBlocks(width);
            int stridePixels = widthBlocks * blockFormat.DivSize;
            int deflatedStrideBytes = stridePixels * blockFormat.PixelDepthBytes;
            int dataLen = heightBlocks * blockFormat.DivSize * deflatedStrideBytes;
            byte[] data = new byte[dataLen];

            int dataIndex = 0;
            int bIndex = 0;

            int stride = stridePixels * blockFormat.PixelDepthBytes;
            int blocksPerStride = widthBlocks;
            int indexPixelsLeft = heightBlocks * blockFormat.DivSize * stride;

            while (indexPixelsLeft > 0)
            {
                int origDataIndex = dataIndex;

                for (int i = 0; i < blocksPerStride; i++)
                {
                    bIndex = decode.Invoke(memBuffer, data, bIndex, dataIndex, stridePixels);
                    dataIndex += blockFormat.DivSize * blockFormat.PixelDepthBytes;
                }

                int filled = stride * blockFormat.DivSize;
                indexPixelsLeft -= filled;

                // Jump down to the block that is exactly (divSize - 1)
                // below the current row we are on
                dataIndex = origDataIndex + filled;
            }

            return data;
        }

        /// <summary>
        /// Clamps the specified value between a min and a max value.
        /// </summary>
        /// <param name="val">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint val, uint min, uint max) => Math.Min(Math.Max(val, min), max);

        /// <summary>
        /// Clamps the specified value between a min and a max value.
        /// </summary>
        /// <param name="val">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int val, int min, int max) => Math.Min(Math.Max(val, min), max);
    }
}
