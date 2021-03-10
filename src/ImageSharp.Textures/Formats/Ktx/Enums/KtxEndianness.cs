// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    internal enum KtxEndianness : uint
    {
        /// <summary>
        /// Texture data is little endian.
        /// </summary>
        LittleEndian = 0x04030201,

        /// <summary>
        /// Texture data is big endian.
        /// </summary>
        BigEndian = 0x01020304,
    }
}
