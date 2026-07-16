// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Dual-plane configuration (ASTC spec §C.2.20). When <see cref="Enabled"/> is false,
/// <see cref="Channel"/> is unused.
/// </summary>
internal readonly record struct DualPlaneInfo(bool Enabled, int Channel);
