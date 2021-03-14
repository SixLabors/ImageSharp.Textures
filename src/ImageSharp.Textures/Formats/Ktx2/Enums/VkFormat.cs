// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Textures.Formats.Ktx2.Enums
{
    /// <summary>
    /// Vulkan pixel formats.
    /// </summary>
    internal enum VkFormat
    {
        /// <summary>
        /// The format is not specified.
        /// </summary>
        VK_FORMAT_UNDEFINED = 0,

        /// <summary>
        /// Specifies a two-component, 8-bit packed unsigned normalized format that has a 4-bit R component in bits 4..7, and a 4-bit G component in bits 0..3.
        /// </summary>
        VK_FORMAT_R4G4_UNORM_PACK8 = 1,

        /// <summary>
        /// specifies a four-component, 16-bit packed unsigned normalized format that has a 4-bit R component in bits 12..15, a 4-bit G component in bits 8..11,
        /// a 4-bit B component in bits 4..7, and a 4-bit A component in bits 0..3.
        /// </summary>
        VK_FORMAT_R4G4B4A4_UNORM_PACK16 = 2,

        /// <summary>
        /// Specifies a four-component, 16-bit packed unsigned normalized format that has a 4-bit B component in bits 12..15, a 4-bit G component in bits 8..11,
        /// a 4-bit R component in bits 4..7, and a 4-bit A component in bits 0..3.
        /// </summary>
        VK_FORMAT_B4G4R4A4_UNORM_PACK16 = 3,

        /// <summary>
        /// specifies a three-component, 16-bit packed unsigned normalized format that has a 5-bit R component in bits 11..15, a 6-bit G component in bits 5..10,
        /// and a 5-bit B component in bits 0..4.
        /// </summary>
        VK_FORMAT_R5G6B5_UNORM_PACK16 = 4,

        /// <summary>
        /// specifies a three-component, 16-bit packed unsigned normalized format that has a 5-bit B component in bits 11..15, a 6-bit G component in bits 5..10,
        /// and a 5-bit R component in bits 0..4.
        /// </summary>
        VK_FORMAT_B5G6R5_UNORM_PACK16 = 5,

        /// <summary>
        /// Specifies a four-component, 16-bit packed unsigned normalized format that has a 5-bit R component in bits 11..15, a 5-bit G component in bits 6..10,
        /// a 5-bit B component in bits 1..5, and a 1-bit A component in bit 0.
        /// </summary>
        VK_FORMAT_R5G5B5A1_UNORM_PACK16 = 6,

        /// <summary>
        /// specifies a four-component, 16-bit packed unsigned normalized format that has a 5-bit B component in bits 11..15, a 5-bit G component in bits 6..10,
        /// a 5-bit R component in bits 1..5, and a 1-bit A component in bit 0.
        /// </summary>
        VK_FORMAT_B5G5R5A1_UNORM_PACK16 = 7,

        /// <summary>
        /// Specifies a four-component, 16-bit packed unsigned normalized format that has a 1-bit A component in bit 15, a 5-bit R component in bits 10..14,
        /// a 5-bit G component in bits 5..9, and a 5-bit B component in bits 0..4.
        /// </summary>
        VK_FORMAT_A1R5G5B5_UNORM_PACK16 = 8,

        /// <summary>
        /// Specifies a one-component, 8-bit unsigned normalized format that has a single 8-bit R component.
        /// </summary>
        VK_FORMAT_R8_UNORM = 9,

        /// <summary>
        /// Specifies a one-component, 8-bit signed normalized format that has a single 8-bit R component.
        /// </summary>
        VK_FORMAT_R8_SNORM = 10,

        /// <summary>
        /// Specifies a one-component, 8-bit unsigned scaled integer format that has a single 8-bit R component.
        /// </summary>
        VK_FORMAT_R8_USCALED = 11,

        /// <summary>
        /// Specifies a one-component, 8-bit signed scaled integer format that has a single 8-bit R component.
        /// </summary>
        VK_FORMAT_R8_SSCALED = 12,

        /// <summary>
        /// Specifies a one-component, 8-bit unsigned integer format that has a single 8-bit R component.
        /// </summary>
        VK_FORMAT_R8_UINT = 13,

        /// <summary>
        /// Specifies a one-component, 8-bit signed integer format that has a single 8-bit R component.
        /// </summary>
        VK_FORMAT_R8_SINT = 14,

        /// <summary>
        /// Specifies a one-component, 8-bit unsigned normalized format that has a single 8-bit R component stored with sRGB nonlinear encoding.
        /// </summary>
        VK_FORMAT_R8_SRGB = 15,

        /// <summary>
        /// Specifies a two-component, 16-bit unsigned normalized format that has an 8-bit R component in byte 0, and an 8-bit G component in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_UNORM = 16,

        /// <summary>
        /// Specifies a two-component, 16-bit signed normalized format that has an 8-bit R component in byte 0, and an 8-bit G component in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_SNORM = 17,

        /// <summary>
        /// Specifies a two-component, 16-bit unsigned scaled integer format that has an 8-bit R component in byte 0, and an 8-bit G component in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_USCALED = 18,

        /// <summary>
        /// Specifies a two-component, 16-bit signed scaled integer format that has an 8-bit R component in byte 0, and an 8-bit G component in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_SSCALED = 19,

        /// <summary>
        /// Specifies a two-component, 16-bit unsigned integer format that has an 8-bit R component in byte 0, and an 8-bit G component in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_UINT = 20,

        /// <summary>
        /// Specifies a two-component, 16-bit signed integer format that has an 8-bit R component in byte 0, and an 8-bit G component in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_SINT = 21,

        /// <summary>
        /// Specifies a two-component, 16-bit unsigned normalized format that has an 8-bit R component stored with sRGB nonlinear encoding in byte 0,
        /// and an 8-bit G component stored with sRGB nonlinear encoding in byte 1.
        /// </summary>
        VK_FORMAT_R8G8_SRGB = 22,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned normalized format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1, and an 8-bit B component in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_UNORM = 23,

        /// <summary>
        /// Specifies a three-component, 24-bit signed normalized format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1, and an 8-bit B component in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_SNORM = 24,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned scaled format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1, and an 8-bit B component in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_USCALED = 25,

        /// <summary>
        /// Specifies a three-component, 24-bit signed scaled format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1, and an 8-bit B component in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_SSCALED = 26,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned integer format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1, and an 8-bit B component in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_UINT = 27,

        /// <summary>
        /// Specifies a three-component, 24-bit signed integer format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1, and an 8-bit B component in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_SINT = 28,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned normalized format that has an 8-bit R component stored with sRGB nonlinear encoding in byte 0,
        /// an 8-bit G component stored with sRGB nonlinear encoding in byte 1, and an 8-bit B component stored with sRGB nonlinear encoding in byte 2.
        /// </summary>
        VK_FORMAT_R8G8B8_SRGB = 29,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned normalized format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, and an 8-bit R component in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_UNORM = 30,

        /// <summary>
        /// Specifies a three-component, 24-bit signed normalized format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, and an 8-bit R component in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_SNORM = 31,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned scaled format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, and an 8-bit R component in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_USCALED = 32,

        /// <summary>
        /// Specifies a three-component, 24-bit signed scaled format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, and an 8-bit R component in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_SSCALED = 33,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned integer format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, and an 8-bit R component in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_UINT = 34,

        /// <summary>
        /// Specifies a three-component, 24-bit signed integer format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, and an 8-bit R component in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_SINT = 35,

        /// <summary>
        /// Specifies a three-component, 24-bit unsigned normalized format that has an 8-bit B component stored with sRGB nonlinear encoding in byte 0,
        /// an 8-bit G component stored with sRGB nonlinear encoding in byte 1, and an 8-bit R component stored with sRGB nonlinear encoding in byte 2.
        /// </summary>
        VK_FORMAT_B8G8R8_SRGB = 36,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned normalized format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1,
        /// an 8-bit B component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_UNORM = 37,

        /// <summary>
        /// Specifies a four-component, 32-bit signed normalized format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1,
        /// an 8-bit B component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_SNORM = 38,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned scaled format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1,
        /// an 8-bit B component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_USCALED = 39,

        /// <summary>
        /// Specifies a four-component, 32-bit signed scaled format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1,
        /// an 8-bit B component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_SSCALED = 40,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned integer format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1,
        /// an 8-bit B component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_UINT = 41,

        /// <summary>
        /// Specifies a four-component, 32-bit signed integer format that has an 8-bit R component in byte 0, an 8-bit G component in byte 1,
        /// an 8-bit B component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_SINT = 42,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned normalized format that has an 8-bit R component stored with sRGB nonlinear encoding in byte 0,
        /// an 8-bit G component stored with sRGB nonlinear encoding in byte 1, an 8-bit B component stored with sRGB nonlinear encoding in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_R8G8B8A8_SRGB = 43,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned normalized format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, an 8-bit R component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_UNORM = 44,

        /// <summary>
        /// Specifies a four-component, 32-bit signed normalized format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, an 8-bit R component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_SNORM = 45,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned scaled format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, an 8-bit R component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_USCALED = 46,

        /// <summary>
        /// Specifies a four-component, 32-bit signed scaled format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, an 8-bit R component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_SSCALED = 47,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned integer format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, an 8-bit R component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_UINT = 48,

        /// <summary>
        /// Specifies a four-component, 32-bit signed integer format that has an 8-bit B component in byte 0, an 8-bit G component in byte 1, an 8-bit R component in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_SINT = 49,

        /// <summary>
        /// Specifies a four-component, 32-bit unsigned normalized format that has an 8-bit B component stored with sRGB nonlinear encoding in byte 0,
        /// an 8-bit G component stored with sRGB nonlinear encoding in byte 1, an 8-bit R component stored with sRGB nonlinear encoding in byte 2, and an 8-bit A component in byte 3.
        /// </summary>
        VK_FORMAT_B8G8R8A8_SRGB = 50,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned normalized format that has an 8-bit A component in bits 24..31, an 8-bit B component in bits 16..23,
        /// an 8-bit G component in bits 8..15, and an 8-bit R component in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_UNORM_PACK32 = 51,

        /// <summary>
        /// specifies a four-component, 32-bit packed signed normalized format that has an 8-bit A component in bits 24..31, an 8-bit B component in bits 16..23,
        /// an 8-bit G component in bits 8..15, and an 8-bit R component in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_SNORM_PACK32 = 52,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned scaled integer format that has an 8-bit A component in bits 24..31, an 8-bit B component in bits 16..23,
        /// an 8-bit G component in bits 8..15, and an 8-bit R component in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_USCALED_PACK32 = 53,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed scaled integer format that has an 8-bit A component in bits 24..31, an 8-bit B component in bits 16..23,
        /// an 8-bit G component in bits 8..15, and an 8-bit R component in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_SSCALED_PACK32 = 54,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned integer format that has an 8-bit A component in bits 24..31, an 8-bit B component in bits 16..23,
        /// an 8-bit G component in bits 8..15, and an 8-bit R component in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_UINT_PACK32 = 55,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed integer format that has an 8-bit A component in bits 24..31, an 8-bit B component in bits 16..23,
        /// an 8-bit G component in bits 8..15, and an 8-bit R component in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_SINT_PACK32 = 56,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned normalized format that has an 8-bit A component in bits 24..31,
        /// an 8-bit B component stored with sRGB nonlinear encoding in bits 16..23, an 8-bit G component stored with sRGB nonlinear encoding in bits 8..15, and an 8-bit R component stored with sRGB nonlinear encoding in bits 0..7.
        /// </summary>
        VK_FORMAT_A8B8G8R8_SRGB_PACK32 = 57,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned normalized format that has a 2-bit A component in bits 30..31, a 10-bit R component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit B component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2R10G10B10_UNORM_PACK32 = 58,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed normalized format that has a 2-bit A component in bits 30..31, a 10-bit R component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit B component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2R10G10B10_SNORM_PACK32 = 59,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned scaled integer format that has a 2-bit A component in bits 30..31, a 10-bit R component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit B component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2R10G10B10_USCALED_PACK32 = 60,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed scaled integer format that has a 2-bit A component in bits 30..31, a 10-bit R component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit B component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2R10G10B10_SSCALED_PACK32 = 61,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned integer format that has a 2-bit A component in bits 30..31, a 10-bit R component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit B component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2R10G10B10_UINT_PACK32 = 62,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed integer format that has a 2-bit A component in bits 30..31, a 10-bit R component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit B component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2R10G10B10_SINT_PACK32 = 63,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned normalized format that has a 2-bit A component in bits 30..31, a 10-bit B component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit R component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2B10G10R10_UNORM_PACK32 = 64,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed normalized format that has a 2-bit A component in bits 30..31, a 10-bit B component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit R component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2B10G10R10_SNORM_PACK32 = 65,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned scaled integer format that has a 2-bit A component in bits 30..31, a 10-bit B component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit R component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2B10G10R10_USCALED_PACK32 = 66,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed scaled integer format that has a 2-bit A component in bits 30..31, a 10-bit B component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit R component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2B10G10R10_SSCALED_PACK32 = 67,

        /// <summary>
        /// Specifies a four-component, 32-bit packed unsigned integer format that has a 2-bit A component in bits 30..31, a 10-bit B component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit R component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2B10G10R10_UINT_PACK32 = 68,

        /// <summary>
        /// Specifies a four-component, 32-bit packed signed integer format that has a 2-bit A component in bits 30..31, a 10-bit B component in bits 20..29,
        /// a 10-bit G component in bits 10..19, and a 10-bit R component in bits 0..9.
        /// </summary>
        VK_FORMAT_A2B10G10R10_SINT_PACK32 = 69,

        /// <summary>
        /// Specifies a one-component, 16-bit unsigned normalized format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_UNORM = 70,

        /// <summary>
        /// Specifies a one-component, 16-bit signed normalized format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_SNORM = 71,

        /// <summary>
        /// Specifies a one-component, 16-bit unsigned scaled integer format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_USCALED = 72,

        /// <summary>
        /// Specifies a one-component, 16-bit signed scaled integer format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_SSCALED = 73,

        /// <summary>
        /// Specifies a one-component, 16-bit unsigned integer format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_UINT = 74,

        /// <summary>
        /// Specifies a one-component, 16-bit signed integer format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_SINT = 75,

        /// <summary>
        /// Specifies a one-component, 16-bit signed floating-point format that has a single 16-bit R component.
        /// </summary>
        VK_FORMAT_R16_SFLOAT = 76,

        /// <summary>
        /// Specifies a two-component, 32-bit unsigned normalized format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_UNORM = 77,

        /// <summary>
        /// Specifies a two-component, 32-bit signed normalized format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_SNORM = 78,

        /// <summary>
        /// Specifies a two-component, 32-bit unsigned scaled integer format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_USCALED = 79,

        /// <summary>
        /// Specifies a two-component, 32-bit signed scaled integer format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_SSCALED = 80,

        /// <summary>
        /// Specifies a two-component, 32-bit unsigned integer format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_UINT = 81,

        /// <summary>
        /// Specifies a two-component, 32-bit signed integer format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_SINT = 82,

        /// <summary>
        /// Specifies a two-component, 32-bit signed floating-point format that has a 16-bit R component in bytes 0..1, and a 16-bit G component in bytes 2..3.
        /// </summary>
        VK_FORMAT_R16G16_SFLOAT = 83,

        /// <summary>
        /// Specifies a three-component, 48-bit unsigned normalized format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_UNORM = 84,

        /// <summary>
        /// Specifies a three-component, 48-bit signed normalized format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_SNORM = 85,

        /// <summary>
        /// Specifies a three-component, 48-bit unsigned scaled integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_USCALED = 86,

        /// <summary>
        /// Specifies a three-component, 48-bit signed scaled integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_SSCALED = 87,

        /// <summary>
        /// Specifies a three-component, 48-bit unsigned integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_UINT = 88,

        /// <summary>
        /// Specifies a three-component, 48-bit signed integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_SINT = 89,

        /// <summary>
        /// Specifies a three-component, 48-bit signed floating-point format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// and a 16-bit B component in bytes 4..5.
        /// </summary>
        VK_FORMAT_R16G16B16_SFLOAT = 90,

        /// <summary>
        /// Specifies a four-component, 64-bit unsigned normalized format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_UNORM = 91,

        /// <summary>
        /// Specifies a four-component, 64-bit signed normalized format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_SNORM = 92,

        /// <summary>
        /// Specifies a four-component, 64-bit unsigned scaled integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_USCALED = 93,

        /// <summary>
        /// Specifies a four-component, 64-bit signed scaled integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_SSCALED = 94,

        /// <summary>
        /// Specifies a four-component, 64-bit unsigned integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_UINT = 95,

        /// <summary>
        /// Specifies a four-component, 64-bit signed integer format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_SINT = 96,

        /// <summary>
        /// Specifies a four-component, 64-bit signed floating-point format that has a 16-bit R component in bytes 0..1, a 16-bit G component in bytes 2..3,
        /// a 16-bit B component in bytes 4..5, and a 16-bit A component in bytes 6..7.
        /// </summary>
        VK_FORMAT_R16G16B16A16_SFLOAT = 97,

        /// <summary>
        /// Specifies a one-component, 32-bit unsigned integer format that has a single 32-bit R component.
        /// </summary>
        VK_FORMAT_R32_UINT = 98,

        /// <summary>
        /// Specifies a one-component, 32-bit signed integer format that has a single 32-bit R component.
        /// </summary>
        VK_FORMAT_R32_SINT = 99,

        /// <summary>
        /// Specifies a one-component, 32-bit signed floating-point format that has a single 32-bit R component.
        /// </summary>
        VK_FORMAT_R32_SFLOAT = 100,

        /// <summary>
        /// Specifies a two-component, 64-bit unsigned integer format that has a 32-bit R component in bytes 0..3, and a 32-bit G component in bytes 4..7.
        /// </summary>
        VK_FORMAT_R32G32_UINT = 101,

        /// <summary>
        /// Specifies a two-component, 64-bit signed integer format that has a 32-bit R component in bytes 0..3, and a 32-bit G component in bytes 4..7.
        /// </summary>
        VK_FORMAT_R32G32_SINT = 102,

        /// <summary>
        /// Specifies a two-component, 64-bit signed floating-point format that has a 32-bit R component in bytes 0..3, and a 32-bit G component in bytes 4..7.
        /// </summary>
        VK_FORMAT_R32G32_SFLOAT = 103,

        /// <summary>
        /// Specifies a three-component, 96-bit unsigned integer format that has a 32-bit R component in bytes 0..3, a 32-bit G component in bytes 4..7, and a 32-bit B component in bytes 8..11.
        /// </summary>
        VK_FORMAT_R32G32B32_UINT = 104,

        /// <summary>
        /// Specifies a three-component, 96-bit signed integer format that has a 32-bit R component in bytes 0..3, a 32-bit G component in bytes 4..7, and a 32-bit B component in bytes 8..11.
        /// </summary>
        VK_FORMAT_R32G32B32_SINT = 105,

        /// <summary>
        /// Specifies a three-component, 96-bit signed floating-point format that has a 32-bit R component in bytes 0..3, a 32-bit G component in bytes 4..7, and a 32-bit B component in bytes 8..11.
        /// </summary>
        VK_FORMAT_R32G32B32_SFLOAT = 106,

        /// <summary>
        /// Specifies a four-component, 128-bit unsigned integer format that has a 32-bit R component in bytes 0..3, a 32-bit G component in bytes 4..7, a 32-bit B component in bytes 8..11, and a 32-bit A component in bytes 12..15.
        /// </summary>
        VK_FORMAT_R32G32B32A32_UINT = 107,

        /// <summary>
        /// Specifies a four-component, 128-bit signed integer format that has a 32-bit R component in bytes 0..3, a 32-bit G component in bytes 4..7, a 32-bit B component in bytes 8..11, and a 32-bit A component in bytes 12..15.
        /// </summary>
        VK_FORMAT_R32G32B32A32_SINT = 108,

        /// <summary>
        /// Specifies a four-component, 128-bit signed floating-point format that has a 32-bit R component in bytes 0..3, a 32-bit G component in bytes 4..7, a 32-bit B component in bytes 8..11, and a 32-bit A component in bytes 12..15.
        /// </summary>
        VK_FORMAT_R32G32B32A32_SFLOAT = 109,

        /// <summary>
        /// Specifies a one-component, 64-bit unsigned integer format that has a single 64-bit R component.
        /// </summary>
        VK_FORMAT_R64_UINT = 110,

        /// <summary>
        /// Specifies a one-component, 64-bit signed integer format that has a single 64-bit R component.
        /// </summary>
        VK_FORMAT_R64_SINT = 111,

        /// <summary>
        /// Specifies a one-component, 64-bit signed floating-point format that has a single 64-bit R component.
        /// </summary>
        VK_FORMAT_R64_SFLOAT = 112,

        /// <summary>
        /// Specifies a two-component, 128-bit unsigned integer format that has a 64-bit R component in bytes 0..7, and a 64-bit G component in bytes 8..15.
        /// </summary>
        VK_FORMAT_R64G64_UINT = 113,

        /// <summary>
        /// Specifies a two-component, 128-bit signed integer format that has a 64-bit R component in bytes 0..7, and a 64-bit G component in bytes 8..15.
        /// </summary>
        VK_FORMAT_R64G64_SINT = 114,

        /// <summary>
        /// Specifies a two-component, 128-bit signed floating-point format that has a 64-bit R component in bytes 0..7, and a 64-bit G component in bytes 8..15.
        /// </summary>
        VK_FORMAT_R64G64_SFLOAT = 115,

        /// <summary>
        /// Specifies a three-component, 192-bit unsigned integer format that has a 64-bit R component in bytes 0..7, a 64-bit G component in bytes 8..15, and a 64-bit B component in bytes 16..23.
        /// </summary>
        VK_FORMAT_R64G64B64_UINT = 116,

        /// <summary>
        /// Specifies a three-component, 192-bit signed integer format that has a 64-bit R component in bytes 0..7, a 64-bit G component in bytes 8..15, and a 64-bit B component in bytes 16..23.
        /// </summary>
        VK_FORMAT_R64G64B64_SINT = 117,

        /// <summary>
        /// specifies a three-component, 192-bit signed floating-point format that has a 64-bit R component in bytes 0..7, a 64-bit G component in bytes 8..15, and a 64-bit B component in bytes 16..23.
        /// </summary>
        VK_FORMAT_R64G64B64_SFLOAT = 118,

        /// <summary>
        /// Specifies a four-component, 256-bit unsigned integer format that has a 64-bit R component in bytes 0..7, a 64-bit G component in bytes 8..15, a 64-bit B component in bytes 16..23, and a 64-bit A component in bytes 24..31.
        /// </summary>
        VK_FORMAT_R64G64B64A64_UINT = 119,

        /// <summary>
        /// Specifies a four-component, 256-bit signed integer format that has a 64-bit R component in bytes 0..7, a 64-bit G component in bytes 8..15, a 64-bit B component in bytes 16..23, and a 64-bit A component in bytes 24..31.
        /// </summary>
        VK_FORMAT_R64G64B64A64_SINT = 120,

        /// <summary>
        /// Specifies a four-component, 256-bit signed floating-point format that has a 64-bit R component in bytes 0..7, a 64-bit G component in bytes 8..15, a 64-bit B component in bytes 16..23, and a 64-bit A component in bytes 24..31.
        /// </summary>
        VK_FORMAT_R64G64B64A64_SFLOAT = 121,

        /// <summary>
        /// Specifies a three-component, 32-bit packed unsigned floating-point format that has a 10-bit B component in bits 22..31, an 11-bit G component in bits 11..21, an 11-bit R component in bits 0..10.
        /// </summary>
        VK_FORMAT_B10G11R11_UFLOAT_PACK32 = 122,

        /// <summary>
        /// Specifies a three-component, 32-bit packed unsigned floating-point format that has a 5-bit shared exponent in bits 27..31, a 9-bit B component mantissa in bits 18..26, a 9-bit G component mantissa in bits 9..17, and a 9-bit R component mantissa in bits 0..8.
        /// </summary>
        VK_FORMAT_E5B9G9R9_UFLOAT_PACK32 = 123,

        /// <summary>
        /// Specifies a one-component, 16-bit unsigned normalized format that has a single 16-bit depth component.
        /// </summary>
        VK_FORMAT_D16_UNORM = 124,

        /// <summary>
        /// Specifies a two-component, 32-bit format that has 24 unsigned normalized bits in the depth component and, optionally:, 8 bits that are unused.
        /// </summary>
        VK_FORMAT_X8_D24_UNORM_PACK32 = 125,

        /// <summary>
        /// Specifies a one-component, 32-bit signed floating-point format that has 32-bits in the depth component.
        /// </summary>
        VK_FORMAT_D32_SFLOAT = 126,

        /// <summary>
        /// Specifies a one-component, 8-bit unsigned integer format that has 8-bits in the stencil component.
        /// </summary>
        VK_FORMAT_S8_UINT = 127,

        /// <summary>
        /// Specifies a two-component, 24-bit format that has 16 unsigned normalized bits in the depth component and 8 unsigned integer bits in the stencil component.
        /// </summary>
        VK_FORMAT_D16_UNORM_S8_UINT = 128,

        /// <summary>
        /// Specifies a two-component, 32-bit packed format that has 8 unsigned integer bits in the stencil component, and 24 unsigned normalized bits in the depth component.
        /// </summary>
        VK_FORMAT_D24_UNORM_S8_UINT = 129,

        /// <summary>
        /// Specifies a two-component format that has 32 signed float bits in the depth component and 8 unsigned integer bits in the stencil component. There are optionally: 24-bits that are unused.
        /// </summary>
        VK_FORMAT_D32_SFLOAT_S8_UINT = 130,

        /// <summary>
        /// Specifies a three-component, block-compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data. This format has no alpha and is considered opaque.
        /// </summary>
        VK_FORMAT_BC1_RGB_UNORM_BLOCK = 131,

        /// <summary>
        /// Specifies a three-component, block-compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data with sRGB nonlinear encoding. This format has no alpha and is considered opaque.
        /// </summary>
        VK_FORMAT_BC1_RGB_SRGB_BLOCK = 132,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data, and provides 1 bit of alpha.
        /// </summary>
        VK_FORMAT_BC1_RGBA_UNORM_BLOCK = 133,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data with sRGB nonlinear encoding, and provides 1 bit of alpha.
        /// </summary>
        VK_FORMAT_BC1_RGBA_SRGB_BLOCK = 134,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with the first 64 bits encoding alpha values followed by 64 bits encoding RGB values.
        /// </summary>
        VK_FORMAT_BC2_UNORM_BLOCK = 135,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with the first 64 bits encoding alpha values followed by 64 bits encoding RGB values with sRGB nonlinear encoding.
        /// </summary>
        VK_FORMAT_BC2_SRGB_BLOCK = 136,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with the first 64 bits encoding alpha values followed by 64 bits encoding RGB values.
        /// </summary>
        VK_FORMAT_BC3_UNORM_BLOCK = 137,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with the first 64 bits encoding alpha values followed by 64 bits encoding RGB values with sRGB nonlinear encoding.
        /// </summary>
        VK_FORMAT_BC3_SRGB_BLOCK = 138,

        /// <summary>
        /// Specifies a one-component, block-compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized red texel data.
        /// </summary>
        VK_FORMAT_BC4_UNORM_BLOCK = 139,

        /// <summary>
        /// Specifies a one-component, block-compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of signed normalized red texel data.
        /// </summary>
        VK_FORMAT_BC4_SNORM_BLOCK = 140,

        /// <summary>
        /// Specifies a two-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RG texel data with the first 64 bits encoding red values followed by 64 bits encoding green values.
        /// </summary>
        VK_FORMAT_BC5_UNORM_BLOCK = 141,

        /// <summary>
        /// Specifies a two-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of signed normalized RG texel data with the first 64 bits encoding red values followed by 64 bits encoding green values.
        /// </summary>
        VK_FORMAT_BC5_SNORM_BLOCK = 142,

        /// <summary>
        /// Specifies a three-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned floating-point RGB texel data.
        /// </summary>
        VK_FORMAT_BC6H_UFLOAT_BLOCK = 143,

        /// <summary>
        /// Specifies a three-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of signed floating-point RGB texel data.
        /// </summary>
        VK_FORMAT_BC6H_SFLOAT_BLOCK = 144,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data
        /// </summary>
        VK_FORMAT_BC7_UNORM_BLOCK = 145,

        /// <summary>
        /// Specifies a four-component, block-compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_BC7_SRGB_BLOCK = 146,

        /// <summary>
        /// Specifies a three-component, ETC2 compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data. This format has no alpha and is considered opaque.
        /// </summary>
        VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK = 147,

        /// <summary>
        /// Specifies a three-component, ETC2 compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data with sRGB nonlinear encoding. This format has no alpha and is considered opaque.
        /// </summary>
        VK_FORMAT_ETC2_R8G8B8_SRGB_BLOCK = 148,

        /// <summary>
        /// Specifies a four-component, ETC2 compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data, and provides 1 bit of alpha.
        /// </summary>
        VK_FORMAT_ETC2_R8G8B8A1_UNORM_BLOCK = 149,

        /// <summary>
        /// Specifies a four-component, ETC2 compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGB texel data with sRGB nonlinear encoding, and provides 1 bit of alpha.
        /// </summary>
        VK_FORMAT_ETC2_R8G8B8A1_SRGB_BLOCK = 150,

        /// <summary>
        /// Specifies a four-component, ETC2 compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with the first 64 bits encoding alpha values followed by 64 bits encoding RGB values.
        /// </summary>
        VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK = 151,

        /// <summary>
        /// Specifies a four-component, ETC2 compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with the first 64 bits encoding alpha values followed by 64 bits encoding RGB values with sRGB nonlinear encoding applied.
        /// </summary>
        VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK = 152,

        /// <summary>
        /// Specifies a one-component, ETC2 compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized red texel data.
        /// </summary>
        VK_FORMAT_EAC_R11_UNORM_BLOCK = 153,

        /// <summary>
        /// Specifies a one-component, ETC2 compressed format where each 64-bit compressed texel block encodes a 4×4 rectangle of signed normalized red texel data.
        /// </summary>
        VK_FORMAT_EAC_R11_SNORM_BLOCK = 154,

        /// <summary>
        /// Specifies a two-component, ETC2 compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RG texel data with the first 64 bits encoding red values followed by 64 bits encoding green values.
        /// </summary>
        VK_FORMAT_EAC_R11G11_UNORM_BLOCK = 155,

        /// <summary>
        /// Specifies a two-component, ETC2 compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of signed normalized RG texel data with the first 64 bits encoding red values followed by 64 bits encoding green values.
        /// </summary>
        VK_FORMAT_EAC_R11G11_SNORM_BLOCK = 156,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_4x4_UNORM_BLOCK = 157,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 4×4 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_4x4_SRGB_BLOCK = 158,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 5×4 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_5x4_UNORM_BLOCK = 159,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 5×4 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_5x4_SRGB_BLOCK = 160,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 5×5 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_5x5_UNORM_BLOCK = 161,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 5×5 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_5x5_SRGB_BLOCK = 162,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 6×5 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_6x5_UNORM_BLOCK = 163,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 6×5 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_6x5_SRGB_BLOCK = 164,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 6×6 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_6x6_UNORM_BLOCK = 165,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 6×6 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_6x6_SRGB_BLOCK = 166,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes an 8×5 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_8x5_UNORM_BLOCK = 167,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes an 8×5 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_8x5_SRGB_BLOCK = 168,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes an 8×6 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_8x6_UNORM_BLOCK = 169,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes an 8×6 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_8x6_SRGB_BLOCK = 170,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes an 8×8 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_8x8_UNORM_BLOCK = 171,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes an 8×8 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_8x8_SRGB_BLOCK = 172,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×5 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_10x5_UNORM_BLOCK = 173,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×5 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_10x5_SRGB_BLOCK = 174,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×6 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_10x6_UNORM_BLOCK = 175,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×6 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_10x6_SRGB_BLOCK = 176,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×8 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_10x8_UNORM_BLOCK = 177,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×8 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_10x8_SRGB_BLOCK = 178,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×10 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_10x10_UNORM_BLOCK = 179,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 10×10 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_10x10_SRGB_BLOCK = 180,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 12×10 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_12x10_UNORM_BLOCK = 181,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 12×10 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_12x10_SRGB_BLOCK = 182,

        /// <summary>
        /// Specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 12×12 rectangle of unsigned normalized RGBA texel data.
        /// </summary>
        VK_FORMAT_ASTC_12x12_UNORM_BLOCK = 183,

        /// <summary>
        /// specifies a four-component, ASTC compressed format where each 128-bit compressed texel block encodes a 12×12 rectangle of unsigned normalized RGBA texel data with sRGB nonlinear encoding applied to the RGB components.
        /// </summary>
        VK_FORMAT_ASTC_12x12_SRGB_BLOCK = 184,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G8B8G8R8_422_UNORM = 1000156000,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_B8G8R8G8_422_UNORM = 1000156001,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G8_B8_R8_3PLANE_420_UNORM = 1000156002,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G8_B8R8_2PLANE_420_UNORM = 1000156003,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G8_B8_R8_3PLANE_422_UNORM = 1000156004,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G8_B8R8_2PLANE_422_UNORM = 1000156005,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G8_B8_R8_3PLANE_444_UNORM = 1000156006,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_R10X6_UNORM_PACK16 = 1000156007,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_R10X6G10X6_UNORM_2PACK16 = 1000156008,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_R10X6G10X6B10X6A10X6_UNORM_4PACK16 = 1000156009,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G10X6B10X6G10X6R10X6_422_UNORM_4PACK16 = 1000156010,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_B10X6G10X6R10X6G10X6_422_UNORM_4PACK16 = 1000156011,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_420_UNORM_3PACK16 = 1000156012,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G10X6_B10X6R10X6_2PLANE_420_UNORM_3PACK16 = 1000156013,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_422_UNORM_3PACK16 = 1000156014,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G10X6_B10X6R10X6_2PLANE_422_UNORM_3PACK16 = 1000156015,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_444_UNORM_3PACK16 = 1000156016,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_R12X4_UNORM_PACK16 = 1000156017,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_R12X4G12X4_UNORM_2PACK16 = 1000156018,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_R12X4G12X4B12X4A12X4_UNORM_4PACK16 = 1000156019,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G12X4B12X4G12X4R12X4_422_UNORM_4PACK16 = 1000156020,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_B12X4G12X4R12X4G12X4_422_UNORM_4PACK16 = 1000156021,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_420_UNORM_3PACK16 = 1000156022,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G12X4_B12X4R12X4_2PLANE_420_UNORM_3PACK16 = 1000156023,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_422_UNORM_3PACK16 = 1000156024,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G12X4_B12X4R12X4_2PLANE_422_UNORM_3PACK16 = 1000156025,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_444_UNORM_3PACK16 = 1000156026,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G16B16G16R16_422_UNORM = 1000156027,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_B16G16R16G16_422_UNORM = 1000156028,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G16_B16_R16_3PLANE_420_UNORM = 1000156029,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G16_B16R16_2PLANE_420_UNORM = 1000156030,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G16_B16_R16_3PLANE_422_UNORM = 1000156031,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G16_B16R16_2PLANE_422_UNORM = 1000156032,

        // Provided by VK_VERSION_1_1
        VK_FORMAT_G16_B16_R16_3PLANE_444_UNORM = 1000156033,
    }
}
