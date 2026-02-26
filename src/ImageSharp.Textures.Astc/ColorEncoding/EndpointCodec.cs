// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.ColorEncoding;

internal static class EndpointCodec
{
    /// <summary>
    /// Decodes color endpoints for the specified mode, returning a polymorphic endpoint pair
    /// that supports both LDR and HDR modes.
    /// </summary>
    /// <param name="values">Quantized integer values from the ASTC block</param>
    /// <param name="maxValue">Maximum quantization value</param>
    /// <param name="mode">The color endpoint mode</param>
    /// <returns>A ColorEndpointPair representing either LDR or HDR endpoints</returns>
    public static ColorEndpointPair DecodeColorsForModePolymorphic(ReadOnlySpan<int> values, int maxValue, ColorEndpointMode mode)
    {
        if (mode.IsHdr())
        {
            (RgbaHdrColor low, RgbaHdrColor high) = HdrEndpointDecoder.DecodeHdrMode(values, maxValue, mode);
            bool alphaIsLdr = mode == ColorEndpointMode.HdrRgbDirectLdrAlpha;
            return ColorEndpointPair.Hdr(low, high, alphaIsLdr);
        }
        else
        {
            (RgbaColor low, RgbaColor high) = DecodeColorsForMode(values, maxValue, mode);
            return ColorEndpointPair.Ldr(low, high);
        }
    }

    public static (RgbaColor EndpointLowRgba, RgbaColor EndpointHighRgba) DecodeColorsForMode(ReadOnlySpan<int> values, int maxValue, ColorEndpointMode mode)
    {
        int count = mode.GetColorValuesCount();
        Span<int> unquantizedValues = stackalloc int[count];
        int copyLen = Math.Min(count, values.Length);
        for (int i = 0; i < copyLen; i++)
        {
            unquantizedValues[i] = values[i];
        }

        UnquantizeInline(unquantizedValues, maxValue);
        ColorEndpointPair pair = DecodeColorsForModeUnquantized(unquantizedValues, mode);
        return (pair.LdrLow, pair.LdrHigh);
    }

    /// <summary>
    /// Decodes color endpoints from already-unquantized values, supporting both LDR and HDR modes.
    /// Called from the fused HDR decode path where BISE decode + batch unquantize
    /// have already been performed. Returns a ColorEndpointPair (LDR or HDR).
    /// </summary>
    internal static ColorEndpointPair DecodeColorsForModePolymorphicUnquantized(ReadOnlySpan<int> unquantizedValues, ColorEndpointMode mode)
    {
        if (mode.IsHdr())
        {
            (RgbaHdrColor low, RgbaHdrColor high) = HdrEndpointDecoder.DecodeHdrModeUnquantized(unquantizedValues, mode);
            bool alphaIsLdr = mode == ColorEndpointMode.HdrRgbDirectLdrAlpha;
            return ColorEndpointPair.Hdr(low, high, alphaIsLdr);
        }

        return DecodeColorsForModeUnquantized(unquantizedValues, mode);
    }

    /// <summary>
    /// Decodes color endpoints from already-unquantized values.
    /// Called from the fused decode path where BISE decode + batch unquantize
    /// have already been performed. Returns an LDR ColorEndpointPair.
    /// </summary>
    internal static ColorEndpointPair DecodeColorsForModeUnquantized(ReadOnlySpan<int> unquantizedValues, ColorEndpointMode mode)
    {
        RgbaColor endpointLowRgba, endpointHighRgba;

        switch (mode)
        {
            case ColorEndpointMode.LdrLumaDirect:
                endpointLowRgba = new RgbaColor(unquantizedValues[0], unquantizedValues[0], unquantizedValues[0]);
                endpointHighRgba = new RgbaColor(unquantizedValues[1], unquantizedValues[1], unquantizedValues[1]);
                break;
            case ColorEndpointMode.LdrLumaBaseOffset:
            {
                int l0 = (unquantizedValues[0] >> 2) | (unquantizedValues[1] & 0xC0);
                int l1 = Math.Min(l0 + (unquantizedValues[1] & 0x3F), 0xFF);
                endpointLowRgba = new RgbaColor(l0, l0, l0);
                endpointHighRgba = new RgbaColor(l1, l1, l1);
                break;
            }

            case ColorEndpointMode.LdrLumaAlphaDirect:
                endpointLowRgba = new RgbaColor(unquantizedValues[0], unquantizedValues[0], unquantizedValues[0], unquantizedValues[2]);
                endpointHighRgba = new RgbaColor(unquantizedValues[1], unquantizedValues[1], unquantizedValues[1], unquantizedValues[3]);
                break;
            case ColorEndpointMode.LdrLumaAlphaBaseOffset:
            {
                (int b0, int a0) = BitOperations.TransferPrecision(unquantizedValues[1], unquantizedValues[0]);
                (int b2, int a2) = BitOperations.TransferPrecision(unquantizedValues[3], unquantizedValues[2]);
                endpointLowRgba = new RgbaColor(a0, a0, a0, a2);
                int highLuma = a0 + b0;
                endpointHighRgba = new RgbaColor(highLuma, highLuma, highLuma, a2 + b2);
                break;
            }

            case ColorEndpointMode.LdrRgbBaseScale:
                endpointLowRgba = new RgbaColor(
                    (unquantizedValues[0] * unquantizedValues[3]) >> 8,
                    (unquantizedValues[1] * unquantizedValues[3]) >> 8,
                    (unquantizedValues[2] * unquantizedValues[3]) >> 8);
                endpointHighRgba = new RgbaColor(unquantizedValues[0], unquantizedValues[1], unquantizedValues[2]);
                break;
            case ColorEndpointMode.LdrRgbDirect:
            {
                int sum0 = unquantizedValues[0] + unquantizedValues[2] + unquantizedValues[4];
                int sum1 = unquantizedValues[1] + unquantizedValues[3] + unquantizedValues[5];
                if (sum1 < sum0)
                {
                    endpointLowRgba = new RgbaColor(
                        r: (unquantizedValues[1] + unquantizedValues[5]) >> 1,
                        g: (unquantizedValues[3] + unquantizedValues[5]) >> 1,
                        b: unquantizedValues[5]);
                    endpointHighRgba = new RgbaColor(
                        r: (unquantizedValues[0] + unquantizedValues[4]) >> 1,
                        g: (unquantizedValues[2] + unquantizedValues[4]) >> 1,
                        b: unquantizedValues[4]);
                }
                else
                {
                    endpointLowRgba = new RgbaColor(unquantizedValues[0], unquantizedValues[2], unquantizedValues[4]);
                    endpointHighRgba = new RgbaColor(unquantizedValues[1], unquantizedValues[3], unquantizedValues[5]);
                }

                break;
            }

            case ColorEndpointMode.LdrRgbBaseOffset:
            {
                (int b0, int a0) = BitOperations.TransferPrecision(unquantizedValues[1], unquantizedValues[0]);
                (int b1, int a1) = BitOperations.TransferPrecision(unquantizedValues[3], unquantizedValues[2]);
                (int b2, int a2) = BitOperations.TransferPrecision(unquantizedValues[5], unquantizedValues[4]);
                if (b0 + b1 + b2 < 0)
                {
                    endpointLowRgba = new RgbaColor(
                        r: (a0 + b0 + a2 + b2) >> 1,
                        g: (a1 + b1 + a2 + b2) >> 1,
                        b: a2 + b2);
                    endpointHighRgba = new RgbaColor(
                        r: (a0 + a2) >> 1,
                        g: (a1 + a2) >> 1,
                        b: a2);
                }
                else
                {
                    endpointLowRgba = new RgbaColor(a0, a1, a2);
                    endpointHighRgba = new RgbaColor(a0 + b0, a1 + b1, a2 + b2);
                }

                break;
            }

            case ColorEndpointMode.LdrRgbBaseScaleTwoA:
                endpointLowRgba = new RgbaColor(
                    r: (unquantizedValues[0] * unquantizedValues[3]) >> 8,
                    g: (unquantizedValues[1] * unquantizedValues[3]) >> 8,
                    b: (unquantizedValues[2] * unquantizedValues[3]) >> 8,
                    a: unquantizedValues[4]);
                endpointHighRgba = new RgbaColor(unquantizedValues[0], unquantizedValues[1], unquantizedValues[2], unquantizedValues[5]);
                break;
            case ColorEndpointMode.LdrRgbaDirect:
            {
                int sum0 = unquantizedValues[0] + unquantizedValues[2] + unquantizedValues[4];
                int sum1 = unquantizedValues[1] + unquantizedValues[3] + unquantizedValues[5];
                if (sum1 >= sum0)
                {
                    endpointLowRgba = new RgbaColor(unquantizedValues[0], unquantizedValues[2], unquantizedValues[4], unquantizedValues[6]);
                    endpointHighRgba = new RgbaColor(unquantizedValues[1], unquantizedValues[3], unquantizedValues[5], unquantizedValues[7]);
                }
                else
                {
                    endpointLowRgba = new RgbaColor(
                        r: (unquantizedValues[1] + unquantizedValues[5]) >> 1,
                        g: (unquantizedValues[3] + unquantizedValues[5]) >> 1,
                        b: unquantizedValues[5],
                        a: unquantizedValues[7]);
                    endpointHighRgba = new RgbaColor(
                        r: (unquantizedValues[0] + unquantizedValues[4]) >> 1,
                        g: (unquantizedValues[2] + unquantizedValues[4]) >> 1,
                        b: unquantizedValues[4],
                        a: unquantizedValues[6]);
                }

                break;
            }

            case ColorEndpointMode.LdrRgbaBaseOffset:
            {
                (int b0, int a0) = BitOperations.TransferPrecision(unquantizedValues[1], unquantizedValues[0]);
                (int b1, int a1) = BitOperations.TransferPrecision(unquantizedValues[3], unquantizedValues[2]);
                (int b2, int a2) = BitOperations.TransferPrecision(unquantizedValues[5], unquantizedValues[4]);
                (int b3, int a3) = BitOperations.TransferPrecision(unquantizedValues[7], unquantizedValues[6]);
                if (b0 + b1 + b2 < 0)
                {
                    endpointLowRgba = new RgbaColor(
                        r: (a0 + b0 + a2 + b2) >> 1,
                        g: (a1 + b1 + a2 + b2) >> 1,
                        b: a2 + b2,
                        a: a3 + b3);
                    endpointHighRgba = new RgbaColor(
                        r: (a0 + a2) >> 1,
                        g: (a1 + a2) >> 1,
                        b: a2,
                        a: a3);
                }
                else
                {
                    endpointLowRgba = new RgbaColor(a0, a1, a2, a3);
                    endpointHighRgba = new RgbaColor(a0 + b0, a1 + b1, a2 + b2, a3 + b3);
                }

                break;
            }

            default:
                endpointLowRgba = RgbaColor.Empty;
                endpointHighRgba = RgbaColor.Empty;
                break;
        }

        return ColorEndpointPair.Ldr(endpointLowRgba, endpointHighRgba);
    }

    internal static int[] UnquantizeArray(int[] values, int maxValue)
    {
        int[] result = new int[values.Length];
        for (int i = 0; i < values.Length; ++i)
        {
            result[i] = Quantization.UnquantizeCEValueFromRange(values[i], maxValue);
        }

        return result;
    }

    private static void UnquantizeInline(Span<int> values, int maxValue)
    {
        for (int i = 0; i < values.Length; ++i)
        {
            values[i] = Quantization.UnquantizeCEValueFromRange(values[i], maxValue);
        }
    }
}
