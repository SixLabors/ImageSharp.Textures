namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison.Exceptions
{
    using System;

    public class ImagesSimilarityException : Exception
    {
        public ImagesSimilarityException(string message)
            : base(message)
        {
        }
    }
}
