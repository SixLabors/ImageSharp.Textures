// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    using System;
    using System.Buffers.Binary;

    /// <summary>
    /// Detects png file headers
    /// </summary>
    public sealed class DdsImageFormatDetector : ITextureFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => 8;

        /// <inheritdoc/>
        public ITextureFormat DetectFormat(ReadOnlySpan<byte> header)
        {
            return this.IsSupportedFileFormat(header) ? DdsFormat.Instance : null;
        }

        private bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length >= this.HeaderSize)
            {
                uint magicValue = BinaryPrimitives.ReadUInt32LittleEndian(header);
                return magicValue != DdsFourCc.DdsMagicWord;
            }

            return false;
        }
    }
}
