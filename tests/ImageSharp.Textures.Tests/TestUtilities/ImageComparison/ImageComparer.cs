// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison.Exceptions;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison
{
    public abstract class ImageComparer
    {
        public static ImageComparer Exact { get; } = Tolerant(0, 0);

        /// <summary>
        /// Returns an instance of <see cref="TolerantImageComparer"/>.
        /// Individual manhattan pixel difference is only added to total image difference when the individual difference is over 'perPixelManhattanThreshold'.
        /// </summary>
        public static ImageComparer Tolerant(
            float imageThreshold = TolerantImageComparer.DefaultImageThreshold,
            int perPixelManhattanThreshold = 0)
        {
            return new TolerantImageComparer(imageThreshold, perPixelManhattanThreshold);
        }

        /// <summary>
        /// Returns Tolerant(imageThresholdInPercents/100)
        /// </summary>
        public static ImageComparer TolerantPercentage(float imageThresholdInPercents, int perPixelManhattanThreshold = 0)
            => Tolerant(imageThresholdInPercents / 100F, perPixelManhattanThreshold);

        public abstract ImageSimilarityReport<TPixelA, TPixelB> CompareImages<TPixelA, TPixelB>(
            Image<TPixelA> expected,
            Image<TPixelB> actual)
            where TPixelA : unmanaged, IPixel<TPixelA>
            where TPixelB : unmanaged, IPixel<TPixelB>;
    }

    public static class ImageComparerExtensions
    {
        public static ImageSimilarityReport<TPixelA, TPixelB> CompareImages<TPixelA, TPixelB>(
            this ImageComparer comparer,
            Image<TPixelA> expected,
            Image<TPixelB> actual)
            where TPixelA : unmanaged, IPixel<TPixelA>
            where TPixelB : unmanaged, IPixel<TPixelB>
        {
            return comparer.CompareImages(expected, actual);
        }

        public static void VerifySimilarity<TPixelA, TPixelB>(
            this ImageComparer comparer,
            Image<TPixelA> expected,
            Image<TPixelB> actual)
            where TPixelA : unmanaged, IPixel<TPixelA>
            where TPixelB : unmanaged, IPixel<TPixelB>
        {
            if (expected.Size() != actual.Size())
            {
                throw new ImageDimensionsMismatchException(expected.Size(), actual.Size());
            }

            if (expected.Frames.Count != actual.Frames.Count)
            {
                throw new ImagesSimilarityException("Image frame count does not match!");
            }

            ImageSimilarityReport report = comparer.CompareImages(expected, actual);
            if ((report.TotalNormalizedDifference ?? 0F) != 0F)
            {
                throw new ImagesSimilarityException(report.ToString());
            }
        }

        public static void VerifySimilarityIgnoreRegion<TPixelA, TPixelB>(
            this ImageComparer comparer,
            Image<TPixelA> expected,
            Image<TPixelB> actual,
            Rectangle ignoredRegion)
            where TPixelA : unmanaged, IPixel<TPixelA>
            where TPixelB : unmanaged, IPixel<TPixelB>
        {
            if (expected.Size() != actual.Size())
            {
                throw new ImageDimensionsMismatchException(expected.Size(), actual.Size());
            }

            if (expected.Frames.Count != actual.Frames.Count)
            {
                throw new ImagesSimilarityException("Image frame count does not match!");
            }

            ImageSimilarityReport report = comparer.CompareImages(expected, actual);
            if ((report.TotalNormalizedDifference ?? 0F) != 0F)
            {
                IEnumerable<PixelDifference> outsideChanges = report.Differences.Where(
                    x =>
                    !(ignoredRegion.X <= x.Position.X
                    && x.Position.X <= ignoredRegion.Right
                    && ignoredRegion.Y <= x.Position.Y
                    && x.Position.Y <= ignoredRegion.Bottom));

                if (outsideChanges.Any())
                {
                    var cleanedReport = new ImageSimilarityReport<TPixelA, TPixelB>(expected, actual, outsideChanges, null);
                    throw new ImagesSimilarityException(cleanedReport.ToString());
                }
            }
        }
    }
}
