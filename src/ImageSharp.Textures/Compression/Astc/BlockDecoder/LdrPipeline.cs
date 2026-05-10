// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;

/// <summary>
/// <see cref="IBlockPipeline{T}"/> implementation for the LDR (byte RGBA) profile.
/// Rejects HDR-content blocks at pre-dispatch per ASTC spec §C.2.19.
/// </summary>
internal readonly struct LdrPipeline : IBlockPipeline<byte>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PreDispatchCheck(UInt128 blockBits, in BlockInfo info)
    {
        if (AstcDecoder.IsHdrBlock(blockBits, in info))
        {
            throw new TextureFormatException(
                "ASTC block uses HDR endpoint data but was passed to the LDR decoder. " +
                "Use AstcDecoder.DecompressHdrImage to decode HDR content.");
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
    public void LogicalWrite(LogicalBlock logicalBlock, Footprint footprint, Span<byte> decodedPixels)
        => logicalBlock.WriteAllPixelsLdr(footprint, decodedPixels);
}
