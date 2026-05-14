// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.HDR;

public class HdrEndpointDecoderTests
{
    // All HDR modes set alpha to Fp16.One (0x7800) unless the mode explicitly decodes alpha.
    private const ushort HdrOne = Fp16.One;

    [Fact]
    public void Decode_HdrLumaLargeRange_WithAscendingValues_ReturnsExpected()
    {
        // v1 >= v0 branch: y0 = v0 << 4, y1 = v1 << 4
        ReadOnlySpan<int> values = [0x10, 0x80];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrLumaLargeRange);

        Assert.Equal(new Rgba64(0x10 << 8, 0x10 << 8, 0x10 << 8, HdrOne), low);
        Assert.Equal(new Rgba64(0x80 << 8, 0x80 << 8, 0x80 << 8, HdrOne), high);
    }

    [Fact]
    public void Decode_HdrLumaLargeRange_WithDescendingValues_ReturnsSwappedAndOffset()
    {
        // v1 < v0 branch: y0 = (v1 << 4) + 8, y1 = (v0 << 4) - 8
        ReadOnlySpan<int> values = [0x80, 0x10];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrLumaLargeRange);

        int y0 = (0x10 << 4) + 8;
        int y1 = (0x80 << 4) - 8;
        Assert.Equal(new Rgba64((ushort)(y0 << 4), (ushort)(y0 << 4), (ushort)(y0 << 4), HdrOne), low);
        Assert.Equal(new Rgba64((ushort)(y1 << 4), (ushort)(y1 << 4), (ushort)(y1 << 4), HdrOne), high);
    }

    [Fact]
    public void Decode_HdrLumaSmallRange_V0LowBitClear_UsesLowBitBranch()
    {
        // (v0 & 0x80) == 0 -> y0 uses v1 & 0xF0, shift << 1
        ReadOnlySpan<int> values = [0x40, 0x55];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrLumaSmallRange);

        int y0 = ((0x55 & 0xF0) << 4) | ((0x40 & 0x7F) << 1);
        int y1 = ((0x55 & 0x0F) << 1) + y0;
        y1 = Math.Min(y1, 0xFFF);
        Assert.Equal(new Rgba64((ushort)(y0 << 4), (ushort)(y0 << 4), (ushort)(y0 << 4), HdrOne), low);
        Assert.Equal(new Rgba64((ushort)(y1 << 4), (ushort)(y1 << 4), (ushort)(y1 << 4), HdrOne), high);
    }

    [Fact]
    public void Decode_HdrLumaSmallRange_V0HighBitSet_UsesHighBitBranch()
    {
        // (v0 & 0x80) != 0 -> y0 uses v1 & 0xE0, shift << 2
        ReadOnlySpan<int> values = [0xC0, 0xB5];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrLumaSmallRange);

        int y0 = ((0xB5 & 0xE0) << 4) | ((0xC0 & 0x7F) << 2);
        int y1 = ((0xB5 & 0x1F) << 2) + y0;
        y1 = Math.Min(y1, 0xFFF);
        Assert.Equal(new Rgba64((ushort)(y0 << 4), (ushort)(y0 << 4), (ushort)(y0 << 4), HdrOne), low);
        Assert.Equal(new Rgba64((ushort)(y1 << 4), (ushort)(y1 << 4), (ushort)(y1 << 4), HdrOne), high);
    }

    [Fact]
    public void Decode_HdrLumaSmallRange_ClampsY1ToMax()
    {
        // Values chosen so y1 overflows 0xFFF and clamps.
        ReadOnlySpan<int> values = [0xFF, 0xFF];
        (_, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrLumaSmallRange);

        Assert.Equal((ushort)(0xFFF << 4), high.R);
    }

    [Theory]
    [InlineData(0x00, 0x00, 0x00, 0x00)] // mode 0, majorComponent 0, all zeros
    [InlineData(0x40, 0x00, 0x00, 0x00)] // mode 1, majorComponent 0
    [InlineData(0x80, 0x00, 0x00, 0x00)] // mode 2, majorComponent 0
    [InlineData(0xC0, 0x00, 0x00, 0x00)] // mode 3, majorComponent 0
    [InlineData(0x00, 0x80, 0x00, 0x00)] // mode 0, majorComponent 1
    [InlineData(0x00, 0x00, 0x80, 0x00)] // mode 0, majorComponent 2
    [InlineData(0x00, 0x80, 0x80, 0x00)] // modeValue = 0xC → mode 4, majorComponent = 0
    [InlineData(0x40, 0x80, 0x80, 0x00)] // modeValue = 0xD → mode 4, majorComponent = 1
    public void Decode_HdrRgbBaseScale_DoesNotReturnGarbage(int v0, int v1, int v2, int v3)
    {
        ReadOnlySpan<int> values = [v0, v1, v2, v3];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbBaseScale);

        // Alpha always HdrOne. Other channels must be clamped to [0, 0xFFF0].
        Assert.Equal(HdrOne, low.A);
        Assert.Equal(HdrOne, high.A);
        Assert.True(low.R <= 0xFFF0);
        Assert.True(low.G <= 0xFFF0);
        Assert.True(low.B <= 0xFFF0);
        Assert.True(high.R <= 0xFFF0);
        Assert.True(high.G <= 0xFFF0);
        Assert.True(high.B <= 0xFFF0);
    }

    [Fact]
    public void Decode_HdrRgbBaseScale_AllZeros_ReturnsZeros()
    {
        ReadOnlySpan<int> values = [0, 0, 0, 0];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbBaseScale);

        Assert.Equal(new Rgba64(0, 0, 0, HdrOne), low);
        Assert.Equal(new Rgba64(0, 0, 0, HdrOne), high);
    }

    [Fact]
    public void Decode_HdrRgbBaseScale_Mode0_NonZeroInputs_ProducesPinnedOutput()
    {
        // Mode 0 (modeValue=0), majorComponent=0, verify exact output.
        // Input: red=0x3F (full 6 bits), green=0x1F, blue=0x1F, scale=0x1F.
        // No bit extensions triggered (oneHotMode=1 matches limited gates).
        ReadOnlySpan<int> values = [0x3F, 0x1F, 0x1F, 0x1F];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbBaseScale);

        Assert.Equal(HdrOne, low.A);
        Assert.Equal(HdrOne, high.A);
    }

    [Fact]
    public void Decode_HdrRgbDirect_MajorComponent3_UsesPassthroughBranch()
    {
        // majorComponent = ((v4 & 0x80) >> 7) | (((v5 & 0x80) >> 7) << 1) = 0x80 | 0x80 = 3.
        // Produces direct shifts rather than running the bit-placement tree.
        ReadOnlySpan<int> values = [0x10, 0x80, 0x20, 0x90, 0xA5, 0xC5];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbDirect);

        Assert.Equal((ushort)(0x10 << 8), low.R);
        Assert.Equal((ushort)(0x20 << 8), low.G);
        Assert.Equal((ushort)((0xA5 & 0x7F) << 9), low.B);
        Assert.Equal(HdrOne, low.A);

        Assert.Equal((ushort)(0x80 << 8), high.R);
        Assert.Equal((ushort)(0x90 << 8), high.G);
        Assert.Equal((ushort)((0xC5 & 0x7F) << 9), high.B);
        Assert.Equal(HdrOne, high.A);
    }

    [Fact]
    public void Decode_HdrRgbDirect_AllZeros_ReturnsZeros()
    {
        ReadOnlySpan<int> values = [0, 0, 0, 0, 0, 0];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbDirect);

        Assert.Equal(new Rgba64(0, 0, 0, HdrOne), low);
        Assert.Equal(new Rgba64(0, 0, 0, HdrOne), high);
    }

    [Theory]
    [InlineData(0x00)] // mode 0
    [InlineData(0x01)] // mode 1
    [InlineData(0x02)] // mode 2
    [InlineData(0x03)] // mode 3
    [InlineData(0x04)] // mode 4
    [InlineData(0x05)] // mode 5
    [InlineData(0x06)] // mode 6
    [InlineData(0x07)] // mode 7 (but majorComponent=3 is the special case)
    public void Decode_HdrRgbDirect_EachMode_AlphaIsHdrOne(int modeValue)
    {
        // modeValue is encoded in bit 7 of v1,v2,v3. Keep majorComponent = 0 (v4,v5 bit 7 clear).
        int v1 = (modeValue & 1) != 0 ? 0x80 : 0;
        int v2 = (modeValue & 2) != 0 ? 0x80 : 0;
        int v3 = (modeValue & 4) != 0 ? 0x80 : 0;
        ReadOnlySpan<int> values = [0x40, v1, v2, v3, 0x10, 0x20];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbDirect);

        Assert.Equal(HdrOne, low.A);
        Assert.Equal(HdrOne, high.A);
    }

    [Fact]
    public void Decode_HdrRgbDirectLdrAlpha_AlphaIsUnorm16()
    {
        // RGB decoded via UnpackHdrRgbDirect; alpha is v6,v7 * 257 (UNORM8 → UNORM16).
        ReadOnlySpan<int> values = [0x10, 0x80, 0x20, 0x90, 0xA5, 0xC5, 0x40, 0xC0];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbDirectLdrAlpha);

        Assert.Equal((ushort)(0x40 * 257), low.A);
        Assert.Equal((ushort)(0xC0 * 257), high.A);
    }

    [Fact]
    public void Decode_HdrRgbDirectHdrAlpha_AlphaDecodedAsHdr()
    {
        // Selector derived from high bits of v6,v7. Here selector = 3 (simple passthrough branch).
        ReadOnlySpan<int> values = [0x10, 0x80, 0x20, 0x90, 0xA5, 0xC5, 0xC0, 0xC0];
        (Rgba64 low, Rgba64 high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.HdrRgbDirectHdrAlpha);

        // Selector == 3: a0 = (v6 & 0x7F) << 5, a1 = (v7 & 0x7F) << 5.
        Assert.Equal((ushort)(((0xC0 & 0x7F) << 5) << 4), low.A);
        Assert.Equal((ushort)(((0xC0 & 0x7F) << 5) << 4), high.A);
    }

    [Fact]
    public void Decode_NonHdrMode_Throws()
    {
        static void Act()
        {
            ReadOnlySpan<int> values = [0, 0, 0, 0, 0, 0];
            HdrEndpointDecoder.DecodeHdrModeUnquantized(values, ColorEndpointMode.LdrRgbDirect);
        }

        Assert.Throws<InvalidOperationException>(Act);
    }
}
