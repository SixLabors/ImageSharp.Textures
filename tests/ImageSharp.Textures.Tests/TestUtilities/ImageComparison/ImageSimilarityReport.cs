// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison
{
    public class ImageSimilarityReport
    {
        // Provide an empty non-generic instance to avoid CA1000 in generic type.
        public static ImageSimilarityReport Empty => new ImageSimilarityReport(null, null, Array.Empty<PixelDifference>(), 0f);

        protected ImageSimilarityReport(
            object expectedImage,
            object actualImage,
            IEnumerable<PixelDifference> differences,
            float? totalNormalizedDifference = null)
        {
            this.ExpectedImage = expectedImage;
            this.ActualImage = actualImage;
            this.TotalNormalizedDifference = totalNormalizedDifference;
            this.Differences = differences.ToArray();
        }

        public object ExpectedImage { get; }

        public object ActualImage { get; }

        // TODO: This should not be a nullable value!
        public float? TotalNormalizedDifference { get; }

        public string DifferencePercentageString
        {
            get
            {
                if (!this.TotalNormalizedDifference.HasValue)
                {
                    return "?";
                }
                else if (this.TotalNormalizedDifference == 0)
                {
                    return "0%";
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:0.0000}%", this.TotalNormalizedDifference.Value * 100);
                }
            }
        }

        public PixelDifference[] Differences { get; }

        public bool IsEmpty => this.Differences.Length == 0;

        public override string ToString()
        {
            return this.IsEmpty ? "[SimilarImages]" : this.PrintDifference();
        }

        private string PrintDifference()
        {
            var sb = new StringBuilder();
            if (this.TotalNormalizedDifference.HasValue)
            {
                sb.AppendLine();
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total difference: {0}", this.DifferencePercentageString));
            }

            int max = Math.Min(5, this.Differences.Length);

            for (int i = 0; i < max; i++)
            {
                sb.Append(this.Differences[i]);
                if (i < max - 1)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ";{0}", Environment.NewLine);
                }
            }

            if (this.Differences.Length >= 5)
            {
                sb.Append("...");
            }

            return sb.ToString();
        }
    }

    public class ImageSimilarityReport<TPixelA, TPixelB> : ImageSimilarityReport
        where TPixelA : unmanaged, IPixel<TPixelA>
        where TPixelB : unmanaged, IPixel<TPixelB>
    {
        public ImageSimilarityReport(
            Image<TPixelA> expectedImage,
            Image<TPixelB> actualImage,
            IEnumerable<PixelDifference> differences,
            float? totalNormalizedDifference = null)
            : base(expectedImage, actualImage, differences, totalNormalizedDifference)
        {
        }

        public new Image<TPixelA> ExpectedImage => (Image<TPixelA>)base.ExpectedImage;

        public new Image<TPixelB> ActualImage => (Image<TPixelB>)base.ActualImage;
    }
}
