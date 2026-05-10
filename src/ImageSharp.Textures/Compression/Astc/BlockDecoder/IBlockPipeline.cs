// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;

/// <summary>
/// Pipeline strategy for the shared image-decode loop in <see cref="AstcDecoder"/>
/// </summary>
/// <typeparam name="T">Pixel element type — <see cref="byte"/> for LDR, <see cref="float"/> for HDR.</typeparam>
internal interface IBlockPipeline<T>
    where T : unmanaged
{
    /// <summary>Validates the block's profile matches the pipeline (LDR throws on HDR content; HDR has no pre-dispatch restriction).</summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    public void PreDispatchCheck(UInt128 blockBits, in BlockInfo info);

    /// <summary>Fused fast path writing straight to the image buffer at (<paramref name="dstBaseX"/>, <paramref name="dstBaseY"/>).</summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="dstBaseX">Destination x origin in pixels.</param>
    /// <param name="dstBaseY">Destination y origin in pixels.</param>
    /// <param name="imageWidth">Image width in pixels (row stride in pixels).</param>
    /// <param name="imageBuffer">Destination image buffer.</param>
    public void FusedToImage(UInt128 blockBits, in BlockInfo info, Footprint footprint, int dstBaseX, int dstBaseY, int imageWidth, Span<T> imageBuffer);

    /// <summary>Fused fast path writing to a per-block scratch buffer (used at image edges that need cropping).</summary>
    /// <param name="blockBits">Raw 128-bit ASTC block.</param>
    /// <param name="info">Decoded block info.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="decodedPixels">Scratch buffer sized for one full block.</param>
    public void FusedToScratch(UInt128 blockBits, in BlockInfo info, Footprint footprint, Span<T> decodedPixels);

    /// <summary>General pipeline writer for void-extent / multi-partition / dual-plane blocks.</summary>
    /// <param name="logicalBlock">Unpacked logical block.</param>
    /// <param name="footprint">Block footprint.</param>
    /// <param name="decodedPixels">Scratch buffer sized for one full block.</param>
    public void LogicalWrite(LogicalBlock logicalBlock, Footprint footprint, Span<T> decodedPixels);
}
