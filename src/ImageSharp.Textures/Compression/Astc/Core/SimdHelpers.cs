// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

internal static class SimdHelpers
{
    private static readonly Vector128<int> Vec32 = Vector128.Create(32);
    private static readonly Vector128<int> Vec64 = Vector128.Create(64);
    private static readonly Vector128<int> Vec255 = Vector128.Create(255);

    /// <summary>
    /// Interpolates one channel for 4 pixels simultaneously.
    /// All 4 pixels share the same endpoint values but have different weights.
    /// Returns 4 byte results packed into the lower bytes of a <see cref="Vector128{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> Interpolate4ChannelPixels(int p0, int p1, Vector128<int> weights)
    {
        // Bit-replicate endpoint bytes to 16-bit
        Vector128<int> c0 = Vector128.Create((p0 << 8) | p0);
        Vector128<int> c1 = Vector128.Create((p1 << 8) | p1);

        // c = (c0 * (64 - w) + c1 * w + 32) >> 6
        // NOTE: Using >> 6 instead of / 64 because Vector128<int> division
        // has no hardware support and decomposes to scalar operations.
        Vector128<int> w64 = Vec64 - weights;
        Vector128<int> c = ((c0 * w64) + (c1 * weights) + Vec32) >> 6;

        // Spec §C.2.19 (Weight Application): for LDR-mode UNORM8 output the final
        // 8-bit result is the top 8 bits of the UNORM16 interpolation. Mask
        // to [0, 255] to defend against malformed endpoints producing c outside
        // [0, 0xFFFF]; well-formed input is already in range.
        return (c >>> 8) & Vec255;
    }

    /// <summary>
    /// Writes 4 LDR pixels directly to output buffer using SIMD.
    /// Processes each channel across 4 pixels in parallel, then interleaves to RGBA output.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write4PixelLdr(
        Span<byte> output,
        int offset,
        int lowR,
        int lowG,
        int lowB,
        int lowA,
        int highR,
        int highG,
        int highB,
        int highA,
        Vector128<int> weights)
    {
        Vector128<int> r = Interpolate4ChannelPixels(lowR, highR, weights);
        Vector128<int> g = Interpolate4ChannelPixels(lowG, highG, weights);
        Vector128<int> b = Interpolate4ChannelPixels(lowB, highB, weights);
        Vector128<int> a = Interpolate4ChannelPixels(lowA, highA, weights);

        // Pack 4 RGBA pixels into 16 bytes via vector OR+shift.
        // Each int element has its channel value in bits [0:7].
        // Combine: element[i] = R[i] | (G[i] << 8) | (B[i] << 16) | (A[i] << 24)
        // On little-endian, storing this int32 writes bytes [R, G, B, A].
        Vector128<int> rgba = r | (g << 8) | (b << 16) | (a << 24);
        rgba.AsByte().CopyTo(output.Slice(offset, 16));
    }

    /// <summary>
    /// Scalar single-pixel LDR interpolation, writing directly to buffer.
    /// No Rgba32 allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSinglePixelLdr(
        Span<byte> output,
        int offset,
        int lowR,
        int lowG,
        int lowB,
        int lowA,
        int highR,
        int highG,
        int highB,
        int highA,
        int weight)
    {
        output[offset + 0] = (byte)InterpolateChannelScalar(lowR, highR, weight);
        output[offset + 1] = (byte)InterpolateChannelScalar(lowG, highG, weight);
        output[offset + 2] = (byte)InterpolateChannelScalar(lowB, highB, weight);
        output[offset + 3] = (byte)InterpolateChannelScalar(lowA, highA, weight);
    }

    /// <summary>
    /// Scalar single-pixel dual-plane LDR interpolation, writing directly to buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSinglePixelLdrDualPlane(
        Span<byte> output,
        int offset,
        int lowR,
        int lowG,
        int lowB,
        int lowA,
        int highR,
        int highG,
        int highB,
        int highA,
        int weight,
        int dpChannel,
        int dpWeight)
    {
        output[offset + 0] = (byte)InterpolateChannelScalar(
            lowR,
            highR,
            dpChannel == 0 ? dpWeight : weight);
        output[offset + 1] = (byte)InterpolateChannelScalar(
            lowG,
            highG,
            dpChannel == 1 ? dpWeight : weight);
        output[offset + 2] = (byte)InterpolateChannelScalar(
            lowB,
            highB,
            dpChannel == 2 ? dpWeight : weight);
        output[offset + 3] = (byte)InterpolateChannelScalar(
            lowA,
            highA,
            dpChannel == 3 ? dpWeight : weight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int InterpolateChannelScalar(int p0, int p1, int weight)
    {
        // Spec §C.2.19 (Weight Application): for LDR-mode UNORM8 output the final
        // 8-bit result is the top 8 bits of the UNORM16 interpolation.
        int c = Interpolation.BlendLdrReplicated(p0, p1, weight);
        return (c >> 8) & 0xFF;
    }
}
