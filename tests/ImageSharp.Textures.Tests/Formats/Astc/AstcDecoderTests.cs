// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Textures.Compression.Astc;
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
public class AstcDecoderTests
{
    [Fact]
    public void DecompressImage_WithDataSizeNotMultipleOfBlockSize_ShouldReturnEmpty()
    {
        byte[] data = new byte[256];
        const int width = 16;
        const int height = 16;
        byte[] invalidData = data.AsSpan(0, data.Length - 1).ToArray();

        Span<byte> result = AstcDecoder.DecompressImage(invalidData, width, height, FootprintType.Footprint4x4);

        Assert.Empty(result.ToArray());
    }

    [Fact]
    public void DecompressImage_WithMismatchedBlockCount_ShouldReturnEmpty()
    {
        byte[] data = new byte[256];
        const int width = 16;
        const int height = 16;
        byte[] mismatchedData = data.AsSpan(0, data.Length - BlockInfo.SizeInBytes).ToArray();

        Span<byte> result = AstcDecoder.DecompressImage(mismatchedData, width, height, FootprintType.Footprint4x4);

        Assert.Empty(result.ToArray());
    }

    [Theory]
    [InlineData(TestData.Astc.Rgba_4x4)]
    [InlineData(TestData.Astc.Rgba_5x5)]
    [InlineData(TestData.Astc.Rgba_6x6)]
    [InlineData(TestData.Astc.Rgba_8x8)]
    [InlineData(TestData.Astc.Checkerboard)]
    [InlineData(TestData.Astc.Checkered_4)]
    [InlineData(TestData.Astc.Checkered_5)]
    [InlineData(TestData.Astc.Checkered_6)]
    [InlineData(TestData.Astc.Checkered_7)]
    [InlineData(TestData.Astc.Checkered_8)]
    [InlineData(TestData.Astc.Checkered_9)]
    [InlineData(TestData.Astc.Checkered_10)]
    [InlineData(TestData.Astc.Checkered_11)]
    [InlineData(TestData.Astc.Checkered_12)]
    [InlineData(TestData.Astc.Footprint_4x4)]
    [InlineData(TestData.Astc.Footprint_5x4)]
    [InlineData(TestData.Astc.Footprint_5x5)]
    [InlineData(TestData.Astc.Footprint_6x5)]
    [InlineData(TestData.Astc.Footprint_6x6)]
    [InlineData(TestData.Astc.Footprint_8x5)]
    [InlineData(TestData.Astc.Footprint_8x6)]
    [InlineData(TestData.Astc.Footprint_8x8)]
    [InlineData(TestData.Astc.Footprint_10x5)]
    [InlineData(TestData.Astc.Footprint_10x6)]
    [InlineData(TestData.Astc.Footprint_10x8)]
    [InlineData(TestData.Astc.Footprint_10x10)]
    [InlineData(TestData.Astc.Footprint_12x10)]
    [InlineData(TestData.Astc.Footprint_12x12)]
    [InlineData(TestData.Astc.Rgb_4x4)]
    [InlineData(TestData.Astc.Rgb_5x4)]
    [InlineData(TestData.Astc.Rgb_6x6)]
    [InlineData(TestData.Astc.Rgb_8x8)]
    [InlineData(TestData.Astc.Rgb_12x12)]
    public void DecompressImage_WithTestdataFile_ShouldReturnExpectedByteCount(string inputFile)
    {
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", inputFile));
        byte[] bytes = File.ReadAllBytes(filePath);
        AstcFile astc = AstcFile.FromMemory(bytes);

        Span<byte> result = AstcDecoder.DecompressImage(astc);

        Assert.Equal(astc.Width * astc.Height * 4, result.Length);
    }

    [Theory]
    [InlineData(TestData.Astc.Rgba_4x4, FootprintType.Footprint4x4, 256, 256)]
    [InlineData(TestData.Astc.Rgba_5x5, FootprintType.Footprint5x5, 256, 256)]
    [InlineData(TestData.Astc.Rgba_6x6, FootprintType.Footprint6x6, 256, 256)]
    [InlineData(TestData.Astc.Rgba_8x8, FootprintType.Footprint8x8, 256, 256)]
    public void DecompressImage_WithValidData_ShouldDecodeAllBlocks(
        string inputFile,
        FootprintType footprintType,
        int width,
        int height)
    {
        byte[] astcData = TestFile.Create(Path.Combine("Astc", inputFile)).Bytes[16..];
        Footprint footprint = Footprint.FromFootprintType(footprintType);
        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;
        int blocksWide = (width + blockWidth - 1) / blockWidth;
        int blocksHigh = (height + blockHeight - 1) / blockHeight;
        int expectedBlockCount = blocksWide * blocksHigh;

        // Check ASTC data structure
        Assert.Equal(0, astcData.Length % BlockInfo.SizeInBytes);
        Assert.Equal(expectedBlockCount, astcData.Length / BlockInfo.SizeInBytes);

        // Verify every block has a valid block-mode encoding.
        for (int i = 0; i < astcData.Length; i += BlockInfo.SizeInBytes)
        {
            byte[] block = astcData.AsSpan(i, BlockInfo.SizeInBytes).ToArray();
            UInt128 bits = new(BitConverter.ToUInt64(block, 8), BitConverter.ToUInt64(block, 0));
            BlockInfo info = BlockModeDecoder.Decode(bits);

            Assert.True(info.IsValid);
        }
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgb_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgb_5x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgb_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgb_8x8)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgb_12x12)]
    public void DecompressImage_WithAstcRgbFile_ShouldMatchExpected(TestTextureProvider provider)
    {
        byte[] astcBytes = File.ReadAllBytes(provider.InputFile);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        string blockSize = $"{file.Footprint.Width}x{file.Footprint.Height}";

        byte[] decodedPixels = AstcDecoder.DecompressImage(file).ToArray();
        using Image<Rgba32> actualImage = Image.LoadPixelData<Rgba32>(decodedPixels, file.Width, file.Height);
        actualImage.Mutate(x => x.Flip(FlipMode.Vertical));

        actualImage.CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: blockSize);
    }

    [Theory]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgba_4x4)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgba_5x5)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgba_6x6)]
    [WithFile(TestTextureFormat.Astc, TestTextureType.Flat, TestTextureTool.AstcEnc, TestData.Astc.Rgba_8x8)]
    public void DecompressImage_WithAstcRgbaFile_ShouldMatchExpected(TestTextureProvider provider)
    {
        byte[] astcBytes = File.ReadAllBytes(provider.InputFile);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        string blockSize = $"{file.Footprint.Width}x{file.Footprint.Height}";

        byte[] decodedPixels = AstcDecoder.DecompressImage(file).ToArray();
        using Image<Rgba32> actualImage = Image.LoadPixelData<Rgba32>(decodedPixels, file.Width, file.Height);
        actualImage.Mutate(x => x.Flip(FlipMode.Vertical));

        actualImage.CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: blockSize);
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(4, -1)]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void DecompressImage_WithInvalidDimensions_ShouldThrowArgumentOutOfRangeException(int width, int height)
    {
        byte[] data = new byte[16];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(data, width, height, FootprintType.Footprint4x4).ToArray());
    }

    [Fact]
    public void DecompressImageToBuffer_WithNegativeWidth_ShouldThrowArgumentOutOfRangeException()
    {
        byte[] data = new byte[16];
        byte[] buffer = new byte[64];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(data, -1, 4, footprint, buffer));
    }

    [Fact]
    public void DecompressImageToBuffer_WithTooSmallBuffer_ShouldThrowArgumentOutOfRangeException()
    {
        // 4x4 image with 4x4 blocks = 1 block = 16 bytes input, needs 4*4*4=64 bytes output
        byte[] data = new byte[16];
        byte[] buffer = new byte[32]; // too small
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(data, 4, 4, footprint, buffer));
    }

    [Theory]
    [InlineData(8, 64)]
    [InlineData(16, 10)]
    public void DecompressBlock_WithInvalidBufferSizes_ShouldThrowArgumentOutOfRangeException(int dataSize, int bufferSize)
    {
        byte[] data = new byte[dataSize];
        byte[] buffer = new byte[bufferSize];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecompressBlock(data, footprint, buffer));
    }

    [Theory]
    [InlineData(8, 64)]
    [InlineData(16, 10)]
    public void DecompressHdrBlock_WithInvalidBufferSizes_ShouldThrowArgumentOutOfRangeException(int dataSize, int bufferSize)
    {
        byte[] data = new byte[dataSize];
        float[] buffer = new float[bufferSize];
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentException>(() =>
            AstcDecoder.DecompressHdrBlock(data, footprint, buffer));
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(4, -1)]
    [InlineData(0, 4)]
    [InlineData(4, 0)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void DecompressHdrImage_WithInvalidDimensions_ShouldThrowArgumentOutOfRangeException(int width, int height)
    {
        byte[] data = new byte[16];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressHdrImage(data, width, height, FootprintType.Footprint4x4).ToArray());
    }

    [Fact]
    public void DecompressHdrImageToBuffer_WithTooSmallBuffer_ShouldThrowArgumentOutOfRangeException()
    {
        byte[] data = new byte[16];
        float[] buffer = new float[32]; // too small for 4x4 image (needs 64)
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressHdrImage(data, 4, 4, footprint, buffer));
    }

    [Fact]
    public void DecompressImage_WhenCalledFromManyThreads_ShouldProduceIdenticalOutput()
    {
        // Smoke test for accidental shared mutable state in the decode pipeline. Each
        // thread decodes the same input into its own buffer; every buffer must match the
        // single-threaded reference byte-for-byte.
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Rgba_6x6));
        byte[] astcBytes = File.ReadAllBytes(filePath);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        byte[] reference = AstcDecoder.DecompressImage(file).ToArray();
        Assert.NotEmpty(reference);

        const int threadCount = 8;
        const int iterationsPerThread = 4;
        byte[][] results = new byte[threadCount][];

        Parallel.For(0, threadCount, i =>
        {
            byte[]? last = null;
            for (int j = 0; j < iterationsPerThread; j++)
            {
                last = AstcDecoder.DecompressImage(file).ToArray();
            }

            results[i] = last!;
        });

        foreach (byte[] result in results)
        {
            Assert.Equal(reference, result);
        }
    }

    [Fact]
    public void DecompressBlock_AndDecompressImage_ShouldReturnIdenticalBlockShape()
    {
        // Cross-validates the per-block (DecompressBlock) and whole-image (DecompressImage)
        // public APIs on a test file that contains multi-partition, dual-plane, and
        // void-extent blocks. Both paths must yield identical pixels for every block.
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Rgba_4x4));
        byte[] astcBytes = File.ReadAllBytes(filePath);
        AstcFile file = AstcFile.FromMemory(astcBytes);

        byte[] imageBuffer = AstcDecoder.DecompressImage(file).ToArray();
        Assert.NotEmpty(imageBuffer);

        int blockWidth = file.Footprint.Width;
        int blockHeight = file.Footprint.Height;
        int blocksWide = (file.Width + blockWidth - 1) / blockWidth;
        int blockCount = file.Blocks.Length / BlockInfo.SizeInBytes;
        int totalValid = 0;
        int voidExtent = 0;
        int singlePartition = 0;
        int twoPartition = 0;
        int threePartition = 0;
        int fourPartition = 0;
        int dualPlane = 0;
        byte[] singleBlockOut = new byte[blockWidth * blockHeight * BlockInfo.ChannelsPerPixel];

        for (int blockIdx = 0; blockIdx < blockCount; blockIdx++)
        {
            ReadOnlySpan<byte> blockSpan = file.Blocks.Slice(blockIdx * BlockInfo.SizeInBytes, BlockInfo.SizeInBytes);
            UInt128 bits = BinaryPrimitives.ReadUInt128LittleEndian(blockSpan);
            BlockInfo info = BlockModeDecoder.Decode(bits);
            Assert.True(info.IsValid, $"Block {blockIdx} of rgba_4x4.astc must decode as a valid block.");

            Array.Clear(singleBlockOut);
            AstcDecoder.DecompressBlock(blockSpan, file.Footprint, singleBlockOut);

            int blockX = blockIdx % blocksWide;
            int blockY = blockIdx / blocksWide;
            AssertBlockMatchesImageSlice(
                singleBlockOut, imageBuffer, file.Width, file.Height, blockX, blockY, blockWidth, blockHeight);

            totalValid++;
            if (info.IsVoidExtent)
            {
                voidExtent++;
                continue;
            }

            _ = info.PartitionCount switch
            {
                1 => singlePartition++,
                2 => twoPartition++,
                3 => threePartition++,
                4 => fourPartition++,
                _ => 0,
            };

            if (info.DualPlane.Enabled)
            {
                dualPlane++;
            }
        }

        Assert.Equal(4096, totalValid);
        Assert.Equal(142, voidExtent);
        Assert.Equal(2528, singlePartition);
        Assert.Equal(1184, twoPartition);
        Assert.Equal(231, threePartition);
        Assert.Equal(11, fourPartition);
        Assert.Equal(661, dualPlane);
    }

    private static void AssertBlockMatchesImageSlice(
        byte[] block,
        byte[] image,
        int imageWidth,
        int imageHeight,
        int blockX,
        int blockY,
        int blockWidth,
        int blockHeight)
    {
        for (int by = 0; by < blockHeight; by++)
        {
            int py = (blockY * blockHeight) + by;
            if (py >= imageHeight)
            {
                continue;
            }

            for (int bx = 0; bx < blockWidth; bx++)
            {
                int px = (blockX * blockWidth) + bx;
                if (px >= imageWidth)
                {
                    continue;
                }

                int blockOffset = ((by * blockWidth) + bx) * BlockInfo.ChannelsPerPixel;
                int imageOffset = ((py * imageWidth) + px) * BlockInfo.ChannelsPerPixel;
                for (int c = 0; c < BlockInfo.ChannelsPerPixel; c++)
                {
                    Assert.Equal(block[blockOffset + c], image[imageOffset + c]);
                }
            }
        }
    }

    [Fact]
    public void DecompressImage_StreamOverload_ShouldMatchSpanOverload()
    {
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Rgba_4x4));
        AstcFile file = AstcFile.FromMemory(File.ReadAllBytes(filePath));

        byte[] expected = AstcDecoder.DecompressImage(file.Blocks, file.Width, file.Height, file.Footprint).ToArray();
        Assert.NotEmpty(expected);

        using MemoryStream stream = new(file.Blocks.ToArray());
        Span<byte> actual = AstcDecoder.DecompressImage(stream, file.Width, file.Height, file.Footprint);

        Assert.Equal(expected, actual.ToArray());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void DecompressImage_StreamOverloadIntoBuffer_ShouldMatchSpanOverload()
    {
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Rgba_4x4));
        AstcFile file = AstcFile.FromMemory(File.ReadAllBytes(filePath));

        byte[] expected = new byte[file.Width * file.Height * BlockInfo.ChannelsPerPixel];
        Assert.True(AstcDecoder.DecompressImage(file.Blocks, file.Width, file.Height, file.Footprint, expected));

        byte[] actual = new byte[expected.Length];
        using MemoryStream stream = new(file.Blocks.ToArray());
        Assert.True(AstcDecoder.DecompressImage(stream, file.Width, file.Height, file.Footprint, actual));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DecompressHdrImage_StreamOverload_ShouldMatchSpanOverload()
    {
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Hdr.Hdr_Tile));
        AstcFile file = AstcFile.FromMemory(File.ReadAllBytes(filePath));

        Span<float> expected = AstcDecoder.DecompressHdrImage(file.Blocks, file.Width, file.Height, file.Footprint);
        Assert.False(expected.IsEmpty);

        using MemoryStream stream = new(file.Blocks.ToArray());
        Span<float> actual = AstcDecoder.DecompressHdrImage(stream, file.Width, file.Height, file.Footprint);

        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void DecompressHdrImage_StreamOverloadIntoBuffer_ShouldMatchSpanOverload()
    {
        string filePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Hdr.Hdr_Tile));
        AstcFile file = AstcFile.FromMemory(File.ReadAllBytes(filePath));

        float[] expected = new float[file.Width * file.Height * BlockInfo.ChannelsPerPixel];
        Assert.True(AstcDecoder.DecompressHdrImage(file.Blocks, file.Width, file.Height, file.Footprint, expected));

        float[] actual = new float[expected.Length];
        using MemoryStream stream = new(file.Blocks.ToArray());
        Assert.True(AstcDecoder.DecompressHdrImage(stream, file.Width, file.Height, file.Footprint, actual));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DecompressImage_StreamOverload_WithNullStream_ShouldThrow()
    {
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentNullException>(() =>
            AstcDecoder.DecompressImage((Stream)null!, 4, 4, footprint).ToArray());
    }

    [Fact]
    public void DecompressHdrImage_StreamOverload_WithNullStream_ShouldThrow()
    {
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentNullException>(() =>
            AstcDecoder.DecompressHdrImage((Stream)null!, 4, 4, footprint).ToArray());
    }

    [Fact]
    public void DecompressImage_StreamOverload_WithTruncatedStream_ShouldThrow()
    {
        // 4×4 image with 4×4 footprint expects 16 bytes; provide 8.
        using MemoryStream stream = new(new byte[8]);
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<EndOfStreamException>(() =>
            AstcDecoder.DecompressImage(stream, 4, 4, footprint).ToArray());
    }

    [Fact]
    public void DecompressHdrImage_StreamOverload_WithTruncatedStream_ShouldThrow()
    {
        using MemoryStream stream = new(new byte[8]);
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<EndOfStreamException>(() =>
            AstcDecoder.DecompressHdrImage(stream, 4, 4, footprint).ToArray());
    }

    [Fact]
    public void DecompressImage_StreamOverloadIntoBuffer_WithTooSmallBuffer_ShouldThrow()
    {
        using MemoryStream stream = new(new byte[16]);
        byte[] buffer = new byte[32]; // too small for a 4×4 image (needs 64)
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AstcDecoder.DecompressImage(stream, 4, 4, footprint, buffer));
    }
}
