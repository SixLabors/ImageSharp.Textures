// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Image decoder for KTX2 textures.
    /// </summary>
    public sealed class Ktx2Decoder : ITextureDecoder, IKtx2DecoderOptions, ITextureInfoDetector
    {
        /// <inheritdoc/>
        public Texture DecodeTexture(Configuration configuration, Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            return new Ktx2DecoderCore(configuration, this).DecodeTexture(stream);
        }

        /// <inheritdoc/>
        public ITextureInfo Identify(Configuration configuration, Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            return new Ktx2DecoderCore(configuration, this).Identify(stream);
        }
    }
}
