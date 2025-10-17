// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures
{
    /// <content>
    /// Adds static methods allowing the creation of new image from a given stream.
    /// </content>
    public abstract partial class Texture
    {
        /// <summary>
        /// By reading the header on the provided stream this calculates the images format type.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>The format type or null if none found.</returns>
        public static ITextureFormat? DetectFormat(Stream stream) => DetectFormat(Configuration.Default, stream);

        /// <summary>
        /// By reading the header on the provided stream this calculates the images format type.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>The format type or null if none found.</returns>
        public static ITextureFormat? DetectFormat(Configuration config, Stream stream)
            => WithSeekableStream(config, stream, s => InternalDetectFormat(s, config));

        /// <summary>
        /// By reading the header on the provided stream this reads the raw image information.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector not found.
        /// </returns>
        public static ITextureInfo? Identify(Stream stream) => Identify(stream, out ITextureFormat? _);

        /// <summary>
        /// By reading the header on the provided stream this reads the raw image information.
        /// </summary>
        /// <param name="stream">The image stream to read the header from.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector not found.
        /// </returns>
        public static ITextureInfo? Identify(Stream stream, out ITextureFormat? format) => Identify(Configuration.Default, stream, out format);

        /// <summary>
        /// Reads the raw image information from the specified stream without fully decoding it.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="stream">The image stream to read the information from.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector is not found.
        /// </returns>
        public static ITextureInfo? Identify(Configuration config, Stream stream, out ITextureFormat? format)
        {
            (ITextureInfo? Info, ITextureFormat? Format) data = WithSeekableStream(config, stream, s => InternalIdentity(s, config ?? Configuration.Default));

            format = data.Format;
            return data.Info;
        }

        /// <summary>
        /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownTextureFormatException">Image cannot be loaded.</exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(Stream stream, out ITextureFormat? format) => Load(Configuration.Default, stream, out format);

        /// <summary>
        /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownTextureFormatException">Image cannot be loaded.</exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(Stream stream) => Load(Configuration.Default, stream);

        /// <summary>
        /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownTextureFormatException">Image cannot be loaded.</exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(Stream stream, ITextureDecoder decoder) => Load(Configuration.Default, stream, decoder);

        /// <summary>
        /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="config">The config for the decoder.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownTextureFormatException">Image cannot be loaded.</exception>
        /// <returns>A new <see cref="Texture"/>.</returns>>
        public static Texture Load(Configuration config, Stream stream, ITextureDecoder decoder) =>
            WithSeekableStream(config, stream, s => decoder.DecodeTexture(config, s));

        /// <summary>
        /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
        /// </summary>
        /// <param name="config">The config for the decoder.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownTextureFormatException">Image cannot be loaded.</exception>
        /// <returns>A new <see cref="Texture"/>.</returns>>
        public static Texture Load(Configuration config, Stream stream) => Load(config, stream, out _);

        /// <summary>
        /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
        /// The pixel format is selected by the decoder.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="stream">The stream containing image information.</param>
        /// <param name="format">The format type of the decoded image.</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
        /// <exception cref="UnknownTextureFormatException">Image cannot be loaded.</exception>
        /// <returns>A new <see cref="Texture"/>.</returns>
        public static Texture Load(Configuration config, Stream stream, out ITextureFormat? format)
        {
            config ??= Configuration.Default;
            (Texture? Img, ITextureFormat? Format) data = WithSeekableStream(config, stream, s => DecodeTexture(s, config));

            format = data.Format;

            if (data.Img != null)
            {
                return data.Img;
            }

            var sb = new StringBuilder();
            _ = sb.AppendLine("Image cannot be loaded. Available decoders:");

            foreach (KeyValuePair<ITextureFormat, ITextureDecoder> val in config.ImageFormatsManager.ImageDecoders)
            {
                _ = sb.AppendLine(CultureInfo.InvariantCulture, $" - {val.Key.Name} : {val.Value.GetType().Name}");
            }

            throw new UnknownTextureFormatException(sb.ToString());
        }

        private static T WithSeekableStream<T>(Configuration config, Stream stream, Func<Stream, T> action)
        {
            if (!stream.CanRead)
            {
                throw new NotSupportedException("Cannot read from the stream.");
            }

            if (stream.CanSeek)
            {
                if (config.ReadOrigin == ReadOrigin.Begin)
                {
                    stream.Position = 0;
                }

                return action(stream);
            }

            // We want to be able to load images from things like HttpContext.Request.Body
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            return action(memoryStream);
        }
    }
}
