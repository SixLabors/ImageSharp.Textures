// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures;

/// <content>
/// Adds static methods allowing the creation of new image from a byte array.
/// </content>
public abstract partial class Texture
{
    /// <summary>
    /// By reading the header on the provided byte array this calculates the images format.
    /// </summary>
    /// <param name="data">The byte array containing encoded texture data to read the header from.</param>
    /// <returns>The format.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureFormat DetectFormat(byte[] data) => DetectFormat(Configuration.Default, data);

    /// <summary>
    /// By reading the header on the provided byte array this calculates the images format.
    /// </summary>
    /// <param name="data">The byte array containing encoded texture data to read the header from.</param>
    /// <returns>The format.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static ITextureFormat DetectFormat(ReadOnlySpan<byte> data) => DetectFormat(Configuration.Default, data);

    /// <summary>
    /// By reading the header on the provided byte array this calculates the images format.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="buffer">The byte array containing encoded texture data to read the header from.</param>
    /// <returns>The mime type.</returns>
    /// <exception cref="ArgumentNullException">The options are null.</exception>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static unsafe ITextureFormat DetectFormat(Configuration config, ReadOnlySpan<byte> buffer)
    {
        Guard.NotNull(config, nameof(config));

        fixed (byte* ptr = buffer)
        {
            using UnmanagedMemoryStream stream = new(ptr, buffer.Length);
            return DetectFormat(config, stream);
        }
    }

    /// <summary>
    /// Reads the raw texture information from the specified stream without fully decoding it.
    /// </summary>
    /// <param name="data">The byte array containing encoded texture data to read the header from.</param>
    /// <returns>
    /// The <see cref="ITextureInfo"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The data is null.</exception>
    /// <exception cref="NotSupportedException">The data is not readable.</exception>
    /// <exception cref="UnknownTextureFormatException">Texture cannot be loaded.</exception>
    public static ITextureInfo Identify(byte[] data) => Identify(data, out ITextureFormat? _);

    /// <summary>
    /// Reads the raw image information from the specified stream without fully decoding it.
    /// </summary>
    /// <param name="data">The byte array containing encoded texture data to read the header from.</param>
    /// <param name="format">The format type of the decoded texture.</param>
    /// <returns>
    /// The <see cref="ITextureInfo"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The data is null.</exception>
    /// <exception cref="NotSupportedException">The data is not readable.</exception>
    /// <exception cref="UnknownTextureFormatException">Texture cannot be loaded.</exception>
    public static ITextureInfo Identify(byte[] data, out ITextureFormat? format) => Identify(Configuration.Default, data, out format);

    /// <summary>
    /// Reads the raw texture information from the specified stream without fully decoding it.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="data">The byte array containing encoded texture data to read the header from.</param>
    /// <param name="format">The format type of the decoded texture.</param>
    /// <returns>
    /// The <see cref="ITextureInfo"/> or null if suitable info detector is not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">The data or configuration is null.</exception>
    /// <exception cref="NotSupportedException">The data is not readable.</exception>
    /// <exception cref="UnknownTextureFormatException">Texture cannot be loaded.</exception>
    public static ITextureInfo Identify(Configuration configuration, byte[] data, out ITextureFormat? format)
    {
        Guard.NotNull(data, nameof(data));

        using MemoryStream stream = new(data, 0, data.Length, false, true);
        return Identify(configuration, stream, out format);
    }

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte array.
    /// </summary>
    /// <param name="data">The byte array containing texture data.</param>
    /// <param name="format">The detected format.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(byte[] data, out ITextureFormat? format) =>
        Load(Configuration.Default, data, out format);

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte array.
    /// </summary>
    /// <param name="data">The byte array containing encoded texture data.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(byte[] data, ITextureDecoder decoder) => Load(Configuration.Default, data, decoder);

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte array.
    /// </summary>
    /// <param name="config">The config for the decoder.</param>
    /// <param name="data">The byte array containing encoded texture data.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, byte[] data) => Load(config, data, out _);

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte array.
    /// </summary>
    /// <param name="config">The config for the decoder.</param>
    /// <param name="data">The byte array containing texture data.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, byte[] data, ITextureDecoder decoder)
    {
        using MemoryStream stream = new(data);
        return Load(config, stream, decoder);
    }

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte array.
    /// </summary>
    /// <param name="config">The config for the decoder.</param>
    /// <param name="data">The byte array containing texture data.</param>
    /// <param name="format">The mime type of the decoded texture.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, byte[] data, out ITextureFormat? format)
    {
        using MemoryStream stream = new(data);
        return Load(config, stream, out format);
    }

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte span.
    /// </summary>
    /// <param name="data">The byte span containing texture data.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(ReadOnlySpan<byte> data) => Load(Configuration.Default, data);

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte span.
    /// </summary>
    /// <param name="data">The byte span containing texture data.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(ReadOnlySpan<byte> data, ITextureDecoder decoder) =>
        Load(Configuration.Default, data, decoder);

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte array.
    /// </summary>
    /// <param name="data">The byte span containing texture data.</param>
    /// <param name="format">The detected format.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(ReadOnlySpan<byte> data, out ITextureFormat? format) =>
        Load(Configuration.Default, data, out format);

    /// <summary>
    /// Decodes a new instance of <see cref="Texture"/> from the given encoded byte span.
    /// </summary>
    /// <param name="config">The configuration options.</param>
    /// <param name="data">The byte span containing texture data.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static Texture Load(Configuration config, ReadOnlySpan<byte> data) => Load(config, data, out _);

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte span.
    /// </summary>
    /// <param name="config">The Configuration.</param>
    /// <param name="data">The byte span containing texture data.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static unsafe Texture Load(
        Configuration config,
        ReadOnlySpan<byte> data,
        ITextureDecoder decoder)
    {
        fixed (byte* ptr = &data.GetPinnableReference())
        {
            using UnmanagedMemoryStream stream = new(ptr, data.Length);
            return Load(config, stream, decoder);
        }
    }

    /// <summary>
    /// Load a new instance of <see cref="Texture"/> from the given encoded byte span.
    /// </summary>
    /// <param name="config">The configuration options.</param>
    /// <param name="data">The byte span containing texture data.</param>
    /// <param name="format">The <see cref="ITextureFormat"/> of the decoded texture.</param>>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="NotSupportedException">The image format is not supported.</exception>
    /// <exception cref="UnknownTextureFormatException">The encoded texture format is unknown.</exception>
    public static unsafe Texture Load(
        Configuration config,
        ReadOnlySpan<byte> data,
        out ITextureFormat? format)
    {
        fixed (byte* ptr = &data.GetPinnableReference())
        {
            using UnmanagedMemoryStream stream = new(ptr, data.Length);
            return Load(config, stream, out format);
        }
    }
}
