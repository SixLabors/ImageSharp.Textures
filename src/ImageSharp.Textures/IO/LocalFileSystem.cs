// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

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
