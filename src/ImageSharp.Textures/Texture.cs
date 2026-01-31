// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;

namespace SixLabors.ImageSharp.Textures
{
    /// <summary>
    /// Represents a texture.
    /// </summary>
    public abstract partial class Texture : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if the image is disposed.
        /// </summary>
        internal abstract void EnsureNotDisposed();

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">Whether to dispose of managed and unmanaged objects.</param>
        protected abstract void Dispose(bool disposing);
    }
}
