// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx.Enums
{
    internal enum GlType : uint
    {
        /// <summary>
        /// Zero indicates, that the texture is compressed.
        /// </summary>
        Compressed = 0,

        Byte = 0x1400,

        UnsignedByte = 0x1401,

        Short = 0x1402,

        UnsignedShort = 0x1403,

        Int = 0x1404,

        UnsignedInt = 0x1405,

        Int64 = 0x140E,

        UnsignedInt64 = 0x140F,

        HalfFloat = 0x140B,

        HalfFloatOes = 0x8D61,

        Float = 0x1406,

        Double = 0x140A,

        UsignedByte332 = 0x8032,

        UnsignedByte233 = 0x8362,

        UnsignedShort565 = 0x8363,

        UnsignedShort4444 = 0x8033,

        UnsignedShort5551 = 0x8034,

        UnsignedInt8888 = 0x8035,

        UnsignedInt1010102 = 0x8036,
    }
}
