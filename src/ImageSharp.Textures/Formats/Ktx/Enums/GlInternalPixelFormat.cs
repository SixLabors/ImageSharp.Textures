// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    internal enum GlInternalPixelFormat : uint
    {
        Luminance4 = 0x803F,

        Luminance8 = 0x8040,

        Luminance4Alpha4 = 0x8043,

        Luminance6Alpha2 = 0x8044,

        Luminance7Alpha8 = 0x8045,

        Rgb4 = 0x804F,

        Rgb5 = 0x8050,

        Rgb8 = 0x8051,

        Rgb16 = 0x8054,

        Rgba8 = 0x8058,

        Rgb565 = 0x8D62,

        Rgb10 = 0x8052,

        Rgb12 = 0x8053,

        Rgba2 = 0x8055,

        Rgba4 = 0x8056,

        Rgba12 = 0x805A,

        Rgb5A1 = 0x8057,

        Rgb10A2 = 0x8059,

        Rgb9E5 = 0x8C3D,

        Rgba16 = 0x805B,

        R8 = 0x8229,

        R8UnsignedInt = 0x8232,

        Rg8UnsignedInt = 0x8238,

        Rgb8UnsignedInt = 0x8D7D,

        RgbaUnsignedInt = 0x8D7C,

        R32UnsignedInt = 0x8236,

        Rg32UnsignedInt = 0x823C,

        Rgb32UnsignedInt = 0x8D71,

        Rgba32UnsignedInt = 0x8D70,

        R16 = 0x822A,

        Rg8 = 0x822B,

        Rg16 = 0x822C,

        R8SNorm = 0x8F94,

        Rg8SNorm = 0x8F95,

        Rgb8SNorm = 0x8F96,

        RgbaSNorm = 0x8F97,

        Etc1Rgb8Oes = 0x8D64,

        RedRgtc1 = 0x8DBB,

        SignedRedRgtc1 = 0x8DBC,

        RedGreenRgtc2 = 0x8DBD,

        SignedRedGreenRgtc2 = 0x8DBE,

        RgbDxt1 = 0x83F0,

        RgbaDxt1 = 0x83F1,

        RgbaDxt3 = 0x83F2,

        RgbaDxt5 = 0x83F3,

        Sr8 = 0x8FBD,

        Srg8 = 0x8FBE,

        Srgb8 = 0x8C41,

        Srgb8Alpha8 = 0x8C43,

        SrgbDxt1 = 0x8C4C,

        SrgbAlphaDxt1 = 0x8C4D,

        SrgbAlphaDxt3 = 0x8C4E,

        SrgbAlphaDxt5 = 0x8C4F,

        CompressedRed11Eac = 0x9270,

        CompressedRedGreen11Eac = 0x9272,

        CompressedRedSignedRedEac = 0x9271,

        CompressedRedGreenSignedEac = 0x9273,

        CompressedRgb8Etc2 = 0x9274,

        CompressedSrgb8Etc2 = 0x9275,

        CompressedRgb8PunchthroughAlpa1Etc2 = 0x9276,

        CompressedSrgb8PunchthroughAlpa1Etc2 = 0x9277,

        CompressedRgb8Etc2Eac = 0x9278,

        CompressedSrgb8Alpha8Etc2Eac = 0x9279,

        CompressedRgbaAstc4x4Khr = 0x93B0,

        CompressedRgbaAstc5x4Khr = 0x93B1,

        CompressedRgbaAstc5x5Khr = 0x93B2,

        CompressedRgbaAstc6x5Khr = 0x93B3,

        CompressedRgbaAstc6x6Khr = 0x93B4,

        CompressedRgbaAstc8x5Khr = 0x93B5,

        CompressedRgbaAstc8x6Khr = 0x93B6,

        CompressedRgbaAstc8x8Khr = 0x93B7,

        CompressedRgbaAstc10x5Khr = 0x93B8,

        CompressedRgbaAstc10x6Khr = 0x93B9,

        CompressedRgbaAstc10x8Khr = 0x93BA,

        CompressedRgbaAstc10x10Khr = 0x93BB,

        CompressedRgbaAstc12x10Khr = 0x93BC,

        CompressedRgbaAstc12x12Khr = 0x93BD,
    }
}
