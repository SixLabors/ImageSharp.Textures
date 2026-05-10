// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;
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
    public void UnpackLogicalBlock_WithErrorBlock_ShouldReturnNull()
    {
        UInt128 bits = UInt128.Zero;
        BlockInfo info = BlockInfo.Decode(bits);

        LogicalBlock? result = LogicalBlock.UnpackLogicalBlock(Footprint.Get8x8(), bits, in info);

        Assert.Null(result);
    }

    [Fact]
    public void UnpackLogicalBlock_WithVoidExtentBlock_ShouldSucceed()
    {
        UInt128 bits = (UInt128)0xFFFFFFFFFFFFFDFCUL;
        BlockInfo info = BlockInfo.Decode(bits);

        LogicalBlock? result = LogicalBlock.UnpackLogicalBlock(Footprint.Get8x8(), bits, in info);

        Assert.NotNull(result);
    }

    [Fact]
    public void UnpackLogicalBlock_WithStandardBlock_ShouldSucceed()
    {
        UInt128 bits = (UInt128)0x0000000001FE000173UL;
        BlockInfo info = BlockInfo.Decode(bits);

        LogicalBlock? result = LogicalBlock.UnpackLogicalBlock(Footprint.Get6x5(), bits, in info);

        Assert.NotNull(result);
    }

    [Theory]
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
    public void UnpackLogicalBlock_FromImage_ShouldDecodeCorrectly(TestTextureProvider provider)
    {
        byte[] astcBytes = File.ReadAllBytes(provider.InputFile);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        string blockSize = $"{file.Footprint.Width}x{file.Footprint.Height}";

        using Image<Rgba32> decodedImage = DecodeAstcBlocksToImage(file.Footprint, file.Blocks.ToArray(), file.Width, file.Height);

        decodedImage.CompareToReferenceOutput(
            ImageComparer.TolerantPercentage(0.03f),
            provider,
            testOutputDetails: blockSize);
    }

    private static Image<Rgba32> DecodeAstcBlocksToImage(Footprint footprint, byte[] astcData, int width, int height)
    {
        // ASTC uses x/y ordering, so we flip Y to match ImageSharp's row/column origin.
        Image<Rgba32> image = new(width, height);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        byte[] blockPixels = new byte[blockWidth * blockHeight * 4];

        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            int blockIndex = i / PhysicalBlock.SizeInBytes;
            int blockX = blockIndex % blocksWide;
            int blockY = blockIndex / blocksWide;

            ReadOnlySpan<byte> blockSpan = astcData.AsSpan(i, PhysicalBlock.SizeInBytes);
            UInt128 bits = new(
                BitConverter.ToUInt64(blockSpan[8..]),
                BitConverter.ToUInt64(blockSpan));
            BlockInfo info = BlockInfo.Decode(bits);
            LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, bits, in info);
            Assert.NotNull(logicalBlock);

            logicalBlock!.WriteAllPixelsLdr(footprint, blockPixels);

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
