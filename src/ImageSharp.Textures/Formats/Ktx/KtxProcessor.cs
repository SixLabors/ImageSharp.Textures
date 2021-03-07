// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;
using System.IO;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Decodes ktx textures.
    /// </summary>
    internal class KtxProcessor
    {
        /// <summary>
        /// A scratch buffer to reduce allocations.
        /// </summary>
        private readonly byte[] buffer = new byte[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="KtxProcessor" /> class.
        /// </summary>
        /// <param name="ktxHeader">The KTX header.</param>
        public KtxProcessor(KtxHeader ktxHeader) => this.KtxHeader = ktxHeader;

        /// <summary>
        /// Gets the KTX header.
        /// </summary>
        public KtxHeader KtxHeader { get; }

        /// <summary>
        /// Decodes the mipmaps of a DDS textures.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="count">The mipmap count.</param>
        /// <returns>The decoded mipmaps.</returns>
        public MipMap[] DecodeKtx(Stream stream, int width, int height, uint count)
        {
            switch (this.KtxHeader.GlFormat)
            {
                // TODO: move texture formats which are same for dds and ktx in a common place once its clear which ones those are.
                case GlPixelFormat.Rgb:
                    return this.AllocateMipMaps<Rgb24>(stream, width, height, count);
                case GlPixelFormat.Rgba:
                    return this.AllocateMipMaps<Rgba32>(stream, width, height, count);
                default:
                    throw new NotSupportedException("The pixel format is not supported");
            }
        }

        /// <summary>
        /// Allocates and decodes all mipmap levels of a ktx texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="count">The mipmap count.</param>
        /// <returns>The decoded mipmaps.</returns>
        private MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, uint count)
            where TBlock : struct, IBlock<TBlock>
        {
            var blockFormat = default(TBlock);

            var mipMaps = new MipMap<TBlock>[count];

            for (int i = 0; i < count; i++)
            {
                int bytesRead = stream.Read(this.buffer, 0, 4);
                if (bytesRead != 4)
                {
                    throw new TextureFormatException("could not read texture data length from the stream");
                }

                var pixelDataSize = BinaryPrimitives.ReadUInt32LittleEndian(this.buffer);
                byte[] mipMapData = new byte[pixelDataSize];
                bytesRead = stream.Read(mipMapData, 0, (int)pixelDataSize);
                if (bytesRead != pixelDataSize)
                {
                    throw new TextureFormatException("could not read enough texture data from the stream");
                }

                mipMaps[i] = new MipMap<TBlock>(blockFormat, mipMapData, width, height);

                width >>= 1;
                height >>= 1;
            }

            return mipMaps;
        }
    }
}
