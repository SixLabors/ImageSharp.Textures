// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Textures.Formats;

namespace SixLabors.ImageSharp.Textures
{
    /// <content>
    /// Adds static methods allowing the creation of new image from a given file.
    /// </content>
    public abstract partial class Texture
    {
        /// <summary>
        /// By reading the header on the provided file this calculates the images mime type.
        /// </summary>
        /// <param name="filePath">The image file to open and to read the header from.</param>
        /// <returns>The mime type or null if none found.</returns>
        public static ITextureFormat DetectFormat(string filePath) => DetectFormat(Configuration.Default, filePath);

        /// <summary>
        /// By reading the header on the provided file this calculates the images mime type.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="filePath">The image file to open and to read the header from.</param>
        /// <returns>The mime type or null if none found.</returns>
        public static ITextureFormat DetectFormat(Configuration config, string filePath)
        {
            config ??= Configuration.Default;
            using (Stream file = config.FileSystem.OpenRead(filePath))
            {
                return DetectFormat(config, file);
            }
        }

        /// <summary>
        /// Reads the raw texture information from the specified stream without fully decoding it.
        /// </summary>
        /// <param name="filePath">The texture file to open and to read the header from.</param>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector not found.
        /// </returns>
        public static ITextureInfo Identify(string filePath)
            => Identify(filePath, out ITextureFormat _);

        /// <summary>
        /// Reads the raw texture information from the specified stream without fully decoding it.
        /// </summary>
        /// <param name="filePath">The image file to open and to read the header from.</param>
        /// <param name="format">The format type of the decoded texture.</param>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector not found.
        /// </returns>
        public static ITextureInfo Identify(string filePath, out ITextureFormat format)
            => Identify(Configuration.Default, filePath, out format);

        /// <summary>
        /// Reads the raw texture information from the specified stream without fully decoding it.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="filePath">The image file to open and to read the header from.</param>
        /// <param name="format">The format type of the decoded texture.</param>
        /// <exception cref="ArgumentNullException">The configuration is null.</exception>
        /// <returns>
        /// The <see cref="ITextureInfo"/> or null if suitable info detector is not found.
        /// </returns>
        public static ITextureInfo Identify(Configuration configuration, string filePath, out ITextureFormat format)
        {
            Guard.NotNull(configuration, nameof(configuration));
            using (Stream file = configuration.FileSystem.OpenRead(filePath))
            {
                return Identify(configuration, file, out format);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Texture"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(string path) => Load(Configuration.Default, path);

        /// <summary>
        /// Create a new instance of the <see cref="Texture"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <param name="format">The mime type of the decoded image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>A new <see cref="Texture"/>.</returns>
        public static Texture Load(string path, out ITextureFormat format) => Load(Configuration.Default, path, out format);

        /// <summary>
        /// Create a new instance of the <see cref="Texture"/> class from the given file.
        /// </summary>
        /// <param name="config">The config for the decoder.</param>
        /// <param name="path">The file path to the image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(Configuration config, string path) => Load(config, path, out _);

        /// <summary>
        /// Create a new instance of the <see cref="Texture"/> class from the given file.
        /// </summary>
        /// <param name="config">The Configuration.</param>
        /// <param name="path">The file path to the image.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(Configuration config, string path, ITextureDecoder decoder)
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load(config, stream, decoder);
            }
        }

        /// <summary>
        /// Create a new instance of the <see cref="Texture"/> class from the given file.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <param name="decoder">The decoder.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(string path, ITextureDecoder decoder) => Load(Configuration.Default, path, decoder);

        /// <summary>
        /// Create a new instance of the <see cref="Texture"/> class from the given file.
        /// The pixel type is selected by the decoder.
        /// </summary>
        /// <param name="config">The configuration options.</param>
        /// <param name="path">The file path to the image.</param>
        /// <param name="format">The mime type of the decoded image.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the stream is not readable nor seekable.
        /// </exception>
        /// <returns>The <see cref="Texture"/>.</returns>
        public static Texture Load(Configuration config, string path, out ITextureFormat format)
        {
            using (Stream stream = config.FileSystem.OpenRead(path))
            {
                return Load(config, stream, out format);
            }
        }
    }
}
