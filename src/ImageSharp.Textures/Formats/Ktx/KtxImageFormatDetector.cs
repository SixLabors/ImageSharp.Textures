// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Detects dds file headers.
    /// </summary>
    public sealed class KtxImageFormatDetector : ITextureFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => 12;

        /// <inheritdoc/>
        public ITextureFormat DetectFormat(ReadOnlySpan<byte> header) => this.IsSupportedFileFormat(header) ? KtxFormat.Instance : null;

        private bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length >= this.HeaderSize)
            {
                ReadOnlySpan<byte> magicBytes = header.Slice(0, 12);
                return magicBytes.SequenceEqual(KtxConstants.MagicBytes);
            }

            return false;
        }
    }
}
