// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Weight grid metadata for a single block (ASTC spec §C.2.7, §C.2.8).
/// </summary>
internal readonly record struct WeightGrid(int Width, int Height, int Range, int BitCount);
