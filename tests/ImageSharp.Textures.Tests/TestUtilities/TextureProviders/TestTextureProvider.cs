// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Textures.Formats;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders
{

    public class TestTextureProvider : ITestTextureProvider
    {
        public string MethodName { get; private set; }
        public TestTextureFormat TextureFormat { get; private set; }
        public TestTextureType TextureType { get; private set; }
        public string InputFile { get; private set; }
        public bool IsRegex { get; private set; }

        public virtual Texture GetTexture(ITextureDecoder decoder)
        {
            using FileStream fileStream = File.OpenRead(this.InputFile);

            Texture result = decoder.DecodeTexture(Configuration.Default, fileStream);

            Assert.Equal(fileStream.Length, fileStream.Position);

            return result;
        }

        public TestTextureProvider(
            string methodName,
            TestTextureFormat textureFormat,
            TestTextureType textureType,
            string inputFile,
            bool isRegex)
        {
            this.MethodName = methodName;
            this.TextureFormat = textureFormat;
            this.TextureType = textureType;
            this.InputFile = inputFile;
            this.IsRegex = isRegex;
        }
    }
}
