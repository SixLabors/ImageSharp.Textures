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
        public const string Atlas_Small_4x4 = "Astc/atlas_small_4x4.astc";
        public const string Atlas_Small_5x5 = "Astc/atlas_small_5x5.astc";
        public const string Atlas_Small_6x6 = "Astc/atlas_small_6x6.astc";
        public const string Atlas_Small_8x8 = "Astc/atlas_small_8x8.astc";

        public const string Checkerboard = "Astc/checkerboard.astc";

        public const string Checkered_4 = "Astc/checkered_4.astc";
        public const string Checkered_5 = "Astc/checkered_5.astc";
        public const string Checkered_6 = "Astc/checkered_6.astc";
        public const string Checkered_7 = "Astc/checkered_7.astc";
        public const string Checkered_8 = "Astc/checkered_8.astc";
        public const string Checkered_9 = "Astc/checkered_9.astc";
        public const string Checkered_10 = "Astc/checkered_10.astc";
        public const string Checkered_11 = "Astc/checkered_11.astc";
        public const string Checkered_12 = "Astc/checkered_12.astc";

        public const string Footprint_4x4 = "Astc/footprint_4x4.astc";
        public const string Footprint_5x4 = "Astc/footprint_5x4.astc";
        public const string Footprint_5x5 = "Astc/footprint_5x5.astc";
        public const string Footprint_6x5 = "Astc/footprint_6x5.astc";
        public const string Footprint_6x6 = "Astc/footprint_6x6.astc";
        public const string Footprint_8x5 = "Astc/footprint_8x5.astc";
        public const string Footprint_8x6 = "Astc/footprint_8x6.astc";
        public const string Footprint_8x8 = "Astc/footprint_8x8.astc";
        public const string Footprint_10x5 = "Astc/footprint_10x5.astc";
        public const string Footprint_10x6 = "Astc/footprint_10x6.astc";
        public const string Footprint_10x8 = "Astc/footprint_10x8.astc";
        public const string Footprint_10x10 = "Astc/footprint_10x10.astc";
        public const string Footprint_12x10 = "Astc/footprint_12x10.astc";
        public const string Footprint_12x12 = "Astc/footprint_12x12.astc";

        public const string Rgb_4x4 = "Astc/rgb_4x4.astc";
        public const string Rgb_5x4 = "Astc/rgb_5x4.astc";
        public const string Rgb_6x6 = "Astc/rgb_6x6.astc";
        public const string Rgb_8x8 = "Astc/rgb_8x8.astc";
        public const string Rgb_12x12 = "Astc/rgb_12x12.astc";

        public static class Expected
        {
            public const string Atlas_Small_4x4 = "Astc/Expected/atlas_small_4x4.bmp";
            public const string Atlas_Small_5x5 = "Astc/Expected/atlas_small_5x5.bmp";
            public const string Atlas_Small_6x6 = "Astc/Expected/atlas_small_6x6.bmp";
            public const string Atlas_Small_8x8 = "Astc/Expected/atlas_small_8x8.bmp";

            public const string Footprint_4x4 = "Astc/Expected/footprint_4x4.bmp";
            public const string Footprint_5x4 = "Astc/Expected/footprint_5x4.bmp";
            public const string Footprint_5x5 = "Astc/Expected/footprint_5x5.bmp";
            public const string Footprint_6x5 = "Astc/Expected/footprint_6x5.bmp";
            public const string Footprint_6x6 = "Astc/Expected/footprint_6x6.bmp";
            public const string Footprint_8x5 = "Astc/Expected/footprint_8x5.bmp";
            public const string Footprint_8x6 = "Astc/Expected/footprint_8x6.bmp";
            public const string Footprint_8x8 = "Astc/Expected/footprint_8x8.bmp";
            public const string Footprint_10x5 = "Astc/Expected/footprint_10x5.bmp";
            public const string Footprint_10x6 = "Astc/Expected/footprint_10x6.bmp";
            public const string Footprint_10x8 = "Astc/Expected/footprint_10x8.bmp";
            public const string Footprint_10x10 = "Astc/Expected/footprint_10x10.bmp";
            public const string Footprint_12x10 = "Astc/Expected/footprint_12x10.bmp";
            public const string Footprint_12x12 = "Astc/Expected/footprint_12x12.bmp";

            public const string Rgb_4x4 = "Astc/Expected/rgb_4x4.bmp";
            public const string Rgb_5x4 = "Astc/Expected/rgb_5x4.bmp";
            public const string Rgb_6x6 = "Astc/Expected/rgb_6x6.bmp";
            public const string Rgb_8x8 = "Astc/Expected/rgb_8x8.bmp";
            public const string Rgb_12x12 = "Astc/Expected/rgb_12x12.bmp";
        }

        public static class Hdr
        {
            public const string Hdr_A_1x1 = "Astc/HDR/HDR-A-1x1.astc";
            public const string Ldr_A_1x1 = "Astc/HDR/LDR-A-1x1.astc";
            public const string Hdr_Tile = "Astc/HDR/hdr-tile.astc";
            public const string Ldr_Tile = "Astc/HDR/ldr-tile.astc";
        }
    }

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
        }
    }
}
