// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    using System;

    public class Cubemap : Texture
    {
        private bool isDisposed;

        public Surface PositiveX { get; }

        public Surface NegativeX { get; }

        public Surface PositiveY { get; }

        public Surface NegativeY { get; }

        public Surface PositiveZ { get; }

        public Surface NegativeZ { get; }

        public Cubemap()
        {
            PositiveX = new Surface();
            NegativeX = new Surface();
            PositiveY = new Surface();
            NegativeY = new Surface();
            PositiveZ = new Surface();
            NegativeZ = new Surface();
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
