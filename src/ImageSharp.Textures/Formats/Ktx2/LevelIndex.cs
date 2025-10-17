// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct LevelIndex
    {
        public readonly ulong ByteOffset;

        public readonly ulong ByteLength;

        public readonly ulong UncompressedByteLength;
    }
}
