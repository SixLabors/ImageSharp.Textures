// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    /// <summary>
    /// Represents a cube map texture.
    /// </summary>
    /// <seealso cref="SixLabors.ImageSharp.Textures.Texture" />
    public class CubemapTexture : Texture
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CubemapTexture"/> class.
        /// </summary>
        public CubemapTexture()
        {
            this.PositiveX = new FlatTexture();
            this.NegativeX = new FlatTexture();
            this.PositiveY = new FlatTexture();
            this.NegativeY = new FlatTexture();
            this.PositiveZ = new FlatTexture();
            this.NegativeZ = new FlatTexture();
        }

        /// <summary>
        /// Gets the positive x texture.
        /// </summary>
        public FlatTexture PositiveX { get; }

        /// <summary>
        /// Gets the negative x texture.
        /// </summary>
        public FlatTexture NegativeX { get; }

        /// <summary>
        /// Gets the positive y texture.
        /// </summary>
        public FlatTexture PositiveY { get; }

        /// <summary>
        /// Gets the negative y texture.
        /// </summary>
        public FlatTexture NegativeY { get; }

        /// <summary>
        /// Gets the positive z texture.
        /// </summary>
        public FlatTexture PositiveZ { get; }

        /// <summary>
        /// Gets the negative z texture.
        /// </summary>
        public FlatTexture NegativeZ { get; }

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
