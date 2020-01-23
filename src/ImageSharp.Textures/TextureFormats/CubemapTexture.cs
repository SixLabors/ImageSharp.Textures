// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    using System;

    public class CubemapTexture : Texture
    {
        private bool isDisposed;

        public FlatTexture PositiveX { get; }

        public FlatTexture NegativeX { get; }

        public FlatTexture PositiveY { get; }

        public FlatTexture NegativeY { get; }

        public FlatTexture PositiveZ { get; }

        public FlatTexture NegativeZ { get; }

        public CubemapTexture()
        {
            PositiveX = new FlatTexture();
            NegativeX = new FlatTexture();
            PositiveY = new FlatTexture();
            NegativeY = new FlatTexture();
            PositiveZ = new FlatTexture();
            NegativeZ = new FlatTexture();
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
                this.PositiveX.Dispose();
                this.NegativeX.Dispose();
                this.PositiveY.Dispose();
                this.NegativeY.Dispose();
                this.PositiveZ.Dispose();
                this.NegativeZ.Dispose();
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
