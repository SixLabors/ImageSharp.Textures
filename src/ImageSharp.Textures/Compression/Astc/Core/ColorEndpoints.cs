// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Colour-endpoint bit-region metadata (ASTC spec §C.2.22 — colour endpoint range and bit
/// budget are derived from the remaining-bits computation).
/// </summary>
internal readonly record struct ColorEndpoints(int StartBit, int BitCount, int Range, int Count);
