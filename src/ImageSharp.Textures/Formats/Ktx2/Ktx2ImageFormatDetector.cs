// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Detects ktx version 2 texture file headers.
    /// </summary>
    public sealed class Ktx2ImageFormatDetector : ITextureFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => 12;

        /// <inheritdoc/>
        public ITextureFormat DetectFormat(ReadOnlySpan<byte> header) => this.IsSupportedFileFormat(header) ? Ktx2Format.Instance : null;

        private bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length >= this.HeaderSize)
            {
                ReadOnlySpan<byte> magicBytes = header.Slice(0, 12);
                return magicBytes.SequenceEqual(Ktx2Constants.MagicBytes);
            }

            return false;
        }
    }
}
