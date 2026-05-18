// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// ASTC (Adaptive scalable texture compression) decoder for all valid block footprints.
/// </summary>
internal static class AstcDecoder
{
    internal const int AstcBlockSize = 16;
    internal const int RgbaPixelDepthBytes = 4;
    internal const int RgbaHdrPixelDepthBytes = 16;

    /// <summary>
    /// Decompresses ASTC-compressed image data to UNORM8 RGBA pixels (4 bytes per pixel).
    /// Blocks that use HDR endpoint modes (spec §C.2.25) are reserved in the LDR profile and produce
    /// the spec-mandated magenta error colour (§C.2.19); call <see cref="DecompressHdrImage"/> for HDR content.
    /// </summary>
    /// <param name="blockData">The compressed block data. May be over-sized — only the bytes implied by
    /// the image and footprint dimensions are read.</param>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="blockWidth">The width of the block footprint.</param>
    /// <param name="blockHeight">The height of the block footprint.</param>
    /// <param name="compressedBytesPerBlock">The number of compressed bytes per block. Must equal
    /// <see cref="AstcBlockSize"/> (16).</param>
    /// <returns>The decompressed UNORM8 RGBA pixel data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockData"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if dimensions or block parameters are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="blockData"/> is shorter than the
    /// length implied by the image and footprint dimensions.</exception>
    public static byte[] DecompressImage(
        byte[] blockData,
        int width,
        int height,
        int blockWidth,
        int blockHeight,
        byte compressedBytesPerBlock)
    {
        long expectedDataLength = GetExpectedBlockStreamLength(width, height, blockWidth, blockHeight, compressedBytesPerBlock);
        long totalPixels = (long)width * height;

        ValidateBlockStream(
            blockData,
            width,
            height,
            compressedBytesPerBlock,
            bytesPerPixel: RgbaPixelDepthBytes,
            expectedDataLength,
            totalPixels);

        Footprint footprint = Footprint.FromFootprintType(FootprintFromDimensions(blockWidth, blockHeight));
        byte[] decompressedData = new byte[totalPixels * RgbaPixelDepthBytes];

        // KTX/KTX2 mip-level slices may be over-sized; trim to the exact block stream the real decoder expects
        ReadOnlySpan<byte> exact = blockData.AsSpan(0, (int)expectedDataLength);
        _ = Compression.Astc.AstcDecoder.DecompressImage(exact, width, height, footprint, decompressedData);

        return decompressedData;
    }

    /// <summary>
    /// Decompresses ASTC-compressed image data to float-RGBA pixels, returned as a raw byte buffer
    /// suitable for <c>Image.LoadPixelData&lt;Rgba128Float&gt;</c>. Each pixel is four IEEE-754 single-precision
    /// floats (R, G, B, A) for a total of <c>width * height * 16</c> bytes. HDR endpoint modes (2, 3, 7,
    /// 11, 14, 15 per ASTC spec §C.2.14) decode to their full unclamped float range; LDR endpoint modes
    /// widen to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="blockData">The compressed block data. May be over-sized — only the bytes implied by
    /// the image and footprint dimensions are read.</param>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="blockWidth">The width of the block footprint.</param>
    /// <param name="blockHeight">The height of the block footprint.</param>
    /// <param name="compressedBytesPerBlock">The number of compressed bytes per block. Must equal
    /// <see cref="AstcBlockSize"/> (16).</param>
    /// <returns>The decompressed RGBA-float pixel data as a raw byte buffer.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockData"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if dimensions or block parameters are invalid.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="blockData"/> is shorter than the
    /// length implied by the image and footprint dimensions.</exception>
    public static byte[] DecompressHdrImage(
        byte[] blockData,
        int width,
        int height,
        int blockWidth,
        int blockHeight,
        byte compressedBytesPerBlock)
    {
        long expectedDataLength = GetExpectedBlockStreamLength(width, height, blockWidth, blockHeight, compressedBytesPerBlock);
        long totalPixels = (long)width * height;

        ValidateBlockStream(
            blockData,
            width,
            height,
            compressedBytesPerBlock,
            bytesPerPixel: RgbaHdrPixelDepthBytes,
            expectedDataLength,
            totalPixels);

        Footprint footprint = Footprint.FromFootprintType(FootprintFromDimensions(blockWidth, blockHeight));

        int floatCount = (int)(totalPixels * 4);
        using IMemoryOwner<float> floatBuffer = MemoryAllocator.Default.Allocate<float>(floatCount);
        Span<float> floatSpan = floatBuffer.Memory.Span[..floatCount];

        // KTX/KTX2 mip-level slices may be over-sized; trim to the exact block stream the real decoder expects.
        ReadOnlySpan<byte> exact = blockData.AsSpan(0, (int)expectedDataLength);
        _ = Compression.Astc.AstcDecoder.DecompressHdrImage(exact, width, height, footprint, floatSpan);

        byte[] bytes = new byte[totalPixels * RgbaHdrPixelDepthBytes];
        MemoryMarshal.AsBytes(floatSpan).CopyTo(bytes);

        return bytes;
    }

    /// <summary>
    /// Returns the exact compressed byte length implied by the image dimensions and ASTCb lock footprint
    /// </summary>
    private static long GetExpectedBlockStreamLength(int width, int height, int blockWidth, int blockHeight, byte compressedBytesPerBlock)
    {
        Guard.MustBeGreaterThan(blockWidth, 0, nameof(blockWidth));
        Guard.MustBeGreaterThan(blockHeight, 0, nameof(blockHeight));

        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;

        return (long)blocksWide * blocksHigh * compressedBytesPerBlock;
    }

    private static void ValidateBlockStream(
        byte[] blockData,
        int width,
        int height,
        byte compressedBytesPerBlock,
        int bytesPerPixel,
        long expectedDataLength,
        long totalPixels)
    {
        Guard.NotNull(blockData);
        Guard.MustBeGreaterThan(width, 0, nameof(width));
        Guard.MustBeGreaterThan(height, 0, nameof(height));
        Guard.IsTrue(compressedBytesPerBlock == AstcBlockSize, nameof(compressedBytesPerBlock), $"ASTC blocks must be {AstcBlockSize} bytes.");
        Guard.MustBeGreaterThanOrEqualTo(blockData.Length, expectedDataLength, nameof(blockData));
        Guard.MustBeLessThanOrEqualTo(totalPixels, (long)int.MaxValue / bytesPerPixel, nameof(totalPixels));
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
