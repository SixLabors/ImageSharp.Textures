// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collection of Image Formats to be used in <see cref="Configuration" /> class.
    /// </summary>
    public class TextureFormatManager
    {
        /// <summary>
        /// Used for locking against as there is no ConcurrentSet type.
        /// <see href="https://github.com/dotnet/corefx/issues/6318"/>
        /// </summary>
        private static readonly object HashLock = new object();

        /// <summary>
        /// The list of supported <see cref="ITextureEncoder"/> keyed to mime types.
        /// </summary>
        private readonly ConcurrentDictionary<ITextureFormat, ITextureEncoder> mimeTypeEncoders = new ConcurrentDictionary<ITextureFormat, ITextureEncoder>();

        /// <summary>
        /// The list of supported <see cref="ITextureEncoder"/> keyed to mime types.
        /// </summary>
        private readonly ConcurrentDictionary<ITextureFormat, ITextureDecoder> mimeTypeDecoders = new ConcurrentDictionary<ITextureFormat, ITextureDecoder>();

        /// <summary>
        /// The list of supported <see cref="ITextureFormat"/>s.
        /// </summary>
        private readonly HashSet<ITextureFormat> imageFormats = new HashSet<ITextureFormat>();

        /// <summary>
        /// The list of supported <see cref="ITextureFormatDetector"/>s.
        /// </summary>
        private ConcurrentBag<ITextureFormatDetector> imageFormatDetectors = new ConcurrentBag<ITextureFormatDetector>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureFormatManager" /> class.
        /// </summary>
        public TextureFormatManager()
        {
        }

        /// <summary>
        /// Gets the maximum header size of all the formats.
        /// </summary>
        internal int MaxHeaderSize { get; private set; }

        /// <summary>
        /// Gets the currently registered <see cref="ITextureFormat"/>s.
        /// </summary>
        public IEnumerable<ITextureFormat> ImageFormats => this.imageFormats;

        /// <summary>
        /// Gets the currently registered <see cref="ITextureFormatDetector"/>s.
        /// </summary>
        internal IEnumerable<ITextureFormatDetector> FormatDetectors => this.imageFormatDetectors;

        /// <summary>
        /// Gets the currently registered <see cref="ITextureDecoder"/>s.
        /// </summary>
        internal IEnumerable<KeyValuePair<ITextureFormat, ITextureDecoder>> ImageDecoders => this.mimeTypeDecoders;

        /// <summary>
        /// Gets the currently registered <see cref="ITextureEncoder"/>s.
        /// </summary>
        internal IEnumerable<KeyValuePair<ITextureFormat, ITextureEncoder>> ImageEncoders => this.mimeTypeEncoders;

        /// <summary>
        /// Registers a new format provider.
        /// </summary>
        /// <param name="format">The format to register as a known format.</param>
        public void AddImageFormat(ITextureFormat format)
        {
            Guard.NotNull(format, nameof(format));
            Guard.NotNull(format.MimeTypes, nameof(format.MimeTypes));
            Guard.NotNull(format.FileExtensions, nameof(format.FileExtensions));

            lock (HashLock)
            {
                if (!this.imageFormats.Contains(format))
                {
                    this.imageFormats.Add(format);
                }
            }
        }

        /// <summary>
        /// For the specified file extensions type find the e <see cref="ITextureFormat"/>.
        /// </summary>
        /// <param name="extension">The extension to discover</param>
        /// <returns>The <see cref="ITextureFormat"/> if found otherwise null</returns>
        public ITextureFormat FindFormatByFileExtension(string extension)
        {
            Guard.NotNullOrWhiteSpace(extension, nameof(extension));

            if (extension[0] == '.')
            {
                extension = extension.Substring(1);
            }

            return this.imageFormats.FirstOrDefault(x => x.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// For the specified mime type find the <see cref="ITextureFormat"/>.
        /// </summary>
        /// <param name="mimeType">The mime-type to discover</param>
        /// <returns>The <see cref="ITextureFormat"/> if found; otherwise null</returns>
        public ITextureFormat FindFormatByMimeType(string mimeType)
        {
            return this.imageFormats.FirstOrDefault(x => x.MimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Sets a specific image encoder as the encoder for a specific image format.
        /// </summary>
        /// <param name="imageFormat">The image format to register the encoder for.</param>
        /// <param name="encoder">The encoder to use,</param>
        public void SetEncoder(ITextureFormat imageFormat, ITextureEncoder encoder)
        {
            Guard.NotNull(imageFormat, nameof(imageFormat));
            Guard.NotNull(encoder, nameof(encoder));
            this.AddImageFormat(imageFormat);
            this.mimeTypeEncoders.AddOrUpdate(imageFormat, encoder, (s, e) => encoder);
        }

        /// <summary>
        /// Sets a specific image decoder as the decoder for a specific image format.
        /// </summary>
        /// <param name="imageFormat">The image format to register the encoder for.</param>
        /// <param name="decoder">The decoder to use,</param>
        public void SetDecoder(ITextureFormat imageFormat, ITextureDecoder decoder)
        {
            Guard.NotNull(imageFormat, nameof(imageFormat));
            Guard.NotNull(decoder, nameof(decoder));
            this.AddImageFormat(imageFormat);
            this.mimeTypeDecoders.AddOrUpdate(imageFormat, decoder, (s, e) => decoder);
        }

        /// <summary>
        /// Removes all the registered image format detectors.
        /// </summary>
        public void ClearImageFormatDetectors()
        {
            this.imageFormatDetectors = new ConcurrentBag<ITextureFormatDetector>();
        }

        /// <summary>
        /// Adds a new detector for detecting mime types.
        /// </summary>
        /// <param name="detector">The detector to add</param>
        public void AddImageFormatDetector(ITextureFormatDetector detector)
        {
            Guard.NotNull(detector, nameof(detector));
            this.imageFormatDetectors.Add(detector);
            this.SetMaxHeaderSize();
        }

        /// <summary>
        /// For the specified mime type find the decoder.
        /// </summary>
        /// <param name="format">The format to discover</param>
        /// <returns>The <see cref="ITextureDecoder"/> if found otherwise null</returns>
        public ITextureDecoder FindDecoder(ITextureFormat format)
        {
            Guard.NotNull(format, nameof(format));

            return this.mimeTypeDecoders.TryGetValue(format, out ITextureDecoder decoder)
                ? decoder
                : null;
        }

        /// <summary>
        /// For the specified mime type find the encoder.
        /// </summary>
        /// <param name="format">The format to discover</param>
        /// <returns>The <see cref="ITextureEncoder"/> if found otherwise null</returns>
        public ITextureEncoder FindEncoder(ITextureFormat format)
        {
            Guard.NotNull(format, nameof(format));

            return this.mimeTypeEncoders.TryGetValue(format, out ITextureEncoder encoder)
                ? encoder
                : null;
        }

        /// <summary>
        /// Sets the max header size.
        /// </summary>
        private void SetMaxHeaderSize()
        {
            this.MaxHeaderSize = this.imageFormatDetectors.Max(x => x.HeaderSize);
        }
    }
}
