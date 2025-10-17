// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Tests.Enums;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders
{
    public interface ITestTextureProvider
    {
        string MethodName { get; }

        /// <summary>
        /// Gets the utility instance to provide information about the test image & manage input/output.
        /// </summary>
        ImagingTestCaseUtility Utility { get; }

        /// <summary>
        /// Gets the texture container format.
        /// </summary>
        TestTextureFormat TextureFormat { get; }

        /// <summary>
        /// Gets the type of the texture, e.g. flat, volume or cubemap.
        /// </summary>
        TestTextureType TextureType { get; }

        TestTextureTool TextureTool { get; }

        string InputFile { get; }

        bool IsRegex { get; }
    }
}
