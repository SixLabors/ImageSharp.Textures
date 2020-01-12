namespace SixLabors.ImageSharp.Textures
{
    using System;

    public sealed class Texture<TTexture> : Texture
        where TTexture : struct, ITexture<TTexture>
    {
        private bool isDisposed;

        public TTexture Value { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.Value.Dispose();
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
