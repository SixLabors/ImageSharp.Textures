// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageProviders
{
    public interface ITestImageProvider
    {
        PixelTypes PixelType { get; }

        ImagingTestCaseUtility Utility { get; }

        string SourceFileOrDescription { get; }

        ImageSharp.Configuration Configuration { get; set; }
    }
}
