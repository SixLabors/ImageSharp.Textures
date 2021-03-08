// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    /// <summary>
    /// Image decoder for DDS textures.
    /// </summary>
    public sealed class DdsDecoder : ITextureDecoder, IDdsDecoderOptions, ITextureInfoDetector
    {
        /// <inheritdoc/>
        public Texture DecodeTexture(Configuration configuration, Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            return new DdsDecoderCore(configuration, this).DecodeTexture(stream);
        }

        /// <inheritdoc/>
        public ITextureInfo Identify(Configuration configuration, Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            return new DdsDecoderCore(configuration, this).Identify(stream);
        }
    }
}
