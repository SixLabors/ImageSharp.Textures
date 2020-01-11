// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Reflection;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit.Sdk;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes
{
    public class WithFileAttribute : DataAttribute
    {
        private readonly TextureType textureType;
        private readonly string inputFile;

        public WithFileAttribute(TextureType textureType, string inputFile)
        {
            this.textureType = textureType;
            this.inputFile = inputFile;
        }

        /// <inheritDoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            var testTextureProvider = new TestTextureProvider(testMethod.Name, this.textureType, this.inputFile);
            yield return new object[] { testTextureProvider };
        }
    }
}
