// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Represents the dimensions of an ASTC block footprint.
/// </summary>
public readonly record struct Footprint
{
    private static readonly Footprint[] All =
    [
        new(FootprintType.Footprint4x4, 4, 4),
        new(FootprintType.Footprint5x4, 5, 4),
        new(FootprintType.Footprint5x5, 5, 5),
        new(FootprintType.Footprint6x5, 6, 5),
        new(FootprintType.Footprint6x6, 6, 6),
        new(FootprintType.Footprint8x5, 8, 5),
        new(FootprintType.Footprint8x6, 8, 6),
        new(FootprintType.Footprint8x8, 8, 8),
        new(FootprintType.Footprint10x5, 10, 5),
        new(FootprintType.Footprint10x6, 10, 6),
        new(FootprintType.Footprint10x8, 10, 8),
        new(FootprintType.Footprint10x10, 10, 10),
        new(FootprintType.Footprint12x10, 12, 10),
        new(FootprintType.Footprint12x12, 12, 12),
    ];

    private Footprint(FootprintType type, int width, int height)
    {
        this.Type = type;
        this.Width = width;
        this.Height = height;
        this.PixelCount = width * height;
    }

    /// <summary>Gets the block width in texels.</summary>
    public int Width { get; }

    /// <summary>Gets the block height in texels.</summary>
    public int Height { get; }

    /// <summary>Gets the footprint type enum value.</summary>
    public FootprintType Type { get; }

    /// <summary>Gets the total number of texels in the block (Width * Height).</summary>
    public int PixelCount { get; }

    /// <summary>
    /// Creates a <see cref="Footprint"/> from the specified <see cref="FootprintType"/>.
    /// </summary>
    /// <param name="type">The footprint type to create a footprint from.</param>
    /// <returns>A <see cref="Footprint"/> matching the specified type.</returns>
    public static Footprint FromFootprintType(FootprintType type)
        => (uint)type < (uint)All.Length
            ? All[(int)type]
            : throw new ArgumentOutOfRangeException(nameof(type), $"Invalid FootprintType: {type}");

    internal static Footprint Get4x4() => All[(int)FootprintType.Footprint4x4];

    internal static Footprint Get5x4() => All[(int)FootprintType.Footprint5x4];

    internal static Footprint Get5x5() => All[(int)FootprintType.Footprint5x5];

    internal static Footprint Get6x5() => All[(int)FootprintType.Footprint6x5];

    internal static Footprint Get6x6() => All[(int)FootprintType.Footprint6x6];

    internal static Footprint Get8x5() => All[(int)FootprintType.Footprint8x5];

    internal static Footprint Get8x6() => All[(int)FootprintType.Footprint8x6];

    internal static Footprint Get8x8() => All[(int)FootprintType.Footprint8x8];

    internal static Footprint Get10x5() => All[(int)FootprintType.Footprint10x5];

    internal static Footprint Get10x6() => All[(int)FootprintType.Footprint10x6];

    internal static Footprint Get10x8() => All[(int)FootprintType.Footprint10x8];

    internal static Footprint Get10x10() => All[(int)FootprintType.Footprint10x10];

    internal static Footprint Get12x10() => All[(int)FootprintType.Footprint12x10];

    internal static Footprint Get12x12() => All[(int)FootprintType.Footprint12x12];
}
