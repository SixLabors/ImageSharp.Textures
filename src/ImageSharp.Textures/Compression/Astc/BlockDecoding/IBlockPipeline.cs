// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// Pipeline strategy for the shared image-decode loop in <see cref="AstcDecoder"/>. Each
/// ASTC decode profile (spec §C.2.5 — LDR or HDR mode) provides a concrete implementation.
/// </summary>
/// <typeparam name="T">Pixel element type — <see cref="byte"/> for LDR, <see cref="float"/> for HDR.</typeparam>
internal interface IBlockPipeline<T>
    where T : unmanaged
{
    /// <summary>
    /// Returns true if <paramref name="info"/> is decodable under this profile. The LDR
    /// pipeline returns false for HDR-mode blocks (spec §C.2.19, §C.2.25 — HDR endpoint
    /// formats are reserved in the LDR profile and produce the error colour). The HDR
    /// pipeline accepts every legal block.
    /// </summary>
    /// <param name="info">Decoded block info.</param>
    /// <returns>True if the block can be decoded by this pipeline.</returns>
    public bool IsBlockLegal(in BlockInfo info);

    /// <summary>
    /// Writes the spec-mandated error colour (ASTC spec §C.2.19, §C.2.24) into a
    /// footprint-sized region of <paramref name="buffer"/> starting at offset 0. Magenta
    /// (R=1, G=0, B=1, A=1) in both profiles.
    /// </summary>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="buffer">Scratch or image buffer; the first <c>footprint.PixelCount</c>
    /// pixels are overwritten.</param>
    public void WriteErrorColor(Footprint footprint, Span<T> buffer);

    /// <summary>
    /// Writes the spec-mandated error colour into the image buffer at
    /// (<paramref name="dstBaseX"/>, <paramref name="dstBaseY"/>) for a footprint-sized
    /// region, clipped to <paramref name="copyWidth"/> × <paramref name="copyHeight"/>.
    /// Used at edge blocks where the footprint extends beyond the image.
    /// </summary>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="dstBaseX">Destination x origin in pixels.</param>
    /// <param name="dstBaseY">Destination y origin in pixels.</param>
    /// <param name="copyWidth">Clipped block width in pixels.</param>
    /// <param name="copyHeight">Clipped block height in pixels.</param>
    /// <param name="imageWidth">Image width in pixels (row stride in pixels).</param>
    /// <param name="imageBuffer">Destination image buffer.</param>
    public void WriteErrorColorClipped(
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int copyWidth,
        int copyHeight,
        int imageWidth,
        Span<T> imageBuffer);

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
    /// Implementations forward to the appropriate <see cref="LogicalBlock"/> decode entry.
    /// </summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="decodedPixels">Scratch buffer sized for one full block.</param>
    public void LogicalWrite(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<T> decodedPixels);
}
