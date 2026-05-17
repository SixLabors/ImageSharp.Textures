// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// Per-pixel writer strategy for the general (logical-block) decode loop in <see cref="LogicalBlock"/>.
/// </summary>
/// <typeparam name="T">Pixel element type — <see cref="byte"/> for LDR (UNORM8 RGBA), <see cref="float"/> for HDR (float32 RGBA).</typeparam>
internal interface IPixelWriter<T>
    where T : unmanaged
{
    /// <summary>
    /// Writes one pixel at <c>buffer[offset..offset+4]</c> using <paramref name="weight"/>
    /// for every channel.
    /// </summary>
    /// <param name="buffer">Destination pixel buffer.</param>
    /// <param name="offset">Element offset of the pixel's first channel.</param>
    /// <param name="endpoint">Per-partition endpoint pair for this texel.</param>
    /// <param name="weight">Unquantised weight (0..64) for every channel.</param>
    void WritePixel(Span<T> buffer, int offset, in ColorEndpointPair endpoint, int weight);

    /// <summary>
    /// Writes one pixel where the channel identified by <paramref name="dualPlaneChannel"/>
    /// uses <paramref name="dualPlaneWeight"/> instead of <paramref name="primaryWeight"/>
    /// (ASTC spec §C.2.20).
    /// </summary>
    /// <param name="buffer">Destination pixel buffer.</param>
    /// <param name="offset">Element offset of the pixel's first channel.</param>
    /// <param name="endpoint">Per-partition endpoint pair for this texel.</param>
    /// <param name="primaryWeight">Unquantised weight (0..64) for the three primary-plane channels.</param>
    /// <param name="dualPlaneChannel">RGBA channel index (0..3) driven by the secondary plane.</param>
    /// <param name="dualPlaneWeight">Unquantised weight (0..64) for the dual-plane channel at this texel.</param>
    void WritePixelDualPlane(
        Span<T> buffer,
        int offset,
        in ColorEndpointPair endpoint,
        int primaryWeight,
        int dualPlaneChannel,
        int dualPlaneWeight);
}
