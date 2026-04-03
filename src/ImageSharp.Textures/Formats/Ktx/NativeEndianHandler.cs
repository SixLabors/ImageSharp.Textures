// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Handles endianness when file endianness matches system endianness (no conversion needed).
    /// </summary>
    internal sealed class NativeEndianHandler : IEndianHandler
    {
        private readonly bool isLittleEndian;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeEndianHandler"/> class.
        /// </summary>
        /// <param name="isLittleEndian">Whether the file is little-endian.</param>
        public NativeEndianHandler(bool isLittleEndian)
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
            // No conversion needed when endianness matches
        }
    }
}
