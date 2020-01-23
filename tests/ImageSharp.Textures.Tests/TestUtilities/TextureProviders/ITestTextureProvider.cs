// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Tests.Enums;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders
{
    public interface ITestTextureProvider
    {
        string MethodName { get; }
        TestTextureFormat TextureFormat { get; }
        TestTextureType TextureType { get; }
        string InputFile { get; }
    }
}
