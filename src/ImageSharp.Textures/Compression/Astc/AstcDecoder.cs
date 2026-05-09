// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Buffers.Binary;
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        long totalPixels = (long)width * height;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalPixels, (long)int.MaxValue / BytesPerPixelUnorm8);

        long totalBytes = totalPixels * BytesPerPixelUnorm8;
        ArgumentOutOfRangeException.ThrowIfLessThan((long)imageBuffer.Length, totalBytes);

        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
        {
            return false;
        }

        // Rent outside the try/finally so a failing Rent doesn't hand the default sentinel to Return.
        byte[] decodedBlock = ArrayPool.Rent(footprint.Width * footprint.Height * BytesPerPixelUnorm8);

        try
        {
            Span<byte> decodedPixels = decodedBlock.AsSpan();
            int blockIndex = 0;
            int footprintWidth = footprint.Width;
            int footprintHeight = footprint.Height;

            for (int blockY = 0; blockY < blocksHigh; blockY++)
            {
                for (int blockX = 0; blockX < blocksWide; blockX++)
                {
                    int blockDataOffset = blockIndex++ * PhysicalBlock.SizeInBytes;
                    if (blockDataOffset + PhysicalBlock.SizeInBytes > astcData.Length)
                    {
                        continue;
                    }

                    ulong low = BinaryPrimitives.ReadUInt64LittleEndian(astcData[blockDataOffset..]);
                    ulong high = BinaryPrimitives.ReadUInt64LittleEndian(astcData[(blockDataOffset + 8)..]);
                    UInt128 blockBits = new(high, low);

                    int dstBaseX = blockX * footprintWidth;
                    int dstBaseY = blockY * footprintHeight;
                    int copyWidth = Math.Min(footprintWidth, width - dstBaseX);
                    int copyHeight = Math.Min(footprintHeight, height - dstBaseY);

                    BlockInfo info = BlockInfo.Decode(blockBits);
                    if (!info.IsValid)
                    {
                        continue;
                    }

                    // Per ASTC spec §C.2.19, the LDR (decode_unorm8) profile cannot decode blocks
                    // that carry HDR content. ARM's astcenc returns ASTCENC_ERR_BAD_DECODE_MODE in
                    // this case; we do the same via an exception. Callers who want HDR values
                    // should use DecompressHdrImage instead.
                    if (IsHdrBlock(blockBits, in info))
                    {
                        throw new TextureFormatException(
                            "ASTC block uses HDR endpoint data but was passed to the LDR decoder. " +
                            "Use AstcDecoder.DecompressHdrImage to decode HDR content.");
                    }

                    // Fast path: fuse decode directly into image buffer for interior full blocks
                    if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
                        && !info.EndpointMode0.IsHdr()
                        && copyWidth == footprintWidth && copyHeight == footprintHeight)
                    {
                        FusedLdrBlockDecoder.DecompressBlockFusedLdrToImage(
                            blockBits,
                            in info,
                            footprint,
                            dstBaseX,
                            dstBaseY,
                            width,
                            imageBuffer);
                        continue;
                    }

                    // Fallback: decode to temp buffer, then copy
                    if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
                        && !info.EndpointMode0.IsHdr())
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

                    int copyBytes = copyWidth * BytesPerPixelUnorm8;
                    for (int pixelY = 0; pixelY < copyHeight; pixelY++)
                    {
                        int srcOffset = pixelY * footprintWidth * BytesPerPixelUnorm8;
                        int dstOffset = (((dstBaseY + pixelY) * width) + dstBaseX) * BytesPerPixelUnorm8;
                        decodedPixels.Slice(srcOffset, copyBytes)
                            .CopyTo(imageBuffer.Slice(dstOffset, copyBytes));
                    }
                }
            }
        }
        finally
        {
            ArrayPool.Return(decodedBlock);
        }

        return true;
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        long totalPixels = (long)width * height;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(totalPixels, (long)int.MaxValue / 4);

        long totalFloats = totalPixels * 4;
        ArgumentOutOfRangeException.ThrowIfLessThan((long)imageBuffer.Length, totalFloats);

        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
        {
            return false;
        }

        const int channelsPerPixel = 4;

        // Rent outside the try/finally so a failing Rent doesn't hand the default sentinel to Return.
        float[] decodedBlock = ArrayPool<float>.Shared.Rent(footprint.Width * footprint.Height * channelsPerPixel);

        try
        {
            Span<float> decodedPixels = decodedBlock.AsSpan();
            int blockIndex = 0;
            int footprintWidth = footprint.Width;
            int footprintHeight = footprint.Height;

            for (int blockY = 0; blockY < blocksHigh; blockY++)
            {
                for (int blockX = 0; blockX < blocksWide; blockX++)
                {
                    int blockDataOffset = blockIndex++ * PhysicalBlock.SizeInBytes;
                    if (blockDataOffset + PhysicalBlock.SizeInBytes > astcData.Length)
                    {
                        continue;
                    }

                    ulong low = BinaryPrimitives.ReadUInt64LittleEndian(astcData[blockDataOffset..]);
                    ulong high = BinaryPrimitives.ReadUInt64LittleEndian(astcData[(blockDataOffset + 8)..]);
                    UInt128 blockBits = new(high, low);

                    int dstBaseX = blockX * footprintWidth;
                    int dstBaseY = blockY * footprintHeight;
                    int copyWidth = Math.Min(footprintWidth, width - dstBaseX);
                    int copyHeight = Math.Min(footprintHeight, height - dstBaseY);

                    BlockInfo info = BlockInfo.Decode(blockBits);
                    if (!info.IsValid)
                    {
                        continue;
                    }

                    // Fast path: fuse decode directly into image buffer for interior full blocks
                    if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
                        && copyWidth == footprintWidth && copyHeight == footprintHeight)
                    {
                        FusedHdrBlockDecoder.DecompressBlockFusedHdrToImage(
                            blockBits,
                            in info,
                            footprint,
                            dstBaseX,
                            dstBaseY,
                            width,
                            imageBuffer);
                        continue;
                    }

                    // Fused decode to temp buffer for single-partition non-dual-plane
                    if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane)
                    {
                        FusedHdrBlockDecoder.DecompressBlockFusedHdr(blockBits, in info, footprint, decodedPixels);
                    }
                    else
                    {
                        // Fallback: LogicalBlock path for void extent, multi-partition, dual plane
                        LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
                        if (logicalBlock is null)
                        {
                            continue;
                        }

                        for (int row = 0; row < footprintHeight; row++)
                        {
                            for (int column = 0; column < footprintWidth; ++column)
                            {
                                int pixelOffset = (footprintWidth * row * channelsPerPixel) + (column * channelsPerPixel);
                                logicalBlock.WriteHdrPixel(column, row, decodedPixels.Slice(pixelOffset, channelsPerPixel));
                            }
                        }
                    }

                    int copyFloats = copyWidth * channelsPerPixel;
                    for (int pixelY = 0; pixelY < copyHeight; pixelY++)
                    {
                        int srcOffset = pixelY * footprintWidth * channelsPerPixel;
                        int dstOffset = (((dstBaseY + pixelY) * width) + dstBaseX) * channelsPerPixel;
                        decodedPixels.Slice(srcOffset, copyFloats)
                            .CopyTo(imageBuffer.Slice(dstOffset, copyFloats));
                    }
                }
            }
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

        const int channelsPerPixel = 4;
        for (int row = 0; row < footprint.Height; row++)
        {
            for (int column = 0; column < footprint.Width; ++column)
            {
                int pixelOffset = (footprint.Width * row * channelsPerPixel) + (column * channelsPerPixel);
                logicalBlock.WriteHdrPixel(column, row, buffer.Slice(pixelOffset, channelsPerPixel));
            }
        }
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
        // Void-extent: bit 9 of the block-mode prefix distinguishes LDR (1) from HDR (0).
        // Matches ARM's astcenc_symbolic_physical.cpp — `if (block_mode & 0x200) SYM_BTYPE_CONST_F16`.
        if (info.IsVoidExtent)
        {
            return (blockBits.Low() & (1UL << 9)) != 0;
        }

        return info.HasHdrEndpoints();
    }
}
