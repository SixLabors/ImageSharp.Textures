// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures
{
    /// <summary>
    /// Encapsulates properties that describe basic image information including dimensions, pixel type information
    /// and additional metadata.
    /// </summary>
    public interface ITextureInfo
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
