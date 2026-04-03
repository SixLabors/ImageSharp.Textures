// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

public class Rgba64ExtensionsTests
{
    [Fact]
    public void GetChannel_WithValidIndices_ShouldReturnCorrectChannels()
    {
        Rgba64 color = new(1000, 2000, 3000, 4000);

        Assert.Equal(1000, color.GetChannel(0));
        Assert.Equal(2000, color.GetChannel(1));
        Assert.Equal(3000, color.GetChannel(2));
        Assert.Equal(4000, color.GetChannel(3));
    }

    [Fact]
    public void GetChannel_WithInvalidIndex_ShouldThrowException()
    {
        Rgba64 color = new(1000, 2000, 3000, 4000);

        void Act() => _ = color.GetChannel(4);

        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void FromLdr_WithMinMaxValues_ShouldScaleCorrectly()
    {
        Rgba32 ldrColor = new(0, 127, 255, 200);

        Rgba64 hdrColor = new(ldrColor);

        Assert.Equal(0, hdrColor.R);        // 0 * 257 = 0
        Assert.Equal(32639, hdrColor.G);    // 127 * 257 = 32639
        Assert.Equal(65535, hdrColor.B);    // 255 * 257 = 65535
        Assert.Equal(51400, hdrColor.A);    // 200 * 257 = 51400
    }

    [Fact]
    public void IsCloseTo_WithSimilarColors_ShouldReturnTrue()
    {
        Rgba64 color1 = new(1000, 2000, 3000, 4000);
        Rgba64 color2 = new(1005, 1995, 3002, 3998);

        bool result = color1.IsCloseTo(color2, 10);

        Assert.True(result);
    }

    [Fact]
    public void IsCloseTo_WithDifferentColors_ShouldReturnFalse()
    {
        Rgba64 color1 = new(1000, 2000, 3000, 4000);
        Rgba64 color2 = new(1020, 2000, 3000, 4000);

        bool result = color1.IsCloseTo(color2, 10);

        Assert.False(result);
    }
}
