// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Reflection;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using Xunit.Sdk;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;

public class WithFileAttribute : DataAttribute
{
    private readonly TestTextureFormat textureFormat;
    private readonly TestTextureType textureType;
    private readonly TestTextureTool textureTool;
    private readonly string inputFile;
    private readonly bool isRegex;

    public WithFileAttribute(TestTextureFormat textureFormat, TestTextureType textureType, TestTextureTool textureTool, string inputFile, bool isRegex = false)
    {
        this.textureFormat = textureFormat;
        this.textureType = textureType;
        this.textureTool = textureTool;
        this.inputFile = inputFile;
        this.isRegex = isRegex;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        ArgumentNullException.ThrowIfNull(testMethod);

        string[] featureLevels = this.textureTool == TestTextureTool.TexConv ? ["9.1", "9.2", "9.3", "10.0", "10.1", "11.0", "11.1", "12.0", "12.1"] : [string.Empty];

        foreach (string featureLevel in featureLevels)
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, this.textureFormat.ToString());

            if (!string.IsNullOrEmpty(featureLevel))
            {
                path = Path.Combine(path, featureLevel);
            }

            string[] files = Directory.GetFiles(path);
            string[] filteredFiles = [.. files.Where(f => this.isRegex ? new Regex(this.inputFile).IsMatch(Path.GetFileName(f)) : Path.GetFileName(f).Equals(this.inputFile, StringComparison.OrdinalIgnoreCase))];
            foreach (string file in filteredFiles)
            {
                TestTextureProvider testTextureProvider = new(testMethod.Name, this.textureFormat, this.textureType, this.textureTool, file, false);
                yield return new object[] { testTextureProvider };
            }
        }
    }
}
