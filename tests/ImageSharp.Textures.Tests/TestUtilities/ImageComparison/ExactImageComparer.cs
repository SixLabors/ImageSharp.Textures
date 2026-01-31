// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison
{
    public class ExactImageComparer : ImageComparer
    {
        public static ExactImageComparer Instance { get; } = new ExactImageComparer();

        public override ImageSimilarityReport<TPixelA, TPixelB> CompareImages<TPixelA, TPixelB>(
            Image<TPixelA> expected,
            Image<TPixelB> actual)
        {
            if (expected.Size != actual.Size)
            {
                throw new InvalidOperationException("Calling ImageComparer is invalid when dimensions mismatch!");
            }

            int width = actual.Width;

            // TODO: Comparing through Rgba64 may not be robust enough because of the existence of super high precision pixel types.
            var aBuffer = new Vector4[width];
            var bBuffer = new Vector4[width];

            Buffer2D<TPixelA> expectedBuffer = expected.Frames.RootFrame.PixelBuffer;
            Buffer2D<TPixelB> actualBuffer = actual.Frames.RootFrame.PixelBuffer;

            var differences = new List<PixelDifference>();
            ImageSharp.Configuration configuration = expected.Configuration;

            for (int y = 0; y < actual.Height; y++)
            {
                Span<TPixelA> aSpan = expectedBuffer.DangerousGetRowSpan(y);
                Span<TPixelB> bSpan = actualBuffer.DangerousGetRowSpan(y);

                PixelOperations<TPixelA>.Instance.ToVector4(configuration, aSpan, aBuffer);
                PixelOperations<TPixelB>.Instance.ToVector4(configuration, bSpan, bBuffer);

                for (int x = 0; x < width; x++)
                {
                    Vector4 aPixel = aBuffer[x];
                    Vector4 bPixel = bBuffer[x];

                    if (aPixel != bPixel)
                    {
                        var diff = new PixelDifference(new Point(x, y), aPixel, bPixel);
                        differences.Add(diff);
                    }
                }
            }

            return new ImageSimilarityReport<TPixelA, TPixelB>(expected, actual, differences);
        }
    }
}
