// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Textures.Common.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an error occurs when applying a process to an image.
    /// </summary>
    public sealed class TextureProcessingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureProcessingException"/> class with the name of the
        /// parameter that causes this exception.
        /// </summary>
        /// <param name="errorMessage">The error message that explains the reason for this exception.</param>
        public TextureProcessingException(string errorMessage)
            : base(errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureProcessingException"/> class with a specified
        /// error message and the exception that is the cause of this exception.
        /// </summary>
        /// <param name="errorMessage">The error message that explains the reason for this exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic)
        /// if no inner exception is specified.</param>
        public TextureProcessingException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }
}
