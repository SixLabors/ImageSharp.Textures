// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// Geometry helpers for placing decoded fixed-footprint blocks into an output image
/// </summary>
internal static class BlockImageWriter
{
    /// <summary>
    /// Computes the destination rectangle for the block at (<paramref name="blockX"/>,
    /// <paramref name="blockY"/>), clipping the footprint extents to fit inside the image.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlockDestination ComputeBlockDestination(int blockX, int blockY, Footprint footprint, int width, int height)
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
    /// <remarks>
    /// The channels-per-pixel factor is fixed at <see cref="BlockInfo.ChannelsPerPixel"/> (RGBA).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBlockRect<T>(
        ReadOnlySpan<T> source,
        Span<T> destination,
        int blockWidth,
        int copyWidth,
        int copyHeight,
        int dstBaseX,
        int dstBaseY,
        int imageWidth)
    {
        int copyElements = copyWidth * BlockInfo.ChannelsPerPixel;
        for (int pixelY = 0; pixelY < copyHeight; pixelY++)
        {
            int srcOffset = pixelY * blockWidth * BlockInfo.ChannelsPerPixel;
            int dstOffset = (((dstBaseY + pixelY) * imageWidth) + dstBaseX) * BlockInfo.ChannelsPerPixel;
            source.Slice(srcOffset, copyElements).CopyTo(destination.Slice(dstOffset, copyElements));
        }
    }

    /// <summary>
    /// Fills <paramref name="buffer"/> with the spec-mandated LDR error colour (ASTC spec §C.2.19).
    /// </summary>
    /// <param name="buffer">The buffer to fill. Must be a multiple of <see cref="BlockInfo.ChannelsPerPixel"/></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillErrorColor(Span<byte> buffer)
    {
        for (int i = 0; i < buffer.Length; i += BlockInfo.ChannelsPerPixel)
        {
            buffer[i] = 0xFF;
            buffer[i + 1] = 0x00;
            buffer[i + 2] = 0xFF;
            buffer[i + 3] = 0xFF;
        }
    }
}
