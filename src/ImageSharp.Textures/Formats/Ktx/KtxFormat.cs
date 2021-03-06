// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Registers the texture decoders and mime type detectors for the ktx format.
    /// </summary>
    public sealed class KtxFormat : ITextureFormat
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="KtxFormat" /> class from being created.
        /// </summary>
        private KtxFormat()
        {
        }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static KtxFormat Instance { get; } = new KtxFormat();

        /// <inheritdoc/>
        public string Name => "KTX";

        /// <inheritdoc/>
        public string DefaultMimeType => "image/vnd.ms-dds";

        /// <inheritdoc/>
        public IEnumerable<string> MimeTypes => KtxConstants.MimeTypes;

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions => KtxConstants.FileExtensions;
    }
}
