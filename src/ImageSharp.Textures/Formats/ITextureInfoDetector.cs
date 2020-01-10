// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.ImageSharp.Textures.Formats
{
    /// <summary>
    /// Encapsulates methods used for detecting the raw image information without fully decoding it.
    /// </summary>
    public interface ITextureInfoDetector
    {
        /// <summary>
        /// Reads the raw image information from the specified stream.
        /// </summary>
        /// <param name="configuration">The configuration for the image.</param>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        /// <returns>The <see cref="TextureTypeInfo"/> object</returns>
        ITextueInfo Identify(Configuration configuration, Stream stream);
    }
}
