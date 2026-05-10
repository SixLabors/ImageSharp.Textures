// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;

/// <summary>
/// <see cref="IBlockPipeline{T}"/> implementation for the HDR (float RGBA) profile. Accepts
/// both HDR and LDR endpoint modes — LDR endpoints widen to the [0,1] float range.
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
