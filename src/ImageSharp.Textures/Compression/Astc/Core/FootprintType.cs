// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// The supported ASTC block footprint sizes.
/// </summary>
public enum FootprintType
{
    /// <summary>4x4 texel block.</summary>
    Footprint4x4,

    /// <summary>5x4 texel block.</summary>
    Footprint5x4,

    /// <summary>5x5 texel block.</summary>
    Footprint5x5,

    /// <summary>6x5 texel block.</summary>
    Footprint6x5,

    /// <summary>6x6 texel block.</summary>
    Footprint6x6,

    /// <summary>8x5 texel block.</summary>
    Footprint8x5,

    /// <summary>8x6 texel block.</summary>
    Footprint8x6,

    /// <summary>8x8 texel block.</summary>
    Footprint8x8,

    /// <summary>10x5 texel block.</summary>
    Footprint10x5,

    /// <summary>10x6 texel block.</summary>
    Footprint10x6,

    /// <summary>10x8 texel block.</summary>
    Footprint10x8,

    /// <summary>10x10 texel block.</summary>
    Footprint10x10,

    /// <summary>12x10 texel block.</summary>
    Footprint12x10,

    /// <summary>12x12 texel block.</summary>
    Footprint12x12,
}
