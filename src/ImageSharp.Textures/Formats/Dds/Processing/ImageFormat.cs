// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    /// <summary>Describes how pixel data is arranged</summary>
    public enum ImageFormat
    {
        /// <summary>Red, green, and blue are the same values contained in a single byte</summary>
        Rgb8,

        /// <summary>Red, green, and blue are contained in a two bytes</summary>
        R5g5b5,

        R5g6b5,

        R5g5b5a1,

        Rgba16,

        /// <summary>Red, green, and blue channels are 8 bits apiece</summary>
        Rgb24,

        /// <summary>
        /// Red, green, blue, and alpha are 8 bits apiece
        /// </summary>
        Rgba32
    }
}
