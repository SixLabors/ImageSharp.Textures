// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Uastc;

[Trait("Format", "Astc")]
public class UastcDecoderTests
{
    // Fixtures are raw UASTC blocks wrapped in the ARM .astc file header.
    // Collectively the fixtures cover modes 0,1,2,3,4,6,8,9,10,11,15,17 — all structural
    // categories (single/multi-subset, dual-plane) and all CEMs (RGB, RGBA, LA, solid).
    [Theory]
    [InlineData("rgb-m1")]
    [InlineData("solid-m8")]
    [InlineData("la-m15")]
    [InlineData("rgb-m4-6")]
    [InlineData("rgb-m6")]
    [InlineData("rgb-la-m4-15")]
    [InlineData("la-solid-m8-17")]
    [InlineData("rgba-m9-10-11")]
    [InlineData("rgb-solid-m0-3-8")]
    [InlineData("rgba-rgb-solid-m0-1-2-3-4-6-8-11")]
    public void DecompressImage_MatchesExpected(string name)
    {
        (byte[] levelData, int width, int height) = LoadFixture(name);
        byte[] expected = LoadExpected(name);

        byte[] actual = UastcStreamCodec.DecodeUastc(levelData, width, height);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DecompressImage_ReservedMode_EmitsMagenta()
    {
        // Byte 0 == 7-bit reserved mode 19 huff code -> error colour for the whole block.
        byte[] block = new byte[16];
        block[0] = 0x45;

        byte[] decoded = UastcStreamCodec.DecodeUastc(block, 4, 4);

        AssertAllMagenta(decoded);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(4, -1)]
    public void DecompressImage_InvalidDimensions_Throws(int width, int height)
    {
        using MemoryStream source = new(new byte[16]);
        using MemoryStream destination = new();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UastcDecoder.DecompressImage(source, destination, width, height));
    }

    [Fact]
    public void DecompressImage_InsufficientData_Throws()
    {
        // 16x16 needs 16 blocks (256 bytes)
        using MemoryStream source = new(new byte[16]);
        using MemoryStream destination = new();

        Assert.Throws<EndOfStreamException>(() =>
            UastcDecoder.DecompressImage(source, destination, 16, 16));
    }

    private static (byte[] LevelData, int Width, int Height) LoadFixture(string name)
    {
        byte[] bytes = File.ReadAllBytes(TestFile.GetInputFileFullPath(Path.Combine("Uastc", name + ".uastc")));

        // The .uastc fixtures use the ARM .astc file header (magic + footprint + dimensions) wrapping
        // raw UASTC blocks, so AstcFile parses the container even though the payload is UASTC.
        AstcFile file = AstcFile.FromMemory(bytes);

        return (file.Blocks.ToArray(), file.Width, file.Height);
    }

    private static byte[] LoadExpected(string name)
        => File.ReadAllBytes(Path.Combine(TestUtilities.TestEnvironment.ReferenceOutputDirectoryFullPath, "Uastc", name + ".raw"));

    private static void AssertAllMagenta(byte[] buffer)
    {
        for (int i = 0; i < buffer.Length; i += BlockInfo.ChannelsPerPixel)
        {
            Assert.Equal(0xFF, buffer[i]);
            Assert.Equal(0x00, buffer[i + 1]);
            Assert.Equal(0xFF, buffer[i + 2]);
            Assert.Equal(0xFF, buffer[i + 3]);
        }
    }
}
