// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Tests;

/// <summary>
/// Relative test data paths under the Images/Input/Formats directory
/// for non-image inputs (e.g. compressed block streams).
/// </summary>
public static class TestData
{
    public static class Astc
    {
        public const string Rgb_4x4 = "rgb-4x4.astc";
        public const string Rgb_5x4 = "rgb-5x4.astc";
        public const string Rgb_6x6 = "rgb-6x6.astc";
        public const string Rgb_8x8 = "rgb-8x8.astc";
        public const string Rgb_12x12 = "rgb-12x12.astc";

        public const string Rgba_4x4 = "rgba-4x4.astc";
        public const string Rgba_5x5 = "rgba-5x5.astc";
        public const string Rgba_6x6 = "rgba-6x6.astc";
        public const string Rgba_8x8 = "rgba-8x8.astc";

        public const string Checkerboard = "checkerboard.astc";

        public const string Checkered_4 = "checkered-4.astc";
        public const string Checkered_5 = "checkered-5.astc";
        public const string Checkered_6 = "checkered-6.astc";
        public const string Checkered_7 = "checkered-7.astc";
        public const string Checkered_8 = "checkered-8.astc";
        public const string Checkered_9 = "checkered-9.astc";
        public const string Checkered_10 = "checkered-10.astc";
        public const string Checkered_11 = "checkered-11.astc";
        public const string Checkered_12 = "checkered-12.astc";

        public const string Footprint_4x4 = "footprint-4x4.astc";
        public const string Footprint_5x4 = "footprint-5x4.astc";
        public const string Footprint_5x5 = "footprint-5x5.astc";
        public const string Footprint_6x5 = "footprint-6x5.astc";
        public const string Footprint_6x6 = "footprint-6x6.astc";
        public const string Footprint_8x5 = "footprint-8x5.astc";
        public const string Footprint_8x6 = "footprint-8x6.astc";
        public const string Footprint_8x8 = "footprint-8x8.astc";
        public const string Footprint_10x5 = "footprint-10x5.astc";
        public const string Footprint_10x6 = "footprint-10x6.astc";
        public const string Footprint_10x8 = "footprint-10x8.astc";
        public const string Footprint_10x10 = "footprint-10x10.astc";
        public const string Footprint_12x10 = "footprint-12x10.astc";
        public const string Footprint_12x12 = "footprint-12x12.astc";

        public static class Hdr
        {
            public const string Hdr_A_1x1 = "HdrPipeline/hdr-a-1x1.astc";
            public const string Ldr_A_1x1 = "HdrPipeline/ldr-a-1x1.astc";
            public const string Hdr_Tile = "HdrPipeline/hdr-tile.astc";
            public const string Ldr_Tile = "HdrPipeline/ldr-tile.astc";
            public const string Hdr_Mixed_256_4x4 = "HdrPipeline/mixed-256-4x4.astc";
            public const string Hdr_Mixed_256_8x8 = "HdrPipeline/mixed-256-8x8.astc";
        }
    }
}
