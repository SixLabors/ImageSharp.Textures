// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Handles endianness when file endianness differs from system endianness (requires byte swapping).
    /// </summary>
    internal sealed class SwappingEndianHandler : IEndianHandler
    {
        private readonly bool isLittleEndian;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwappingEndianHandler"/> class.
        /// </summary>
        /// <param name="isLittleEndian">Whether the file is little-endian.</param>
        public SwappingEndianHandler(bool isLittleEndian)
        {
            this.isLittleEndian = isLittleEndian;
        }

        /// <inheritdoc/>
        public uint ReadUInt32(ReadOnlySpan<byte> buffer)
        {
            return this.isLittleEndian
                ? BinaryPrimitives.ReadUInt32LittleEndian(buffer)
                : BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }

        /// <inheritdoc/>
        public void ConvertPixelData(Span<byte> data, uint typeSize)
        {
            if (typeSize == 2)
            {
                SwapEndian16(data);
            }
            else if (typeSize == 4)
            {
                SwapEndian32(data);
            }
        }

        /// <summary>
        /// Swaps endianness for 16-bit values in-place.
        /// </summary>
        /// <param name="data">The data to swap.</param>
        private static void SwapEndian16(Span<byte> data)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                byte temp = data[i];
                data[i] = data[i + 1];
                data[i + 1] = temp;
            }
        }

        /// <summary>
        /// Swaps endianness for 32-bit values in-place.
        /// </summary>
        /// <param name="data">The data to swap.</param>
        private static void SwapEndian32(Span<byte> data)
        {
            for (int i = 0; i < data.Length; i += 4)
            {
                byte temp0 = data[i];
                byte temp1 = data[i + 1];
                data[i] = data[i + 3];
                data[i + 1] = data[i + 2];
                data[i + 2] = temp1;
                data[i + 3] = temp0;
            }
        }
    }
}
