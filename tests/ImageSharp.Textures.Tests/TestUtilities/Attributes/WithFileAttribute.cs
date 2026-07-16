// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit.Sdk;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes
{
    public class WithFileAttribute : DataAttribute
    {
        private readonly TestTextureFormat textureFormat;
        private readonly TestTextureType textureType;
        private readonly TestTextureTool textureTool;
        private readonly string inputFile;

        public WithFileAttribute(TestTextureFormat textureFormat, TestTextureType textureType, TestTextureTool textureTool, string inputFile)
        {
            this.textureFormat = textureFormat;
            this.textureType = textureType;
            this.textureTool = textureTool;
            this.inputFile = inputFile;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            ArgumentNullException.ThrowIfNull(testMethod);

            string outputSubfolderName = testMethod.DeclaringType?.GetCustomAttribute<GroupOutputAttribute>()?.Subfolder ?? string.Empty;
            string testGroupName = testMethod.DeclaringType?.Name ?? string.Empty;

            string[] featureLevels = this.textureTool == TestTextureTool.TexConv ? new[] { "9.1", "9.2", "9.3", "10.0", "10.1", "11.0", "11.1", "12.0", "12.1" } : new[] { string.Empty };

            foreach (string featureLevel in featureLevels)
            {
                string basePath = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, this.textureFormat.ToString());

                if (!string.IsNullOrEmpty(featureLevel))
                {
                    basePath = Path.Combine(basePath, featureLevel);
                }

                if (!Directory.Exists(basePath))
                {
                    continue;
                }

                // First try direct path construction (handles subdirectory paths like "Flat/Astc/file.ktx2").
                string file = Path.Combine(basePath, this.inputFile);
                if (File.Exists(file))
                {
                    TestTextureProvider testTextureProvider = new(testMethod.Name, this.textureFormat, this.textureType, this.textureTool, file, false, testGroupName, outputSubfolderName);
                    yield return new object[] { testTextureProvider };
                    continue;
                }

                // Fall back to case-insensitive filename matching to handle
                // cross-platform casing differences (e.g. ".DDS" vs ".dds").
                string match = Directory.GetFiles(basePath)
                    .FirstOrDefault(f => Path.GetFileName(f).Equals(this.inputFile, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                {
                    TestTextureProvider testTextureProvider = new(testMethod.Name, this.textureFormat, this.textureType, this.textureTool, match, false, testGroupName, outputSubfolderName);
                    yield return new object[] { testTextureProvider };
                }
            }
        }
    }
}
