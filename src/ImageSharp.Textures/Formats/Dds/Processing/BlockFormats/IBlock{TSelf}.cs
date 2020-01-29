namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    public interface IBlock<TSelf> : IBlock
        where TSelf : struct, IBlock<TSelf>
    {
    }
}
