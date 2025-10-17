// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Constants for ktx version 2 textures.
    /// </summary>
    internal static class Ktx2Constants
    {
        /// <summary>
        /// The size of a KTX header in bytes.
        /// </summary>
        public const int KtxHeaderSize = 68;

        /// <summary>
        /// The list of mimetypes that equate to a ktx2 file.
        /// </summary>
        public static readonly IEnumerable<string> MimeTypes = new[] { "image/ktx2" };

        /// <summary>
        /// The list of file extensions that equate to a ktx2 file.
        /// </summary>
        public static readonly IEnumerable<string> FileExtensions = new[] { "ktx2" };

        /// <summary>
        /// Gets the magic bytes identifying a ktx2 texture.
        /// </summary>
        public static ReadOnlySpan<byte> MagicBytes => new byte[]
        {
            0xAB, // «
            0x4B, // K
            0x54, // T
            0x58, // X
            0x20, // " "
            0x32, // 2
            0x30, // 0
            0xBB, // »
            0x0D, // \r
            0x0A, // \n
            0x1A,
            0x0A, // \n
        };
    }
}
