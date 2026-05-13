// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// <see cref="IBlockPipeline{T}"/> implementation for the HDR (float RGBA) <c>decode_fp16</c>
/// profile (ASTC spec §C.2.25). Accepts both HDR and LDR endpoint modes — LDR endpoints widen
/// to the [0,1] float range; HDR endpoint modes (2, 3, 7, 11, 14, 15 per §C.2.14) decode
/// through LNS → FP16 per §C.2.15.
/// </summary>
internal readonly struct HdrPipeline : IBlockPipeline<float>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PreDispatchCheck(UInt128 blockBits, in BlockInfo info)
    {
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
    public void LogicalWrite(LogicalBlock logicalBlock, Footprint footprint, Span<float> decodedPixels)
        => logicalBlock.WriteAllPixelsHdr(footprint, decodedPixels);
}
