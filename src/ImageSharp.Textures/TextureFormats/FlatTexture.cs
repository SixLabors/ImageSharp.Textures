// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    public class FlatTexture : Texture
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatTexture"/> class.
        /// </summary>
        public FlatTexture()
        {
            this.MipMaps = new List<MipMap>();
        }

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
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("Trying to execute an operation on a disposed image.");
            }
        }
    }
}
