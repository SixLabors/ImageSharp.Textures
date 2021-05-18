// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Text;
using SixLabors.ImageSharp.Textures.Formats;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.TextureFormats;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders
{
    public class TestTextureProvider : ITestTextureProvider
    {
        public string MethodName { get; }

        /// <inheritdoc/>
        public ImagingTestCaseUtility Utility { get; private set; }

        /// <inheritdoc/>
        public TestTextureFormat TextureFormat { get; }

        /// <inheritdoc/>
        public TestTextureType TextureType { get; }

        /// <inheritdoc/>
        public TestTextureTool TextureTool { get; }

        public string InputFile { get;  }

        public bool IsRegex { get; }

        public virtual Texture GetTexture(ITextureDecoder decoder)
        {
            using FileStream fileStream = File.OpenRead(this.InputFile);

            Texture result = decoder.DecodeTexture(Configuration.Default, fileStream);

            Assert.True(fileStream.Length == fileStream.Position, "The texture file stream was not read to the end");

            return result;
        }

        public TestTextureProvider(
            string methodName,
            TestTextureFormat textureFormat,
            TestTextureType textureType,
            TestTextureTool textureTool,
            string inputFile,
            bool isRegex)
        {
            this.MethodName = methodName;
            this.TextureFormat = textureFormat;
            this.TextureType = textureType;
            this.TextureTool = textureTool;
            this.InputFile = inputFile;
            this.IsRegex = isRegex;
            this.Utility = new ImagingTestCaseUtility
            {
                SourceFileOrDescription = inputFile,
                TestName = methodName
            };
        }

        private void SaveMipMaps(MipMap[] mipMaps, string name)
        {
            string path = Path.Combine(TestEnvironment.ActualOutputDirectoryFullPath, this.TextureFormat.ToString(), this.TextureType.ToString(), this.TextureTool.ToString(), this.MethodName, Path.GetFileNameWithoutExtension(this.InputFile));

            Directory.CreateDirectory(path);

            for (int i = 0; i < mipMaps.Length; i++)
            {
                string filename = $"mipmap-{i + 1}";
                if (!string.IsNullOrEmpty(name))
                {
                    filename = $"{filename}-{name}";
                }

                using Image image = mipMaps[i].GetImage();
                image.Save(Path.Combine(path, $"{filename}.png"));
            }
        }

        public void SaveTextures(Texture texture)
        {
            if (TestEnvironment.RunsOnCI)
            {
                return;
            }

            if (texture is CubemapTexture cubemapTexture)
            {
                this.SaveMipMaps(cubemapTexture.PositiveX.MipMaps.ToArray(), "positive-x");
                this.SaveMipMaps(cubemapTexture.NegativeX.MipMaps.ToArray(), "negative-x");
                this.SaveMipMaps(cubemapTexture.PositiveY.MipMaps.ToArray(), "positive-y");
                this.SaveMipMaps(cubemapTexture.NegativeY.MipMaps.ToArray(), "negative-y");
                this.SaveMipMaps(cubemapTexture.PositiveZ.MipMaps.ToArray(), "positive-z");
                this.SaveMipMaps(cubemapTexture.NegativeZ.MipMaps.ToArray(), "negative-z");
            }

            if (texture is FlatTexture flatTexture)
            {
                this.SaveMipMaps(flatTexture.MipMaps.ToArray(), null);
            }

            if (texture is VolumeTexture volumeTexture)
            {
                for (int i = 0; i < volumeTexture.Slices.Count; i++)
                {
                    this.SaveMipMaps(volumeTexture.Slices[i].MipMaps.ToArray(), $"slice{i + 1}");
                }
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Method Name: {this.MethodName}");
            stringBuilder.AppendLine($"Texture Format: {this.TextureFormat}");
            stringBuilder.AppendLine($"Texture Type: {this.TextureType}");
            stringBuilder.AppendLine($"Texture Tool: {this.TextureTool}");
            stringBuilder.AppendLine($"Input File: {this.InputFile}");
            stringBuilder.AppendLine($"Is Regex: {this.IsRegex}");
            return stringBuilder.ToString();
        }
    }
}
