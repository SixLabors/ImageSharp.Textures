namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    using System.Collections.Generic;

    public struct Surface : ITexture<Surface>
    {
        private bool isDisposed;
        private List<MipMap> mipMaps;

        public List<MipMap> MipMaps
        {
            get
            {
                if (mipMaps == null)
                {
                    mipMaps = new List<MipMap>();
                }
                return mipMaps;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            foreach (MipMap mipMap in this.MipMaps)
            {
                mipMap.Dispose();
            }

            this.isDisposed = true;
        }
    }
}
