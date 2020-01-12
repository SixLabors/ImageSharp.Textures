namespace SixLabors.ImageSharp.Textures
{
    public interface ITexture<TSelf> : ITexture
        where TSelf : struct, ITexture<TSelf>
    {
    }

    public interface ITexture
    {
        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        void Dispose();
    }
}
