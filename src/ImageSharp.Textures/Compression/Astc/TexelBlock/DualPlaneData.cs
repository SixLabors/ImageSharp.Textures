// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

/// <summary>
/// Per-pixel second weight plane (ASTC spec §C.2.20). A dual-plane block writes weights for
/// two channels: the primary plane drives three channels, and the plane identified by
/// <paramref name="Channel"/> drives the fourth using <paramref name="Weights"/>.
/// </summary>
/// <param name="Channel">Index (0=R, 1=G, 2=B, 3=A) of the channel driven by the secondary plane.</param>
/// <param name="Weights">Per-texel weights for the secondary plane. Array contents are filled
/// in place by the decoder after construction.</param>
internal readonly record struct DualPlaneData(int Channel, int[] Weights);
