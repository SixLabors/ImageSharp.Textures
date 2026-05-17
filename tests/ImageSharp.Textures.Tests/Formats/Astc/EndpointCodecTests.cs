// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class EndpointCodecTests
{
    internal static TheoryData<Rgba32, Rgba32> RgbBaseOffsetColorPairs() => new()
    {
        { new Rgba32(80, 16, 112, 255), new Rgba32(87, 18, 132, 255) },
        { new Rgba32(80, 74, 82, 255), new Rgba32(90, 92, 110, 255) },
        { new Rgba32(0, 0, 0, 255), new Rgba32(2, 2, 2, 255) },
    };

    [Theory]
#pragma warning disable xUnit1016 // MemberData is internal because Rgba32Extensions are internal
    [MemberData(nameof(RgbBaseOffsetColorPairs))]
#pragma warning restore xUnit1016
    internal void DecodeColorsForMode_WithRgbBaseOffset_AndSpecificColorPairs_ShouldDecodeCorrectly(
        Rgba32 expectedLow, Rgba32 expectedHigh)
    {
        Span<int> values = stackalloc int[6];
        EncodeRgbBaseOffset(expectedLow, expectedHigh, values);
        Quantization.UnquantizeCEValuesBatch(values, 255);
        ColorEndpointPair decoded = EndpointCodec.Decode(values, ColorEndpointMode.LdrRgbBaseOffset);

        Assert.True(decoded.LdrLow == expectedLow);
        Assert.True(decoded.LdrHigh == expectedHigh);
    }

    [Fact]
    public void DecodeColorsForMode_WithRgbBaseOffset_AndIdenticalColors_ShouldDecodeCorrectly()
    {
        Random random = new(unchecked((int)0xdeadbeef));

        for (int i = 0; i < 100; ++i)
        {
            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);

            // Ensure even channels (reference test skips odd)
            if (((r | g | b) & 1) != 0)
            {
                continue;
            }

            Rgba32 color = new((byte)r, (byte)g, (byte)b, 255);
            Span<int> values = stackalloc int[6];
            EncodeRgbBaseOffset(color, color, values);
            Quantization.UnquantizeCEValuesBatch(values, 255);
            ColorEndpointPair decoded = EndpointCodec.Decode(values, ColorEndpointMode.LdrRgbBaseOffset);

            Assert.True(decoded.LdrLow == color);
            Assert.True(decoded.LdrHigh == color);
        }
    }

    [Fact]
    public void DecodeCheckerboard_ShouldDecodeToGrayscaleEndpoints()
    {
        string astcFilePath = TestFile.GetInputFileFullPath(Path.Combine("Astc", TestData.Astc.Checkerboard));
        byte[] astcData = File.ReadAllBytes(astcFilePath);

        int blocksDecoded = 0;

        for (int i = 0; i < astcData.Length; i += BlockInfo.SizeInBytes)
        {
            UInt128 blockBits = BinaryPrimitives.ReadUInt128LittleEndian(astcData.AsSpan(i, BlockInfo.SizeInBytes));
            BlockInfo info = BlockModeDecoder.Decode(blockBits);
            Assert.True(info.IsValid);
            Assert.False(info.IsVoidExtent);
            Assert.True(info.PartitionCount > 0, "block should have endpoints");

            Span<int> colors = stackalloc int[info.Colors.Count];
            FusedBlockDecoder.DecodeBiseValues(
                blockBits,
                info.Colors.StartBit,
                info.Colors.BitCount,
                info.Colors.Range,
                info.Colors.Count,
                colors);
            Quantization.UnquantizeCEValuesBatch(colors, info.Colors.Range);

            // The checkerboard content is LDR but the encoder happens to emit HDR luma
            // endpoint modes for it, so the test must go through the polymorphic decoder
            // and assert on both LDR and HDR pairs.
            int colorIndex = 0;
            for (int ep = 0; ep < info.PartitionCount; ep++)
            {
                ColorEndpointMode mode = info.GetEndpointMode(ep);
                int colorCount = mode.GetColorValuesCount();
                ReadOnlySpan<int> slice = ((ReadOnlySpan<int>)colors).Slice(colorIndex, colorCount);
                ColorEndpointPair pair = EndpointCodec.Decode(slice, mode);
                colorIndex += colorCount;

                if (pair.IsHdr)
                {
                    Assert.True(pair.HdrLow.R == pair.HdrLow.G, $"block {i} low endpoint should be grayscale");
                    Assert.True(pair.HdrLow.G == pair.HdrLow.B, $"block {i} low endpoint should be grayscale");
                    Assert.True(pair.HdrHigh.R == pair.HdrHigh.G, $"block {i} high endpoint should be grayscale");
                    Assert.True(pair.HdrHigh.G == pair.HdrHigh.B, $"block {i} high endpoint should be grayscale");
                }
                else
                {
                    Assert.True(pair.LdrLow.R == pair.LdrLow.G, $"block {i} low endpoint should be grayscale");
                    Assert.True(pair.LdrLow.G == pair.LdrLow.B, $"block {i} low endpoint should be grayscale");
                    Assert.True(pair.LdrHigh.R == pair.LdrHigh.G, $"block {i} high endpoint should be grayscale");
                    Assert.True(pair.LdrHigh.G == pair.LdrHigh.B, $"block {i} high endpoint should be grayscale");
                }
            }

            blocksDecoded++;
        }

        Assert.True(blocksDecoded > 0);
    }

    /// <summary>
    /// Manually encodes an RGB base+offset endpoint pair (ASTC spec §C.2.14 mode 9). Hand-rolled
    /// rather than calling a real encoder so the decoder can be exercised without the encoder
    /// stack being present.
    /// </summary>
    private static void EncodeRgbBaseOffset(Rgba32 low, Rgba32 high, Span<int> values)
    {
        for (int i = 0; i < 3; ++i)
        {
            bool isLarge = low.GetChannel(i) >= 128;
            values[i * 2] = (low.GetChannel(i) * 2) & 0xFF;
            int diff = (high.GetChannel(i) - low.GetChannel(i)) * 2;
            if (isLarge)
            {
                diff |= 0x80;
            }

            values[(i * 2) + 1] = diff;
        }
    }
}
