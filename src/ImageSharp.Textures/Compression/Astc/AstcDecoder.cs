// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

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
    private const int BytesPerPixelUnorm8 = 4;

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
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalPixels, (long)int.MaxValue / BytesPerPixelUnorm8);

        int totalBytes = (int)(totalPixels * BytesPerPixelUnorm8);
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
        ValidateImageArgs(width, height, imageBuffer.Length, BytesPerPixelUnorm8);

        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
        {
            return false;
        }

        // Scratch is rented outside the try/finally so a failing Rent never hands the default
        // sentinel to Return.
        byte[] decodedBlock = ArrayPool.Rent(footprint.PixelCount * BytesPerPixelUnorm8);
        try
        {
            DecodeAllBlocksLdr(astcData, width, height, footprint, blocksWide, blocksHigh, imageBuffer, decodedBlock);
        }
        finally
        {
            ArrayPool.Return(decodedBlock);
        }

        return true;
    }

    private static void DecodeAllBlocksLdr(
        ReadOnlySpan<byte> astcData,
        int width,
        int height,
        Footprint footprint,
        int blocksWide,
        int blocksHigh,
        Span<byte> imageBuffer,
        byte[] decodedBlock)
    {
        Span<byte> decodedPixels = decodedBlock.AsSpan();
        int blockIndex = 0;
        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;

        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            for (int blockX = 0; blockX < blocksWide; blockX++)
            {
                int index = blockIndex++;
                if (!TryReadBlockBits(astcData, index, out UInt128 blockBits))
                {
                    continue;
                }

                BlockInfo info = BlockInfo.Decode(blockBits);
                if (!info.IsValid)
                {
                    continue;
                }

                // ASTC spec §C.2.19: the LDR (decode_unorm8) profile cannot decode blocks that
                // carry HDR content. Callers wanting HDR values must use DecompressHdrImage.
                if (IsHdrBlock(blockBits, in info))
                {
                    throw new TextureFormatException(
                        "ASTC block uses HDR endpoint data but was passed to the LDR decoder. " +
                        "Use AstcDecoder.DecompressHdrImage to decode HDR content.");
                }

                int dstBaseX = blockX * footprintWidth;
                int dstBaseY = blockY * footprintHeight;
                int copyWidth = Math.Min(footprintWidth, width - dstBaseX);
                int copyHeight = Math.Min(footprintHeight, height - dstBaseY);

                bool isFusableLdr = !info.IsVoidExtent && info.PartitionCount == 1
                    && !info.IsDualPlane && !info.EndpointMode0.IsHdr();
                bool isFullInteriorBlock = copyWidth == footprintWidth && copyHeight == footprintHeight;

                // Fast path: fuse decode directly into image buffer for interior full blocks.
                if (isFusableLdr && isFullInteriorBlock)
                {
                    FusedLdrBlockDecoder.DecompressBlockFusedLdrToImage(
                        blockBits, in info, footprint, dstBaseX, dstBaseY, width, imageBuffer);
                    continue;
                }

                if (isFusableLdr)
                {
                    FusedLdrBlockDecoder.DecompressBlockFusedLdr(blockBits, in info, footprint, decodedPixels);
                }
                else
                {
                    LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
                    if (logicalBlock is null)
                    {
                        continue;
                    }

                    logicalBlock.WriteAllPixelsLdr(footprint, decodedPixels);
                }

                CopyBlockRect(decodedPixels, imageBuffer, footprintWidth, copyWidth, copyHeight, dstBaseX, dstBaseY, width, BytesPerPixelUnorm8);
            }
        }
    }

    /// <summary>
    /// Decompresses a single ASTC block to RGBA8 pixel data
    /// </summary>
    /// <param name="blockData">The data to decode</param>
    /// <param name="footprint">The type of ASTC block footprint e.g. 4x4, 5x5, etc.</param>
    /// <param name="buffer">The buffer to write the decoded pixels into</param>
    public static void DecompressBlock(ReadOnlySpan<byte> blockData, Footprint footprint, Span<byte> buffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blockData.Length, PhysicalBlock.SizeInBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, footprint.PixelCount * BytesPerPixelUnorm8);

        // Read the 16 bytes that make up the ASTC block as a 128-bit value
        ulong low = BinaryPrimitives.ReadUInt64LittleEndian(blockData);
        ulong high = BinaryPrimitives.ReadUInt64LittleEndian(blockData[8..]);
        UInt128 blockBits = new(high, low);

        BlockInfo info = BlockInfo.Decode(blockBits);
        if (!info.IsValid)
        {
            return;
        }

        // Per ASTC spec §C.2.19, the LDR (decode_unorm8) profile cannot decode blocks
        // that carry HDR content. See DecompressImage for the same guard.
        if (IsHdrBlock(blockBits, in info))
        {
            throw new TextureFormatException(
                "ASTC block uses HDR endpoint data but was passed to the LDR decoder. " +
                "Use AstcDecoder.DecompressHdrBlock to decode HDR content.");
        }

        // Fully fused fast path for single-partition, non-dual-plane, LDR blocks
        if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
            && !info.EndpointMode0.IsHdr())
        {
            FusedLdrBlockDecoder.DecompressBlockFusedLdr(blockBits, in info, footprint, buffer);
            return;
        }

        // Fallback for void extent, multi-partition, dual plane, HDR
        LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
        if (logicalBlock is null)
        {
            return;
        }

        logicalBlock.WriteAllPixelsLdr(footprint, buffer);
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
        const int channelsPerPixel = 4;
        ValidateImageArgs(width, height, imageBuffer.Length, channelsPerPixel);

        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
        {
            return false;
        }

        float[] decodedBlock = ArrayPool<float>.Shared.Rent(footprint.PixelCount * channelsPerPixel);
        try
        {
            DecodeAllBlocksHdr(astcData, width, height, footprint, blocksWide, blocksHigh, imageBuffer, decodedBlock);
        }
        finally
        {
            ArrayPool<float>.Shared.Return(decodedBlock);
        }

        return true;
    }

    private static void DecodeAllBlocksHdr(
        ReadOnlySpan<byte> astcData,
        int width,
        int height,
        Footprint footprint,
        int blocksWide,
        int blocksHigh,
        Span<float> imageBuffer,
        float[] decodedBlock)
    {
        const int channelsPerPixel = 4;
        Span<float> decodedPixels = decodedBlock.AsSpan();
        int blockIndex = 0;
        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;

        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            for (int blockX = 0; blockX < blocksWide; blockX++)
            {
                int index = blockIndex++;
                if (!TryReadBlockBits(astcData, index, out UInt128 blockBits))
                {
                    continue;
                }

                BlockInfo info = BlockInfo.Decode(blockBits);
                if (!info.IsValid)
                {
                    continue;
                }

                int dstBaseX = blockX * footprintWidth;
                int dstBaseY = blockY * footprintHeight;
                int copyWidth = Math.Min(footprintWidth, width - dstBaseX);
                int copyHeight = Math.Min(footprintHeight, height - dstBaseY);

                bool isFusableHdr = !info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane;
                bool isFullInteriorBlock = copyWidth == footprintWidth && copyHeight == footprintHeight;

                // Fast path: fuse decode directly into image buffer for interior full blocks.
                if (isFusableHdr && isFullInteriorBlock)
                {
                    FusedHdrBlockDecoder.DecompressBlockFusedHdrToImage(
                        blockBits, in info, footprint, dstBaseX, dstBaseY, width, imageBuffer);
                    continue;
                }

                if (isFusableHdr)
                {
                    FusedHdrBlockDecoder.DecompressBlockFusedHdr(blockBits, in info, footprint, decodedPixels);
                }
                else
                {
                    // Fallback: void-extent, multi-partition or dual-plane blocks go through the
                    // generic LogicalBlock pipeline.
                    LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
                    if (logicalBlock is null)
                    {
                        continue;
                    }

                    logicalBlock.WriteAllPixelsHdr(footprint, decodedPixels);
                }

                CopyBlockRect(decodedPixels, imageBuffer, footprintWidth, copyWidth, copyHeight, dstBaseX, dstBaseY, width, channelsPerPixel);
            }
        }
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
        ArgumentOutOfRangeException.ThrowIfLessThan(blockData.Length, PhysicalBlock.SizeInBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, footprint.PixelCount * 4);

        // Read the 16 bytes that make up the ASTC block as a 128-bit value
        ulong low = BinaryPrimitives.ReadUInt64LittleEndian(blockData);
        ulong high = BinaryPrimitives.ReadUInt64LittleEndian(blockData[8..]);
        UInt128 blockBits = new(high, low);

        BlockInfo info = BlockInfo.Decode(blockBits);
        if (!info.IsValid)
        {
            return;
        }

        // Fused fast path for single-partition, non-dual-plane blocks
        if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane)
        {
            FusedHdrBlockDecoder.DecompressBlockFusedHdr(blockBits, in info, footprint, buffer);
            return;
        }

        // Fallback for void extent, multi-partition, dual plane
        LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
        if (logicalBlock is null)
        {
            return;
        }

        logicalBlock.WriteAllPixelsHdr(footprint, buffer);
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
        if (astcData.Length % PhysicalBlock.SizeInBytes != 0 || astcData.Length / PhysicalBlock.SizeInBytes != expectedBlockCount)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the given ASTC block encodes HDR content (HDR endpoint modes in
    /// any partition, or an HDR void-extent flag). The LDR decoder paths use this as a
    /// precondition check — HDR content must be routed through the HDR decoder instead.
    /// </summary>
    private static bool IsHdrBlock(UInt128 blockBits, in BlockInfo info)
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
        ArgumentOutOfRangeException.ThrowIfLessThan((long)bufferLength, totalElements);
    }

    /// <summary>
    /// Reads the 16 bytes of the ASTC block at <paramref name="blockIndex"/> into a
    /// <see cref="UInt128"/> (little-endian). Returns false if the stream is short.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReadBlockBits(ReadOnlySpan<byte> astcData, int blockIndex, out UInt128 blockBits)
    {
        int offset = blockIndex * PhysicalBlock.SizeInBytes;
        if (offset + PhysicalBlock.SizeInBytes > astcData.Length)
        {
            blockBits = default;
            return false;
        }

        ulong low = BinaryPrimitives.ReadUInt64LittleEndian(astcData[offset..]);
        ulong high = BinaryPrimitives.ReadUInt64LittleEndian(astcData[(offset + 8)..]);
        blockBits = new UInt128(high, low);
        return true;
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
