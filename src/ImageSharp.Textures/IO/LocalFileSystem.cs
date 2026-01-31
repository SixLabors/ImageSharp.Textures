// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.IO
{
    using System.IO;

    /// <summary>
    /// A wrapper around the local File apis.
    /// </summary>
    internal sealed class LocalFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public Stream OpenRead(string path) => File.OpenRead(path);

        /// <inheritdoc/>
        public Stream Create(string path) => File.Create(path);
    }
}
