// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Scalar weighted-blend primitives from ASTC spec §C.2.19 (Weight Application),
/// shared by the fused fast paths and the general <c>LogicalBlock</c> pipeline.
/// The weight is in the 6-bit range [0, 64]; callers pre-unquantise per §C.2.17.
/// </summary>
internal static class Interpolation
{
    /// <summary>
    /// Weighted blend of two values with the ASTC rounding convention from §C.2.19:
    /// <c>(p0 * (64 - weight) + p1 * weight + 32) / 64</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlendWeighted(int p0, int p1, int weight)
        => ((p0 * (64 - weight)) + (p1 * weight) + 32) / 64;

    /// <summary>
    /// LDR-to-UNORM16 blend: each 8-bit endpoint is bit-replicated to 16 bits
    /// (<c>(p &lt;&lt; 8) | p</c>) per §C.2.19 before the weighted blend. Every LDR decode
    /// path that produces 16-bit intermediate values goes through this primitive.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlendLdrReplicated(int p0, int p1, int weight)
        => BlendWeighted((p0 << 8) | p0, (p1 << 8) | p1, weight);

    /// <summary>
    /// Normalises a UNORM16 value (clamped to [0, 0xFFFF]) to the [0.0, 1.0] float range.
    /// Used by the HDR output path when an LDR endpoint or mode-14 LDR alpha (ASTC spec §C.2.14)
    /// has already been interpolated as an integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Unorm16ToFloat(int interpolated)
        => Math.Clamp(interpolated, 0, 0xFFFF) / 65535.0f;

    /// <summary>
    /// <see cref="BlendLdrReplicated"/> followed by clamp-to-UNORM16 — the LDR-channel
    /// interpolation path used by the HDR output writer (ASTC spec §C.2.19).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort BlendLdrReplicatedAsUnorm16(int p0, int p1, int weight)
        => (ushort)Math.Clamp(BlendLdrReplicated(p0, p1, weight), 0, 0xFFFF);

    /// <summary>
    /// <see cref="BlendWeighted"/> followed by clamp-to-UNORM16 — the HDR-channel
    /// interpolation path. HDR endpoints are already 16-bit values (FP16 bit patterns), so
    /// no 8→16 expansion is needed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort BlendWeightedAsUnorm16(int p0, int p1, int weight)
        => (ushort)Math.Clamp(BlendWeighted(p0, p1, weight), 0, 0xFFFF);
}
