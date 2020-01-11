// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders
{
    public interface ITestTextureProvider
    {
        string MethodName { get; }
        TextureType TextureType { get; }
        string InputFile { get; }
    }
}
