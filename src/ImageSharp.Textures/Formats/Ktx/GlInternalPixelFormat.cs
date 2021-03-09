// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    internal enum GlInternalPixelFormat : uint
    {
        RedRgtc1 = 0x8DBB,

        SignedRedRgtc1 = 0x8DBC,

        RedGreenRgtc2 = 0x8DBD,

        SignedRedGreenRgtc2 = 0x8DBE,

        RgbDxt1 = 0x83F0,

        RgbaDxt1 = 0x83F1,

        RgbaDxt3 = 0x83F2,

        RgbaDxt5 = 0x83F3,

        SrgbDxt1 = 0x8C4C,

        SrgbAlphaDxt1 = 0x8C4D,

        SrgbAlphaDxt3 = 0x8C4E,

        SrgbAlphaDxt5 = 0x8C4F
    }
}
