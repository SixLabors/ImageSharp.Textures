// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    using System;
    using System.Collections.Generic;

    public class VolumeTexture : Texture
    {
        private bool isDisposed;

        public List<FlatTexture> Slices { get; }

        public VolumeTexture()
        {
            Slices = new List<FlatTexture>();
        }

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
