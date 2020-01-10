// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.ImageSharp.Textures.Formats
{
    /// <summary>
    /// Encapsulates properties and methods required for decoding an image from a stream.
    /// </summary>
    public interface ITextureDecoder
    {
        /// <summary>
        /// Decodes the image from the specified stream to an <see cref="Texture"/>.
        /// </summary>
        /// <param name="configuration">The configuration for the image.</param>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        /// <returns>The <see cref="Image"/>.</returns>
        Texture DecodeTexture(Configuration configuration, Stream stream);
    }
}
