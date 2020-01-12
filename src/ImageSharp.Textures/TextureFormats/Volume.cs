namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    using System.Collections.Generic;

    public struct Volume : ITexture<Volume>
    {
        private bool isDisposed;
        private List<Surface> slices;

        public List<Surface> Slices
        {
            get
            {
                if (slices == null)
                {
                    slices = new List<Surface>();
                }
                return slices;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            foreach (Surface slice in this.Slices)
            {
                slice.Dispose();
            }

            this.isDisposed = true;
        }
    }
}
