// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures;

/// <content>
/// Adds static methods allowing the creation of new image from a given stream.
/// </content>
public abstract partial class Texture
{
    /// <summary>
    /// By reading the header on the provided stream this calculates the images format type.
    /// </summary>
    /// <param name="stream">The image stream to read the header from.</param>
    /// <returns>The format type.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureFormat DetectFormat(Stream stream) => DetectFormat(Configuration.Default, stream);

    /// <summary>
    /// By reading the header on the provided stream this calculates the images format type.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="stream">The image stream to read the header from.</param>
    /// <returns>The format type.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureFormat DetectFormat(Configuration config, Stream stream)
        => WithSeekableStream(config, stream, s => InternalDetectFormat(s, config));

    /// <summary>
    /// By reading the header on the provided stream this reads the raw image information.
    /// </summary>
    /// <param name="stream">The image stream to read the header from.</param>
    /// <returns>
    /// The <see cref="ITextureInfo"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureInfo Identify(Stream stream) => Identify(stream, out ITextureFormat? _);

    /// <summary>
    /// By reading the header on the provided stream this reads the raw image information.
    /// </summary>
    /// <param name="stream">The image stream to read the header from.</param>
    /// <param name="format">The format type of the decoded image.</param>
    /// <returns>
    /// The <see cref="ITextureInfo"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureInfo Identify(Stream stream, out ITextureFormat? format) => Identify(Configuration.Default, stream, out format);

    /// <summary>
    /// Reads the raw image information from the specified stream without fully decoding it.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="stream">The image stream to read the information from.</param>
    /// <param name="format">The format type of the decoded image.</param>
    /// <returns>
    /// The <see cref="ITextureInfo"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureInfo Identify(Configuration config, Stream stream, out ITextureFormat format)
    {
        (ITextureInfo info, format) = WithSeekableStream(config, stream, s => InternalIdentity(s, config ?? Configuration.Default));

        return info;
    }

    /// <summary>
    /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
    /// The pixel format is selected by the decoder.
    /// </summary>
    /// <param name="stream">The stream containing image information.</param>
    /// <param name="format">The format type of the decoded image.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Stream stream, out ITextureFormat? format) => Load(Configuration.Default, stream, out format);

    /// <summary>
    /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
    /// The pixel format is selected by the decoder.
    /// </summary>
    /// <param name="stream">The stream containing image information.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Stream stream) => Load(Configuration.Default, stream);

    /// <summary>
    /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
    /// The pixel format is selected by the decoder.
    /// </summary>
    /// <param name="stream">The stream containing image information.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Stream stream, ITextureDecoder decoder) => Load(Configuration.Default, stream, decoder);

    /// <summary>
    /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
    /// The pixel format is selected by the decoder.
    /// </summary>
    /// <param name="config">The config for the decoder.</param>
    /// <param name="stream">The stream containing image information.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>A new <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, Stream stream, ITextureDecoder decoder) =>
        WithSeekableStream(config, stream, s => decoder.DecodeTexture(config, s));

    /// <summary>
    /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
    /// </summary>
    /// <param name="config">The config for the decoder.</param>
    /// <param name="stream">The stream containing image information.</param>
    /// <returns>A new <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, Stream stream) => Load(config, stream, out _);

    /// <summary>
    /// Decode a new instance of the <see cref="Texture"/> class from the given stream.
    /// The pixel format is selected by the decoder.
    /// </summary>
    /// <param name="config">The configuration options.</param>
    /// <param name="stream">The stream containing image information.</param>
    /// <param name="format">The format type of the decoded image.</param>
    /// <returns>A new <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown if the stream is not readable.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, Stream stream, out ITextureFormat? format)
    {
        config ??= Configuration.Default;
        (Texture texture, ITextureFormat textureFormat) = WithSeekableStream(config, stream, s => DecodeTexture(s, config));

        format = textureFormat;

        if (texture is null)
        {
            TextureFormatManager.ThrowInvalidDecoder(config.ImageFormatsManager);
        }

        return texture;
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
