// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Enum for the different OpenGl pixel formats.
    /// </summary>
    internal enum GlPixelFormat : uint
    {
        /// <summary>
        /// Zero indicates, that the texture is compressed.
        /// </summary>
        Compressed = 0,

        /// <summary>
        /// Only the red channel.
        /// </summary>
        Red = 0x1903,

        /// <summary>
        /// Only the green channel.
        /// </summary>
        Green = 0x1904,

        /// <summary>
        /// Only the blue channel.
        /// </summary>
        Blue = 0x1905,

        /// <summary>
        /// Only the alpha channel.
        /// </summary>
        Alpha = 0x1906,

        /// <summary>
        /// Only luminance.
        /// </summary>
        Luminance = 0x1909,

        /// <summary>
        /// Luminance and alpha.
        /// </summary>
        LuminanceAlpha = 0x190A,

        /// <summary>
        /// Pixels are stored only with the red and green channel present.
        /// </summary>
        Rg = 0x8227,

        /// <summary>
        /// Pixels are stored only with the red and green channel present.
        /// </summary>
        RgInteger = 0x8228,

        /// <summary>
        /// Pixels are stored as RGB.
        /// </summary>
        Rgb = 0x1907,

        /// <summary>
        /// Pixels are stored as RGBA.
        /// </summary>
        Rgba = 0x1908,

        /// <summary>
        /// Pixels are stored as BGR.
        /// </summary>
        Bgr = 0x80E0,

        /// <summary>
        /// Pixels are stored as BGRA.
        /// </summary>
        Bgra = 0x80E1
    }
}
