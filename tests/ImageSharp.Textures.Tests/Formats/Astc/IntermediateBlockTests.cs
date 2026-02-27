// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

#nullable enable
public class IntermediateBlockTests
{
    private static readonly UInt128 ErrorBlock = UInt128.Zero;

    [Fact]
    public void UnpackVoidExtent_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock errorBlock = PhysicalBlock.Create(ErrorBlock);

        IntermediateBlock.VoidExtentData? result = IntermediateBlock.UnpackVoidExtent(errorBlock);

        Assert.Null(result);
    }

    [Fact]
    public void UnpackIntermediateBlock_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock errorBlock = PhysicalBlock.Create(ErrorBlock);

        IntermediateBlock.IntermediateBlockData? result = IntermediateBlock.UnpackIntermediateBlock(errorBlock);

        Assert.Null(result);
    }

    [Fact]
    public void EndpointRangeForBlock_WithoutWeights_ShouldReturnNegativeOne()
    {
        IntermediateBlock.IntermediateBlockData data = new()
        {
            WeightRange = 15,
            WeightGridX = 6,
            WeightGridY = 6
        };

        int result = IntermediateBlock.EndpointRangeForBlock(data);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void Pack_WithIncorrectNumberOfWeights_ShouldReturnError()
    {
        IntermediateBlock.IntermediateBlockData data = new()
        {
            WeightRange = 15,
            WeightGridX = 6,
            WeightGridY = 6
        };

        (string? error, UInt128 _) = IntermediateBlockPacker.Pack(data);

        Assert.NotNull(error);
        Assert.Contains("Incorrect number of weights", error);
    }

    [Fact]
    public void EndpointRangeForBlock_WithNotEnoughBits_ShouldReturnNegativeTwo()
    {
        IntermediateBlock.IntermediateBlockData data = new()
        {
            WeightRange = 1,
            PartitionId = 0,
            WeightGridX = 8,
            WeightGridY = 8,
            EndpointCount = 3
        };
        data.Endpoints[0] = new() { Mode = ColorEndpointMode.LdrRgbDirect };
        data.Endpoints[1] = new() { Mode = ColorEndpointMode.LdrRgbDirect };
        data.Endpoints[2] = new() { Mode = ColorEndpointMode.LdrRgbDirect };

        int result = IntermediateBlock.EndpointRangeForBlock(data);

        Assert.Equal(-2, result);
    }

    [Fact]
    public void Pack_WithNotEnoughBitsForColors_ShouldReturnError()
    {
        IntermediateBlock.IntermediateBlockData data = new()
        {
            WeightRange = 1,
            PartitionId = 0,
            WeightGridX = 8,
            WeightGridY = 8,
            Weights = new int[64],
            EndpointCount = 3
        };
        data.Endpoints[0] = new() { Mode = ColorEndpointMode.LdrRgbDirect };
        data.Endpoints[1] = new() { Mode = ColorEndpointMode.LdrRgbDirect };
        data.Endpoints[2] = new() { Mode = ColorEndpointMode.LdrRgbDirect };

        (string? error, UInt128 _) = IntermediateBlockPacker.Pack(data);

        Assert.NotNull(error);
        Assert.Contains("illegal color range", error);
    }

    [Fact]
    public void EndpointRangeForBlock_WithIncreasingWeightGrid_ShouldDecreaseColorRange()
    {
        IntermediateBlock.IntermediateBlockData data = new()
        {
            WeightRange = 2,
            DualPlaneChannel = null,
            EndpointCount = 2
        };
        data.Endpoints[0] = new() { Mode = ColorEndpointMode.LdrRgbDirect };
        data.Endpoints[1] = new() { Mode = ColorEndpointMode.LdrRgbDirect };

        List<(int W, int H)> weightParams = [];
        for (int y = 2; y < 8; ++y)
        {
            for (int x = 2; x < 8; ++x)
            {
                weightParams.Add((x, y));
            }
        }

        weightParams.Sort((a, b) => (a.W * a.H).CompareTo(b.W * b.H));

        int lastColorRange = byte.MaxValue;
        foreach ((int w, int h) in weightParams)
        {
            data.WeightGridX = w;
            data.WeightGridY = h;
            int colorRange = IntermediateBlock.EndpointRangeForBlock(data);

            Assert.True(colorRange <= lastColorRange);
            lastColorRange = Math.Min(colorRange, lastColorRange);
        }

        Assert.True(lastColorRange < byte.MaxValue);
    }

    [Fact]
    public void EndpointRange_WithStandardBlock_ShouldBe255()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0x0000000001FE000173UL);

        IntermediateBlock.IntermediateBlockData? data = IntermediateBlock.UnpackIntermediateBlock(block);

        Assert.Equal(255, block.GetColorValuesRange());
        Assert.NotNull(data);
        IntermediateBlock.IntermediateBlockData ib = data!.Value;
        Assert.Equal(1, ib.EndpointCount);
        Assert.Equal(ColorEndpointMode.LdrLumaDirect, ib.Endpoints[0].Mode);
        Assert.Equal(byte.MinValue, ib.Endpoints[0].Colors[0]);
        Assert.Equal(byte.MaxValue, ib.Endpoints[0].Colors[1]);
        Assert.Equal(2, ib.Endpoints[0].ColorCount);
        Assert.Equal(byte.MaxValue, ib.EndpointRange);
    }

    [Fact]
    public void UnpackIntermediateBlock_WithStandardBlock_ShouldReturnCorrectData()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0x0000000001FE000173UL);

        IntermediateBlock.IntermediateBlockData? result = IntermediateBlock.UnpackIntermediateBlock(block);

        Assert.NotNull(result);
        IntermediateBlock.IntermediateBlockData data = result!.Value;

        Assert.Equal(6, data.WeightGridX);
        Assert.Equal(5, data.WeightGridY);
        Assert.Equal(7, data.WeightRange);
        Assert.Null(data.PartitionId);
        Assert.Null(data.DualPlaneChannel);

        Assert.Equal(30, data.WeightsCount);
        Assert.All(data.Weights.AsSpan(0, data.WeightsCount).ToArray(), item => Assert.Equal(0, item));

        Assert.Equal(1, data.EndpointCount);
        IntermediateBlock.IntermediateEndpointData endpoint = data.Endpoints[0];
        Assert.Equal(ColorEndpointMode.LdrLumaDirect, endpoint.Mode);
        Assert.Equal(2, endpoint.ColorCount);
        Assert.Equal(byte.MinValue, endpoint.Colors[0]);
        Assert.Equal(byte.MaxValue, endpoint.Colors[1]);
    }

    [Fact]
    public void Pack_WithStandardBlockData_ShouldProduceExpectedBits()
    {
        IntermediateBlock.IntermediateBlockData data = new()
        {
            WeightGridX = 6,
            WeightGridY = 5,
            WeightRange = 7,
            PartitionId = null,
            DualPlaneChannel = null,
            Weights = new int[30]
        };

        IntermediateBlock.IntermediateEndpointData endpoint = new()
        {
            Mode = ColorEndpointMode.LdrLumaDirect,
            ColorCount = 2
        };
        endpoint.Colors[0] = byte.MinValue;
        endpoint.Colors[1] = byte.MaxValue;
        data.Endpoints[0] = endpoint;
        data.EndpointCount = 1;

        (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(data);

        Assert.Null(error);
        Assert.Equal((UInt128)0x0000000001FE000173UL, packed);
    }

    [Fact]
    public void Pack_WithLargeGapInBits_ShouldPreserveOriginalEncoding()
    {
        UInt128 original = new(0xBEDEAD0000000000UL, 0x0000000001FE032EUL);
        PhysicalBlock block = PhysicalBlock.Create(original);
        IntermediateBlock.IntermediateBlockData? data = IntermediateBlock.UnpackIntermediateBlock(block);

        Assert.NotNull(data);
        IntermediateBlock.IntermediateBlockData intermediate = data!.Value;

        // Check unpacked values
        Assert.Equal(2, intermediate.WeightGridX);
        Assert.Equal(3, intermediate.WeightGridY);
        Assert.Equal(15, intermediate.WeightRange);
        Assert.Null(intermediate.PartitionId);
        Assert.Null(intermediate.DualPlaneChannel);
        Assert.Equal(1, intermediate.EndpointCount);
        Assert.Equal(ColorEndpointMode.LdrLumaDirect, intermediate.Endpoints[0].Mode);
        Assert.Equal(2, intermediate.Endpoints[0].ColorCount);
        Assert.Equal(255, intermediate.Endpoints[0].Colors[0]);
        Assert.Equal(0, intermediate.Endpoints[0].Colors[1]);

        // Repack
        (string? error, UInt128 repacked) = IntermediateBlockPacker.Pack(intermediate);

        Assert.Null(error);
        Assert.Equal(original, repacked);
    }

    [Fact]
    public void UnpackVoidExtent_WithAllOnesPattern_ShouldReturnZeroColors()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0xFFFFFFFFFFFFFDFCUL);

        IntermediateBlock.VoidExtentData? result = IntermediateBlock.UnpackVoidExtent(block);

        Assert.NotNull(result);
        IntermediateBlock.VoidExtentData data = result!.Value;

        Assert.Equal(0, data.R);
        Assert.Equal(0, data.G);
        Assert.Equal(0, data.B);
        Assert.Equal(0, data.A);

        Assert.All(data.Coords, c => Assert.Equal((1 << 13) - 1, c));
    }

    [Fact]
    public void UnpackVoidExtent_WithColorData_ShouldReturnCorrectColors()
    {
        UInt128 blockBits = new(0xdeadbeefdeadbeefUL, 0xFFF8003FFE000DFCUL);
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        IntermediateBlock.VoidExtentData? result = IntermediateBlock.UnpackVoidExtent(block);

        Assert.NotNull(result);
        IntermediateBlock.VoidExtentData data = result!.Value;

        Assert.Equal(0xbeef, data.R);
        Assert.Equal(0xdead, data.G);
        Assert.Equal(0xbeef, data.B);
        Assert.Equal(0xdead, data.A);

        Assert.Equal(0, data.Coords[0]);
        Assert.Equal(8191, data.Coords[1]);
        Assert.Equal(0, data.Coords[2]);
        Assert.Equal(8191, data.Coords[3]);
    }

    [Fact]
    public void Pack_WithZeroColorVoidExtent_ShouldProduceAllOnesPattern()
    {
        IntermediateBlock.VoidExtentData data = new()
        {
            R = 0,
            G = 0,
            B = 0,
            A = 0,
            Coords = new ushort[4]
        };

        for (int i = 0; i < 4; ++i)
        {
            data.Coords[i] = (1 << 13) - 1;
        }

        (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(data);

        Assert.Null(error);
        Assert.Equal((UInt128)0xFFFFFFFFFFFFFDFCUL, packed);
    }

    [Fact]
    public void Pack_WithColorVoidExtent_ShouldProduceExpectedBits()
    {
        IntermediateBlock.VoidExtentData data = new()
        {
            R = 0xbeef,
            G = 0xdead,
            B = 0xbeef,
            A = 0xdead,
            Coords = [0, 8191, 0, 8191]
        };

        (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(data);

        Assert.Null(error);
        Assert.Equal(new UInt128(0xdeadbeefdeadbeefUL, 0xFFF8003FFE000DFCUL), packed);
    }

    [Theory]
    [InlineData(0xe8e8eaea20000980UL, 0x20000200cb73f045UL)]
    [InlineData(0x3300c30700cb01c5UL, 0x0573907b8c0f6879UL)]
    public void PackUnpack_WithSameCEM_ShouldRoundTripCorrectly(ulong high, ulong low)
    {
        UInt128 original = new(high, low);
        PhysicalBlock block = PhysicalBlock.Create(original);

        IntermediateBlock.IntermediateBlockData? unpacked = IntermediateBlock.UnpackIntermediateBlock(block);

        Assert.NotNull(unpacked);
        IntermediateBlock.IntermediateBlockData ib = unpacked!.Value;

        (string? error, UInt128 repacked) = IntermediateBlockPacker.Pack(ib);

        Assert.Null(error);
        Assert.Equal(original, repacked);
    }

    [Theory]
    [InlineData(TestImages.Astc.Checkered_4, 4)]
    [InlineData(TestImages.Astc.Checkered_5, 5)]
    [InlineData(TestImages.Astc.Checkered_6, 6)]
    [InlineData(TestImages.Astc.Checkered_7, 7)]
    [InlineData(TestImages.Astc.Checkered_8, 8)]
    [InlineData(TestImages.Astc.Checkered_9, 9)]
    [InlineData(TestImages.Astc.Checkered_10, 10)]
    [InlineData(TestImages.Astc.Checkered_11, 11)]
    [InlineData(TestImages.Astc.Checkered_12, 12)]
    public void PackUnpack_WithTestDataBlocks_ShouldPreserveBlockProperties(string inputFile, int checkeredDim)
    {
        const int astcDim = 8;
        int imgDim = checkeredDim * astcDim;
        byte[] astcData = LoadASTCFile(inputFile);
        int numBlocks = (imgDim / astcDim) * (imgDim / astcDim);

        Assert.Equal(0, astcData.Length % PhysicalBlock.SizeInBytes);

        for (int i = 0; i < numBlocks; ++i)
        {
            ReadOnlySpan<byte> slice = new(astcData, i * PhysicalBlock.SizeInBytes, PhysicalBlock.SizeInBytes);
            UInt128 blockBits = new(
                BitConverter.ToUInt64(slice.Slice(8, 8)),
                BitConverter.ToUInt64(slice[..8]));
            PhysicalBlock originalBlock = PhysicalBlock.Create(blockBits);

            // Unpack and repack
            UInt128 repacked;
            if (originalBlock.IsVoidExtent)
            {
                IntermediateBlock.VoidExtentData? voidData = IntermediateBlock.UnpackVoidExtent(originalBlock);
                Assert.NotNull(voidData);

                (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(voidData!.Value);
                Assert.Null(error);
                repacked = packed;
            }
            else
            {
                IntermediateBlock.IntermediateBlockData? intermediateData = IntermediateBlock.UnpackIntermediateBlock(originalBlock);
                Assert.NotNull(intermediateData);
                IntermediateBlock.IntermediateBlockData ibData = intermediateData!.Value;

                // Verify endpoint range was set
                Assert.Equal(originalBlock.GetColorValuesRange(), ibData.EndpointRange);

                // Clear endpoint range before repacking (to test calculation)
                ibData.EndpointRange = null;
                (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(ibData);
                Assert.Null(error);
                repacked = packed;
            }

            // Verify repacked block
            PhysicalBlock repackedBlock = PhysicalBlock.Create(repacked);
            VerifyBlockPropertiesMatch(repackedBlock, originalBlock);
        }
    }

    private static void VerifyBlockPropertiesMatch(PhysicalBlock repacked, PhysicalBlock original)
    {
        Assert.False(repacked.IsIllegalEncoding);

        // Verify color bits match
        int repackedColorBitCount = repacked.GetColorBitCount() ?? 0;
        UInt128 repackedColorMask = UInt128Extensions.OnesMask(repackedColorBitCount);
        UInt128 repackedColorBits = (repacked.BlockBits >> (repacked.GetColorStartBit() ?? 0)) & repackedColorMask;

        int originalColorBitCount = original.GetColorBitCount() ?? 0;
        UInt128 originalColorMask = UInt128Extensions.OnesMask(originalColorBitCount);
        UInt128 originalColorBits = (original.BlockBits >> (original.GetColorStartBit() ?? 0)) & originalColorMask;

        Assert.Equal(originalColorMask, repackedColorMask);
        Assert.Equal(originalColorBits, repackedColorBits);

        // Verify void extent properties
        Assert.Equal(original.IsVoidExtent, repacked.IsVoidExtent);
        Assert.Equal(original.GetVoidExtentCoordinates(), repacked.GetVoidExtentCoordinates());

        // Verify weight properties
        Assert.Equal(original.GetWeightGridDimensions(), repacked.GetWeightGridDimensions());
        Assert.Equal(original.GetWeightRange(), repacked.GetWeightRange());
        Assert.Equal(original.GetWeightBitCount(), repacked.GetWeightBitCount());
        Assert.Equal(original.GetWeightStartBit(), repacked.GetWeightStartBit());

        // Verify dual plane properties
        Assert.Equal(original.IsDualPlane, repacked.IsDualPlane);
        Assert.Equal(original.GetDualPlaneChannel(), repacked.GetDualPlaneChannel());

        // Verify partition properties
        Assert.Equal(original.GetPartitionsCount(), repacked.GetPartitionsCount());
        Assert.Equal(original.GetPartitionId(), repacked.GetPartitionId());

        // Verify color value properties
        Assert.Equal(original.GetColorValuesCount(), repacked.GetColorValuesCount());
        Assert.Equal(original.GetColorValuesRange(), repacked.GetColorValuesRange());

        // Verify endpoint modes for all partitions
        int numParts = repacked.GetPartitionsCount().GetValueOrDefault(0);
        for (int j = 0; j < numParts; ++j)
        {
            Assert.True(repacked.GetEndpointMode(j) == original.GetEndpointMode(j), $"Endpoint mode mismatch at partition {j}");
        }
    }

    private static byte[] LoadASTCFile(string inputFile)
    {
        string filename = TestFile.GetInputFileFullPath(Path.Combine("Astc", inputFile));
        Assert.True(File.Exists(filename), $"Testdata missing: {filename}");
        byte[] data = File.ReadAllBytes(filename);
        Assert.True(data.Length >= 16, "ASTC file too small");
        return [.. data.Skip(16)];
    }
}
