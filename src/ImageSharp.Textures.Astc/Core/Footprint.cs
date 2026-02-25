// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.Core;

/// <summary>
/// Represents the dimensions of an ASTC block footprint.
/// </summary>
public readonly record struct Footprint
{
    /// <summary>Gets the block width in texels.</summary>
    public int Width { get; }

    /// <summary>Gets the block height in texels.</summary>
    public int Height { get; }

    /// <summary>Gets the footprint type enum value.</summary>
    public FootprintType Type { get; }

    /// <summary>Gets the total number of texels in the block (Width * Height).</summary>
    public int PixelCount { get; }

    private Footprint(FootprintType type, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);

        this.Type = type;
        this.Width = width;
        this.Height = height;
        this.PixelCount = width * height;
    }

    /// <summary>
    /// Creates a <see cref="Footprint"/> from the specified <see cref="FootprintType"/>.
    /// </summary>
    public static Footprint FromFootprintType(FootprintType type) => type switch
    {
        FootprintType.Footprint4x4 => Get4x4(),
        FootprintType.Footprint5x4 => Get5x4(),
        FootprintType.Footprint5x5 => Get5x5(),
        FootprintType.Footprint6x5 => Get6x5(),
        FootprintType.Footprint6x6 => Get6x6(),
        FootprintType.Footprint8x5 => Get8x5(),
        FootprintType.Footprint8x6 => Get8x6(),
        FootprintType.Footprint8x8 => Get8x8(),
        FootprintType.Footprint10x5 => Get10x5(),
        FootprintType.Footprint10x6 => Get10x6(),
        FootprintType.Footprint10x8 => Get10x8(),
        FootprintType.Footprint10x10 => Get10x10(),
        FootprintType.Footprint12x10 => Get12x10(),
        FootprintType.Footprint12x12 => Get12x12(),
        _ => throw new ArgumentOutOfRangeException($"Invalid FootprintType: {type}"),
    };

    internal static Footprint Get4x4() => new(FootprintType.Footprint4x4, 4, 4);
    internal static Footprint Get5x4() => new(FootprintType.Footprint5x4, 5, 4);
    internal static Footprint Get5x5() => new(FootprintType.Footprint5x5, 5, 5);
    internal static Footprint Get6x5() => new(FootprintType.Footprint6x5, 6, 5);
    internal static Footprint Get6x6() => new(FootprintType.Footprint6x6, 6, 6);
    internal static Footprint Get8x5() => new(FootprintType.Footprint8x5, 8, 5);
    internal static Footprint Get8x6() => new(FootprintType.Footprint8x6, 8, 6);
    internal static Footprint Get8x8() => new(FootprintType.Footprint8x8, 8, 8);
    internal static Footprint Get10x5() => new(FootprintType.Footprint10x5, 10, 5);
    internal static Footprint Get10x6() => new(FootprintType.Footprint10x6, 10, 6);
    internal static Footprint Get10x8() => new(FootprintType.Footprint10x8, 10, 8);
    internal static Footprint Get10x10() => new(FootprintType.Footprint10x10, 10, 10);
    internal static Footprint Get12x10() => new(FootprintType.Footprint12x10, 12, 10);
    internal static Footprint Get12x12() => new(FootprintType.Footprint12x12, 12, 12);
}
