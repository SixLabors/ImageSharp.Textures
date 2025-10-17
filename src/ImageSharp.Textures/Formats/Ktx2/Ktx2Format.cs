// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Registers the texture decoders and mime type detectors for the ktx2 format.
    /// </summary>
    public sealed class Ktx2Format : ITextureFormat
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="Ktx2Format" /> class from being created.
        /// </summary>
        private Ktx2Format()
        {
        }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static Ktx2Format Instance { get; } = new Ktx2Format();

        /// <inheritdoc/>
        public string Name => "KTX2";

        /// <inheritdoc/>
        public string DefaultMimeType => "image/ktx2";

        /// <inheritdoc/>
        public IEnumerable<string> MimeTypes => Ktx2Constants.MimeTypes;

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions => Ktx2Constants.FileExtensions;
    }
}
