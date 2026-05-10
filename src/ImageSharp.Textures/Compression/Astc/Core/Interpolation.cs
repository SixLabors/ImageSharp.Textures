// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Scalar weighted-blend primitives from ASTC spec §C.2.24 (texel selection and
/// interpolation), shared by the fused fast paths and the general <c>LogicalBlock</c>
/// pipeline. The weight is in the 6-bit range [0, 64]; callers pre-unquantise per §C.2.18.
/// </summary>
internal static class Interpolation
{
    /// <summary>
    /// Weighted blend of two values with the ASTC rounding convention from §C.2.24:
    /// <c>(p0 * (64 - weight) + p1 * weight + 32) / 64</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlendWeighted(int p0, int p1, int weight)
        => ((p0 * (64 - weight)) + (p1 * weight) + 32) / 64;

    /// <summary>
    /// LDR-to-UNORM16 blend: each 8-bit endpoint is bit-replicated to 16 bits
    /// (<c>(p &lt;&lt; 8) | p</c>) per §C.2.24 before the weighted blend. Every LDR decode
    /// path that produces 16-bit intermediate values goes through this primitive.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlendLdrReplicated(int p0, int p1, int weight)
        => BlendWeighted((p0 << 8) | p0, (p1 << 8) | p1, weight);
}
