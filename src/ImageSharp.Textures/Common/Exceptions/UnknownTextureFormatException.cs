// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures
{
    /// <summary>
    /// The exception that is thrown when the library tries to load
    /// an image which has an unknown format.
    /// </summary>
    public sealed class UnknownTextureFormatException : TextureFormatException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownTextureFormatException"/> class with the name of the
        /// parameter that causes this exception.
        /// </summary>
        /// <param name="errorMessage">The error message that explains the reason for this exception.</param>
        public UnknownTextureFormatException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
