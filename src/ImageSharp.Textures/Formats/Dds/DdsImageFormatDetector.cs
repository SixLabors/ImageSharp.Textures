// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    /// <summary>
    /// Detects dds file headers.
    /// </summary>
    public sealed class DdsImageFormatDetector : ITextureFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => 8;

        /// <inheritdoc/>
        public ITextureFormat DetectFormat(ReadOnlySpan<byte> header) => this.IsSupportedFileFormat(header) ? DdsFormat.Instance : null;

        private bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
        {
            if (header.Length >= this.HeaderSize)
            {
                uint magicValue = BinaryPrimitives.ReadUInt32LittleEndian(header);
                return magicValue == DdsFourCc.DdsMagicWord;
            }

            return false;
        }
    }
}
