// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx2.Enums;
using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Decodes ktx textures.
    /// </summary>
    internal class Ktx2Processor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ktx2Processor" /> class.
        /// </summary>
        /// <param name="ktxHeader">The KTX header.</param>
        public Ktx2Processor(Ktx2Header ktxHeader) => this.KtxHeader = ktxHeader;

        /// <summary>
        /// Gets the KTX header.
        /// </summary>
        public Ktx2Header KtxHeader { get; }

        /// <summary>
        /// Decodes the mipmaps of a KTX2 textures.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="levelIndices">The start offsets and byte length of each texture.</param>
        /// <returns>The decoded mipmaps.</returns>
        public MipMap[] DecodeMipMaps(Stream stream, int width, int height, LevelIndex[] levelIndices)
        {
            DebugGuard.MustBeGreaterThan(width, 0, nameof(width));
            DebugGuard.MustBeGreaterThan(height, 0, nameof(height));
            DebugGuard.MustBeGreaterThan(levelIndices.Length, 0, nameof(levelIndices.Length));

            var allMipMapBytes = ReadAllMipMapBytes(stream, levelIndices);
            using var memoryStream = new MemoryStream(allMipMapBytes);

            switch (this.KtxHeader.VkFormat)
            {
                case VkFormat.VK_FORMAT_R8G8B8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8_USCALED:
                case VkFormat.VK_FORMAT_R8G8B8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8B8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8_SRGB:
                    return this.AllocateMipMaps<Rgb24>(memoryStream, width, height, levelIndices);
                case VkFormat.VK_FORMAT_R8G8B8A8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_USCALED:
                case VkFormat.VK_FORMAT_R8G8B8A8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8B8A8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SRGB:
                    return this.AllocateMipMaps<Rgba32>(memoryStream, width, height, levelIndices);
            }

            throw new NotSupportedException("The pixel format is not supported");
        }

        /// <summary>
        /// Allocates and decodes all mipmap levels of a ktx texture.
        /// </summary>
        /// <param name="stream">The stream to read the texture data from.</param>
        /// <param name="width">The width of the texture at level 0.</param>
        /// <param name="height">The height of the texture at level 0.</param>
        /// <param name="levelIndices">The start offsets and byte length of each texture.</param>
        /// <returns>The decoded mipmaps.</returns>
        private MipMap[] AllocateMipMaps<TBlock>(Stream stream, int width, int height, LevelIndex[] levelIndices)
            where TBlock : struct, IBlock<TBlock>
        {
            var blockFormat = default(TBlock);
            var mipMaps = new MipMap<TBlock>[levelIndices.Length];
            for (int i = 0; i < levelIndices.Length; i++)
            {
                var pixelDataSize = levelIndices[i].UncompressedByteLength;
                byte[] mipMapData = new byte[pixelDataSize];
                ReadTextureData(stream, mipMapData);
                mipMaps[i] = new MipMap<TBlock>(blockFormat, mipMapData, width, height);

                width >>= 1;
                height >>= 1;
            }

            return mipMaps;
        }

        /// <summary>
        /// Read all mip maps and store them in a byte array so the level 0 mipmap will be at the beginning
        /// followed by all other mip map levels.
        /// </summary>
        /// <param name="stream">The stream to read the mipmap data from.</param>
        /// <param name="levelIndices">The level indices.</param>
        /// <returns>A byte array containing all the mipmaps.</returns>
        private static byte[] ReadAllMipMapBytes(Stream stream, LevelIndex[] levelIndices)
        {
            ulong totalBytes = 0;
            for (int i = 0; i < levelIndices.Length; i++)
            {
                totalBytes += levelIndices[i].UncompressedByteLength;
            }

            byte[] allMipMapBytes = new byte[totalBytes];
            var idx = 0;
            for (int i = 0; i < levelIndices.Length; i++)
            {
                stream.Position = (long)levelIndices[i].ByteOffset;
                stream.Read(allMipMapBytes, idx, (int)levelIndices[i].UncompressedByteLength);
                idx += (int)levelIndices[i].UncompressedByteLength;
            }

            return allMipMapBytes;
        }

        private static void ReadTextureData(Stream stream, byte[] mipMapData)
        {
            int bytesRead = stream.Read(mipMapData, 0, mipMapData.Length);
            if (bytesRead != mipMapData.Length)
            {
                throw new TextureFormatException("could not read enough texture data from the stream");
            }
        }
    }
}
