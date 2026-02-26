// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class FootprintTests
{
    [Theory]
    [InlineData(FootprintType.Footprint4x4, 4, 4)]
    [InlineData(FootprintType.Footprint5x4, 5, 4)]
    [InlineData(FootprintType.Footprint5x5, 5, 5)]
    [InlineData(FootprintType.Footprint6x5, 6, 5)]
    [InlineData(FootprintType.Footprint6x6, 6, 6)]
    [InlineData(FootprintType.Footprint8x5, 8, 5)]
    [InlineData(FootprintType.Footprint8x6, 8, 6)]
    [InlineData(FootprintType.Footprint8x8, 8, 8)]
    [InlineData(FootprintType.Footprint10x5, 10, 5)]
    [InlineData(FootprintType.Footprint10x6, 10, 6)]
    [InlineData(FootprintType.Footprint10x8, 10, 8)]
    [InlineData(FootprintType.Footprint10x10, 10, 10)]
    [InlineData(FootprintType.Footprint12x10, 12, 10)]
    [InlineData(FootprintType.Footprint12x12, 12, 12)]
    public void FromFootprintType_WithValidType_ShouldReturnCorrectDimensions(
        FootprintType type, int expectedWidth, int expectedHeight)
    {
        Footprint footprint = Footprint.FromFootprintType(type);

        footprint.Type.Should().Be(type);
        footprint.Width.Should().Be(expectedWidth);
        footprint.Height.Should().Be(expectedHeight);
        footprint.PixelCount.Should().Be(expectedWidth * expectedHeight);
    }

    [Fact]
    public void FromFootprintType_WithAllValidTypes_ShouldReturnUniqueFootprints()
    {
        FootprintType[] allTypes =
        [
            FootprintType.Footprint4x4, FootprintType.Footprint5x4, FootprintType.Footprint5x5,
            FootprintType.Footprint6x5, FootprintType.Footprint6x6, FootprintType.Footprint8x5,
            FootprintType.Footprint8x6, FootprintType.Footprint8x8, FootprintType.Footprint10x5,
            FootprintType.Footprint10x6, FootprintType.Footprint10x8, FootprintType.Footprint10x10,
            FootprintType.Footprint12x10, FootprintType.Footprint12x12
        ];

        List<Footprint> footprints = [.. allTypes.Select(Footprint.FromFootprintType)];

        footprints.Should().HaveCount(allTypes.Length);
        footprints.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Footprint_PixelCount_ShouldEqualWidthTimesHeight()
    {
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint10x8);

        footprint.PixelCount.Should().Be(footprint.Width * footprint.Height);
        footprint.PixelCount.Should().Be(80);
    }

    [Fact]
    public void Footprint_ValueEquality_WithSameType_ShouldBeEqual()
    {
        Footprint footprint1 = Footprint.FromFootprintType(FootprintType.Footprint6x6);
        Footprint footprint2 = Footprint.FromFootprintType(FootprintType.Footprint6x6);

        footprint1.Should().Be(footprint2);
        (footprint1 == footprint2).Should().BeTrue();
    }

    [Fact]
    public void Footprint_ValueEquality_WithDifferentType_ShouldNotBeEqual()
    {
        Footprint footprint1 = Footprint.FromFootprintType(FootprintType.Footprint6x6);
        Footprint footprint2 = Footprint.FromFootprintType(FootprintType.Footprint8x8);

        footprint1.Should().NotBe(footprint2);
        (footprint1 != footprint2).Should().BeTrue();
    }
}
