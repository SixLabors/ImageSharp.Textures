// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// <see cref="IBlockPipeline{T}"/> implementation for the HDR (float RGBA) decode profile
/// (ASTC spec §C.2.5 "HDR Mode"). Accepts both HDR and LDR endpoint modes — LDR endpoints
/// widen to the [0,1] float range; HDR endpoint modes (2, 3, 7, 11, 14, 15 per §C.2.14)
/// decode through LNS → FP16 per §C.2.15.
/// </summary>
internal readonly struct HdrPipeline : IBlockPipeline<float>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockLegal(in BlockInfo info) => true;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteErrorColor(Footprint footprint, Span<float> buffer)
        => FillMagenta(buffer[..(footprint.PixelCount * BlockInfo.ChannelsPerPixel)]);

    /// <inheritdoc />
    public void WriteErrorColorClipped(
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int copyWidth,
        int copyHeight,
        int imageWidth,
        Span<float> imageBuffer)
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
    public void FusedToImage(UInt128 blockBits, in BlockInfo info, Footprint footprint, int dstBaseX, int dstBaseY, int imageWidth, Span<float> imageBuffer)
        => FusedHdrBlockDecoder.DecompressBlockFusedHdrToImage(blockBits, in info, footprint, dstBaseX, dstBaseY, imageWidth, imageBuffer);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FusedToScratch(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<float> decodedPixels)
        => FusedHdrBlockDecoder.DecompressBlockFusedHdr(blockBits, in info, footprint, decodedPixels);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogicalWrite(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<float> decodedPixels)
        => LogicalBlock.DecodeToFloats(blockBits, in info, footprint, decodedPixels);

    /// <summary>
    /// Spec §C.2.19 error colour: opaque magenta in the float profile — <c>(1, 0, 1, 1)</c>.
    /// </summary>
    private static void FillMagenta(Span<float> buffer)
    {
        for (int i = 0; i < buffer.Length; i += BlockInfo.ChannelsPerPixel)
        {
            buffer[i] = 1f;
            buffer[i + 1] = 0f;
            buffer[i + 2] = 1f;
            buffer[i + 3] = 1f;
        }
    }
}
