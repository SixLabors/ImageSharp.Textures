// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;
using SixLabors.ImageSharp.Textures.Astc.BlockDecoder;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Astc;

/// <summary>
/// Provides methods to decode ASTC-compressed texture data into uncompressed pixel formats.
/// </summary>
public static class AstcDecoder
{
    private static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    private const int BytesPerPixelUnorm8 = 4;

    /// <summary>
    /// Decompresses ASTC-compressed data to uncompressed RGBA8 format (4 bytes per pixel).
    /// </summary>
    /// <param name="astcData">The ASTC-compressed texture data</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="footprint">The ASTC block footprint (e.g., 4x4, 5x5)</param>
    /// <returns>Array of bytes in RGBA8 format (width * height * 4 bytes total)</returns>
    /// <exception cref="InvalidOperationException">If decompression fails for any block</exception>
    public static Span<byte> DecompressImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint)
    {
        var imageBuffer = new byte[width * height * BytesPerPixelUnorm8];

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
    /// <returns>True if decompression succeeded, false if input was invalid.</returns>
    /// <exception cref="InvalidOperationException">If decompression fails for any block</exception>
    public static bool DecompressImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint, Span<byte> imageBuffer)
    {
        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
            return false;

        var decodedBlock = Array.Empty<byte>();

        try
        {
            // Create a buffer once for fallback blocks; fast path writes directly to image
            decodedBlock = _arrayPool.Rent(footprint.Width * footprint.Height * BytesPerPixelUnorm8);
            var decodedPixels = decodedBlock.AsSpan();
            int blockIndex = 0;
            int footprintWidth = footprint.Width;
            int footprintHeight = footprint.Height;

            for (int blockY = 0; blockY < blocksHigh; blockY++)
            {
                for (int blockX = 0; blockX < blocksWide; blockX++)
                {
                    int blockDataOffset = blockIndex++ * PhysicalBlock.SizeInBytes;
                    if (blockDataOffset + PhysicalBlock.SizeInBytes > astcData.Length)
                        continue;

                    ulong low = BinaryPrimitives.ReadUInt64LittleEndian(astcData.Slice(blockDataOffset));
                    ulong high = BinaryPrimitives.ReadUInt64LittleEndian(astcData.Slice(blockDataOffset + 8));
                    var blockBits = new UInt128(high, low);

                    int dstBaseX = blockX * footprintWidth;
                    int dstBaseY = blockY * footprintHeight;
                    int copyWidth = Math.Min(footprintWidth, width - dstBaseX);
                    int copyHeight = Math.Min(footprintHeight, height - dstBaseY);

                    var info = BlockInfo.Decode(blockBits);
                    if (!info.IsValid) continue;

                    // Fast path: fuse decode directly into image buffer for interior full blocks
                    if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
                        && !info.EndpointMode0.IsHdr()
                        && copyWidth == footprintWidth && copyHeight == footprintHeight)
                    {
                        FusedLdrBlockDecoder.DecompressBlockFusedLdrToImage(
                            blockBits, in info, footprint,
                            dstBaseX, dstBaseY, width, imageBuffer);
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
                        var logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
                        if (logicalBlock is null) continue;
                        logicalBlock.WriteAllPixelsLdr(footprint, decodedPixels);
                    }

                    int copyBytes = copyWidth * BytesPerPixelUnorm8;
                    for (int pixelY = 0; pixelY < copyHeight; pixelY++)
                    {
                        int srcOffset = pixelY * footprintWidth * BytesPerPixelUnorm8;
                        int dstOffset = ((dstBaseY + pixelY) * width + dstBaseX) * BytesPerPixelUnorm8;
                        decodedPixels.Slice(srcOffset, copyBytes)
                            .CopyTo(imageBuffer.Slice(dstOffset, copyBytes));
                    }
                }
            }
        }
        finally
        {
            _arrayPool.Return(decodedBlock);
        }

        return true;
    }

    /// <summary>
    /// Decompress a single ASTC block to RGBA8 pixel data
    /// </summary>
    /// <param name="blockData">The data to decode</param>
    /// <param name="footprint">The type of ASTC block footprint e.g. 4x4, 5x5, etc.</param>
    /// <returns>The decoded block of pixels as RGBA values</returns>
    public static Span<byte> DecompressBlock(ReadOnlySpan<byte> blockData, Footprint footprint)
    {
        var decodedPixels = Array.Empty<byte>();
        try
        {
            decodedPixels = _arrayPool.Rent(footprint.Width * footprint.Height * BytesPerPixelUnorm8);
            var decodedPixelBuffer = decodedPixels.AsSpan();

            DecompressBlock(blockData, footprint, decodedPixelBuffer);
        }
        finally
        {
            _arrayPool.Return(decodedPixels);
        }

        return decodedPixels;
    }

    /// <summary>
    /// Decompresses a single ASTC block to RGBA8 pixel data
    /// </summary>
    /// <param name="blockData">The data to decode</param>
    /// <param name="footprint">The type of ASTC block footprint e.g. 4x4, 5x5, etc.</param>
    /// <param name="buffer">The buffer to write the decoded pixels into</param>
    /// <returns>The decoded block of pixels as RGBA values</returns>
    public static void DecompressBlock(ReadOnlySpan<byte> blockData, Footprint footprint, Span<byte> buffer)
    {
        // Read the 16 bytes that make up the ASTC block as a 128-bit value
        ulong low = BinaryPrimitives.ReadUInt64LittleEndian(blockData);
        ulong high = BinaryPrimitives.ReadUInt64LittleEndian(blockData.Slice(8));
        var blockBits = new UInt128(high, low);

        var info = BlockInfo.Decode(blockBits);
        if (!info.IsValid) return;

        // Fully fused fast path for single-partition, non-dual-plane, LDR blocks
        if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
            && !info.EndpointMode0.IsHdr())
        {
            FusedLdrBlockDecoder.DecompressBlockFusedLdr(blockBits, in info, footprint, buffer);
            return;
        }

        // Fallback for void extent, multi-partition, dual plane, HDR
        var logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
        if (logicalBlock is null) return;
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
        const int channelsPerPixel = 4;
        var imageBuffer = new float[width * height * channelsPerPixel];
        if (!DecompressHdrImage(astcData, width, height, footprint, imageBuffer))
            return [];
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
    /// <returns>True if decompression succeeded, false if input was invalid.</returns>
    /// <exception cref="InvalidOperationException">If decompression fails for any block</exception>
    public static bool DecompressHdrImage(ReadOnlySpan<byte> astcData, int width, int height, Footprint footprint, Span<float> imageBuffer)
    {
        if (!TryGetBlockLayout(astcData, width, height, footprint, out int blocksWide, out int blocksHigh))
            return false;

        const int channelsPerPixel = 4;
        var decodedBlock = Array.Empty<float>();

        try
        {
            // Create a buffer once for fallback blocks; fast path writes directly to image
            decodedBlock = ArrayPool<float>.Shared.Rent(footprint.Width * footprint.Height * channelsPerPixel);
            var decodedPixels = decodedBlock.AsSpan();
            int blockIndex = 0;
            int footprintWidth = footprint.Width;
            int footprintHeight = footprint.Height;

            for (int blockY = 0; blockY < blocksHigh; blockY++)
            {
                for (int blockX = 0; blockX < blocksWide; blockX++)
                {
                    int blockDataOffset = blockIndex++ * PhysicalBlock.SizeInBytes;
                    if (blockDataOffset + PhysicalBlock.SizeInBytes > astcData.Length)
                        continue;

                    ulong low = BinaryPrimitives.ReadUInt64LittleEndian(astcData.Slice(blockDataOffset));
                    ulong high = BinaryPrimitives.ReadUInt64LittleEndian(astcData.Slice(blockDataOffset + 8));
                    var blockBits = new UInt128(high, low);

                    int dstBaseX = blockX * footprintWidth;
                    int dstBaseY = blockY * footprintHeight;
                    int copyWidth = Math.Min(footprintWidth, width - dstBaseX);
                    int copyHeight = Math.Min(footprintHeight, height - dstBaseY);

                    var info = BlockInfo.Decode(blockBits);
                    if (!info.IsValid) continue;

                    // Fast path: fuse decode directly into image buffer for interior full blocks
                    if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
                        && copyWidth == footprintWidth && copyHeight == footprintHeight)
                    {
                        FusedHdrBlockDecoder.DecompressBlockFusedHdrToImage(
                            blockBits, in info, footprint,
                            dstBaseX, dstBaseY, width, imageBuffer);
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
                        var logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
                        if (logicalBlock is null) continue;
                        for (int row = 0; row < footprintHeight; row++)
                        {
                            for (int column = 0; column < footprintWidth; ++column)
                            {
                                var pixelOffset = (footprintWidth * row * channelsPerPixel) + (column * channelsPerPixel);
                                logicalBlock.WriteHdrPixel(column, row, decodedPixels.Slice(pixelOffset, channelsPerPixel));
                            }
                        }
                    }

                    int copyFloats = copyWidth * channelsPerPixel;
                    for (int pixelY = 0; pixelY < copyHeight; pixelY++)
                    {
                        int srcOffset = pixelY * footprintWidth * channelsPerPixel;
                        int dstOffset = ((dstBaseY + pixelY) * width + dstBaseX) * channelsPerPixel;
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
        var footPrint = Footprint.FromFootprintType(footprint);
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
        // Read the 16 bytes that make up the ASTC block as a 128-bit value
        ulong low = BinaryPrimitives.ReadUInt64LittleEndian(blockData);
        ulong high = BinaryPrimitives.ReadUInt64LittleEndian(blockData.Slice(8));
        var blockBits = new UInt128(high, low);

        var info = BlockInfo.Decode(blockBits);
        if (!info.IsValid) return;

        // Fused fast path for single-partition, non-dual-plane blocks
        if (!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane)
        {
            FusedHdrBlockDecoder.DecompressBlockFusedHdr(blockBits, in info, footprint, buffer);
            return;
        }

        // Fallback for void extent, multi-partition, dual plane
        var logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, blockBits, in info);
        if (logicalBlock is null) return;

        const int channelsPerPixel = 4;
        for (int row = 0; row < footprint.Height; row++)
        {
            for (int column = 0; column < footprint.Width; ++column)
            {
                var pixelOffset = (footprint.Width * row * channelsPerPixel) + (column * channelsPerPixel);
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
        var footPrint = Footprint.FromFootprintType(footprint);

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

        if (blockWidth == 0 || blockHeight == 0 || width == 0 || height == 0)
            return false;

        blocksWide = (width + blockWidth - 1) / blockWidth;
        if (blocksWide == 0)
            return false;

        blocksHigh = (height + blockHeight - 1) / blockHeight;
        int expectedBlockCount = blocksWide * blocksHigh;
        if (astcData.Length % PhysicalBlock.SizeInBytes != 0 || astcData.Length / PhysicalBlock.SizeInBytes != expectedBlockCount)
            return false;

        return true;
    }
}
