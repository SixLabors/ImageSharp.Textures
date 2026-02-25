// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.Core;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Astc.Tests.HDR;

public class RgbaHdrColorTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldInitializeCorrectly()
    {
        var color = new RgbaHdrColor(1000, 2000, 3000, 4000);

        color.R.Should().Be(1000);
        color.G.Should().Be(2000);
        color.B.Should().Be(3000);
        color.A.Should().Be(4000);
    }

    [Fact]
    public void Indexer_WithValidIndices_ShouldReturnCorrectChannels()
    {
        var color = new RgbaHdrColor(1000, 2000, 3000, 4000);

        color[0].Should().Be(1000);
        color[1].Should().Be(2000);
        color[2].Should().Be(3000);
        color[3].Should().Be(4000);
    }

    [Fact]
    public void Indexer_WithInvalidIndex_ShouldThrowException()
    {
        var color = new RgbaHdrColor(1000, 2000, 3000, 4000);

        Action act = () => _ = color[4];

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FromLdr_WithMinMaxValues_ShouldScaleCorrectly()
    {
        var ldrColor = new RgbaColor(0, 127, 255, 200);

        var hdrColor = RgbaHdrColor.FromRgba(ldrColor);

        hdrColor.R.Should().Be(0);        // 0 * 257 = 0
        hdrColor.G.Should().Be(32639);    // 127 * 257 = 32639
        hdrColor.B.Should().Be(65535);    // 255 * 257 = 65535
        hdrColor.A.Should().Be(51400);    // 200 * 257 = 51400
    }

    [Fact]
    public void ToLdr_WithHdrValues_ShouldDownscaleCorrectly()
    {
        var hdrColor = new RgbaHdrColor(0, 32639, 65535, 51400);

        var ldrColor = hdrColor.ToLowDynamicRange();

        ldrColor.R.Should().Be(0);     // 0 >> 8 = 0
        ldrColor.G.Should().Be(127);   // 32639 >> 8 = 127
        ldrColor.B.Should().Be(255);   // 65535 >> 8 = 255
        ldrColor.A.Should().Be(200);   // 51400 >> 8 = 200
    }

    [Fact]
    public void FromLdr_ToLdr_RoundTrip_ShouldPreserveValues()
    {
        var original = new RgbaColor(50, 100, 150, 200);

        var hdrColor = RgbaHdrColor.FromRgba(original);
        var result = hdrColor.ToLowDynamicRange();

        result.R.Should().Be(original.R);
        result.G.Should().Be(original.G);
        result.B.Should().Be(original.B);
        result.A.Should().Be(original.A);
    }

    [Fact]
    public void IsCloseTo_WithSimilarColors_ShouldReturnTrue()
    {
        var color1 = new RgbaHdrColor(1000, 2000, 3000, 4000);
        var color2 = new RgbaHdrColor(1005, 1995, 3002, 3998);

        var result = color1.IsCloseTo(color2, 10);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCloseTo_WithDifferentColors_ShouldReturnFalse()
    {
        var color1 = new RgbaHdrColor(1000, 2000, 3000, 4000);
        var color2 = new RgbaHdrColor(1020, 2000, 3000, 4000);

        var result = color1.IsCloseTo(color2, 10);

        result.Should().BeFalse();
    }

    [Fact]
    public void Empty_ShouldReturnBlackTransparent()
    {
        var empty = RgbaHdrColor.Empty;

        empty.R.Should().Be(0);
        empty.G.Should().Be(0);
        empty.B.Should().Be(0);
        empty.A.Should().Be(0);
    }

}
