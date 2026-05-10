// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;

/// <summary>
/// Pipeline strategy for the shared image-decode loop in <see cref="AstcDecoder"/>. Each
/// ASTC decode profile (spec §C.2.25 — <c>decode_unorm8</c>, <c>decode_fp16</c>) provides a
/// concrete implementation.
/// </summary>
/// <typeparam name="T">Pixel element type — <see cref="byte"/> for LDR, <see cref="float"/> for HDR.</typeparam>
internal interface IBlockPipeline<T>
    where T : unmanaged
{
    /// <summary>
    /// Validates the block's profile matches the pipeline.
    /// The LDR (<c>decode_unorm8</c>) profile rejects HDR-mode
    /// blocks per spec §C.2.19; the HDR pipeline accepts both.</summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    public void PreDispatchCheck(UInt128 blockBits, in BlockInfo info);

    /// <summary>
    /// Fused fast path writing straight to the image buffer at
    /// (<paramref name="dstBaseX"/>, <paramref name="dstBaseY"/>).
    /// Handles the common shape — single-partition, single-plane,
    /// non-void-extent (spec §C.2.10–§C.2.20) — by fusing BISE
    /// decode + unquantise + weight infill + pixel write.
    /// </summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="dstBaseX">Destination x origin in pixels.</param>
    /// <param name="dstBaseY">Destination y origin in pixels.</param>
    /// <param name="imageWidth">Image width in pixels (row stride in pixels).</param>
    /// <param name="imageBuffer">Destination image buffer.</param>
    public void FusedToImage(UInt128 blockBits, in BlockInfo info, Footprint footprint, int dstBaseX, int dstBaseY, int imageWidth, Span<T> imageBuffer);

    /// <summary>
    /// Fused fast path writing to a per-block scratch buffer (used at
    /// image edges that need cropping). Same decode shape as <see cref="FusedToImage"/>.
    /// </summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="decodedPixels">Scratch buffer sized for one full block.</param>
    public void FusedToScratch(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<T> decodedPixels);

    /// <summary>
    /// General pipeline writer for blocks the fused path cannot handle:
    /// void-extent (spec §C.2.23), multi-partition (spec §C.2.21), and dual-plane (spec §C.2.20).
    /// </summary>
    /// <param name="logicalBlock">Unpacked logical block.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="decodedPixels">Scratch buffer sized for one full block.</param>
    public void LogicalWrite(LogicalBlock logicalBlock, Footprint footprint, Span<T> decodedPixels);
}
