// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures
{
    /// <summary>
    /// Represents a MipMap.
    /// </summary>
    public abstract class MipMap
    {
        /// <summary>
        /// Gets the image at a given mipmap level.
        /// </summary>
        /// <returns>The image.</returns>
        public abstract Image GetImage();
    }
}
