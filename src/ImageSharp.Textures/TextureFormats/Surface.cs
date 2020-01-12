namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    using System;
    using System.Collections.Generic;

    public class Surface : Texture
    {
        private bool isDisposed;

        public List<MipMap> MipMaps { get; }

        public Surface()
        {
            MipMaps = new List<MipMap>();
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
                foreach (MipMap mipMap in this.MipMaps)
                {
                    mipMap.Dispose();
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
