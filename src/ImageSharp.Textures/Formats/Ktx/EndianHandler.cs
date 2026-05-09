// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Handles endianness conversions for KTX texture data. Reads header fields using the file's
    /// declared endianness and optionally swaps pixel bytes when the file and host disagree.
    /// </summary>
    internal sealed class EndianHandler : IEndianHandler
    {
        private readonly bool isFileLittleEndian;
        private readonly bool swapPixelData;

        public EndianHandler(bool isFileLittleEndian)
        {
            this.isFileLittleEndian = isFileLittleEndian;
            this.swapPixelData = isFileLittleEndian != BitConverter.IsLittleEndian;
        }

        /// <inheritdoc/>
        public uint ReadUInt32(ReadOnlySpan<byte> buffer)
            => this.isFileLittleEndian
                ? BinaryPrimitives.ReadUInt32LittleEndian(buffer)
                : BinaryPrimitives.ReadUInt32BigEndian(buffer);

        /// <inheritdoc/>
        public void ConvertPixelData(Span<byte> data, uint typeSize)
        {
            if (!this.swapPixelData)
            {
                return;
            }

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
