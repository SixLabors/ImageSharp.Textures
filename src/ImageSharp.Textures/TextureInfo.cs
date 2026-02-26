// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures;

/// <summary>
/// Contains information about the image including dimensions, pixel type information and additional metadata
/// </summary>
internal sealed class TextureInfo : ITextureInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureInfo"/> class.
    /// </summary>
    /// <param name="pixelType">The image pixel type information.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    public TextureInfo(TextureTypeInfo pixelType, int width, int height)
    {
        this.PixelType = pixelType;
        this.Width = width;
        this.Height = height;
    }

    /// <inheritdoc />
    public TextureTypeInfo PixelType { get; }

    /// <inheritdoc />
    public int Width { get; }

    /// <inheritdoc />
    public int Height { get; }
}
