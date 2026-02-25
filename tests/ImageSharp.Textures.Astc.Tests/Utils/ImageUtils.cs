// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Astc.Tests.Utils;

internal static class ImageUtils
{
    public static void CompareSumOfSquaredDifferences(ImageBuffer expected, ImageBuffer actual, double threshold)
    {
        actual.DataSize.Should().Be(expected.DataSize);
        actual.Stride.Should().Be(expected.Stride);
        actual.BytesPerPixel.Should().Be(expected.BytesPerPixel);

        var expectedData = expected.Data;
        var actualData = actual.Data;

        double sum = 0.0;
        for (int i = 0; i < actualData.Length; ++i)
        {
            double diff = (double)actualData[i] - expectedData[i];
            sum += diff * diff;
        }

        sum.Should().BeLessThanOrEqualTo(threshold * actualData.Length, because: $"Per pixel {(sum / actualData.Length)}, expected <= {threshold}");
        if (sum > threshold * actualData.Length)
        {
            actualData.Should().BeEquivalentTo(expectedData);
        }
    }
}
