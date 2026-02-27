// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AstcSharp.Core;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// ASTC (Adaptive scalable texture compression) decoder for all valid block footprints.
/// </summary>
internal static class AstcDecoder
{
    internal const int AstcBlockSize = 16;

    /// <summary>
    /// Decodes an ASTC block into RGBA pixels.
    /// </summary>
    /// <param name="blockData">The 16-byte ASTC block data.</param>
    /// <param name="blockWidth">The width of the block footprint (4-12).</param>
    /// <param name="blockHeight">The height of the block footprint (4-12).</param>
    /// <param name="decodedPixels">The output span for decoded RGBA pixels.</param>
    /// <exception cref="ArgumentException">Thrown if blockData is not 16 bytes or decodedPixels is the wrong size.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the block dimensions are invalid.</exception>
    public static void DecodeBlock(ReadOnlySpan<byte> blockData, int blockWidth, int blockHeight, Span<byte> decodedPixels)
    {
        if (blockData.Length != AstcBlockSize)
        {
            throw new ArgumentException($"ASTC block data must be exactly {AstcBlockSize} bytes. Received {blockData.Length} bytes.", nameof(blockData));
        }

        int expectedDecodedSize = blockWidth * blockHeight * 4;
        if (decodedPixels.Length < expectedDecodedSize)
        {
            throw new ArgumentException($"Output buffer must be at least {expectedDecodedSize} bytes for {blockWidth}x{blockHeight} block. Received {decodedPixels.Length} bytes.", nameof(decodedPixels));
        }

        Footprint footprint = Footprint.FromFootprintType(FootprintFromDimensions(blockWidth, blockHeight));

        AstcSharp.AstcDecoder.DecompressBlock(blockData, footprint, decodedPixels);
    }

    /// <summary>
    /// Decompresses ASTC-compressed image data to RGBA pixels.
    /// </summary>
    /// <param name="blockData">The compressed block data.</param>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="blockWidth">The width of the block footprint.</param>
    /// <param name="blockHeight">The height of the block footprint.</param>
    /// <param name="compressedBytesPerBlock">The number of compressed bytes per block.</param>
    /// <returns>The decompressed RGBA pixel data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if blockData is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if dimensions or block parameters are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown if blockData length is invalid.</exception>
    public static byte[] DecompressImage(
        byte[] blockData,
        int width,
        int height,
        int blockWidth,
        int blockHeight,
        byte compressedBytesPerBlock)
    {
        ArgumentNullException.ThrowIfNull(blockData);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);

        if (compressedBytesPerBlock != AstcBlockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(compressedBytesPerBlock), compressedBytesPerBlock, $"ASTC blocks must be {AstcBlockSize} bytes.");
        }

        // Validate block dimensions (will throw if invalid)
        _ = FootprintFromDimensions(blockWidth, blockHeight);

        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int totalBlocks = blocksWide * blocksHigh;
        int expectedDataLength = totalBlocks * compressedBytesPerBlock;

        if (blockData.Length < expectedDataLength)
        {
            throw new ArgumentException($"Block data is too small. Expected at least {expectedDataLength} bytes for {width}x{height} texture with {blockWidth}x{blockHeight} blocks, but received {blockData.Length} bytes.", nameof(blockData));
        }

        byte[] decompressedData = new byte[width * height * 4];
        byte[] decodedBlock = new byte[blockWidth * blockHeight * 4];

        int blockIndex = 0;

        for (int by = 0; by < blocksHigh; by++)
        {
            for (int bx = 0; bx < blocksWide; bx++)
            {
                int blockDataOffset = blockIndex * compressedBytesPerBlock;
                if (blockDataOffset + compressedBytesPerBlock <= blockData.Length)
                {
                    DecodeBlock(
                        blockData.AsSpan(blockDataOffset, compressedBytesPerBlock),
                        blockWidth,
                        blockHeight,
                        decodedBlock);

                    for (int py = 0; py < blockHeight && ((by * blockHeight) + py) < height; py++)
                    {
                        for (int px = 0; px < blockWidth && ((bx * blockWidth) + px) < width; px++)
                        {
                            int srcIndex = ((py * blockWidth) + px) * 4;
                            int dstX = (bx * blockWidth) + px;
                            int dstY = (by * blockHeight) + py;
                            int dstIndex = ((dstY * width) + dstX) * 4;

                            decompressedData[dstIndex] = decodedBlock[srcIndex];
                            decompressedData[dstIndex + 1] = decodedBlock[srcIndex + 1];
                            decompressedData[dstIndex + 2] = decodedBlock[srcIndex + 2];
                            decompressedData[dstIndex + 3] = decodedBlock[srcIndex + 3];
                        }
                    }
                }

                blockIndex++;
            }
        }

        return decompressedData;
    }

    private static FootprintType FootprintFromDimensions(int width, int height)
        => (width, height) switch
        {
            (4, 4) => FootprintType.Footprint4x4,
            (5, 4) => FootprintType.Footprint5x4,
            (5, 5) => FootprintType.Footprint5x5,
            (6, 5) => FootprintType.Footprint6x5,
            (6, 6) => FootprintType.Footprint6x6,
            (8, 5) => FootprintType.Footprint8x5,
            (8, 6) => FootprintType.Footprint8x6,
            (8, 8) => FootprintType.Footprint8x8,
            (10, 5) => FootprintType.Footprint10x5,
            (10, 6) => FootprintType.Footprint10x6,
            (10, 8) => FootprintType.Footprint10x8,
            (10, 10) => FootprintType.Footprint10x10,
            (12, 10) => FootprintType.Footprint12x10,
            (12, 12) => FootprintType.Footprint12x12,
            _ => throw new ArgumentOutOfRangeException(nameof(width), $"Invalid ASTC block dimensions: {width}x{height}. Valid sizes are 4x4, 5x4, 5x5, 6x5, 6x6, 8x5, 8x6, 8x8, 10x5, 10x6, 10x8, 10x10, 12x10, 12x12."),
        };
}
