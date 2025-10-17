// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.IO;
using System.Text;
using System.Globalization;
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

        public string InputFile { get; }

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
                string filename = string.Format(CultureInfo.InvariantCulture, "mipmap-{0}", i + 1);
                if (!string.IsNullOrEmpty(name))
                {
                    filename = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", filename, name);
                }

                using Image image = mipMaps[i].GetImage();
                image.Save(Path.Combine(path, string.Format(CultureInfo.InvariantCulture, "{0}.png", filename)));
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
                    this.SaveMipMaps(volumeTexture.Slices[i].MipMaps.ToArray(), string.Format(CultureInfo.InvariantCulture, "slice{0}", i + 1));
                }
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Method Name: {0}", this.MethodName));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Texture Format: {0}", this.TextureFormat));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Texture Type: {0}", this.TextureType));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Texture Tool: {0}", this.TextureTool));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Input File: {0}", this.InputFile));
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Is Regex: {0}", this.IsRegex));
            return stringBuilder.ToString();
        }
    }
}
