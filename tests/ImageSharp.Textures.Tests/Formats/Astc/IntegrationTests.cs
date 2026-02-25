// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc;
using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class IntegrationTests
{
    [Theory]
    [InlineData("atlas_small_4x4")]
    [InlineData("atlas_small_5x5")]
    [InlineData("atlas_small_6x6")]
    [InlineData("atlas_small_8x8")]
    [InlineData("checkerboard")]
    [InlineData("checkered_4")]
    [InlineData("checkered_5")]
    [InlineData("checkered_6")]
    [InlineData("checkered_7")]
    [InlineData("checkered_8")]
    [InlineData("checkered_9")]
    [InlineData("checkered_10")]
    [InlineData("checkered_11")]
    [InlineData("checkered_12")]
    [InlineData("footprint_4x4")]
    [InlineData("footprint_5x4")]
    [InlineData("footprint_5x5")]
    [InlineData("footprint_6x5")]
    [InlineData("footprint_6x6")]
    [InlineData("footprint_8x5")]
    [InlineData("footprint_8x6")]
    [InlineData("footprint_8x8")]
    [InlineData("footprint_10x5")]
    [InlineData("footprint_10x6")]
    [InlineData("footprint_10x8")]
    [InlineData("footprint_10x10")]
    [InlineData("footprint_12x10")]
    [InlineData("footprint_12x12")]
    [InlineData("rgb_4x4")]
    [InlineData("rgb_5x4")]
    [InlineData("rgb_6x6")]
    [InlineData("rgb_8x8")]
    [InlineData("rgb_12x12")]
    public void DecompressToImage_WithTestdataFile_ShouldDecodeSuccessfully(string basename)
    {
        var filePath = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.InputFolder, basename + ".astc"));
        var bytes = File.ReadAllBytes(filePath);
        var astc = AstcFile.FromMemory(bytes);

        var result = AstcDecoder.DecompressImage(astc);

        result.Length.Should().BeGreaterThan(0, because: $"decoding should succeed for {basename}");
    }
}
