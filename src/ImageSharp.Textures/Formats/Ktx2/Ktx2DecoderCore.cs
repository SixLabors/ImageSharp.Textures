// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using SixLabors.ImageSharp.Memory;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Performs the ktx decoding operation.
    /// </summary>
    internal sealed class Ktx2DecoderCore
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
