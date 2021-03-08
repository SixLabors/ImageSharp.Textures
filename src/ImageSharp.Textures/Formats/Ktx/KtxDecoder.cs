// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Image decoder for KTX textures.
    /// </summary>
    public sealed class KtxDecoder : ITextureDecoder, IKtxDecoderOptions, ITextureInfoDetector
    {
        /// <inheritdoc/>
        public Texture DecodeTexture(Configuration configuration, Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            return new KtxDecoderCore(configuration, this).DecodeTexture(stream);
        }

        /// <inheritdoc/>
        public ITextureInfo Identify(Configuration configuration, Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            return new KtxDecoderCore(configuration, this).Identify(stream);
        }
    }
}
