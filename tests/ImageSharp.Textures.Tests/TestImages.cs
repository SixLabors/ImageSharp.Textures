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
        public const string Rgba = "rgba8888.ktx";

        public static class Astc
        {
            public const string Rgb32_8x8 = "astc-rgba32-8x8.ktx";
        }
    }

    public static class Ktx2
    {
        public static class Astc
        {
            // Flat textures with various block sizes
            public const string Rgba32_4x4 = "astc_rgba32_4x4.ktx2";
            public const string Rgba32_5x4 = "astc_rgba32_5x4.ktx2";
            public const string Rgba32_5x5 = "astc_rgba32_5x5.ktx2";
            public const string Rgba32_6x5 = "astc_rgba32_6x5.ktx2";
            public const string Rgba32_6x6 = "astc_rgba32_6x6.ktx2";
            public const string Rgba32_8x5 = "astc_rgba32_8x5.ktx2";
            public const string Rgba32_8x6 = "astc_rgba32_8x6.ktx2";
            public const string Rgba32_8x8 = "astc_rgba32_8x8.ktx2";
            public const string Rgba32_10x5 = "astc_rgba32_10x5.ktx2";
            public const string Rgba32_10x6 = "astc_rgba32_10x6.ktx2";
            public const string Rgba32_10x8 = "astc_rgba32_10x8.ktx2";
            public const string Rgba32_10x10 = "astc_rgba32_10x10.ktx2";
            public const string Rgba32_12x10 = "astc_rgba32_12x10.ktx2";
            public const string Rgba32_12x12 = "astc_rgba32_12x12.ktx2";

            public const string Rgb32_sRgb_4x4 = "valid_ASTC_4x4_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_5x4 = "valid_ASTC_5x4_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_5x5 = "valid_ASTC_5x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_6x5 = "valid_ASTC_6x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_6x6 = "valid_ASTC_6x6_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_8x5 = "valid_ASTC_8x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_8x6 = "valid_ASTC_8x6_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_8x8 = "valid_ASTC_8x8_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x5 = "valid_ASTC_10x5_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x6 = "valid_ASTC_10x6_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x8 = "valid_ASTC_10x8_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_10x10 = "valid_ASTC_10x10_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_12x10 = "valid_ASTC_12x10_SRGB_BLOCK_2D.ktx2";
            public const string Rgb32_sRgb_12x12 = "valid_ASTC_12x12_SRGB_BLOCK_2D.ktx2";

            public const string Rgb32_Unorm_4x4 = "valid_ASTC_4x4_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_5x4 = "valid_ASTC_5x4_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_5x5 = "valid_ASTC_5x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_6x5 = "valid_ASTC_6x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_6x6 = "valid_ASTC_6x6_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_8x5 = "valid_ASTC_8x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_8x6 = "valid_ASTC_8x6_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_8x8 = "valid_ASTC_8x8_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x5 = "valid_ASTC_10x5_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x6 = "valid_ASTC_10x6_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x8 = "valid_ASTC_10x8_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_10x10 = "valid_ASTC_10x10_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_12x10 = "valid_ASTC_12x10_UNORM_BLOCK_2D.ktx2";
            public const string Rgb32_Unorm_12x12 = "valid_ASTC_12x12_UNORM_BLOCK_2D.ktx2";

            public const string Rgb32_Srgb_Large = "astc_ldr_10x5_FlightHelmet_baseColor.ktx2";

            // Textures with several levels of MipMaps
            public const string Rgb32_Srgb_6x6_MipMap = "astc_ldr_6x6_arraytex_7_mipmap.ktx2";

            // Supercompressed textures (ZLIB)
            public const string Rgb32_Unorm_4x4_Zlib1 = "valid_ASTC_4x4_UNORM_BLOCK_2D_ZLIB_1.ktx2";
            public const string Rgb32_Unorm_4x4_Zlib9 = "valid_ASTC_4x4_UNORM_BLOCK_2D_ZLIB_9.ktx2";

            // Supercompressed textures (ZSTD)
            public const string Rgb32_Unorm_4x4_Zstd1 = "valid_ASTC_4x4_UNORM_BLOCK_2D_ZSTD_1.ktx2";
            public const string Rgb32_Unorm_4x4_Zstd9 = "valid_ASTC_4x4_UNORM_BLOCK_2D_ZSTD_9.ktx2";

            // Cubemap textures
            public const string Rgb32_Srgb_6x6_Cube = "astc_ldr_cubemap_6x6.ktx2";
        }
    }
}
