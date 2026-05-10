// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Scalar weighted-blend primitives shared by the fused fast paths and the general
/// <c>LogicalBlock</c> pipeline. The weight is always in the ASTC 6-bit range [0, 64];
/// callers pre-unquantise before invoking these helpers.
/// </summary>
internal static class Interpolation
{
    /// <summary>
    /// Weighted blend of two values with the ASTC rounding convention:
    /// <c>(p0 * (64 - weight) + p1 * weight + 32) / 64</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlendWeighted(int p0, int p1, int weight)
        => ((p0 * (64 - weight)) + (p1 * weight) + 32) / 64;

    /// <summary>
    /// LDR-to-UNORM16 blend: each 8-bit endpoint is bit-replicated to 16 bits
    /// (<c>(p &lt;&lt; 8) | p</c>) before the weighted blend. Used by every decode path that
    /// turns LDR endpoints into 16-bit intermediate values.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlendLdrReplicated(int p0, int p1, int weight)
        => BlendWeighted((p0 << 8) | p0, (p1 << 8) | p1, weight);
}
