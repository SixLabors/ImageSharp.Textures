// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Performs the ktx decoding operation.
    /// </summary>
    internal sealed class Ktx2DecoderCore
    {
        /// <summary>
        /// A scratch buffer to reduce allocations.
        /// </summary>
        private readonly byte[] buffer = new byte[24];

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
        private Ktx2Header ktxHeader;

        /// <summary>
        /// The texture decoder options.
        /// </summary>
        private readonly IKtx2DecoderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ktx2DecoderCore"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        public Ktx2DecoderCore(Configuration configuration, IKtx2DecoderOptions options)
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

            if (this.ktxHeader.PixelWidth == 0)
            {
                throw new UnknownTextureFormatException("Width cannot be 0");
            }

            int width = (int)this.ktxHeader.PixelWidth;
            int height = (int)this.ktxHeader.PixelHeight;

            // Level indices start immediately after the header
            var levelIndices = new LevelIndex[this.ktxHeader.LevelCount];
            for (int i = 0; i < levelIndices.Length; i++)
            {
                stream.Read(this.buffer, 0, 24);
                LevelIndex levelIndex = MemoryMarshal.Cast<byte, LevelIndex>(this.buffer)[0];
                levelIndices[i] = levelIndex;
            }

            if (this.ktxHeader.SupercompressionScheme != 0)
            {
                throw new NotSupportedException("SupercompressionSchemes are not yet supported");
            }

            var ktxProcessor = new Ktx2Processor(this.ktxHeader);

            Texture texture;
            if (this.ktxHeader.FaceCount == 6)
            {
                texture = ktxProcessor.DecodeCubeMap(stream, width, height, levelIndices);
            }
            else
            {
                var flatTexture = new FlatTexture();
                MipMap[] mipMaps = ktxProcessor.DecodeMipMaps(stream, width, height, levelIndices);
                flatTexture.MipMaps.AddRange(mipMaps);
                texture = flatTexture;
            }

            // Seek to the end of the file to ensure the entire stream is consumed.
            // KTX2 files use byte offsets for mipmap data, so the stream position may not
            // be at the end after reading. We need to find the furthest point read.
            if (levelIndices.Length > 0)
            {
                long maxEndPosition = 0;
                for (int i = 0; i < levelIndices.Length; i++)
                {
                    long endPosition = (long)(levelIndices[i].ByteOffset + levelIndices[i].UncompressedByteLength);
                    if (endPosition > maxEndPosition)
                    {
                        maxEndPosition = endPosition;
                    }
                }

                if (stream.Position < maxEndPosition && stream.CanSeek)
                {
                    stream.Position = maxEndPosition;
                }
            }

            return texture;
        }

        /// <summary>
        /// Reads the raw texture information from the specified stream.
        /// </summary>
        /// <param name="currentStream">The <see cref="Stream"/> containing texture data.</param>
        public ITextureInfo Identify(Stream currentStream)
        {
            this.ReadFileHeader(currentStream);

            var textureInfo = new TextureInfo(new TextureTypeInfo((int)this.ktxHeader.PixelDepth), (int)this.ktxHeader.PixelWidth, (int)this.ktxHeader.PixelHeight);

            return textureInfo;
        }

        /// <summary>
        /// Reads the dds file header from the stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing texture data.</param>
        private void ReadFileHeader(Stream stream)
        {
            // Discard the magic bytes, we already know at this point its a ktx2 file.
            stream.Position += Ktx2Constants.MagicBytes.Length;

            byte[] ktxHeaderBuffer = new byte[Ktx2Constants.KtxHeaderSize];
            stream.Read(ktxHeaderBuffer, 0, Ktx2Constants.KtxHeaderSize);

            this.ktxHeader = Ktx2Header.Parse(ktxHeaderBuffer);
        }
    }
}
