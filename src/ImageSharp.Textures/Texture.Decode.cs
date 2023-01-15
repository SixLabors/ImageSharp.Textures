// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Common.Extensions;
using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures
{
    /// <content>
    /// Adds static methods allowing the decoding of new images.
    /// </content>
    public abstract partial class Texture
    {
        /// <summary>
        /// By reading the header on the provided stream this calculates the images format.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <param name="config">The configuration.</param>
        /// <returns>The mime type or null if none found.</returns>
        private static ITextureFormat InternalDetectFormat(Stream stream, Configuration config)
        {
            // We take a minimum of the stream length vs the max header size and always check below
            // to ensure that only formats that headers fit within the given buffer length are tested.
            int headerSize = (int)Math.Min(config.MaxHeaderSize, stream.Length);
            if (headerSize <= 0)
            {
                return null;
            }

            using System.Buffers.IMemoryOwner<byte> buffer = config.MemoryAllocator.Allocate<byte>(headerSize, AllocationOptions.Clean);
            long startPosition = stream.Position;
            Span<byte> bufferSpan = buffer.Memory.Span;
            stream.Read(bufferSpan, 0, headerSize);
            stream.Position = startPosition;

            // Does the given stream contain enough data to fit in the header for the format
            // and does that data match the format specification?
            // Individual formats should still check since they are public.
            return config.ImageFormatsManager.FormatDetectors
                .Where(x => x.HeaderSize <= headerSize)
                .Select(x => x.DetectFormat(buffer.Memory.Span)).LastOrDefault(x => x != null);
        }

        /// <summary>
        /// By reading the header on the provided stream this calculates the images format.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="format">The IImageFormat.</param>
        /// <returns>The image format or null if none found.</returns>
        private static ITextureDecoder DiscoverDecoder(Stream stream, Configuration config, out ITextureFormat format)
        {
            format = InternalDetectFormat(stream, config);

            return format != null
                ? config.ImageFormatsManager.FindDecoder(format)
                : null;
        }

        /// <summary>
        /// Decodes the image stream to the current image.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="config">the configuration.</param>
        private static (Texture Texture, ITextureFormat Format) DecodeTexture(Stream stream, Configuration config)
        {
            ITextureDecoder decoder = DiscoverDecoder(stream, config, out ITextureFormat format);
            if (decoder is null)
            {
                return (null, null);
            }

            Texture texture = decoder.DecodeTexture(config, stream);
            return (texture, format);
        }

        /// <summary>
        /// Reads the raw image information from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="config">the configuration.</param>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector not found.
        /// </returns>
        private static (ITextureInfo Info, ITextureFormat Format) InternalIdentity(Stream stream, Configuration config)
        {
            if (DiscoverDecoder(stream, config, out ITextureFormat format) is not ITextureInfoDetector detector)
            {
                return (null, null);
            }

            return (detector?.Identify(config, stream), format);
        }
    }
}
