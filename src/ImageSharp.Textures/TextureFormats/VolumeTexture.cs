// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    /// <summary>
    /// Represents a volume texture.
    /// </summary>
    /// <seealso cref="SixLabors.ImageSharp.Textures.Texture" />
    public class VolumeTexture : Texture
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeTexture"/> class.
        /// </summary>
        public VolumeTexture() => this.Slices = new List<FlatTexture>();

        /// <summary>
        /// Gets a list of flat textures from which the volume texture is composed of.
        /// </summary>
        public List<FlatTexture> Slices { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (FlatTexture slice in this.Slices)
                {
                    slice.Dispose();
                }
            }

            this.isDisposed = true;
        }

        /// <inheritdoc/>
        internal override void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("Trying to execute an operation on a disposed image.");
            }
        }
    }
}
