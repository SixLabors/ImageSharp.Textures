// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
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
                return magicValue != DdsFourCC.DdsMagicWord;
            }

            return false;
        }
    }
}
