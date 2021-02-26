// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures
{
    using SixLabors.ImageSharp.Textures.Formats;

    /// <summary>
    /// Encapsulates properties that describe basic image information including dimensions, pixel type information
    /// and additional metadata.
    /// </summary>
    public interface ITextueInfo
    {
        /// <summary>
        /// Gets information about the image pixels.
        /// </summary>
        TextureTypeInfo PixelType { get; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        int Height { get; }
    }
}
