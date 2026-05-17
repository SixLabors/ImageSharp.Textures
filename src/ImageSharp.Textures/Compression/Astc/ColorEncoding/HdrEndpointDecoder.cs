// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

/// <summary>
/// Decodes HDR (High Dynamic Range) color endpoints for ASTC texture compression.
/// </summary>
/// <remarks>
/// HDR modes produce 12-bit intermediate values (0-4095) which are shifted left by 4
/// to produce the final 16-bit values (0-65520) stored as FP16 bit patterns.
/// </remarks>
internal static class HdrEndpointDecoder
{
    /// <summary>
    /// Output slot index for the HdrRgbBaseScale (CEM 7) bit-placement table.
    /// </summary>
    private enum BaseScaleTarget
    {
        Red,
        Green,
        Blue,
        Scale,
    }

    /// <summary>
    /// Output slot index for the HdrRgbDirect (CEM 11) bit-placement table.
    /// </summary>
    private enum DirectTarget
    {
        A,
        B0,
        B1,
        C,
        D0,
        D1,
    }

    /// <summary>
    /// One row of an HDR bit-placement table. When the current one-hot mode matches
    /// <see cref="ModeMask"/>, the bit at source index <see cref="SourceBit"/> is OR'd into
    /// the output slot at index <see cref="Slot"/>, shifted left by <see cref="TargetShift"/>.
    /// The slot is stored as a plain <c>int</c> so the same row type serves both placement
    /// tables; callers populate it from <see cref="BaseScaleTarget"/> or <see cref="DirectTarget"/>.
    /// </summary>
    private readonly record struct BitPlacement(int Slot, int ModeMask, int SourceBit, int TargetShift);

    // Shift amounts for the HdrRgbBaseScale mode, indexed by the mode selector (0..5).
    // See ARM astcenc_color_unquantize.cpp rgb_hdr_unpack.
#pragma warning disable SA1201 // Readability: keep tables adjacent to the types they use.
    private static readonly int[] BaseScaleShiftByMode = [1, 1, 2, 3, 4, 5];

    // Bit placements for the HdrRgbBaseScale mode (ASTC CEM 7). Each entry represents:
    // "if the current one-hot mode matches ModeMask, OR sourceBits[SourceBit] into Slot at
    // position TargetShift." The table reproduces the if-statement ladder from the ARM
    // reference while making the per-mode pattern directly inspectable.
    private static readonly BitPlacement[] BaseScalePlacements =
    [
        new(Slot: (int)BaseScaleTarget.Green, ModeMask: 0x30, SourceBit: 0, TargetShift: 6),
        new(Slot: (int)BaseScaleTarget.Green, ModeMask: 0x3A, SourceBit: 1, TargetShift: 5),
        new(Slot: (int)BaseScaleTarget.Blue,  ModeMask: 0x30, SourceBit: 2, TargetShift: 6),
        new(Slot: (int)BaseScaleTarget.Blue,  ModeMask: 0x3A, SourceBit: 3, TargetShift: 5),
        new(Slot: (int)BaseScaleTarget.Scale, ModeMask: 0x3D, SourceBit: 6, TargetShift: 5),
        new(Slot: (int)BaseScaleTarget.Scale, ModeMask: 0x2D, SourceBit: 5, TargetShift: 6),
        new(Slot: (int)BaseScaleTarget.Scale, ModeMask: 0x04, SourceBit: 4, TargetShift: 7),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x3B, SourceBit: 4, TargetShift: 6),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x04, SourceBit: 3, TargetShift: 6),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x10, SourceBit: 5, TargetShift: 7),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x0F, SourceBit: 2, TargetShift: 7),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x05, SourceBit: 1, TargetShift: 8),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x0A, SourceBit: 0, TargetShift: 8),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x05, SourceBit: 0, TargetShift: 9),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x02, SourceBit: 6, TargetShift: 9),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x01, SourceBit: 3, TargetShift: 10),
        new(Slot: (int)BaseScaleTarget.Red,   ModeMask: 0x02, SourceBit: 5, TargetShift: 10),
    ];

    // Data-bit widths for the HdrRgbDirect mode (ASTC CEM 11), indexed by modeValue (0..7).
    // Used for sign-extension of the d0/d1 offsets. From ARM reference.
    private static readonly int[] DirectDataBitsByMode = [7, 6, 7, 6, 5, 6, 5, 6];

    // Bit placements for the HdrRgbDirect mode (ASTC CEM 11). Each entry: if the current
    // one-hot modeValue matches ModeMask, OR sourceBits[SourceBit] into Slot at TargetShift.
    // Entries are grouped by Slot (A, C, B0/B1, D0/D1 — see the ARM reference).
    // Pairs like (B0, B1) or (D0, D1) share a single ModeMask in the ARM reference but
    // consume different source bits per slot, so they appear as two entries here.
    private static readonly BitPlacement[] DirectPlacements =
    [
        new(Slot: (int)DirectTarget.A,  ModeMask: 0xA4, SourceBit: 0, TargetShift: 9),
        new(Slot: (int)DirectTarget.A,  ModeMask: 0x08, SourceBit: 2, TargetShift: 9),
        new(Slot: (int)DirectTarget.A,  ModeMask: 0x50, SourceBit: 4, TargetShift: 9),
        new(Slot: (int)DirectTarget.A,  ModeMask: 0x50, SourceBit: 5, TargetShift: 10),
        new(Slot: (int)DirectTarget.A,  ModeMask: 0xA0, SourceBit: 1, TargetShift: 10),
        new(Slot: (int)DirectTarget.A,  ModeMask: 0xC0, SourceBit: 2, TargetShift: 11),
        new(Slot: (int)DirectTarget.C,  ModeMask: 0x04, SourceBit: 1, TargetShift: 6),
        new(Slot: (int)DirectTarget.C,  ModeMask: 0xE8, SourceBit: 3, TargetShift: 6),
        new(Slot: (int)DirectTarget.C,  ModeMask: 0x20, SourceBit: 2, TargetShift: 7),
        new(Slot: (int)DirectTarget.B0, ModeMask: 0x5B, SourceBit: 0, TargetShift: 6),
        new(Slot: (int)DirectTarget.B1, ModeMask: 0x5B, SourceBit: 1, TargetShift: 6),
        new(Slot: (int)DirectTarget.B0, ModeMask: 0x12, SourceBit: 2, TargetShift: 7),
        new(Slot: (int)DirectTarget.B1, ModeMask: 0x12, SourceBit: 3, TargetShift: 7),
        new(Slot: (int)DirectTarget.D0, ModeMask: 0xAF, SourceBit: 4, TargetShift: 5),
        new(Slot: (int)DirectTarget.D1, ModeMask: 0xAF, SourceBit: 5, TargetShift: 5),
        new(Slot: (int)DirectTarget.D0, ModeMask: 0x05, SourceBit: 2, TargetShift: 6),
        new(Slot: (int)DirectTarget.D1, ModeMask: 0x05, SourceBit: 3, TargetShift: 6),
    ];
#pragma warning restore SA1201

    /// <summary>
    /// Applies a mode-gated bit-placement table. For each row, if the current one-hot mode
    /// matches <see cref="BitPlacement.ModeMask"/>, the bit at the row's source index is
    /// OR'd into <paramref name="targets"/>[<c>p.Slot</c>] at the row's target shift.
    /// </summary>
    /// <param name="placements">The table rows to apply (constant per decoder).</param>
    /// <param name="oneHotMode">1 &lt;&lt; modeValue — the one-hot mode selector.</param>
    /// <param name="sourceBits">The per-bit source values extracted from the v-inputs.</param>
    /// <param name="targets">The output slots; each entry is OR'd in place.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyBitPlacements(
        ReadOnlySpan<BitPlacement> placements,
        int oneHotMode,
        ReadOnlySpan<int> sourceBits,
        Span<int> targets)
    {
        foreach (BitPlacement p in placements)
        {
            if ((oneHotMode & p.ModeMask) != 0)
            {
                targets[p.Slot] |= sourceBits[p.SourceBit] << p.TargetShift;
            }
        }
    }

    /// <summary>
    /// Swaps the R/G/B channels of a 12-bit HDR endpoint pair according to
    /// <paramref name="majorComponent"/> (ASTC spec §C.2.14) and shifts each channel left
    /// by 4 to produce the FP16 bit patterns stored in the returned <see cref="Rgba64"/>
    /// pair; alpha is set to <see cref="Fp16.One"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (Rgba64 Low, Rgba64 High) PackHdrRgbPairWithSwap(
        int red0,
        int green0,
        int blue0,
        int red1,
        int green1,
        int blue1,
        int majorComponent)
    {
        (red0, green0, blue0, red1, green1, blue1) = majorComponent switch
        {
            1 => (green0, red0, blue0, green1, red1, blue1),
            2 => (blue0, green0, red0, blue1, green1, red1),
            _ => (red0, green0, blue0, red1, green1, blue1)
        };

        Rgba64 low = new((ushort)(red0 << 4), (ushort)(green0 << 4), (ushort)(blue0 << 4), Fp16.One);
        Rgba64 high = new((ushort)(red1 << 4), (ushort)(green1 << 4), (ushort)(blue1 << 4), Fp16.One);
        return (low, high);
    }

    /// <summary>
    /// Decodes HDR endpoints from already-unquantized values. Called from the fused decode
    /// path where BISE decode + batch unquantize have already been performed.
    /// </summary>
    public static (Rgba64 Low, Rgba64 High) DecodeHdrModeUnquantized(ReadOnlySpan<int> value, ColorEndpointMode mode) => mode switch
    {
        ColorEndpointMode.HdrLumaLargeRange => UnpackHdrLuminanceLargeRangeCore(value[0], value[1]),
        ColorEndpointMode.HdrLumaSmallRange => UnpackHdrLuminanceSmallRangeCore(value[0], value[1]),
        ColorEndpointMode.HdrRgbBaseScale => UnpackHdrRgbBaseScaleCore(value[0], value[1], value[2], value[3]),
        ColorEndpointMode.HdrRgbDirect => UnpackHdrRgbDirectCore(value[0], value[1], value[2], value[3], value[4], value[5]),
        ColorEndpointMode.HdrRgbDirectLdrAlpha => UnpackHdrRgbDirectLdrAlphaCore(value),
        ColorEndpointMode.HdrRgbDirectHdrAlpha => UnpackHdrRgbDirectHdrAlphaCore(value),
        _ => throw new InvalidOperationException($"Mode {mode} is not an HDR mode")
    };

    /// <summary>
    /// Performs an unsigned left shift of a signed value, avoiding undefined behavior
    /// that would occur with signed left shift of negative values.
    /// </summary>
    private static int SafeSignedLeftShift(int value, int shift) => (int)((uint)value << shift);

    /// <summary>
    /// Decodes the HDR luminance large-range endpoint pair (CEM 2) per ASTC spec §C.2.14.
    /// </summary>
    private static (Rgba64 Low, Rgba64 High) UnpackHdrLuminanceLargeRangeCore(int v0, int v1)
    {
        int y0, y1;
        if (v1 >= v0)
        {
            y0 = v0 << 4;
            y1 = v1 << 4;
        }
        else
        {
            y0 = (v1 << 4) + 8;
            y1 = (v0 << 4) - 8;
        }

        Rgba64 low = new((ushort)(y0 << 4), (ushort)(y0 << 4), (ushort)(y0 << 4), Fp16.One);
        Rgba64 high = new((ushort)(y1 << 4), (ushort)(y1 << 4), (ushort)(y1 << 4), Fp16.One);
        return (low, high);
    }

    /// <summary>
    /// Decodes the HDR luminance small-range endpoint pair (CEM 3) per ASTC spec §C.2.14.
    /// </summary>
    private static (Rgba64 Low, Rgba64 High) UnpackHdrLuminanceSmallRangeCore(int v0, int v1)
    {
        int y0, y1;
        if ((v0 & 0x80) != 0)
        {
            y0 = ((v1 & 0xE0) << 4) | ((v0 & 0x7F) << 2);
            y1 = (v1 & 0x1F) << 2;
        }
        else
        {
            y0 = ((v1 & 0xF0) << 4) | ((v0 & 0x7F) << 1);
            y1 = (v1 & 0x0F) << 1;
        }

        y1 += y0;
        if (y1 > 0xFFF)
        {
            y1 = 0xFFF;
        }

        Rgba64 low = new((ushort)(y0 << 4), (ushort)(y0 << 4), (ushort)(y0 << 4), Fp16.One);
        Rgba64 high = new((ushort)(y1 << 4), (ushort)(y1 << 4), (ushort)(y1 << 4), Fp16.One);
        return (low, high);
    }

    /// <summary>
    /// Decodes the HDR RGB base+scale endpoint pair (CEM 7) per ASTC spec §C.2.14. Selects
    /// one of six sub-modes from the high bits of v0/v1/v2, then re-routes individual bits
    /// into the four 12-bit output slots via <see cref="BaseScalePlacements"/> and applies
    /// the major-component channel swap.
    /// </summary>
    private static (Rgba64 Low, Rgba64 High) UnpackHdrRgbBaseScaleCore(int v0, int v1, int v2, int v3)
    {
        int modeValue = ((v0 & 0xC0) >> 6) | (((v1 & 0x80) >> 7) << 2) | (((v2 & 0x80) >> 7) << 3);

        (int majorComponent, int mode) = modeValue switch
        {
            _ when (modeValue & 0xC) != 0xC => (modeValue >> 2, modeValue & 3),
            not 0xF => (modeValue & 3, 4),
            _ => (0, 5)
        };

        // Targets indexed by BaseScaleTarget: [Red, Green, Blue, Scale].
        Span<int> targets =
        [
            v0 & 0x3F,
            v1 & 0x1F,
            v2 & 0x1F,
            v3 & 0x1F,
        ];

        Span<int> sourceBits =
        [
            (v1 >> 6) & 1,
            (v1 >> 5) & 1,
            (v2 >> 6) & 1,
            (v2 >> 5) & 1,
            (v3 >> 7) & 1,
            (v3 >> 6) & 1,
            (v3 >> 5) & 1,
        ];

        ApplyBitPlacements(BaseScalePlacements, oneHotMode: 1 << mode, sourceBits, targets);

        int red = targets[(int)BaseScaleTarget.Red];
        int green = targets[(int)BaseScaleTarget.Green];
        int blue = targets[(int)BaseScaleTarget.Blue];
        int scale = targets[(int)BaseScaleTarget.Scale];

        int shiftAmount = BaseScaleShiftByMode[mode];
        red <<= shiftAmount;
        green <<= shiftAmount;
        blue <<= shiftAmount;
        scale <<= shiftAmount;

        if (mode != 5)
        {
            green = red - green;
            blue = red - blue;
        }

        // Low endpoint = base minus scale; clamp negatives to zero before channel swap.
        int red0 = Math.Max(red - scale, 0);
        int green0 = Math.Max(green - scale, 0);
        int blue0 = Math.Max(blue - scale, 0);
        int red1 = Math.Max(red, 0);
        int green1 = Math.Max(green, 0);
        int blue1 = Math.Max(blue, 0);

        return PackHdrRgbPairWithSwap(red0, green0, blue0, red1, green1, blue1, majorComponent);
    }

    /// <summary>
    /// Decodes the HDR RGB direct endpoint pair (CEM 11) per ASTC spec §C.2.14. Selects
    /// one of eight sub-modes (5 + 3 reserved) from high bits of the v-inputs, then routes
    /// per-bit data through <see cref="DirectPlacements"/>, decodes deltas and majorness,
    /// and produces the channel-swapped 12-bit endpoint pair.
    /// </summary>
    private static (Rgba64 Low, Rgba64 High) UnpackHdrRgbDirectCore(int v0, int v1, int v2, int v3, int v4, int v5)
    {
        int modeValue = ((v1 & 0x80) >> 7) | (((v2 & 0x80) >> 7) << 1) | (((v3 & 0x80) >> 7) << 2);
        int majorComponent = ((v4 & 0x80) >> 7) | (((v5 & 0x80) >> 7) << 1);

        // majorComponent == 3: skip bit-placement tree and use direct passthrough of v0..v5.
        if (majorComponent == 3)
        {
            Rgba64 passthroughLow = new((ushort)(v0 << 8), (ushort)(v2 << 8), (ushort)((v4 & 0x7F) << 9), Fp16.One);
            Rgba64 passthroughHigh = new((ushort)(v1 << 8), (ushort)(v3 << 8), (ushort)((v5 & 0x7F) << 9), Fp16.One);
            return (passthroughLow, passthroughHigh);
        }

        // Targets indexed by DirectTarget: [A, B0, B1, C, D0, D1].
        Span<int> targets =
        [
            v0 | ((v1 & 0x40) << 2),
            v2 & 0x3F,
            v3 & 0x3F,
            v1 & 0x3F,
            v4 & 0x7F,
            v5 & 0x7F,
        ];

        Span<int> sourceBits =
        [
            (v2 >> 6) & 1,
            (v3 >> 6) & 1,
            (v4 >> 6) & 1,
            (v5 >> 6) & 1,
            (v4 >> 5) & 1,
            (v5 >> 5) & 1,
        ];

        ApplyBitPlacements(DirectPlacements, oneHotMode: 1 << modeValue, sourceBits, targets);

        int a = targets[(int)DirectTarget.A];
        int b0 = targets[(int)DirectTarget.B0];
        int b1 = targets[(int)DirectTarget.B1];
        int c = targets[(int)DirectTarget.C];
        int d0 = targets[(int)DirectTarget.D0];
        int d1 = targets[(int)DirectTarget.D1];

        // Sign-extend the signed offsets d0, d1 based on mode-specific data-bit width.
        int dataBits = DirectDataBitsByMode[modeValue];
        int signExtendShift = 32 - dataBits;
        d0 = (d0 << signExtendShift) >> signExtendShift;
        d1 = (d1 << signExtendShift) >> signExtendShift;

        // Expand to 12 bits: per ARM reference, shift amount depends on mode.
        int valueShift = (modeValue >> 1) ^ 3;
        a = SafeSignedLeftShift(a, valueShift);
        b0 = SafeSignedLeftShift(b0, valueShift);
        b1 = SafeSignedLeftShift(b1, valueShift);
        c = SafeSignedLeftShift(c, valueShift);
        d0 = SafeSignedLeftShift(d0, valueShift);
        d1 = SafeSignedLeftShift(d1, valueShift);

        // Compose high and low endpoints per ASTC spec §C.2.14, then clamp to [0, 0xFFF].
        int red1 = Math.Clamp(a, 0, 0xFFF);
        int green1 = Math.Clamp(a - b0, 0, 0xFFF);
        int blue1 = Math.Clamp(a - b1, 0, 0xFFF);
        int red0 = Math.Clamp(a - c, 0, 0xFFF);
        int green0 = Math.Clamp(a - b0 - c - d0, 0, 0xFFF);
        int blue0 = Math.Clamp(a - b1 - c - d1, 0, 0xFFF);

        return PackHdrRgbPairWithSwap(red0, green0, blue0, red1, green1, blue1, majorComponent);
    }

    /// <summary>
    /// Decodes the CEM 14 endpoint pair (HDR RGB + LDR alpha) per ASTC spec §C.2.14.
    /// RGB is decoded as for CEM 11; alpha is bit-replicated UNORM16 (the same expansion
    /// LDR endpoints use, so the HDR pipeline can blend it as if it were HDR).
    /// </summary>
    private static (Rgba64 Low, Rgba64 High) UnpackHdrRgbDirectLdrAlphaCore(ReadOnlySpan<int> unquantizedValues)
    {
        (Rgba64 rgbLow, Rgba64 rgbHigh) = UnpackHdrRgbDirectCore(unquantizedValues[0], unquantizedValues[1], unquantizedValues[2], unquantizedValues[3], unquantizedValues[4], unquantizedValues[5]);

        ushort alpha0 = (ushort)(unquantizedValues[6] * 257);
        ushort alpha1 = (ushort)(unquantizedValues[7] * 257);

        Rgba64 low = new(rgbLow.R, rgbLow.G, rgbLow.B, alpha0);
        Rgba64 high = new(rgbHigh.R, rgbHigh.G, rgbHigh.B, alpha1);
        return (low, high);
    }

    /// <summary>
    /// Decodes the CEM 15 endpoint pair (HDR RGB + HDR alpha) per ASTC spec §C.2.14.
    /// </summary>
    private static (Rgba64 Low, Rgba64 High) UnpackHdrRgbDirectHdrAlphaCore(ReadOnlySpan<int> unquantizedValues)
    {
        (Rgba64 rgbLow, Rgba64 rgbHigh) = UnpackHdrRgbDirectCore(unquantizedValues[0], unquantizedValues[1], unquantizedValues[2], unquantizedValues[3], unquantizedValues[4], unquantizedValues[5]);

        (ushort alpha0, ushort alpha1) = UnpackHdrAlpha(unquantizedValues[6], unquantizedValues[7]);

        Rgba64 low = new(rgbLow.R, rgbLow.G, rgbLow.B, alpha0);
        Rgba64 high = new(rgbHigh.R, rgbHigh.G, rgbHigh.B, alpha1);
        return (low, high);
    }

    /// <summary>
    /// Decodes the HDR alpha pair shared by CEM 15 per ASTC spec §C.2.14: a 2-bit selector
    /// from the high bits of v6/v7 picks one of four sub-modes that determine how the low
    /// 7 bits of each input map to the 12-bit alpha endpoints (a0, a1).
    /// </summary>
    private static (ushort Low, ushort High) UnpackHdrAlpha(int v6, int v7)
    {
        int selector = ((v6 >> 7) & 1) | ((v7 >> 6) & 2);
        v6 &= 0x7F;
        v7 &= 0x7F;

        int a0, a1;

        if (selector == 3)
        {
            // Simple mode: direct 7-bit values shifted to 12-bit
            a0 = v6 << 5;
            a1 = v7 << 5;
        }
        else
        {
            // Complex mode: base + sign-extended offset
            v6 |= (v7 << (selector + 1)) & 0x780;
            v7 &= 0x3F >> selector;
            v7 ^= 32 >> selector;
            v7 -= 32 >> selector;
            v6 <<= 4 - selector;
            v7 <<= 4 - selector;
            v7 += v6;

            if (v7 < 0)
            {
                v7 = 0;
            }
            else if (v7 > 0xFFF)
            {
                v7 = 0xFFF;
            }

            a0 = v6;
            a1 = v7;
        }

        a0 = Math.Clamp(a0, 0, 0xFFF);
        a1 = Math.Clamp(a1, 0, 0xFFF);

        return ((ushort)(a0 << 4), (ushort)(a1 << 4));
    }
}
