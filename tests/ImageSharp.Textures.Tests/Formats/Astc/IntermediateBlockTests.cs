// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;

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

        result.Should().BeNull();
    }

    [Fact]
    public void UnpackIntermediateBlock_WithErrorBlock_ShouldReturnNull()
    {
        PhysicalBlock errorBlock = PhysicalBlock.Create(ErrorBlock);

        IntermediateBlock.IntermediateBlockData? result = IntermediateBlock.UnpackIntermediateBlock(errorBlock);

        result.Should().BeNull();
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

        result.Should().Be(-1);
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

        error.Should().NotBeNull();
        error.Should().Contain("Incorrect number of weights");
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

        result.Should().Be(-2);
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

        error.Should().NotBeNull();
        error.Should().Contain("illegal color range");
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

            colorRange.Should().BeLessThanOrEqualTo(lastColorRange);
            lastColorRange = Math.Min(colorRange, lastColorRange);
        }

        lastColorRange.Should().BeLessThan(byte.MaxValue);
    }

    [Fact]
    public void EndpointRange_WithStandardBlock_ShouldBe255()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0x0000000001FE000173UL);

        IntermediateBlock.IntermediateBlockData? data = IntermediateBlock.UnpackIntermediateBlock(block);

        block.GetColorValuesRange().Should().Be(255);
        data.Should().NotBeNull();
        IntermediateBlock.IntermediateBlockData ib = data!.Value;
        ib.EndpointCount.Should().Be(1);
        ib.Endpoints[0].Mode.Should().Be(ColorEndpointMode.LdrLumaDirect);
        ib.Endpoints[0].Colors[0].Should().Be(byte.MinValue);
        ib.Endpoints[0].Colors[1].Should().Be(byte.MaxValue);
        ib.Endpoints[0].ColorCount.Should().Be(2);
        ib.EndpointRange.Should().Be(byte.MaxValue);
    }

    [Fact]
    public void UnpackIntermediateBlock_WithStandardBlock_ShouldReturnCorrectData()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0x0000000001FE000173UL);

        IntermediateBlock.IntermediateBlockData? result = IntermediateBlock.UnpackIntermediateBlock(block);

        result.Should().NotBeNull();
        IntermediateBlock.IntermediateBlockData data = result!.Value;

        data.WeightGridX.Should().Be(6);
        data.WeightGridY.Should().Be(5);
        data.WeightRange.Should().Be(7);
        data.PartitionId.Should().BeNull();
        data.DualPlaneChannel.Should().BeNull();

        data.WeightsCount.Should().Be(30);
        data.Weights.AsSpan(0, data.WeightsCount).ToArray().Should().AllBeEquivalentTo(0);

        data.EndpointCount.Should().Be(1);
        IntermediateBlock.IntermediateEndpointData endpoint = data.Endpoints[0];
        endpoint.Mode.Should().Be(ColorEndpointMode.LdrLumaDirect);
        endpoint.ColorCount.Should().Be(2);
        endpoint.Colors[0].Should().Be(byte.MinValue);
        endpoint.Colors[1].Should().Be(byte.MaxValue);
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

        error.Should().BeNull();
        packed.Should().Be((UInt128)0x0000000001FE000173UL);
    }

    [Fact]
    public void Pack_WithLargeGapInBits_ShouldPreserveOriginalEncoding()
    {
        UInt128 original = new(0xBEDEAD0000000000UL, 0x0000000001FE032EUL);
        PhysicalBlock block = PhysicalBlock.Create(original);
        IntermediateBlock.IntermediateBlockData? data = IntermediateBlock.UnpackIntermediateBlock(block);

        data.Should().NotBeNull();
        IntermediateBlock.IntermediateBlockData intermediate = data!.Value;

        // Check unpacked values
        intermediate.WeightGridX.Should().Be(2);
        intermediate.WeightGridY.Should().Be(3);
        intermediate.WeightRange.Should().Be(15);
        intermediate.PartitionId.Should().BeNull();
        intermediate.DualPlaneChannel.Should().BeNull();
        intermediate.EndpointCount.Should().Be(1);
        intermediate.Endpoints[0].Mode.Should().Be(ColorEndpointMode.LdrLumaDirect);
        intermediate.Endpoints[0].ColorCount.Should().Be(2);
        intermediate.Endpoints[0].Colors[0].Should().Be(255);
        intermediate.Endpoints[0].Colors[1].Should().Be(0);

        // Repack
        (string? error, UInt128 repacked) = IntermediateBlockPacker.Pack(intermediate);

        error.Should().BeNull();
        repacked.Should().Be(original);
    }

    [Fact]
    public void UnpackVoidExtent_WithAllOnesPattern_ShouldReturnZeroColors()
    {
        PhysicalBlock block = PhysicalBlock.Create((UInt128)0xFFFFFFFFFFFFFDFCUL);

        IntermediateBlock.VoidExtentData? result = IntermediateBlock.UnpackVoidExtent(block);

        result.Should().NotBeNull();
        IntermediateBlock.VoidExtentData data = result!.Value;

        data.R.Should().Be(0);
        data.G.Should().Be(0);
        data.B.Should().Be(0);
        data.A.Should().Be(0);

        data.Coords.Should().AllSatisfy(c => c.Should().Be((1 << 13) - 1));
    }

    [Fact]
    public void UnpackVoidExtent_WithColorData_ShouldReturnCorrectColors()
    {
        UInt128 blockBits = new(0xdeadbeefdeadbeefUL, 0xFFF8003FFE000DFCUL);
        PhysicalBlock block = PhysicalBlock.Create(blockBits);

        IntermediateBlock.VoidExtentData? result = IntermediateBlock.UnpackVoidExtent(block);

        result.Should().NotBeNull();
        IntermediateBlock.VoidExtentData data = result!.Value;

        data.R.Should().Be(0xbeef);
        data.G.Should().Be(0xdead);
        data.B.Should().Be(0xbeef);
        data.A.Should().Be(0xdead);

        data.Coords[0].Should().Be(0);
        data.Coords[1].Should().Be(8191);
        data.Coords[2].Should().Be(0);
        data.Coords[3].Should().Be(8191);
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

        error.Should().BeNull();
        packed.Should().Be((UInt128)0xFFFFFFFFFFFFFDFCUL);
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

        error.Should().BeNull();
        packed.Should().Be(new UInt128(0xdeadbeefdeadbeefUL, 0xFFF8003FFE000DFCUL));
    }

    [Theory]
    [InlineData(0xe8e8eaea20000980UL, 0x20000200cb73f045UL)]
    [InlineData(0x3300c30700cb01c5UL, 0x0573907b8c0f6879UL)]
    public void PackUnpack_WithSameCEM_ShouldRoundTripCorrectly(ulong high, ulong low)
    {
        UInt128 original = new(high, low);
        PhysicalBlock block = PhysicalBlock.Create(original);

        IntermediateBlock.IntermediateBlockData? unpacked = IntermediateBlock.UnpackIntermediateBlock(block);

        unpacked.Should().NotBeNull();
        IntermediateBlock.IntermediateBlockData ib = unpacked!.Value;

        (string? error, UInt128 repacked) = IntermediateBlockPacker.Pack(ib);

        error.Should().BeNull();
        repacked.Should().Be(original);
    }

    [Theory]
    [InlineData("checkered_4", 4)]
    [InlineData("checkered_5", 5)]
    [InlineData("checkered_6", 6)]
    [InlineData("checkered_7", 7)]
    [InlineData("checkered_8", 8)]
    [InlineData("checkered_9", 9)]
    [InlineData("checkered_10", 10)]
    [InlineData("checkered_11", 11)]
    [InlineData("checkered_12", 12)]
    public void PackUnpack_WithTestDataBlocks_ShouldPreserveBlockProperties(string imageName, int checkeredDim)
    {
        const int astcDim = 8;
        int imgDim = checkeredDim * astcDim;
        byte[] astcData = LoadASTCFile(imageName);
        int numBlocks = (imgDim / astcDim) * (imgDim / astcDim);

        (astcData.Length % PhysicalBlock.SizeInBytes).Should().Be(0);

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
                voidData.Should().NotBeNull();

                (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(voidData!.Value);
                error.Should().BeNull();
                repacked = packed;
            }
            else
            {
                IntermediateBlock.IntermediateBlockData? intermediateData = IntermediateBlock.UnpackIntermediateBlock(originalBlock);
                intermediateData.Should().NotBeNull();
                IntermediateBlock.IntermediateBlockData ibData = intermediateData!.Value;

                // Verify endpoint range was set
                ibData.EndpointRange.Should().Be(originalBlock.GetColorValuesRange());

                // Clear endpoint range before repacking (to test calculation)
                ibData.EndpointRange = null;
                (string? error, UInt128 packed) = IntermediateBlockPacker.Pack(ibData);
                error.Should().BeNull();
                repacked = packed;
            }

            // Verify repacked block
            PhysicalBlock repackedBlock = PhysicalBlock.Create(repacked);
            VerifyBlockPropertiesMatch(repackedBlock, originalBlock);
        }
    }

    private static void VerifyBlockPropertiesMatch(PhysicalBlock repacked, PhysicalBlock original)
    {
        repacked.IsIllegalEncoding.Should().BeFalse();

        // Verify color bits match
        int repackedColorBitCount = repacked.GetColorBitCount().Value;
        UInt128 repackedColorMask = UInt128Extensions.OnesMask(repackedColorBitCount);
        UInt128 repackedColorBits = (repacked.BlockBits >> repacked.GetColorStartBit().Value) & repackedColorMask;

        int originalColorBitCount = original.GetColorBitCount().Value;
        UInt128 originalColorMask = UInt128Extensions.OnesMask(originalColorBitCount);
        UInt128 originalColorBits = (original.BlockBits >> original.GetColorStartBit().Value) & originalColorMask;

        repackedColorMask.Should().Be(originalColorMask);
        repackedColorBits.Should().Be(originalColorBits);

        // Verify void extent properties
        repacked.IsVoidExtent.Should().Be(original.IsVoidExtent);
        repacked.GetVoidExtentCoordinates().Should().Equal(original.GetVoidExtentCoordinates());

        // Verify weight properties
        repacked.GetWeightGridDimensions().Should().Be(original.GetWeightGridDimensions());
        repacked.GetWeightRange().Should().Be(original.GetWeightRange());
        repacked.GetWeightBitCount().Should().Be(original.GetWeightBitCount());
        repacked.GetWeightStartBit().Should().Be(original.GetWeightStartBit());

        // Verify dual plane properties
        repacked.IsDualPlane.Should().Be(original.IsDualPlane);
        repacked.GetDualPlaneChannel().Should().Be(original.GetDualPlaneChannel());

        // Verify partition properties
        repacked.GetPartitionsCount().Should().Be(original.GetPartitionsCount());
        repacked.GetPartitionId().Should().Be(original.GetPartitionId());

        // Verify color value properties
        repacked.GetColorValuesCount().Should().Be(original.GetColorValuesCount());
        repacked.GetColorValuesRange().Should().Be(original.GetColorValuesRange());

        // Verify endpoint modes for all partitions
        int numParts = repacked.GetPartitionsCount().GetValueOrDefault(0);
        for (int j = 0; j < numParts; ++j)
        {
            repacked.GetEndpointMode(j).Should().Be(original.GetEndpointMode(j));
        }
    }

    private static byte[] LoadASTCFile(string basename)
    {
        string filename = TestFile.GetInputFileFullPath(Path.Combine(TestImages.Astc.InputFolder, basename + ".astc"));
        File.Exists(filename).Should().BeTrue($"Testdata missing: {filename}");
        byte[] data = File.ReadAllBytes(filename);
        data.Length.Should().BeGreaterThanOrEqualTo(16, "ASTC file too small");
        return [.. data.Skip(16)];
    }
}
