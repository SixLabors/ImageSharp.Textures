// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.IO;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Performs the ktx decoding operation.
    /// </summary>
    internal sealed class KtxDecoderCore
    {
        /// <summary>
        /// The global configuration.
        /// </summary>
        private readonly Configuration configuration;

        /// <summary>
        /// Used for allocating memory during processing operations.
        /// </summary>
        private readonly MemoryAllocator memoryAllocator;

        /// <summary>
        /// The file header containing general information about the texture.
        /// </summary>
        private KtxHeader ktxHeader;

        /// <summary>
        /// The texture decoder options.
        /// </summary>
        private readonly IKtxDecoderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="KtxDecoderCore"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        public KtxDecoderCore(Configuration configuration, IKtxDecoderOptions options)
        {
            this.configuration = configuration;
            this.memoryAllocator = configuration.MemoryAllocator;
            this.options = options;
        }

        /// <summary>
        /// Decodes the texture from the specified stream.
        /// </summary>
        /// <param name="stream">The stream, where the texture should be decoded from. Cannot be null.</param>
        /// <returns>The decoded image.</returns>
        public Texture DecodeTexture(Stream stream)
        {
            this.ReadFileHeader(stream);

            if (this.ktxHeader.Width == 0)
            {
                throw new UnknownTextureFormatException("Width cannot be 0");
            }

            int width = (int)this.ktxHeader.Width;
            int height = (int)this.ktxHeader.Height;

            // Skip over bytesOfKeyValueData, if any is present.
            stream.Position += this.ktxHeader.BytesOfKeyValueData;

            var ktxProcessor = new KtxProcessor(this.ktxHeader);

            if (this.ktxHeader.NumberOfFaces == 6)
            {
                CubemapTexture cubeMapTexture = ktxProcessor.DecodeCubeMap(stream, width, height);
                return cubeMapTexture;
            }

            var texture = new FlatTexture();
            MipMap[] mipMaps = ktxProcessor.DecodeMipMaps(stream, width, height, this.ktxHeader.NumberOfMipmapLevels);
            texture.MipMaps.AddRange(mipMaps);
            return texture;
        }

        /// <summary>
        /// Reads the raw texture information from the specified stream.
        /// </summary>
        /// <param name="currentStream">The <see cref="Stream"/> containing texture data.</param>
        public ITextureInfo Identify(Stream currentStream)
        {
            this.ReadFileHeader(currentStream);

            var textureInfo = new TextureInfo(new TextureTypeInfo((int)this.ktxHeader.PixelDepth), (int)this.ktxHeader.Width, (int)this.ktxHeader.Height);

            return textureInfo;
        }

        /// <summary>
        /// Reads the dds file header from the stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing texture data.</param>
        private void ReadFileHeader(Stream stream)
        {
            // Discard the magic bytes, we already know at this point its a ktx file.
            stream.Position += KtxConstants.MagicBytes.Length;

            byte[] ktxHeaderBuffer = new byte[KtxConstants.KtxHeaderSize];
            stream.Read(ktxHeaderBuffer, 0, KtxConstants.KtxHeaderSize);

            this.ktxHeader = KtxHeader.Parse(ktxHeaderBuffer);
        }
    }
}
