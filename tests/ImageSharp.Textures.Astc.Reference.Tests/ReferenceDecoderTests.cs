// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.Reference.Tests.Utils;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;

namespace SixLabors.ImageSharp.Textures.Astc.Reference.Tests;

/// <summary>
/// LDR comparison tests between SixLabors.ImageSharp.Textures.Astc and the ARM reference ASTC decoder.
/// These validate that SixLabors.ImageSharp.Textures.Astc produces output matching the official ARM implementation.
/// </summary>
[Trait("Format", "Astc")]
public class ReferenceDecoderTests
{
    // Per-channel tolerance for RGBA8 comparisons.
    // ASTC spec conformance allows ±1 for UNORM8 output due to rounding differences.
    private const int Ldr8BitTolerance = 1;

    public static TheoryData<FootprintType> AllFootprintTypes =>
        new()
        {
            FootprintType.Footprint4x4,
            FootprintType.Footprint5x4,
            FootprintType.Footprint5x5,
            FootprintType.Footprint6x5,
            FootprintType.Footprint6x6,
            FootprintType.Footprint8x5,
            FootprintType.Footprint8x6,
            FootprintType.Footprint8x8,
            FootprintType.Footprint10x5,
            FootprintType.Footprint10x6,
            FootprintType.Footprint10x8,
            FootprintType.Footprint10x10,
            FootprintType.Footprint12x10,
            FootprintType.Footprint12x12,
        };

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkerboard)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_7)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_9)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_10)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_11)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Checkered_12)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_5x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_5x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_6x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_8x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_8x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_8x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_10x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_10x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_10x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_10x10)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_12x10)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Footprint_12x12)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_5x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_8x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgb_12x12)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_5x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestImages.Astc.Rgba_8x8)]
    public void DecompressLdr_WithImage_ShouldMatch(TestTextureProvider provider)
    {
        byte[] bytes = File.ReadAllBytes(provider.InputFile);
        AstcFile astcFile = AstcFile.FromMemory(bytes);
        (int blockX, int blockY) = ReferenceDecoder.ToBlockDimensions(astcFile.Footprint.Type);
        string basename = Path.GetFileNameWithoutExtension(provider.InputFile);

        byte[] expected = ReferenceDecoder.DecompressLdr(
            astcFile.Blocks, astcFile.Width, astcFile.Height, blockX, blockY);
        Span<byte> actual = AstcDecoder.DecompressImage(
            astcFile.Blocks, astcFile.Width, astcFile.Height, astcFile.Footprint);

        CompareRgba8(actual, expected, astcFile.Width, astcFile.Height, basename);
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    public void DecompressLdr_SolidColor_ShouldMatch(FootprintType footprintType)
    {
        (int blockX, int blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);
        int width = blockX;
        int height = blockY;

        // Single solid color block
        byte[] pixels = new byte[width * height * 4];
        for (int index = 0; index < width * height; index++)
        {
            pixels[(index * 4) + 0] = 128; // R
            pixels[(index * 4) + 1] = 64; // G
            pixels[(index * 4) + 2] = 200; // B
            pixels[(index * 4) + 3] = 255; // A
        }

        byte[] compressed = ReferenceDecoder.CompressLdr(pixels, width, height, blockX, blockY);
        Footprint footprint = Footprint.FromFootprintType(footprintType);

        byte[] expected = ReferenceDecoder.DecompressLdr(compressed, width, height, blockX, blockY);
        Span<byte> actual = AstcDecoder.DecompressImage(compressed, width, height, footprint);

        CompareRgba8(actual, expected, width, height, $"SolidColor_{footprintType}");
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    public void DecompressLdr_Gradient_ShouldMatch(FootprintType footprintType)
    {
        (int blockX, int blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);

        // 2×2 blocks for gradient
        int width = blockX * 2;
        int height = blockY * 2;

        byte[] pixels = new byte[width * height * 4];
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int idx = ((row * width) + col) * 4;
                pixels[idx + 0] = (byte)(255 * col / (width - 1)); // R: left-to-right
                pixels[idx + 1] = (byte)(255 * row / (height - 1)); // G: top-to-bottom
                pixels[idx + 2] = (byte)(255 - (255 * col / (width - 1))); // B: inverse of R
                pixels[idx + 3] = 255;
            }
        }

        byte[] compressed = ReferenceDecoder.CompressLdr(pixels, width, height, blockX, blockY);
        Footprint footprint = Footprint.FromFootprintType(footprintType);

        byte[] expected = ReferenceDecoder.DecompressLdr(compressed, width, height, blockX, blockY);
        Span<byte> actual = AstcDecoder.DecompressImage(compressed, width, height, footprint);

        CompareRgba8(actual, expected, width, height, $"Gradient_{footprintType}");
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    public void DecompressLdr_RandomNoise_ShouldMatch(FootprintType footprintType)
    {
        (int blockX, int blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);

        // 2×2 blocks
        int width = blockX * 2;
        int height = blockY * 2;

        Random rng = new(42); // Fixed seed for reproducibility
        byte[] pixels = new byte[width * height * 4];
        rng.NextBytes(pixels);

        // Force alpha to 255 so compression doesn't introduce alpha-related variance
        for (int index = 3; index < pixels.Length; index += 4)
        {
            pixels[index] = byte.MaxValue;
        }

        byte[] compressed = ReferenceDecoder.CompressLdr(pixels, width, height, blockX, blockY);
        Footprint footprint = Footprint.FromFootprintType(footprintType);

        byte[] expected = ReferenceDecoder.DecompressLdr(compressed, width, height, blockX, blockY);
        Span<byte> actual = AstcDecoder.DecompressImage(compressed, width, height, footprint);

        CompareRgba8(actual, expected, width, height, $"RandomNoise_{footprintType}");
    }

    [Theory]
    [MemberData(nameof(AllFootprintTypes))]
    public void DecompressLdr_NonBlockAlignedDimensions_ShouldMatch(FootprintType footprintType)
    {
        (int blockX, int blockY) = ReferenceDecoder.ToBlockDimensions(footprintType);

        // Non-block-aligned dimensions: use dimensions that don't evenly divide by block size
        int width = blockX + (blockX / 2) + 1; // e.g. for 4x4: 7, for 8x8: 13
        int height = blockY + (blockY / 2) + 1;

        Random rng = new(123);
        byte[] pixels = new byte[width * height * 4];
        rng.NextBytes(pixels);
        for (int index = 3; index < pixels.Length; index += 4)
        {
            pixels[index] = byte.MaxValue;
        }

        byte[] compressed = ReferenceDecoder.CompressLdr(pixels, width, height, blockX, blockY);
        Footprint footprint = Footprint.FromFootprintType(footprintType);

        byte[] expected = ReferenceDecoder.DecompressLdr(compressed, width, height, blockX, blockY);
        Span<byte> actual = AstcDecoder.DecompressImage(compressed, width, height, footprint);

        CompareRgba8(actual, expected, width, height, $"NonAligned_{footprintType}");
    }

    [Fact]
    public void DecompressLdr_VoidExtentBlock_ShouldMatch()
    {
        // Manually construct a void-extent constant-color block (128 bits):
        // Bits [0..8]   = 0b111111100 (0x1FC, void-extent marker)
        // Bit  [9]      = 0 (LDR mode)
        // Bits [10..11]  = 0b11 (reserved, must be 11 for valid void-extent)
        // Bits [12..63]  = all 1s (no extent coordinates = constant color block)
        // Bits [64..79]  = R (UNORM16)
        // Bits [80..95]  = G (UNORM16)
        // Bits [96..111] = B (UNORM16)
        // Bits [112..127]= A (UNORM16)
        byte[] block = new byte[16];
        ulong low = 0xFFFFFFFFFFFFFDFC;
        ulong high = (0xFFFFUL << 48) | (0xC000UL << 32) | (0x4000UL << 16) | 0x8000;
        BitConverter.TryWriteBytes(block.AsSpan(0, 8), low);
        BitConverter.TryWriteBytes(block.AsSpan(8, 8), high);

        const int blockX = 4;
        const int blockY = 4;
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        byte[] expected = ReferenceDecoder.DecompressLdr(block, blockX, blockY, blockX, blockY);
        Span<byte> actual = AstcDecoder.DecompressImage(block, blockX, blockY, footprint);

        CompareRgba8(actual, expected, blockX, blockY, "VoidExtent");
    }

    /// <summary>
    /// Compare RGBA8 output from both decoders with per-channel tolerance.
    /// </summary>
    private static void CompareRgba8(Span<byte> actual, byte[] expected, int width, int height, string label)
    {
        int pixelCount = width * height * 4;
        Assert.True(actual.Length == pixelCount, $"actual output size should match for {label}");
        Assert.True(expected.Length == pixelCount, $"expected output size should match for {label}");

        int mismatches = 0;
        int worstDiff = 0;
        int worstPixel = -1;
        int worstChannel = -1;

        for (int index = 0; index < pixelCount; index++)
        {
            int diff = Math.Abs(actual[index] - expected[index]);
            if (diff > Ldr8BitTolerance)
            {
                mismatches++;
                if (diff > worstDiff)
                {
                    worstDiff = diff;
                    worstPixel = index / 4;
                    worstChannel = index % 4;
                }
            }
        }

        if (mismatches > 0)
        {
            string channelName = worstChannel switch { 0 => "R", 1 => "G", 2 => "B", _ => "A" };
            int pixelX = worstPixel % width;
            int pixelY = worstPixel / width;
            Assert.Fail(
                $"[{label}] {mismatches} channel mismatches exceed tolerance ±{Ldr8BitTolerance}. " +
                $"Worst: pixel ({pixelX},{pixelY}) channel {channelName}, " +
                $"actual={actual[(worstPixel * 4) + worstChannel]} vs expected={expected[(worstPixel * 4) + worstChannel]} (diff={worstDiff})");
        }
    }
}
