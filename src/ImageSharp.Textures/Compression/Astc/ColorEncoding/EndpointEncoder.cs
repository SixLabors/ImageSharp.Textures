// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using static SixLabors.ImageSharp.Textures.Compression.Astc.Core.Rgba32Extensions;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

internal static class EndpointEncoder
{
    public static bool UsesBlueContract(int maxValue, ColorEndpointMode mode, List<int> values)
    {
        int valueCount = mode.GetColorValuesCount();
        ArgumentOutOfRangeException.ThrowIfLessThan(values.Count, valueCount);

        switch (mode)
        {
            case ColorEndpointMode.LdrRgbDirect:
            case ColorEndpointMode.LdrRgbaDirect:
            {
                int maxValueCount = Math.Max(ColorEndpointMode.LdrRgbDirect.GetColorValuesCount(), ColorEndpointMode.LdrRgbaDirect.GetColorValuesCount());
                int[] v = new int[maxValueCount];
                for (int i = 0; i < maxValueCount; ++i)
                {
                    v[i] = i < values.Count ? values[i] : 0;
                }

                int[] unquantizedValues = EndpointCodec.UnquantizeArray(v, maxValue);
                int s0 = unquantizedValues[0] + unquantizedValues[2] + unquantizedValues[4];
                int s1 = unquantizedValues[1] + unquantizedValues[3] + unquantizedValues[5];
                return s0 > s1;
            }

            case ColorEndpointMode.LdrRgbBaseOffset:
            case ColorEndpointMode.LdrRgbaBaseOffset:
            {
                int maxValueCount = Math.Max(ColorEndpointMode.LdrRgbBaseOffset.GetColorValuesCount(), ColorEndpointMode.LdrRgbaBaseOffset.GetColorValuesCount());
                int[] v = new int[maxValueCount];
                for (int i = 0; i < maxValueCount; ++i)
                {
                    v[i] = i < values.Count ? values[i] : 0;
                }

                int[] unquantizedValues = EndpointCodec.UnquantizeArray(v, maxValue);
                (int b0, int a0) = BitOperations.TransferPrecision(unquantizedValues[1], unquantizedValues[0]);
                (int b1, int a1) = BitOperations.TransferPrecision(unquantizedValues[3], unquantizedValues[2]);
                (int b2, int a2) = BitOperations.TransferPrecision(unquantizedValues[5], unquantizedValues[4]);
                return (b0 + b1 + b2) < 0;
            }

            default:
                return false;
        }
    }

    // TODO: Extract an interface and implement instances for each encoding mode
    public static bool EncodeColorsForMode(Rgba32 endpointLowRgba, Rgba32 endpointHighRgba, int maxValue, EndpointEncodingMode encodingMode, out ColorEndpointMode astcMode, List<int> values)
    {
        bool needsWeightSwap = false;
        astcMode = ColorEndpointMode.LdrLumaDirect;
        int valueCount = encodingMode.GetValuesCount();
        for (int i = values.Count; i < valueCount; ++i)
        {
            values.Add(0);
        }

        switch (encodingMode)
        {
            case EndpointEncodingMode.DirectLuma:
                return EncodeColorsLuma(endpointLowRgba, endpointHighRgba, maxValue, out astcMode, values);
            case EndpointEncodingMode.DirectLumaAlpha:
            {
                int avg1 = endpointLowRgba.GetAverage();
                int avg2 = endpointHighRgba.GetAverage();
                values[0] = Quantization.QuantizeCEValueToRange(avg1, maxValue);
                values[1] = Quantization.QuantizeCEValueToRange(avg2, maxValue);
                values[2] = Quantization.QuantizeCEValueToRange(endpointLowRgba.GetChannel(3), maxValue);
                values[3] = Quantization.QuantizeCEValueToRange(endpointHighRgba.GetChannel(3), maxValue);
                astcMode = ColorEndpointMode.LdrLumaAlphaDirect;
                break;
            }

            case EndpointEncodingMode.BaseScaleRgb:
            case EndpointEncodingMode.BaseScaleRgba:
            {
                Rgba32 baseColor = endpointHighRgba;
                Rgba32 scaled = endpointLowRgba;

                int numChannelsGe = 0;
                for (int i = 0; i < 3; ++i)
                {
                    numChannelsGe += endpointHighRgba.GetChannel(i) >= endpointLowRgba.GetChannel(i) ? 1 : 0;
                }

                if (numChannelsGe < 2)
                {
                    needsWeightSwap = true;
                    (scaled, baseColor) = (baseColor, scaled);
                }

                int[] quantizedBase = QuantizeColorArray(baseColor, maxValue);
                int[] unquantizedBase = EndpointCodec.UnquantizeArray(quantizedBase, maxValue);

                int numSamples = 0;
                int scaleSum = 0;
                for (int i = 0; i < 3; ++i)
                {
                    int x = unquantizedBase[i];
                    if (x != 0)
                    {
                        ++numSamples;
                        scaleSum += (scaled.GetChannel(i) * 256) / x;
                    }
                }

                values[0] = quantizedBase[0];
                values[1] = quantizedBase[1];
                values[2] = quantizedBase[2];
                if (numSamples > 0)
                {
                    int avgScale = Math.Clamp(scaleSum / numSamples, 0, 255);
                    values[3] = Quantization.QuantizeCEValueToRange(avgScale, maxValue);
                }
                else
                {
                    values[3] = maxValue;
                }

                astcMode = ColorEndpointMode.LdrRgbBaseScale;

                if (encodingMode == EndpointEncodingMode.BaseScaleRgba)
                {
                    values[4] = Quantization.QuantizeCEValueToRange(scaled.GetChannel(3), maxValue);
                    values[5] = Quantization.QuantizeCEValueToRange(baseColor.GetChannel(3), maxValue);
                    astcMode = ColorEndpointMode.LdrRgbBaseScaleTwoA;
                }

                break;
            }

            case EndpointEncodingMode.DirectRgb:
            case EndpointEncodingMode.DirectRgba:
                return EncodeColorsRGBA(endpointLowRgba, endpointHighRgba, maxValue, encodingMode == EndpointEncodingMode.DirectRgba, out astcMode, values);
            default:
                throw new InvalidOperationException("Unimplemented color encoding.");
        }

        return needsWeightSwap;
    }

    private static int[] QuantizeColorArray(Rgba32 c, int maxValue)
    {
        int[] array = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            array[i] = Quantization.QuantizeCEValueToRange(c.GetChannel(i), maxValue);
        }

        return array;
    }

    private static bool EncodeColorsLuma(Rgba32 endpointLow, Rgba32 endpointHigh, int maxValue, out ColorEndpointMode astcMode, List<int> values)
    {
        astcMode = ColorEndpointMode.LdrLumaDirect;
        ArgumentOutOfRangeException.ThrowIfLessThan(values.Count, 2);

        int avg1 = endpointLow.GetAverage();
        int avg2 = endpointHigh.GetAverage();

        bool needsWeightSwap = false;
        if (avg1 > avg2)
        {
            needsWeightSwap = true;
            (avg2, avg1) = (avg1, avg2);
        }

        int offset = Math.Min(avg2 - avg1, 0x3F);
        int quantOffLow = Quantization.QuantizeCEValueToRange((avg1 & 0x3F) << 2, maxValue);
        int quantOffHigh = Quantization.QuantizeCEValueToRange((avg1 & 0xC0) | offset, maxValue);

        int quantLow = Quantization.QuantizeCEValueToRange(avg1, maxValue);
        int quantHigh = Quantization.QuantizeCEValueToRange(avg2, maxValue);

        values[0] = quantOffLow;
        values[1] = quantOffHigh;
        (Rgba32 decLowOff, Rgba32 decHighOff) = EndpointCodec.DecodeColorsForMode(values.ToArray(), maxValue, ColorEndpointMode.LdrLumaBaseOffset);

        values[0] = quantLow;
        values[1] = quantHigh;
        (Rgba32 decLowDir, Rgba32 decHighDir) = EndpointCodec.DecodeColorsForMode(values.ToArray(), maxValue, ColorEndpointMode.LdrLumaDirect);

        int calculateErrorOff = 0;
        int calculateErrorDir = 0;
        if (needsWeightSwap)
        {
            calculateErrorDir = SquaredError(decLowDir, endpointHigh) + SquaredError(decHighDir, endpointLow);
            calculateErrorOff = SquaredError(decLowOff, endpointHigh) + SquaredError(decHighOff, endpointLow);
        }
        else
        {
            calculateErrorDir = SquaredError(decLowDir, endpointLow) + SquaredError(decHighDir, endpointHigh);
            calculateErrorOff = SquaredError(decLowOff, endpointLow) + SquaredError(decHighOff, endpointHigh);
        }

        if (calculateErrorDir <= calculateErrorOff)
        {
            values[0] = quantLow;
            values[1] = quantHigh;
            astcMode = ColorEndpointMode.LdrLumaDirect;
        }
        else
        {
            values[0] = quantOffLow;
            values[1] = quantOffHigh;
            astcMode = ColorEndpointMode.LdrLumaBaseOffset;
        }

        return needsWeightSwap;
    }

    private static bool EncodeColorsRGBA(Rgba32 endpointLowRgba, Rgba32 endpointHighRgba, int maxValue, bool withAlpha, out ColorEndpointMode astcMode, List<int> values)
    {
        astcMode = ColorEndpointMode.LdrRgbDirect;

        // Blue-contract (ASTC spec §C.2.14) rewrites (R,G,B) such that B stays and R,G
        // shift toward 2R-B / 2G-B. The inverted form is fed back through decode to score
        // a candidate representation.
        Rgba32 invertedBcLow = endpointLowRgba.WithInvertedBlueContract();
        Rgba32 invertedBcHigh = endpointHighRgba.WithInvertedBlueContract();

        // Build four (base, offset) encoded pairs: direct and blue-contract forms, each
        // with normal (low=base) and swapped (high=base) variants. These feed the
        // base-offset mode (spec §C.2.14 "RGB/RGBA, base+offset").
        QuantizedEndpointPair offsetQuantized = BuildBaseOffsetPair(endpointLowRgba, endpointHighRgba, swapped: false, maxValue);
        QuantizedEndpointPair bcOffsetQuantized = BuildBaseOffsetPair(invertedBcHigh, invertedBcLow, swapped: false, maxValue);
        QuantizedEndpointPair offsetSwappedQuantized = BuildBaseOffsetPair(endpointLowRgba, endpointHighRgba, swapped: true, maxValue);
        QuantizedEndpointPair bcOffsetSwappedQuantized = BuildBaseOffsetPair(invertedBcLow, invertedBcHigh, swapped: true, maxValue);

        QuantizedEndpointPair directQuantized = new(endpointLowRgba, endpointHighRgba, maxValue);
        QuantizedEndpointPair bcQuantized = new(invertedBcLow, invertedBcHigh, maxValue);

        // Rank six candidate encodings by reconstruction error; pack the first that fits.
        List<CEEncodingOption> candidates =
        [
            ScoreDirect(directQuantized, endpointLowRgba, endpointHighRgba, withAlpha),
            ScoreBlueContract(bcQuantized, endpointLowRgba, endpointHighRgba, withAlpha),
            ScoreBaseOffset(offsetQuantized, endpointLowRgba, endpointHighRgba, swapped: false, withAlpha),
            ScoreBaseOffsetBlueContract(bcOffsetQuantized, endpointLowRgba, endpointHighRgba, swapped: false, withAlpha),
            ScoreBaseOffset(offsetSwappedQuantized, endpointLowRgba, endpointHighRgba, swapped: true, withAlpha),
            ScoreBaseOffsetBlueContract(bcOffsetSwappedQuantized, endpointLowRgba, endpointHighRgba, swapped: true, withAlpha),
        ];

        candidates.Sort((a, b) => a.Error.CompareTo(b.Error));

        foreach (CEEncodingOption candidate in candidates)
        {
            bool needsWeightSwap = false;
            if (candidate.Pack(withAlpha, out ColorEndpointMode _, values, ref needsWeightSwap))
            {
                return needsWeightSwap;
            }
        }

        throw new InvalidOperationException("No candidate color-endpoint encoding fit the available bits");
    }

    /// <summary>
    /// Builds a quantized (base, offset) endpoint pair for base-offset mode. Takes the
    /// channel-wise difference, clamps to ASTC's signed 6-bit offset range, then converts
    /// via <see cref="BitOperations.TransferPrecisionInverse"/>.
    /// </summary>
    private static QuantizedEndpointPair BuildBaseOffsetPair(Rgba32 low, Rgba32 high, bool swapped, int maxValue)
    {
        Rgba32 baseColor = swapped ? high : low;
        Rgba32 offsetColor = swapped ? low : high;
        Span<int> baseChannels = stackalloc int[4];
        Span<int> offsetChannels = stackalloc int[4];
        for (int i = 0; i < 4; ++i)
        {
            baseChannels[i] = baseColor.GetChannel(i);
            offsetChannels[i] = Math.Clamp(offsetColor.GetChannel(i) - baseColor.GetChannel(i), -32, 31);
            (offsetChannels[i], baseChannels[i]) = BitOperations.TransferPrecisionInverse(offsetChannels[i], baseChannels[i]);
        }

        return new QuantizedEndpointPair(
            ClampedRgba32(baseChannels[0], baseChannels[1], baseChannels[2], baseChannels[3]),
            ClampedRgba32(offsetChannels[0], offsetChannels[1], offsetChannels[2], offsetChannels[3]),
            maxValue);
    }

    /// <summary>Scores the direct (unswapped) encoding: compare decoded endpoints to originals.</summary>
    private static CEEncodingOption ScoreDirect(
        QuantizedEndpointPair pair,
        Rgba32 originalLow,
        Rgba32 originalHigh,
        bool withAlpha)
    {
        Rgba32 decodedLow = ArrayToRgba32(pair.UnquantizedLow);
        Rgba32 decodedHigh = ArrayToRgba32(pair.UnquantizedHigh);
        int error = ChannelError(decodedLow, originalLow, withAlpha) + ChannelError(decodedHigh, originalHigh, withAlpha);
        return new CEEncodingOption(error, pair, swapEndpoints: false, blueContract: false, useOffsetMode: false);
    }

    /// <summary>Scores a blue-contracted direct encoding (ASTC spec §C.2.14 blue-contract branch).</summary>
    private static CEEncodingOption ScoreBlueContract(
        QuantizedEndpointPair pair,
        Rgba32 originalLow,
        Rgba32 originalHigh,
        bool withAlpha)
    {
        int[] decodedLow = pair.UnquantizedLow;
        int[] decodedHigh = pair.UnquantizedHigh;
        Rgba32 contractedLow = RgbaColorExtensions.WithBlueContract(decodedLow[0], decodedLow[1], decodedLow[2], decodedLow[3]);
        Rgba32 contractedHigh = RgbaColorExtensions.WithBlueContract(decodedHigh[0], decodedHigh[1], decodedHigh[2], decodedHigh[3]);
        int error = ChannelError(contractedLow, originalLow, withAlpha) + ChannelError(contractedHigh, originalHigh, withAlpha);
        return new CEEncodingOption(error, pair, swapEndpoints: false, blueContract: true, useOffsetMode: false);
    }

    /// <summary>
    /// Scores a base-offset encoding (spec §C.2.14). The candidate stores (base, offset)
    /// in its low/high slots; we reconstruct the decoded low/high endpoints by adding the
    /// offset and compare to the original.
    /// </summary>
    private static CEEncodingOption ScoreBaseOffset(
        QuantizedEndpointPair pair,
        Rgba32 originalLow,
        Rgba32 originalHigh,
        bool swapped,
        bool withAlpha)
    {
        (Rgba32 decodedLow, Rgba32 decodedHigh) = ReconstructBaseOffset(pair, swapped);
        int error = ChannelError(decodedLow, originalLow, withAlpha) + ChannelError(decodedHigh, originalHigh, withAlpha);
        return new CEEncodingOption(error, pair, swapEndpoints: swapped, blueContract: false, useOffsetMode: true);
    }

    /// <summary>
    /// Scores a base-offset encoding combined with blue-contract application on the
    /// reconstructed decoded endpoints.
    /// </summary>
    private static CEEncodingOption ScoreBaseOffsetBlueContract(
        QuantizedEndpointPair pair,
        Rgba32 originalLow,
        Rgba32 originalHigh,
        bool swapped,
        bool withAlpha)
    {
        (Rgba32 decodedLow, Rgba32 decodedHigh) = ReconstructBaseOffset(pair, swapped);
        decodedLow = decodedLow.WithBlueContract();
        decodedHigh = decodedHigh.WithBlueContract();

        // Note: the swap flag here compares decodedLow to originalHigh (and vice versa).
        int error = swapped
            ? ChannelError(decodedLow, originalLow, withAlpha) + ChannelError(decodedHigh, originalHigh, withAlpha)
            : ChannelError(decodedLow, originalHigh, withAlpha) + ChannelError(decodedHigh, originalLow, withAlpha);
        return new CEEncodingOption(error, pair, swapEndpoints: swapped, blueContract: true, useOffsetMode: true);
    }

    /// <summary>
    /// Reconstructs decoded (low, high) from a base-offset candidate.
    /// When <paramref name="swapped"/>, the stored "high" slot is the offset applied to the
    /// "low" base, and the caller's decoded low corresponds to the original high endpoint.
    /// </summary>
    private static (Rgba32 DecodedLow, Rgba32 DecodedHigh) ReconstructBaseOffset(QuantizedEndpointPair pair, bool swapped)
    {
        Rgba32 baseColor = ArrayToRgba32(pair.UnquantizedLow);
        Rgba32 offsetColor = ArrayToRgba32(pair.UnquantizedHigh).AsOffsetFrom(baseColor);
        return swapped ? (offsetColor, baseColor) : (baseColor, offsetColor);
    }

    private static Rgba32 ArrayToRgba32(int[] channels)
        => ClampedRgba32(channels[0], channels[1], channels[2], channels[3]);

    private static int ChannelError(Rgba32 a, Rgba32 b, bool withAlpha)
        => withAlpha ? SquaredError(a, b) : SquaredErrorRgb(a, b);
}
