// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc;

/// <summary>
/// Destination pixel rectangle for one ASTC block in the output image: the top-left pixel
/// offset, the clipped copy extents (equal to the footprint for interior blocks, smaller
/// for right/bottom edge blocks), and a flag set when the block's full footprint fits in
/// the image and the fused direct-to-image fast path is usable.
/// </summary>
internal readonly record struct BlockDestination(int DstBaseX, int DstBaseY, int CopyWidth, int CopyHeight, bool IsFullInteriorBlock);
