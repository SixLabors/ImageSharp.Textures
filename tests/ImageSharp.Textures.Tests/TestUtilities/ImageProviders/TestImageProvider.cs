// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Reflection;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Textures.Formats;
using Xunit.Abstractions;

namespace SixLabors.ImageSharp.Textures.Tests
{
    public interface ITestImageProvider
    {
        string FileName { get; }
    }

    public class TestImageProvider : ITestImageProvider
    {
        public string FileName { get; private set; }
        public string MethodName { get; private set; }
        public string OutputSubfolderName { get; private set; }

        public virtual Texture GetTexture(ITextureDecoder decoder)
        {
            throw new NotSupportedException($"Decoder specific GetTexture() is not supported with {this.GetType().Name}!");
        }

        public TestImageProvider(
            string filename,
            string methodName,
            string outputSubfolderName)
        {
            this.FileName = filename;
            this.MethodName = methodName;
            this.OutputSubfolderName = outputSubfolderName;
        }
    }
}
