// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;

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
                if (data.Length % 2 != 0)
                {
                    throw new TextureFormatException("Pixel data length is not a multiple of 2 for a 16-bit typed format");
                }

                BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(data), MemoryMarshal.Cast<byte, ushort>(data));
            }
            else if (typeSize == 4)
            {
                if (data.Length % 4 != 0)
                {
                    throw new TextureFormatException("Pixel data length is not a multiple of 4 for a 32-bit typed format");
                }

                BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, uint>(data), MemoryMarshal.Cast<byte, uint>(data));
            }
        }
    }
}
