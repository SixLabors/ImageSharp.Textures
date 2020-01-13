// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures
{
    using System;
    using System.IO;
    using System.Linq;
    using SixLabors.ImageSharp.Textures.Formats;
    using SixLabors.Memory;

    /// <content>
    /// Adds static methods allowing the decoding of new images.
    /// </content>
    public abstract partial class Texture
    {
        /// <summary>
        /// </summary>
        /// <param name="textureType"><see cref="TextureType" /></param>
        /// <returns>The result <see cref="Texture"/></returns>
        //internal static Texture CreateUninitialized(TextureType textureType)
        //{
        //    return new Texture(textureType);
        //}

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

            using (IManagedByteBuffer buffer = config.MemoryAllocator.AllocateManagedByteBuffer(headerSize, AllocationOptions.Clean))
            {
                long startPosition = stream.Position;
                stream.Read(buffer.Array, 0, headerSize);
                stream.Position = startPosition;

                // Does the given stream contain enough data to fit in the header for the format
                // and does that data match the format specification?
                // Individual formats should still check since they are public.
                return config.ImageFormatsManager.FormatDetectors
                    .Where(x => x.HeaderSize <= headerSize)
                    .Select(x => x.DetectFormat(buffer.Memory.Span)).LastOrDefault(x => x != null);
            }
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
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <returns>
        /// A new <see cref="Texture{TPixel}"/>.
        /// </returns>
        private static (Texture texture, ITextureFormat format) DecodeTexture(Stream stream, Configuration config)
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
        /// The <see cref="ITextueInfo"/> or null if suitable info detector not found.
        /// </returns>
        private static (ITextueInfo info, ITextureFormat format) InternalIdentity(Stream stream, Configuration config)
        {
            if (!(DiscoverDecoder(stream, config, out ITextureFormat format) is ITextureInfoDetector detector))
            {
                return (null, null);
            }

            return (detector?.Identify(config, stream), format);
        }
    }
}
