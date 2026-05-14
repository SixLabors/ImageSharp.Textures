// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// LDR <see cref="IPixelWriter{T}"/> — writes UNORM8 RGBA bytes via the scalar SIMD helpers.
/// </summary>
internal readonly struct LdrPixelWriter : IPixelWriter<byte>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePixel(Span<byte> buffer, int offset, in ColorEndpointPair endpoint, int weight)
        => SimdHelpers.WriteSinglePixelLdr(
            buffer,
            offset,
            endpoint.LdrLow.R,
            endpoint.LdrLow.G,
            endpoint.LdrLow.B,
            endpoint.LdrLow.A,
            endpoint.LdrHigh.R,
            endpoint.LdrHigh.G,
            endpoint.LdrHigh.B,
            endpoint.LdrHigh.A,
            weight);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePixelDualPlane(
        Span<byte> buffer,
        int offset,
        in ColorEndpointPair endpoint,
        int primaryWeight,
        int dualPlaneChannel,
        int dualPlaneWeight)
        => SimdHelpers.WriteSinglePixelLdrDualPlane(
            buffer,
            offset,
            endpoint.LdrLow.R,
            endpoint.LdrLow.G,
            endpoint.LdrLow.B,
            endpoint.LdrLow.A,
            endpoint.LdrHigh.R,
            endpoint.LdrHigh.G,
            endpoint.LdrHigh.B,
            endpoint.LdrHigh.A,
            primaryWeight,
            dualPlaneChannel,
            dualPlaneWeight);
}
