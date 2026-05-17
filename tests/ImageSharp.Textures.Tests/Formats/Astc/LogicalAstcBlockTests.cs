// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

#nullable enable

[GroupOutput("Astc")]
[Trait("Format", "Astc")]
public class LogicalAstcBlockTests
{
    [Fact]
    public void DecodeToBytes_WithErrorBlock_ShouldLeaveBufferUntouched()
    {
        UInt128 bits = UInt128.Zero;
        BlockInfo info = BlockModeDecoder.Decode(bits);
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint8x8);
        byte[] pixels = new byte[footprint.PixelCount * 4];
        Array.Fill(pixels, (byte)0xCC);

        LogicalBlock.DecodeToBytes(bits, in info, footprint, pixels);

        // Invalid blocks short-circuit without touching the output buffer.
        Assert.All(pixels, b => Assert.Equal(0xCC, b));
    }

    [Fact]
    public void DecodeToBytes_WithVoidExtentBlock_ShouldFillUniformPixels()
    {
        // 0xFFFFFFFFFFFFFDFCUL is the canonical "all-ones" void-extent block (zero RGBA).
        UInt128 bits = (UInt128)0xFFFFFFFFFFFFFDFCUL;
        BlockInfo info = BlockModeDecoder.Decode(bits);
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint8x8);
        byte[] pixels = new byte[footprint.PixelCount * 4];

        LogicalBlock.DecodeToBytes(bits, in info, footprint, pixels);

        Assert.All(pixels, b => Assert.Equal(0, b));
    }

    [Fact]
    public void DecodeToBytes_WithStandardBlock_Succeeds()
    {
        UInt128 bits = (UInt128)0x0000000001FE000173UL;
        BlockInfo info = BlockModeDecoder.Decode(bits);
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint6x5);
        byte[] pixels = new byte[footprint.PixelCount * 4];

        LogicalBlock.DecodeToBytes(bits, in info, footprint, pixels);

        // Block carries valid LDR data and decodes without throwing; we don't pin specific
        // pixel values here — those are covered by the image roundtrip tests below.
        Assert.True(info.IsValid);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_5x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_5x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_6x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_8x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_8x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_8x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_10x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_10x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_10x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_10x10)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_12x10)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Footprint_12x12)]
    public void UnpackLogicalBlock_FromImage_ShouldDecodeCorrectly(TestTextureProvider provider)
    {
        byte[] astcBytes = File.ReadAllBytes(provider.InputFile);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        string blockSize = $"{file.Footprint.Width}x{file.Footprint.Height}";

        using Image<Rgba32> decodedImage = DecodeAstcBlocksToImage(file.Footprint, file.Blocks.ToArray(), file.Width, file.Height);

        decodedImage.CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: blockSize);
    }

    private static Image<Rgba32> DecodeAstcBlocksToImage(Footprint footprint, byte[] astcData, int width, int height)
    {
        // ASTC uses x/y ordering, so we flip Y to match ImageSharp's row/column origin.
        Image<Rgba32> image = new(width, height);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        byte[] blockPixels = new byte[blockWidth * blockHeight * 4];

        for (int i = 0; i < astcData.Length; i += BlockInfo.SizeInBytes)
        {
            int blockIndex = i / BlockInfo.SizeInBytes;
            int blockX = blockIndex % blocksWide;
            int blockY = blockIndex / blocksWide;

            ReadOnlySpan<byte> blockSpan = astcData.AsSpan(i, BlockInfo.SizeInBytes);
            UInt128 bits = new(
                BitConverter.ToUInt64(blockSpan[8..]),
                BitConverter.ToUInt64(blockSpan));
            BlockInfo info = BlockModeDecoder.Decode(bits);
            Assert.True(info.IsValid);

            LogicalBlock.DecodeToBytes(bits, in info, footprint, blockPixels);

            for (int y = 0; y < blockHeight; ++y)
            {
                for (int x = 0; x < blockWidth; ++x)
                {
                    int px = (blockWidth * blockX) + x;
                    int py = (blockHeight * blockY) + y;
                    if (px >= width || py >= height)
                    {
                        continue;
                    }

                    int offset = ((y * blockWidth) + x) * 4;
                    image[px, height - 1 - py] = new Rgba32(
                        blockPixels[offset + 0],
                        blockPixels[offset + 1],
                        blockPixels[offset + 2],
                        blockPixels[offset + 3]);
                }
            }
        }

        return image;
    }
}
