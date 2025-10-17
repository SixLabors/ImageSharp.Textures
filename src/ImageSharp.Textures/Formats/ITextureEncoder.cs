// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Formats
{
    using System.IO;

    /// <summary>
    /// Encapsulates properties and methods required for encoding an image to a stream.
    /// </summary>
    public interface ITextureEncoder
    {
        /// <summary>
        /// Encodes the image to the specified stream from the <see cref="Texture"/>.
        /// </summary>
        /// <param name="texture">The <see cref="Texture"/> to encode from.</param>
        /// <param name="stream">The <see cref="Stream"/> to encode the image data to.</param>
        void Encode(Texture texture, Stream stream);
    }
}
