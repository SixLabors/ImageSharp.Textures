// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using static SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding.Rgba32Extensions;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

internal static class EndpointCodec
{
    /// <summary>
    /// Decodes color endpoints for the specified mode from already-unquantized values.
    /// Handles both LDR and HDR endpoint modes (ASTC spec §C.2.14).
    /// </summary>
    /// <remarks>
    /// Quantized input should be run through <see cref="Quantization.UnquantizeCEValuesBatch"/>
    /// (or <see cref="Quantization.UnquantizeCEValuesArray"/>) first.
    /// </remarks>
    public static ColorEndpointPair Decode(ReadOnlySpan<int> unquantizedValues, ColorEndpointMode mode)
    {
        if (mode.IsHdr())
        {
            (Rgba64 hdrLow, Rgba64 hdrHigh) = HdrEndpointDecoder.DecodeHdrModeUnquantized(unquantizedValues, mode);
            bool alphaIsLdr = mode == ColorEndpointMode.HdrRgbDirectLdrAlpha;
            return ColorEndpointPair.Hdr(hdrLow, hdrHigh, alphaIsLdr);
        }

        (Rgba32 low, Rgba32 high) = mode switch
        {
            ColorEndpointMode.LdrLumaDirect => DecodeLumaDirect(unquantizedValues),
            ColorEndpointMode.LdrLumaBaseOffset => DecodeLumaBaseOffset(unquantizedValues),
            ColorEndpointMode.LdrLumaAlphaDirect => DecodeLumaAlphaDirect(unquantizedValues),
            ColorEndpointMode.LdrLumaAlphaBaseOffset => DecodeLumaAlphaBaseOffset(unquantizedValues),
            ColorEndpointMode.LdrRgbBaseScale => DecodeRgbBaseScale(unquantizedValues),
            ColorEndpointMode.LdrRgbDirect => DecodeRgbDirect(unquantizedValues),
            ColorEndpointMode.LdrRgbBaseOffset => DecodeRgbBaseOffset(unquantizedValues),
            ColorEndpointMode.LdrRgbBaseScaleTwoA => DecodeRgbBaseScaleTwoAlpha(unquantizedValues),
            ColorEndpointMode.LdrRgbaDirect => DecodeRgbaDirect(unquantizedValues),
            ColorEndpointMode.LdrRgbaBaseOffset => DecodeRgbaBaseOffset(unquantizedValues),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown endpoint mode"),
        };

        return ColorEndpointPair.Ldr(low, high);
    }

    // Each decoder below implements one LDR endpoint mode per ASTC spec §C.2.14
    // (Color Endpoint Decoding). Inputs are the unquantized color values for that mode.

    // Mode 0 (§C.2.14 "LDR luminance, direct"): two 8-bit luma values.
    private static (Rgba32 Low, Rgba32 High) DecodeLumaDirect(ReadOnlySpan<int> v)
        => (ClampedRgba32(v[0], v[0], v[0]),
            ClampedRgba32(v[1], v[1], v[1]));

    // Mode 1 (§C.2.14 "LDR luminance, base+offset"): v0 plus the top bits of v1 form the low
    // luma; the bottom six bits of v1 are a saturated offset added to form the high luma.
    private static (Rgba32 Low, Rgba32 High) DecodeLumaBaseOffset(ReadOnlySpan<int> v)
    {
        int l0 = (v[0] >> 2) | (v[1] & 0xC0);
        int l1 = Math.Min(l0 + (v[1] & 0x3F), 0xFF);
        return (ClampedRgba32(l0, l0, l0),
                ClampedRgba32(l1, l1, l1));
    }

    // Mode 4 (§C.2.14 "LDR luminance+alpha, direct"): v0,v1 → luma; v2,v3 → alpha.
    private static (Rgba32 Low, Rgba32 High) DecodeLumaAlphaDirect(ReadOnlySpan<int> v)
        => (ClampedRgba32(v[0], v[0], v[0], v[2]),
            ClampedRgba32(v[1], v[1], v[1], v[3]));

    // Mode 5 (§C.2.14 "LDR luminance+alpha, base+offset"): TransferPrecision unpacks each
    // (high,low) pair into a signed offset b and a base a.
    private static (Rgba32 Low, Rgba32 High) DecodeLumaAlphaBaseOffset(ReadOnlySpan<int> v)
    {
        (int bL, int aL) = BitOperations.TransferPrecision(v[1], v[0]);
        (int bA, int aA) = BitOperations.TransferPrecision(v[3], v[2]);
        int highLuma = aL + bL;
        return (ClampedRgba32(aL, aL, aL, aA),
                ClampedRgba32(highLuma, highLuma, highLuma, aA + bA));
    }

    // Mode 6 (§C.2.14 "LDR RGB, base+scale"): high = (v0,v1,v2); low = high * v3 >> 8.
    private static (Rgba32 Low, Rgba32 High) DecodeRgbBaseScale(ReadOnlySpan<int> v)
    {
        Rgba32 low = ClampedRgba32((v[0] * v[3]) >> 8, (v[1] * v[3]) >> 8, (v[2] * v[3]) >> 8);
        Rgba32 high = ClampedRgba32(v[0], v[1], v[2]);
        return (low, high);
    }

    // Mode 8 (§C.2.14 "LDR RGB, direct"): if the high triple is dimmer than the low triple
    // the endpoints are swapped and the R/G channels are averaged against the B channel
    // ("blue contract" per §C.2.14).
    private static (Rgba32 Low, Rgba32 High) DecodeRgbDirect(ReadOnlySpan<int> v)
    {
        int sumLow = v[0] + v[2] + v[4];
        int sumHigh = v[1] + v[3] + v[5];

        if (sumHigh < sumLow)
        {
            return (ClampedRgba32((v[1] + v[5]) >> 1, (v[3] + v[5]) >> 1, v[5]),
                    ClampedRgba32((v[0] + v[4]) >> 1, (v[2] + v[4]) >> 1, v[4]));
        }

        return (ClampedRgba32(v[0], v[2], v[4]),
                ClampedRgba32(v[1], v[3], v[5]));
    }

    // Mode 9 (§C.2.14 "LDR RGB, base+offset"): per-channel (base, offset). When the sum of
    // offsets is negative the blue-contract branch applies, otherwise low = base and
    // high = base + offset.
    private static (Rgba32 Low, Rgba32 High) DecodeRgbBaseOffset(ReadOnlySpan<int> v)
    {
        (int bR, int aR) = BitOperations.TransferPrecision(v[1], v[0]);
        (int bG, int aG) = BitOperations.TransferPrecision(v[3], v[2]);
        (int bB, int aB) = BitOperations.TransferPrecision(v[5], v[4]);

        if (bR + bG + bB < 0)
        {
            return (ClampedRgba32((aR + bR + aB + bB) >> 1, (aG + bG + aB + bB) >> 1, aB + bB),
                    ClampedRgba32((aR + aB) >> 1, (aG + aB) >> 1, aB));
        }

        return (ClampedRgba32(aR, aG, aB),
                ClampedRgba32(aR + bR, aG + bG, aB + bB));
    }

    // Mode 10 (§C.2.14 "LDR RGB, base+scale plus two alpha values"): same RGB scaling as
    // mode 6, but v4 and v5 carry independent low/high alpha values.
    private static (Rgba32 Low, Rgba32 High) DecodeRgbBaseScaleTwoAlpha(ReadOnlySpan<int> v)
    {
        Rgba32 low = ClampedRgba32(
            r: (v[0] * v[3]) >> 8,
            g: (v[1] * v[3]) >> 8,
            b: (v[2] * v[3]) >> 8,
            a: v[4]);
        Rgba32 high = ClampedRgba32(v[0], v[1], v[2], v[5]);
        return (low, high);
    }

    // Mode 12 (§C.2.14 "LDR RGBA, direct"): like RGB-direct plus alpha. When the high
    // triple is dimmer the endpoints are swapped (RGB via blue-contract, alpha by
    // index-swap).
    private static (Rgba32 Low, Rgba32 High) DecodeRgbaDirect(ReadOnlySpan<int> v)
    {
        int sumLow = v[0] + v[2] + v[4];
        int sumHigh = v[1] + v[3] + v[5];

        if (sumHigh >= sumLow)
        {
            return (ClampedRgba32(v[0], v[2], v[4], v[6]),
                    ClampedRgba32(v[1], v[3], v[5], v[7]));
        }

        return (ClampedRgba32((v[1] + v[5]) >> 1, (v[3] + v[5]) >> 1, v[5], v[7]),
                ClampedRgba32((v[0] + v[4]) >> 1, (v[2] + v[4]) >> 1, v[4], v[6]));
    }

    // Mode 13 (§C.2.14 "LDR RGBA, base+offset"): mode 9 extended with alpha.
    private static (Rgba32 Low, Rgba32 High) DecodeRgbaBaseOffset(ReadOnlySpan<int> v)
    {
        (int bR, int aR) = BitOperations.TransferPrecision(v[1], v[0]);
        (int bG, int aG) = BitOperations.TransferPrecision(v[3], v[2]);
        (int bB, int aB) = BitOperations.TransferPrecision(v[5], v[4]);
        (int bA, int aA) = BitOperations.TransferPrecision(v[7], v[6]);

        if (bR + bG + bB < 0)
        {
            return (ClampedRgba32((aR + bR + aB + bB) >> 1, (aG + bG + aB + bB) >> 1, aB + bB, aA + bA),
                    ClampedRgba32((aR + aB) >> 1, (aG + aB) >> 1, aB, aA));
        }

        return (ClampedRgba32(aR, aG, aB, aA),
                ClampedRgba32(aR + bR, aG + bG, aB + bB, aA + bA));
    }
}
