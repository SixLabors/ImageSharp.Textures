// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Tests;

/// <summary>
/// Class that contains all the relative test image paths in the Images/Input/Formats directory.
/// </summary>
public static class TestImages
{
    public static class Ktx
    {
        public const string Rgba32UnormMipMap = "Flat/rgba32-unorm-mipmap.ktx";

        public static class Astc
        {
            public const string Rgb32_8x8 = "Flat/Astc/rgba32-srgb-8x8.ktx";

            public const string Rgb32_sRgb_4x4 = "Flat/Astc/rgba32-srgb-4x4-valid.ktx";
            public const string Rgb32_sRgb_5x4 = "Flat/Astc/rgba32-srgb-5x4-valid.ktx";
            public const string Rgb32_sRgb_5x5 = "Flat/Astc/rgba32-srgb-5x5-valid.ktx";
            public const string Rgb32_sRgb_6x5 = "Flat/Astc/rgba32-srgb-6x5-valid.ktx";
            public const string Rgb32_sRgb_6x6 = "Flat/Astc/rgba32-srgb-6x6-valid.ktx";
            public const string Rgb32_sRgb_8x5 = "Flat/Astc/rgba32-srgb-8x5-valid.ktx";
            public const string Rgb32_sRgb_8x6 = "Flat/Astc/rgba32-srgb-8x6-valid.ktx";
            public const string Rgb32_sRgb_8x8 = "Flat/Astc/rgba32-srgb-8x8-valid.ktx";
            public const string Rgb32_sRgb_10x5 = "Flat/Astc/rgba32-srgb-10x5-valid.ktx";
            public const string Rgb32_sRgb_10x6 = "Flat/Astc/rgba32-srgb-10x6-valid.ktx";
            public const string Rgb32_sRgb_10x8 = "Flat/Astc/rgba32-srgb-10x8-valid.ktx";
            public const string Rgb32_sRgb_10x10 = "Flat/Astc/rgba32-srgb-10x10-valid.ktx";
            public const string Rgb32_sRgb_12x10 = "Flat/Astc/rgba32-srgb-12x10-valid.ktx";
            public const string Rgb32_sRgb_12x12 = "Flat/Astc/rgba32-srgb-12x12-valid.ktx";

            public const string Rgb32_Unorm_4x4 = "Flat/Astc/rgba32-unorm-4x4-valid.ktx";
            public const string Rgb32_Unorm_5x4 = "Flat/Astc/rgba32-unorm-5x4-valid.ktx";
            public const string Rgb32_Unorm_5x5 = "Flat/Astc/rgba32-unorm-5x5-valid.ktx";
            public const string Rgb32_Unorm_6x5 = "Flat/Astc/rgba32-unorm-6x5-valid.ktx";
            public const string Rgb32_Unorm_6x6 = "Flat/Astc/rgba32-unorm-6x6-valid.ktx";
            public const string Rgb32_Unorm_8x5 = "Flat/Astc/rgba32-unorm-8x5-valid.ktx";
            public const string Rgb32_Unorm_8x6 = "Flat/Astc/rgba32-unorm-8x6-valid.ktx";
            public const string Rgb32_Unorm_8x8 = "Flat/Astc/rgba32-unorm-8x8-valid.ktx";
            public const string Rgb32_Unorm_10x5 = "Flat/Astc/rgba32-unorm-10x5-valid.ktx";
            public const string Rgb32_Unorm_10x6 = "Flat/Astc/rgba32-unorm-10x6-valid.ktx";
            public const string Rgb32_Unorm_10x8 = "Flat/Astc/rgba32-unorm-10x8-valid.ktx";
            public const string Rgb32_Unorm_10x10 = "Flat/Astc/rgba32-unorm-10x10-valid.ktx";
            public const string Rgb32_Unorm_12x10 = "Flat/Astc/rgba32-unorm-12x10-valid.ktx";
            public const string Rgb32_Unorm_12x12 = "Flat/Astc/rgba32-unorm-12x12-valid.ktx";
        }

        public static class Hdr
        {
            public const string R16 = "Flat/Hdr/r16.ktx";
            public const string R32 = "Flat/Hdr/r32.ktx";
            public const string Rg32 = "Flat/Hdr/rg32.ktx";
            public const string Rg64 = "Flat/Hdr/rg64.ktx";
            public const string Rgb48 = "Flat/Hdr/rgb48.ktx";
            public const string Rgb96 = "Flat/Hdr/rgb96.ktx";
            public const string Rgba64 = "Flat/Hdr/rgba64.ktx";
            public const string Rgba128 = "Flat/Hdr/rgba128.ktx";
        }
    }

    public static class Ktx2
    {
        public const string Rgb48UnormMips = "Flat/rgb48-unorm-mips.ktx2";

        public static class Astc
        {
            // Flat textures with various block sizes
            public const string Rgba32_4x4 = "Flat/Astc/rgba32-srgb-4x4.ktx2";
            public const string Rgba32_5x4 = "Flat/Astc/rgba32-srgb-5x4.ktx2";
            public const string Rgba32_5x5 = "Flat/Astc/rgba32-srgb-5x5.ktx2";
            public const string Rgba32_6x5 = "Flat/Astc/rgba32-srgb-6x5.ktx2";
            public const string Rgba32_6x6 = "Flat/Astc/rgba32-srgb-6x6.ktx2";
            public const string Rgba32_8x5 = "Flat/Astc/rgba32-srgb-8x5.ktx2";
            public const string Rgba32_8x6 = "Flat/Astc/rgba32-srgb-8x6.ktx2";
            public const string Rgba32_8x8 = "Flat/Astc/rgba32-srgb-8x8.ktx2";
            public const string Rgba32_10x5 = "Flat/Astc/rgba32-srgb-10x5.ktx2";
            public const string Rgba32_10x6 = "Flat/Astc/rgba32-srgb-10x6.ktx2";
            public const string Rgba32_10x8 = "Flat/Astc/rgba32-srgb-10x8.ktx2";
            public const string Rgba32_10x10 = "Flat/Astc/rgba32-srgb-10x10.ktx2";
            public const string Rgba32_12x10 = "Flat/Astc/rgba32-srgb-12x10.ktx2";
            public const string Rgba32_12x12 = "Flat/Astc/rgba32-srgb-12x12.ktx2";

            public const string Rgb32_sRgb_4x4 = "Flat/Astc/rgba32-srgb-4x4-valid.ktx2";
            public const string Rgb32_sRgb_5x4 = "Flat/Astc/rgba32-srgb-5x4-valid.ktx2";
            public const string Rgb32_sRgb_5x5 = "Flat/Astc/rgba32-srgb-5x5-valid.ktx2";
            public const string Rgb32_sRgb_6x5 = "Flat/Astc/rgba32-srgb-6x5-valid.ktx2";
            public const string Rgb32_sRgb_6x6 = "Flat/Astc/rgba32-srgb-6x6-valid.ktx2";
            public const string Rgb32_sRgb_8x5 = "Flat/Astc/rgba32-srgb-8x5-valid.ktx2";
            public const string Rgb32_sRgb_8x6 = "Flat/Astc/rgba32-srgb-8x6-valid.ktx2";
            public const string Rgb32_sRgb_8x8 = "Flat/Astc/rgba32-srgb-8x8-valid.ktx2";
            public const string Rgb32_sRgb_10x5 = "Flat/Astc/rgba32-srgb-10x5-valid.ktx2";
            public const string Rgb32_sRgb_10x6 = "Flat/Astc/rgba32-srgb-10x6-valid.ktx2";
            public const string Rgb32_sRgb_10x8 = "Flat/Astc/rgba32-srgb-10x8-valid.ktx2";
            public const string Rgb32_sRgb_10x10 = "Flat/Astc/rgba32-srgb-10x10-valid.ktx2";
            public const string Rgb32_sRgb_12x10 = "Flat/Astc/rgba32-srgb-12x10-valid.ktx2";
            public const string Rgb32_sRgb_12x12 = "Flat/Astc/rgba32-srgb-12x12-valid.ktx2";

            public const string Rgb32_Unorm_4x4 = "Flat/Astc/rgba32-unorm-4x4-valid.ktx2";
            public const string Rgb32_Unorm_5x4 = "Flat/Astc/rgba32-unorm-5x4-valid.ktx2";
            public const string Rgb32_Unorm_5x5 = "Flat/Astc/rgba32-unorm-5x5-valid.ktx2";
            public const string Rgb32_Unorm_6x5 = "Flat/Astc/rgba32-unorm-6x5-valid.ktx2";
            public const string Rgb32_Unorm_6x6 = "Flat/Astc/rgba32-unorm-6x6-valid.ktx2";
            public const string Rgb32_Unorm_8x5 = "Flat/Astc/rgba32-unorm-8x5-valid.ktx2";
            public const string Rgb32_Unorm_8x6 = "Flat/Astc/rgba32-unorm-8x6-valid.ktx2";
            public const string Rgb32_Unorm_8x8 = "Flat/Astc/rgba32-unorm-8x8-valid.ktx2";
            public const string Rgb32_Unorm_10x5 = "Flat/Astc/rgba32-unorm-10x5-valid.ktx2";
            public const string Rgb32_Unorm_10x6 = "Flat/Astc/rgba32-unorm-10x6-valid.ktx2";
            public const string Rgb32_Unorm_10x8 = "Flat/Astc/rgba32-unorm-10x8-valid.ktx2";
            public const string Rgb32_Unorm_10x10 = "Flat/Astc/rgba32-unorm-10x10-valid.ktx2";
            public const string Rgb32_Unorm_12x10 = "Flat/Astc/rgba32-unorm-12x10-valid.ktx2";
            public const string Rgb32_Unorm_12x12 = "Flat/Astc/rgba32-unorm-12x12-valid.ktx2";

            public const string Rgb32_Srgb_Large = "Flat/Astc/rgba32-10x5-flighthelmet-basecolor.ktx2";

            // Supercompressed textures (ZLIB)
            public const string Rgb32_Unorm_4x4_Zlib1 = "Flat/Astc/Supercompressed/rgba32-unorm-4x4-zlib-1-valid.ktx2";
            public const string Rgb32_Unorm_4x4_Zlib9 = "Flat/Astc/Supercompressed/rgba32-unorm-4x4-zlib-9-valid.ktx2";

            // Supercompressed textures (ZSTD)
            public const string Rgb32_Unorm_4x4_Zstd1 = "Flat/Astc/Supercompressed/rgba32-unorm-4x4-zstd-1-valid.ktx2";
            public const string Rgb32_Unorm_4x4_Zstd9 = "Flat/Astc/Supercompressed/rgba32-unorm-4x4-zstd-9-valid.ktx2";

            // Flat HDR uncompressed textures
            public const string R16_Unorm = "Flat/Astc/Hdr/r16-unorm.ktx2";
            public const string Rg32_Unorm = "Flat/Astc/Hdr/rg32-unorm.ktx2";
            public const string Rgb48_Unorm = "Flat/Astc/Hdr/rgb48-unorm.ktx2";
            public const string Rgba64_Unorm = "Flat/Astc/Hdr/rgba64-unorm.ktx2";
            public const string R32_Sfloat = "Flat/Astc/Hdr/r32-sfloat.ktx2";
            public const string Rg32_Sfloat = "Flat/Astc/Hdr/rg32-sfloat.ktx2";
            public const string Rg64_Sfloat = "Flat/Astc/Hdr/rg64-sfloat.ktx2";
            public const string Rgb48_Sfloat = "Flat/Astc/Hdr/rgb48-sfloat.ktx2";
            public const string Rgb96_Sfloat = "Flat/Astc/Hdr/rgb96-sfloat.ktx2";
            public const string Rgba64_Sfloat = "Flat/Astc/Hdr/rgba64-sfloat.ktx2";
            public const string Rgba128_Sfloat = "Flat/Astc/Hdr/rgba128-sfloat.ktx2";
            public const string Rgb9e5_Ufloat = "Flat/Astc/Hdr/rgb9e5-ufloat.ktx2";
            public const string B10g11r11_Ufloat = "Flat/Astc/Hdr/b10g11r11-ufloat.ktx2";

            // Flat ASTC HDR (compressed SFLOAT) textures
            public const string Rgba64_Sfloat_4x4 = "Flat/Astc/Hdr/rgba64-sfloat-4x4.ktx2";
            public const string Rgba64_Sfloat_6x6 = "Flat/Astc/Hdr/rgba64-sfloat-6x6.ktx2";
            public const string Rgba64_Sfloat_10x5 = "Flat/Astc/Hdr/rgba64-sfloat-10x5.ktx2";

            // ASTC blocks encoded with HDR endpoint modes but wrapped in a UNORM container.
            // Spec allows this; the decoder should clamp HDR output to UNORM8 range.
            public const string Astc4x4_HdrInUnorm = "Flat/Astc/Hdr/rgba32-4x4-hdr-in-unorm.ktx2";

            public static class Array
            {
                // LDR sRGB ASTC array texture with mipmaps
                public const string Rgb32_Srgb_6x6_MipMap = "Array/Astc/rgba32-6x6-mipmap.ktx2";
            }

            public static class Cubemap
            {
                // LDR sRGB ASTC cubemap
                public const string Rgb32_Srgb_6x6 = "Cubemap/Astc/rgba32-6x6.ktx2";

                // HDR uncompressed cubemaps
                public const string R32_Sfloat = "Cubemap/Hdr/r32-sfloat.ktx2";
                public const string Rg32_Sfloat = "Cubemap/Hdr/rg32-sfloat.ktx2";
                public const string Rgb48_Sfloat = "Cubemap/Hdr/rgb48-sfloat.ktx2";
                public const string Rgba64_Sfloat = "Cubemap/Hdr/rgba64-sfloat.ktx2";

                // ASTC HDR (compressed SFLOAT) cubemaps
                public const string Rgba64_Sfloat_4x4 = "Cubemap/Hdr/rgba64-sfloat-4x4.ktx2";
                public const string Rgba64_Sfloat_6x6 = "Cubemap/Hdr/rgba64-sfloat-6x6.ktx2";
                public const string Rgba64_Sfloat_10x5 = "Cubemap/Hdr/rgba64-sfloat-10x5.ktx2";
            }
        }

    }
}
