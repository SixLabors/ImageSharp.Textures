namespace SixLabors.ImageSharp.Textures.TextureFormats
{
    public struct Cubemap : ITexture<Cubemap>
    {
        private bool isDisposed;

        public Surface PositiveX { get; }

        public Surface NegativeX { get; }

        public Surface PositiveY { get; }

        public Surface NegativeY { get; }

        public Surface PositiveZ { get; }

        public Surface NegativeZ { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.PositiveX.Dispose();
            this.NegativeX.Dispose();
            this.PositiveY.Dispose();
            this.NegativeY.Dispose();
            this.PositiveZ.Dispose();
            this.NegativeZ.Dispose();

            this.isDisposed = true;
        }
    }
}
