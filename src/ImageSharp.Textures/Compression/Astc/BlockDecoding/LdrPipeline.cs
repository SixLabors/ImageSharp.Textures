// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// <see cref="IBlockPipeline{T}"/> implementation for the LDR (byte RGBA) decode profile
/// (ASTC spec §C.2.5 "LDR Mode"). HDR-mode blocks are reserved in the LDR profile per §C.2.25
/// and produce the error colour (magenta) per §C.2.19, §C.2.24.
/// </summary>
internal readonly struct LdrPipeline : IBlockPipeline<byte>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockLegal(in BlockInfo info) => !info.IsHdr;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteErrorColor(Footprint footprint, Span<byte> buffer)
        => FillMagenta(buffer[..(footprint.PixelCount * BlockInfo.ChannelsPerPixel)]);

    /// <inheritdoc />
    public void WriteErrorColorClipped(
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int copyWidth,
        int copyHeight,
        int imageWidth,
        Span<byte> imageBuffer)
    {
        int rowElements = copyWidth * BlockInfo.ChannelsPerPixel;
        for (int pixelY = 0; pixelY < copyHeight; pixelY++)
        {
            int dstOffset = (((dstBaseY + pixelY) * imageWidth) + dstBaseX) * BlockInfo.ChannelsPerPixel;
            FillMagenta(imageBuffer.Slice(dstOffset, rowElements));
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FusedToImage(UInt128 blockBits, in BlockInfo info, Footprint footprint, int dstBaseX, int dstBaseY, int imageWidth, Span<byte> imageBuffer)
        => FusedLdrBlockDecoder.DecompressBlockFusedLdrToImage(blockBits, in info, footprint, dstBaseX, dstBaseY, imageWidth, imageBuffer);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FusedToScratch(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<byte> decodedPixels)
        => FusedLdrBlockDecoder.DecompressBlockFusedLdr(blockBits, in info, footprint, decodedPixels);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogicalWrite(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<byte> decodedPixels)
        => LogicalBlock.DecodeToBytes(blockBits, in info, footprint, decodedPixels);

    /// <summary>
    /// Spec §C.2.19 error colour: opaque magenta <c>(0xFF, 0x00, 0xFF, 0xFF)</c> as UNORM8 RGBA.
    /// </summary>
    private static void FillMagenta(Span<byte> buffer)
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
