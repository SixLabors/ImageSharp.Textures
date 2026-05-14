// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

// Per-mode pinned decoding tests for the LDR endpoint modes defined in ASTC
// spec §C.2.14 (Color Endpoint Decoding).
// Inputs are chosen to exercise each mode's distinct branches (blue-contract swap,
// base+offset/underflow, base+scale scaling).
public class EndpointCodecLdrTests
{
    private static (Rgba32 Low, Rgba32 High) Decode(ColorEndpointMode mode, params int[] unquantized)
    {
        ColorEndpointPair pair = EndpointCodec.Decode(unquantized, mode);
        return (pair.LdrLow, pair.LdrHigh);
    }

    // ---- Mode 0: LdrLumaDirect (spec §C.2.14 — "Direct luminance") ----

    [Fact]
    public void Decode_LdrLumaDirect_ProducesGrayscaleWithFullAlpha()
    {
        // v0 → low luminance, v1 → high luminance. Alpha defaults to 255.
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrLumaDirect, 0x20, 0xE0);

        Assert.Equal(new Rgba32(0x20, 0x20, 0x20, 255), low);
        Assert.Equal(new Rgba32(0xE0, 0xE0, 0xE0, 255), high);
    }

    // ---- Mode 1: LdrLumaBaseOffset (spec §C.2.14 — "Luminance, base+offset") ----

    [Fact]
    public void Decode_LdrLumaBaseOffset_DecodesBaseAndOffset()
    {
        // L0 = (v0 >> 2) | (v1 & 0xC0); L1 = L0 + (v1 & 0x3F), saturated at 0xFF.
        int v0 = 0x80;
        int v1 = 0x6F;
        int l0 = (v0 >> 2) | (v1 & 0xC0);
        int l1 = Math.Min(l0 + (v1 & 0x3F), 0xFF);

        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrLumaBaseOffset, v0, v1);

        Assert.Equal(new Rgba32((byte)l0, (byte)l0, (byte)l0, 255), low);
        Assert.Equal(new Rgba32((byte)l1, (byte)l1, (byte)l1, 255), high);
    }

    [Fact]
    public void Decode_LdrLumaBaseOffset_SaturatesOffsetAtFF()
    {
        // Choose v1 so L0 + offset > 0xFF.
        (Rgba32 _, Rgba32 high) = Decode(ColorEndpointMode.LdrLumaBaseOffset, 0xFF, 0xFF);

        Assert.Equal(255, high.R);
    }

    // ---- Mode 4: LdrLumaAlphaDirect (spec §C.2.14 — "Luminance+alpha, direct") ----

    [Fact]
    public void Decode_LdrLumaAlphaDirect_DecodesLumaAndAlphaIndependently()
    {
        // v0/v1 → low/high luma; v2/v3 → low/high alpha.
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrLumaAlphaDirect, 0x10, 0xF0, 0x40, 0xC0);

        Assert.Equal(new Rgba32(0x10, 0x10, 0x10, 0x40), low);
        Assert.Equal(new Rgba32(0xF0, 0xF0, 0xF0, 0xC0), high);
    }

    // ---- Mode 5: LdrLumaAlphaBaseOffset (spec §C.2.14) ----

    [Fact]
    public void Decode_LdrLumaAlphaBaseOffset_DecodesTransferPrecisionPairs()
    {
        // TransferPrecision unpacks each (high, low) pair into (offset, base).
        (int b0, int a0) = BitOperations.TransferPrecision(0x30, 0x80);
        (int b2, int a2) = BitOperations.TransferPrecision(0x10, 0x40);

        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrLumaAlphaBaseOffset, 0x80, 0x30, 0x40, 0x10);

        Assert.Equal(new Rgba32((byte)a0, (byte)a0, (byte)a0, (byte)a2), low);
        int highLuma = Math.Clamp(a0 + b0, 0, 255);
        int highAlpha = Math.Clamp(a2 + b2, 0, 255);
        Assert.Equal(new Rgba32((byte)highLuma, (byte)highLuma, (byte)highLuma, (byte)highAlpha), high);
    }

    // ---- Mode 6: LdrRgbBaseScale (spec §C.2.14 — "RGB, base+scale") ----

    [Fact]
    public void Decode_LdrRgbBaseScale_LowIsScaledHigh()
    {
        // low = (v0,v1,v2) * v3 >> 8 ; high = (v0,v1,v2). Alpha = 255 on both.
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbBaseScale, 0xFF, 0x80, 0x40, 0x80);

        Assert.Equal(new Rgba32((byte)((0xFF * 0x80) >> 8), (byte)((0x80 * 0x80) >> 8), (byte)((0x40 * 0x80) >> 8), 255), low);
        Assert.Equal(new Rgba32(0xFF, 0x80, 0x40, 255), high);
    }

    // ---- Mode 8: LdrRgbDirect (spec §C.2.14) with blue-contract swap ----

    [Fact]
    public void Decode_LdrRgbDirect_WhenHighIsDimmer_SwapsEndpointsAndAveragesBlue()
    {
        // sum1 (high) < sum0 (low) triggers blue-contract swap.
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbDirect, 0xC0, 0x20, 0xC0, 0x20, 0xC0, 0x20);

        // Swapped: low uses odd-indexed (high-side) values with blue-contract averaging.
        Assert.Equal(new Rgba32((byte)((0x20 + 0x20) >> 1), (byte)((0x20 + 0x20) >> 1), 0x20, 255), low);
        Assert.Equal(new Rgba32((byte)((0xC0 + 0xC0) >> 1), (byte)((0xC0 + 0xC0) >> 1), 0xC0, 255), high);
    }

    [Fact]
    public void Decode_LdrRgbDirect_WhenHighIsBrighter_KeepsDirectValues()
    {
        // sum1 (high) >= sum0 (low) → no swap.
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbDirect, 0x20, 0xC0, 0x20, 0xC0, 0x20, 0xC0);

        Assert.Equal(new Rgba32(0x20, 0x20, 0x20, 255), low);
        Assert.Equal(new Rgba32(0xC0, 0xC0, 0xC0, 255), high);
    }

    // ---- Mode 9: LdrRgbBaseOffset (spec §C.2.14 — with blue-contract) ----

    [Fact]
    public void Decode_LdrRgbBaseOffset_NonNegativeSum_ProducesBasePlusOffset()
    {
        // b0+b1+b2 >= 0 → low = base, high = base + offset.
        (int b0, int a0) = BitOperations.TransferPrecision(0x10, 0x80);
        (int b1, int a1) = BitOperations.TransferPrecision(0x08, 0x40);
        (int b2, int a2) = BitOperations.TransferPrecision(0x04, 0x20);

        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbBaseOffset, 0x80, 0x10, 0x40, 0x08, 0x20, 0x04);

        Assert.Equal(new Rgba32((byte)a0, (byte)a1, (byte)a2, 255), low);
        int hr = Math.Clamp(a0 + b0, 0, 255);
        int hg = Math.Clamp(a1 + b1, 0, 255);
        int hb = Math.Clamp(a2 + b2, 0, 255);
        Assert.Equal(new Rgba32((byte)hr, (byte)hg, (byte)hb, 255), high);
    }

    // ---- Mode 10: LdrRgbBaseScaleTwoA (spec §C.2.14 — base+scale with separate alpha) ----

    [Fact]
    public void Decode_LdrRgbBaseScaleTwoA_AppliesScaleAndSeparateAlphaChannels()
    {
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbBaseScaleTwoA, 0xFF, 0x80, 0x40, 0x80, 0x20, 0xE0);

        Assert.Equal(new Rgba32((byte)((0xFF * 0x80) >> 8), (byte)((0x80 * 0x80) >> 8), (byte)((0x40 * 0x80) >> 8), 0x20), low);
        Assert.Equal(new Rgba32(0xFF, 0x80, 0x40, 0xE0), high);
    }

    // ---- Mode 12: LdrRgbaDirect (spec §C.2.14) ----

    [Fact]
    public void Decode_LdrRgbaDirect_WhenHighIsBrighter_KeepsDirectValues()
    {
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbaDirect, 0x20, 0xC0, 0x20, 0xC0, 0x20, 0xC0, 0x30, 0xB0);

        Assert.Equal(new Rgba32(0x20, 0x20, 0x20, 0x30), low);
        Assert.Equal(new Rgba32(0xC0, 0xC0, 0xC0, 0xB0), high);
    }

    [Fact]
    public void Decode_LdrRgbaDirect_WhenHighIsDimmer_AppliesBlueContractAndSwaps()
    {
        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbaDirect, 0xC0, 0x20, 0xC0, 0x20, 0xC0, 0x20, 0x30, 0xB0);

        // Blue-contract swap: alpha indexes (6,7) swap too — low gets v7, high gets v6.
        Assert.Equal(new Rgba32((byte)((0x20 + 0x20) >> 1), (byte)((0x20 + 0x20) >> 1), 0x20, 0xB0), low);
        Assert.Equal(new Rgba32((byte)((0xC0 + 0xC0) >> 1), (byte)((0xC0 + 0xC0) >> 1), 0xC0, 0x30), high);
    }

    // ---- Mode 13: LdrRgbaBaseOffset (spec §C.2.14) ----

    [Fact]
    public void Decode_LdrRgbaBaseOffset_DecodesAllFourChannelsWithTransferPrecision()
    {
        (int b0, int a0) = BitOperations.TransferPrecision(0x10, 0x80);
        (int b1, int a1) = BitOperations.TransferPrecision(0x08, 0x40);
        (int b2, int a2) = BitOperations.TransferPrecision(0x04, 0x20);
        (int b3, int a3) = BitOperations.TransferPrecision(0x02, 0xC0);

        (Rgba32 low, Rgba32 high) = Decode(ColorEndpointMode.LdrRgbaBaseOffset, 0x80, 0x10, 0x40, 0x08, 0x20, 0x04, 0xC0, 0x02);

        Assert.Equal(new Rgba32((byte)a0, (byte)a1, (byte)a2, (byte)a3), low);
        int hr = Math.Clamp(a0 + b0, 0, 255);
        int hg = Math.Clamp(a1 + b1, 0, 255);
        int hb = Math.Clamp(a2 + b2, 0, 255);
        int ha = Math.Clamp(a3 + b3, 0, 255);
        Assert.Equal(new Rgba32((byte)hr, (byte)hg, (byte)hb, (byte)ha), high);
    }
}
