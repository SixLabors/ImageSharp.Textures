// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Tests;

/// <summary>
/// Class that contains all the relative test image paths in the Images/Input/Formats directory.
/// </summary>
public static class TestImages
{
    public static class Astc
    {
        public const string Rgb_4x4 = "rgb_4x4.astc";
        public const string Rgb_5x4 = "rgb_5x4.astc";
        public const string Rgb_6x6 = "rgb_6x6.astc";
        public const string Rgb_8x8 = "rgb_8x8.astc";
        public const string Rgb_12x12 = "rgb_12x12.astc";

        public const string Rgba_4x4 = "rgba_4x4.astc";
        public const string Rgba_5x5 = "rgba_5x5.astc";
        public const string Rgba_6x6 = "rgba_6x6.astc";
        public const string Rgba_8x8 = "rgba_8x8.astc";

        public const string Checkerboard = "checkerboard.astc";

        public const string Checkered_4 = "checkered_4.astc";
        public const string Checkered_5 = "checkered_5.astc";
        public const string Checkered_6 = "checkered_6.astc";
        public const string Checkered_7 = "checkered_7.astc";
        public const string Checkered_8 = "checkered_8.astc";
        public const string Checkered_9 = "checkered_9.astc";
        public const string Checkered_10 = "checkered_10.astc";
        public const string Checkered_11 = "checkered_11.astc";
        public const string Checkered_12 = "checkered_12.astc";

        public const string Footprint_4x4 = "footprint_4x4.astc";
        public const string Footprint_5x4 = "footprint_5x4.astc";
        public const string Footprint_5x5 = "footprint_5x5.astc";
        public const string Footprint_6x5 = "footprint_6x5.astc";
        public const string Footprint_6x6 = "footprint_6x6.astc";
        public const string Footprint_8x5 = "footprint_8x5.astc";
        public const string Footprint_8x6 = "footprint_8x6.astc";
        public const string Footprint_8x8 = "footprint_8x8.astc";
        public const string Footprint_10x5 = "footprint_10x5.astc";
        public const string Footprint_10x6 = "footprint_10x6.astc";
        public const string Footprint_10x8 = "footprint_10x8.astc";
        public const string Footprint_10x10 = "footprint_10x10.astc";
        public const string Footprint_12x10 = "footprint_12x10.astc";
        public const string Footprint_12x12 = "footprint_12x12.astc";

        public static class Hdr
        {
            public const string Hdr_A_1x1 = "HDR/HDR-A-1x1.astc";
            public const string Ldr_A_1x1 = "HDR/LDR-A-1x1.astc";
            public const string Hdr_Tile = "HDR/hdr-tile.astc";
            public const string Ldr_Tile = "HDR/ldr-tile.astc";
        }
    }

    public static class Ktx
    {
        public const string Rgba = "rgba8888.ktx";

        public static class Astc
        {
            public const string Rgb32_8x8 = "astc-rgba32-8x8.ktx";
        }

        public static class Hdr
        {
            public const string R16 = "HDR/hdr-r16.ktx";
            public const string R32 = "HDR/hdr-r32.ktx";
            public const string Rg32 = "HDR/hdr-rg32.ktx";
            public const string Rg64 = "HDR/hdr-rg64.ktx";
            public const string Rgb48 = "HDR/hdr-rgb48.ktx";
            public const string Rgb96 = "HDR/hdr-rgb96.ktx";
            public const string Rgba64 = "HDR/hdr-rgba64.ktx";
            public const string Rgba128 = "HDR/hdr-rgba128.ktx";
        }
    }

    public static class Ktx2
    {
        public static class Astc
        {
            // Flat textures with various block sizes
            public const string Rgba32_4x4 = "Flat/Astc/astc_rgba32_4x4.ktx2";
            public const string Rgba32_5x4 = "Flat/Astc/astc_rgba32_5x4.ktx2";
            public const string Rgba32_5x5 = "Flat/Astc/astc_rgba32_5x5.ktx2";
            public const string Rgba32_6x5 = "Flat/Astc/astc_rgba32_6x5.ktx2";
            public const string Rgba32_6x6 = "Flat/Astc/astc_rgba32_6x6.ktx2";
            public const string Rgba32_8x5 = "Flat/Astc/astc_rgba32_8x5.ktx2";
            public const string Rgba32_8x6 = "Flat/Astc/astc_rgba32_8x6.ktx2";
            public const string Rgba32_8x8 = "Flat/Astc/astc_rgba32_8x8.ktx2";
            public const string Rgba32_10x5 = "Flat/Astc/astc_rgba32_10x5.ktx2";
            public const string Rgba32_10x6 = "Flat/Astc/astc_rgba32_10x6.ktx2";
            public const string Rgba32_10x8 = "Flat/Astc/astc_rgba32_10x8.ktx2";
            public const string Rgba32_10x10 = "Flat/Astc/astc_rgba32_10x10.ktx2";
            public const string Rgba32_12x10 = "Flat/Astc/astc_rgba32_12x10.ktx2";
            public const string Rgba32_12x12 = "Flat/Astc/astc_rgba32_12x12.ktx2";

            public const string Rgb32_sRgb_4x4 = "Flat/Astc/valid_ASTC_4x4_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_5x4 = "Flat/Astc/valid_ASTC_5x4_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_5x5 = "Flat/Astc/valid_ASTC_5x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_6x5 = "Flat/Astc/valid_ASTC_6x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_6x6 = "Flat/Astc/valid_ASTC_6x6_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_8x5 = "Flat/Astc/valid_ASTC_8x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_8x6 = "Flat/Astc/valid_ASTC_8x6_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_8x8 = "Flat/Astc/valid_ASTC_8x8_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x5 = "Flat/Astc/valid_ASTC_10x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x6 = "Flat/Astc/valid_ASTC_10x6_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x8 = "Flat/Astc/valid_ASTC_10x8_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x10 = "Flat/Astc/valid_ASTC_10x10_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_12x10 = "Flat/Astc/valid_ASTC_12x10_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_12x12 = "Flat/Astc/valid_ASTC_12x12_SRGB_BLOCK_2D.ktx2";

            public const string Rgb32_Unorm_4x4 = "Flat/Astc/valid_ASTC_4x4_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_5x4 = "Flat/Astc/valid_ASTC_5x4_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_5x5 = "Flat/Astc/valid_ASTC_5x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_6x5 = "Flat/Astc/valid_ASTC_6x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_6x6 = "Flat/Astc/valid_ASTC_6x6_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_8x5 = "Flat/Astc/valid_ASTC_8x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_8x6 = "Flat/Astc/valid_ASTC_8x6_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_8x8 = "Flat/Astc/valid_ASTC_8x8_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x5 = "Flat/Astc/valid_ASTC_10x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x6 = "Flat/Astc/valid_ASTC_10x6_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x8 = "Flat/Astc/valid_ASTC_10x8_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x10 = "Flat/Astc/valid_ASTC_10x10_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_12x10 = "Flat/Astc/valid_ASTC_12x10_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_12x12 = "Flat/Astc/valid_ASTC_12x12_UNORM_BLOCK_2D.ktx2";

            public const string Rgb32_Srgb_Large = "Flat/Astc/astc_ldr_10x5_FlightHelmet_baseColor.ktx2";

            // Textures with several levels of MipMaps
            public const string Rgb32_Srgb_6x6_MipMap = "Flat/Astc/astc_ldr_6x6_arraytex_7_mipmap.ktx2";

            // Supercompressed textures (ZLIB)
            public const string Rgb32_Unorm_4x4_Zlib1 = "Flat/Astc/Supercompressed/valid_ASTC_4x4_UNORM_BLOCK_2D_ZLIB_1.ktx2";
            public const string Rgb32_Unorm_4x4_Zlib9 = "Flat/Astc/Supercompressed/valid_ASTC_4x4_UNORM_BLOCK_2D_ZLIB_9.ktx2";

            // Supercompressed textures (ZSTD)
            public const string Rgb32_Unorm_4x4_Zstd1 = "Flat/Astc/Supercompressed/valid_ASTC_4x4_UNORM_BLOCK_2D_ZSTD_1.ktx2";
            public const string Rgb32_Unorm_4x4_Zstd9 = "Flat/Astc/Supercompressed/valid_ASTC_4x4_UNORM_BLOCK_2D_ZSTD_9.ktx2";

            // Cubemap textures
            public const string Rgb32_Srgb_6x6_Cube = "Cubemap/Astc/astc_ldr_cubemap_6x6.ktx2";

            public static class Hdr
            {
                public const string R16 = "Flat/Astc/HDR/hdr-r16-unorm.ktx2";
                public const string Rg32 = "Flat/Astc/HDR/hdr-rg32-unorm.ktx2";
                public const string Rgb48 = "Flat/Astc/HDR/hdr-rgb48-unorm.ktx2";
                public const string Rgba64 = "Flat/Astc/HDR/hdr-rgba64-unorm.ktx2";
                public const string R32 = "Flat/Astc/HDR/hdr-r32-sfloat.ktx2";
                public const string Rg64 = "Flat/Astc/HDR/hdr-rg64-sfloat.ktx2";
                public const string Rgb96 = "Flat/Astc/HDR/hdr-rgb96-sfloat.ktx2";
                public const string Rgba128 = "Flat/Astc/HDR/hdr-rgba128-sfloat.ktx2";
                public const string Rgb9e5 = "Flat/Astc/HDR/hdr-rgb9e5-ufloat.ktx2";
                public const string B10g11r11 = "Flat/Astc/HDR/hdr-b10g11r11-ufloat.ktx2";
            }
        }
    }
}
