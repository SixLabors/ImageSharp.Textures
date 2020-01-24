// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit.Sdk;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes
{
    public class WithFileAttribute : DataAttribute
    {
        private readonly TestTextureFormat textureFormat;
        private readonly TestTextureType textureType;
        private readonly string inputFile;
        private readonly bool isRegex;

        public WithFileAttribute(TestTextureFormat textureFormat, TestTextureType textureType, string inputFile, bool isRegex = false)
        {
            this.textureFormat = textureFormat;
            this.textureType = textureType;
            this.inputFile = inputFile;
            this.isRegex = isRegex;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, this.textureFormat.ToString(), this.textureType.ToString());
            string[] files = Directory.GetFiles(path);
            string[] filteredFiles = files.Where(f => this.isRegex ? new Regex(this.inputFile).IsMatch(Path.GetFileName(f)) : Path.GetFileName(f).Equals(this.inputFile, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            foreach (string file in filteredFiles)
            {
                var testTextureProvider = new TestTextureProvider(testMethod.Name, this.textureFormat, this.textureType, file, false);
                yield return new object[] { testTextureProvider };
            }
        }
    }
}
