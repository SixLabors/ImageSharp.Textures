// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders
{

    public class TestTextureProvider : ITestTextureProvider
    {
        public string MethodName { get; private set; }
        public TextureType TextureType { get; private set; }
        public string InputFile { get; private set; }

        public virtual Texture GetTexture(ITextureDecoder decoder)
        {
            string extension = Path.GetExtension(this.InputFile).Substring(1);
            string inputPath = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, extension, this.TextureType.ToString(), this.InputFile);
            using (FileStream fileStream = File.OpenRead(inputPath))
            {
                return decoder.DecodeTexture(Configuration.Default, fileStream);
            }
        }

        public TestTextureProvider(
            string methodName,
            TextureType textureType,
            string inputFile)
        {
            this.MethodName = methodName;
            this.TextureType = textureType;
            this.InputFile = inputFile;
        }
    }
}
