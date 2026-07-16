// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Handles endianness conversions for KTX texture data.
    /// </summary>
    internal interface IEndianHandler
    {
        /// <summary>
        /// Reads a UInt32 value from a buffer with appropriate endianness.
        /// </summary>
        /// <param name="buffer">The buffer containing the UInt32 data.</param>
        /// <returns>The UInt32 value.</returns>
        uint ReadUInt32(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Converts pixel data endianness if needed based on the type size.
        /// </summary>
        /// <param name="data">The pixel data to convert.</param>
        /// <param name="typeSize">The size of each data element (2 for half-float, 4 for float).</param>
        void ConvertPixelData(Span<byte> data, uint typeSize);
    }
}
