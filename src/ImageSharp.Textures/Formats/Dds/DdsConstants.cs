// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    internal static class DdsConstants
    {
        /// <summary>
        /// The list of mimetypes that equate to a dds file.
        /// </summary>
        public static readonly IEnumerable<string> MimeTypes = new[] { "image/vnd.ms-dds" };

        /// <summary>
        /// The list of file extensions that equate to a dds file.
        /// </summary>
        public static readonly IEnumerable<string> FileExtensions = new[] { "dds" };

        /// <summary>
        /// The dds header size in bytes.
        /// </summary>
        public const int DdsHeaderSize = 124;

        /// <summary>
        /// The dds pixel format size in bytes.
        /// </summary>
        public const int DdsPixelFormatSize = 32;

        /// <summary>
        /// The dds dxt10 header size in bytes.
        /// </summary>
        public const int DdsDxt10HeaderSize = 20;
    }
}
