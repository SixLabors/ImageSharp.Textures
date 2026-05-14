// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Compression.Astc;

/// <summary>
/// Provides methods to decode ASTC-compressed texture data into uncompressed pixel formats.
/// </summary>
/// <remarks>
/// The decoder returns raw decoded values and does not apply any gamma or color-space
/// transform. Callers loading ASTC data from an sRGB-tagged container (e.g. a KTX file
/// with an *_SRGB_BLOCK format) are responsible for applying sRGB-to-linear conversion
/// downstream if they need linear values.
/// </remarks>
public static class AstcDecoder
{
    private static readonly ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

    // ASTC decodes to 4-channel RGBA in both LDR (UNORM8) and HDR (float32) profiles.
    // For LDR this is bytes-per-pixel; for HDR it's float-elements-per-pixel.
    private const int ChannelsPerPixel = 4;

    /// <summary>
    /// Decompresses ASTC-compressed data to uncompressed RGBA8 format (4 bytes per pixel).
    /// </summary>
    /// <param name="astcData">The ASTC-compressed texture data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="footprint">The ASTC block footprint (e.g., 4x4, 5x5)</param>
    /// <returns>
    /// Array of bytes in RGBA8 format (width * height * 4 bytes total), or an empty span if the
    /// input is structurally invalid. Individual malformed blocks are skipped and leave zeros in the output.
    /// </returns>
    public static Span<byte> DecompressImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        long totalPixels = (long)width * height;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalPixels, (long)int.MaxValue / ChannelsPerPixel);

        int totalBytes = (int)(totalPixels * ChannelsPerPixel);
        byte[] imageBuffer = new byte[totalBytes];

        return DecompressImage(astcData, width, height, footprint, imageBuffer)
            ? imageBuffer
            : [];
    }

    /// <summary>
    /// Decompresses ASTC-compressed data to uncompressed RGBA8 format into a caller-provided buffer.
    /// </summary>
    /// <param name="astcData">The ASTC-compressed texture data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="footprint">The ASTC block footprint (e.g., 4x4, 5x5)</param>
    /// <param name="imageBuffer">Output buffer. Must be at least width * height * 4 bytes.</param>
    /// <returns>
    /// True if the input was structurally valid and decoding ran, false if it was rejected
    /// up front. Individual malformed blocks are skipped and leave zeros in the output.
    /// </returns>
    public static bool DecompressImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint, Span<byte> imageBuffer)
    {
        ValidateImageArgs(width, height, imageBuffer.Length, ChannelsPerPixel);

        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
        {
            return false;
        }

        // Scratch is rented outside the try/finally so a failing Rent never hands the default
        // sentinel to Return.
        byte[] decodedBlock = ArrayPool.Rent(footprint.PixelCount * ChannelsPerPixel);
        try
        {
            DecodeAllBlocks<LdrPipeline, byte>(astcData, width, height, footprint, blocksWide, blocksHigh, imageBuffer, decodedBlock.AsSpan());
        }
        finally
        {
            ArrayPool.Return(decodedBlock);
        }

        return true;
    }

    /// <summary>
    /// Shared image-decode loop for both LDR and HDR profiles (ASTC spec §C.2.25). Iterates
    /// the compressed block array in raster order, parses each block via
    /// <see cref="BlockModeDecoder.Decode"/>, runs the pipeline's profile check, and dispatches to
    /// the appropriate per-block decoder.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeAllBlocks<TPipeline, T>(
        ReadOnlySpan<byte> astcData,
        int width,
        int height,
        Footprint footprint,
        int blocksWide,
        int blocksHigh,
        Span<T> imageBuffer,
        Span<T> decodedPixels)
        where TPipeline : struct, IBlockPipeline<T>
        where T : unmanaged
    {
        TPipeline pipeline = default;
        int blockIndex = 0;

        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            for (int blockX = 0; blockX < blocksWide; blockX++)
            {
                int index = blockIndex++;
                if (!TryReadBlockBits(astcData, index, out UInt128 blockBits))
                {
                    continue;
                }

                BlockInfo info = BlockModeDecoder.Decode(blockBits);
                if (!info.IsValid)
                {
                    continue;
                }

                pipeline.PreDispatchCheck(blockBits, in info);

                BlockDestination dest = ComputeBlockDestination(blockX, blockY, footprint, width, height);
                DecodeBlock<TPipeline, T>(blockBits, in info, footprint, dest, width, imageBuffer, decodedPixels);
            }
        }
    }

    /// <summary>
    /// Routes a single block to the best available path. Single-partition, single-plane,
    /// non-void-extent blocks (the common shape per ASTC spec §C.2.10, §C.2.20, §C.2.23) take
    /// the fused fast path — directly to the image buffer when the block fits entirely inside
    /// the image, or to a scratch buffer at image edges that need cropping. Everything else
    /// (void-extent, multi-partition, dual-plane) falls through to the general
    /// <see cref="LogicalBlock"/> pipeline.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeBlock<TPipeline, T>(
        UInt128 blockBits,
        in BlockInfo info,
        Footprint footprint,
        BlockDestination dest,
        int imageWidth,
        Span<T> imageBuffer,
        Span<T> decodedPixels)
        where TPipeline : struct, IBlockPipeline<T>
        where T : unmanaged
    {
        TPipeline pipeline = default;
        bool isFusable = !info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane;

        if (isFusable && dest.IsFullInteriorBlock)
        {
            pipeline.FusedToImage(blockBits, in info, footprint, dest.DstBaseX, dest.DstBaseY, imageWidth, imageBuffer);
            return;
        }

        if (isFusable)
        {
            pipeline.FusedToScratch(blockBits, in info, footprint, decodedPixels);
        }
        else
        {
            pipeline.LogicalWrite(blockBits, in info, footprint, decodedPixels);
        }

        CopyBlockRect(decodedPixels, imageBuffer, footprint.Width, dest.CopyWidth, dest.CopyHeight, dest.DstBaseX, dest.DstBaseY, imageWidth, ChannelsPerPixel);
    }

    /// <summary>
    /// Shared single-block decode path for the public <c>DecompressBlock</c> entry points.
    /// Runs the pipeline's profile check (LDR rejects HDR content per ASTC spec §C.2.19),
    /// then dispatches to the fused fast path for the common shape (single-partition,
    /// single-plane, non-void-extent — spec §C.2.10, §C.2.20, §C.2.23) or the general
    /// <see cref="LogicalBlock"/> pipeline otherwise. The caller's <paramref name="buffer"/>
    /// is sized for exactly one block, so there's no interior/edge distinction.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeSingleBlock<TPipeline, T>(ReadOnlySpan<byte> blockData, Footprint footprint, Span<T> buffer)
        where TPipeline : struct, IBlockPipeline<T>
        where T : unmanaged
    {
        if (!TryReadBlockInfo(blockData, out UInt128 blockBits, out BlockInfo info))
        {
            return;
        }

        TPipeline pipeline = default;
        pipeline.PreDispatchCheck(blockBits, in info);

        if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane)
        {
            pipeline.FusedToScratch(blockBits, in info, footprint, buffer);
            return;
        }

        pipeline.LogicalWrite(blockBits, in info, footprint, buffer);
    }

    /// <summary>
    /// Decompresses a single ASTC block to RGBA8 pixel data
    /// </summary>
    /// <param name="blockData">The data to decode</param>
    /// <param name="footprint">The type of ASTC block footprint e.g. 4x4, 5x5, etc.</param>
    /// <param name="buffer">The buffer to write the decoded pixels into</param>
    public static void DecompressBlock(ReadOnlySpan<byte> blockData, Footprint footprint, Span<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockData.Length, BlockInfo.SizeInBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, footprint.PixelCount * ChannelsPerPixel);

        DecodeSingleBlock<LdrPipeline, byte>(blockData, footprint, buffer);
    }

    /// <summary>
    /// Decompresses ASTC-compressed data to RGBA values.
    /// </summary>
    /// <param name="astcData">The ASTC-compressed texture data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="footprint">The ASTC block footprint (e.g., 4x4, 5x5)</param>
    /// <returns>
    /// Values in RGBA order. For HDR content, values may exceed 1.0.
    /// </returns>
    public static Span<float> DecompressHdrImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        long totalPixels = (long)width * height;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalPixels, (long)int.MaxValue / 4);

        int totalFloats = (int)(totalPixels * 4);
        float[] imageBuffer = new float[totalFloats];
        if (!DecompressHdrImage(astcData, width, height, footprint, imageBuffer))
        {
            return [];
        }

        return imageBuffer;
    }

    /// <summary>
    /// Decompresses ASTC-compressed data to RGBA float values into a caller-provided buffer.
    /// </summary>
    /// <param name="astcData">The ASTC-compressed texture data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="footprint">The ASTC block footprint (e.g., 4x4, 5x5)</param>
    /// <param name="imageBuffer">Output buffer. Must be at least width * height * 4 floats.</param>
    /// <returns>
    /// True if the input was structurally valid and decoding ran, false if it was rejected
    /// up front. Individual malformed blocks are skipped and leave zeros in the output.
    /// </returns>
    public static bool DecompressHdrImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint, Span<float> imageBuffer)
    {
        ValidateImageArgs(width, height, imageBuffer.Length, ChannelsPerPixel);

        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
        {
            return false;
        }

        float[] decodedBlock = ArrayPool<float>.Shared.Rent(footprint.PixelCount * ChannelsPerPixel);
        try
        {
            DecodeAllBlocks<HdrPipeline, float>(
                astcData, width, height, footprint, blocksWide, blocksHigh, imageBuffer, decodedBlock.AsSpan());
        }
        finally
        {
            ArrayPool<float>.Shared.Return(decodedBlock);
        }

        return true;
    }

    /// <summary>
    /// Decompresses ASTC-compressed data to RGBA values.
    /// </summary>
    /// <param name="astcData">The ASTC-compressed texture data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="footprint">The ASTC block footprint type</param>
    /// <returns>
    /// Values in RGBA order. For HDR content, values may exceed 1.0.
    /// </returns>
    public static Span<float> DecompressHdrImage(ReadOnlySpan<byte> astcData, int width, int height, FootprintType footprint)
    {
        Footprint footPrint = Footprint.FromFootprintType(footprint);
        return DecompressHdrImage(astcData, width, height, footPrint);
    }

    /// <summary>
    /// Decompresses a single ASTC block to float RGBA values.
    /// </summary>
    /// <param name="blockData">The 16-byte ASTC block to decode</param>
    /// <param name="footprint">The ASTC block footprint</param>
    /// <param name="buffer">The buffer to write decoded values into (must be at least footprint.Width * footprint.Height * 4 elements)</param>
    public static void DecompressHdrBlock(ReadOnlySpan<byte> blockData, Footprint footprint, Span<float> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockData.Length, BlockInfo.SizeInBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, footprint.PixelCount * ChannelsPerPixel);

        DecodeSingleBlock<HdrPipeline, float>(blockData, footprint, buffer);
    }

    internal static Span<byte> DecompressImage(AstcFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        return DecompressImage(file.Blocks, file.Width, file.Height, file.Footprint);
    }

    internal static Span<byte> DecompressImage(ReadOnlySpan<byte> astcData, int width, int height, FootprintType footprint)
    {
        Footprint footPrint = Footprint.FromFootprintType(footprint);

        return DecompressImage(astcData, width, height, footPrint);
    }

    private static bool TryGetBlockLayout(
        ReadOnlySpan<byte> astcData,
        int width,
        int height,
        Footprint footprint,
        out int blocksWide,
        out int blocksHigh)
    {
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        blocksWide = 0;
        blocksHigh = 0;

        if (blockWidth <= 0 || blockHeight <= 0 || width <= 0 || height <= 0)
        {
            return false;
        }

        blocksWide = (width + blockWidth - 1) / blockWidth;
        blocksHigh = (height + blockHeight - 1) / blockHeight;

        // Guard against integer overflow in block count calculation
        long expectedBlockCount = (long)blocksWide * blocksHigh;
        if (astcData.Length % BlockInfo.SizeInBytes != 0 || astcData.Length / BlockInfo.SizeInBytes != expectedBlockCount)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the given ASTC block encodes HDR content: either the HDR void-extent
    /// flag (bit 9 of the block mode, ASTC spec §C.2.23) or any HDR endpoint mode in its
    /// partitions (modes 2, 3, 7, 11, 14, 15 per §C.2.14). Used by the LDR decoder to reject
    /// HDR content before dispatch per §C.2.19.
    /// </summary>
    internal static bool IsHdrBlock(UInt128 blockBits, in BlockInfo info)
    {
        // ASTC spec §C.2.23: for void-extent blocks bit 9 of the block mode distinguishes
        // LDR (1, UNORM16) from HDR (0, FP16).
        if (info.IsVoidExtent)
        {
            return (blockBits.Low() & (1UL << 9)) != 0;
        }

        return info.HasHdrEndpoints();
    }

    /// <summary>
    /// Validates that <paramref name="width"/> and <paramref name="height"/> are positive,
    /// that width × height × <paramref name="bytesPerPixel"/> does not overflow
    /// <see cref="int.MaxValue"/>, and that <paramref name="bufferLength"/> has room for
    /// the decoded output.
    /// </summary>
    private static void ValidateImageArgs(int width, int height, int bufferLength, int bytesPerPixel)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        long totalPixels = (long)width * height;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalPixels, (long)int.MaxValue / bytesPerPixel);

        long totalElements = totalPixels * bytesPerPixel;
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferLength, totalElements);
    }

    /// <summary>
    /// Reads a 16-byte ASTC block starting at <paramref name="blockData"/>[0] into a
    /// <see cref="UInt128"/> and decodes its <see cref="BlockInfo"/>. Returns false when
    /// the block is structurally invalid (the caller should skip it without output).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReadBlockInfo(ReadOnlySpan<byte> blockData, out UInt128 blockBits, out BlockInfo info)
    {
        blockBits = BinaryPrimitives.ReadUInt128LittleEndian(blockData);
        info = BlockModeDecoder.Decode(blockBits);
        return info.IsValid;
    }

    /// <summary>
    /// Reads the 16 bytes of the ASTC block at <paramref name="blockIndex"/> into a
    /// <see cref="UInt128"/> (little-endian). Returns false if the stream is short.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReadBlockBits(ReadOnlySpan<byte> astcData, int blockIndex, out UInt128 blockBits)
    {
        int offset = blockIndex * BlockInfo.SizeInBytes;
        if (offset + BlockInfo.SizeInBytes > astcData.Length)
        {
            blockBits = default;
            return false;
        }

        blockBits = BinaryPrimitives.ReadUInt128LittleEndian(astcData.Slice(offset, BlockInfo.SizeInBytes));
        return true;
    }

    /// <summary>
    /// Computes the destination rectangle for the block at (<paramref name="blockX"/>,
    /// <paramref name="blockY"/>) given the image bounds, clipping the footprint extents
    /// to fit inside the image.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BlockDestination ComputeBlockDestination(int blockX, int blockY, Footprint footprint, int width, int height)
    {
        int dstBaseX = blockX * footprint.Width;
        int dstBaseY = blockY * footprint.Height;
        int copyWidth = Math.Min(footprint.Width, width - dstBaseX);
        int copyHeight = Math.Min(footprint.Height, height - dstBaseY);
        bool isFullInterior = copyWidth == footprint.Width && copyHeight == footprint.Height;
        return new BlockDestination(dstBaseX, dstBaseY, copyWidth, copyHeight, isFullInterior);
    }

    /// <summary>
    /// Copies a decoded block from its scratch buffer into the image at the block's pixel
    /// offset, row by row, clamped to the image bounds on right/bottom edges.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopyBlockRect<T>(
        ReadOnlySpan<T> source,
        Span<T> destination,
        int blockWidth,
        int copyWidth,
        int copyHeight,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        int channelsPerPixel)
    {
        int copyElements = copyWidth * channelsPerPixel;
        for (int pixelY = 0; pixelY < copyHeight; pixelY++)
        {
            int srcOffset = pixelY * blockWidth * channelsPerPixel;
            int dstOffset = (((dstBaseY + pixelY) * imageWidth) + dstBaseX) * channelsPerPixel;
            source.Slice(srcOffset, copyElements).CopyTo(destination.Slice(dstOffset, copyElements));
        }
    }
}
