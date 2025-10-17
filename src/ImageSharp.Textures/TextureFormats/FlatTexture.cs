// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    /// <summary>
    /// A flat texture.
    /// </summary>
    /// <seealso cref="SixLabors.ImageSharp.Textures.Texture" />
    public class FlatTexture : Texture
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatTexture"/> class.
        /// </summary>
        public FlatTexture() => this.MipMaps = new List<MipMap>();

        /// <summary>
        /// Gets the list of mip maps of the texture.
        /// </summary>
        public List<MipMap> MipMaps { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (MipMap mipMap in this.MipMaps)
                {
                    // mipMap.Dispose();
                }
            }

            this.isDisposed = true;
        }

        /// <inheritdoc/>
        internal override void EnsureNotDisposed()
            => ObjectDisposedException.ThrowIf(this.isDisposed, "Trying to execute an operation on a disposed image.");
    }
}
