namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    public interface IBlock
    {
        int BitsPerPixel { get; }
        byte PixelDepthBytes { get; }
        byte DivSize { get; }
        byte CompressedBytesPerBlock { get; }
        bool Compressed { get; }

        Image GetImage(byte[] blockData, int width, int height);

        byte[] Decompress(byte[] blockData, int width, int height);
    }
}
