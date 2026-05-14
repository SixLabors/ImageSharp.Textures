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

    /// <summary>
    /// Encodes the given (<paramref name="endpointLowRgba"/>, <paramref name="endpointHighRgba"/>)
    /// endpoint pair using the requested <paramref name="encodingMode"/>, writing the quantized
    /// color values into <paramref name="values"/> and selecting the concrete ASTC color endpoint
    /// mode (<paramref name="astcMode"/>) in <see cref="ColorEndpointMode"/>.
    /// </summary>
    /// <returns>
    /// true when the weight plane needs to be swapped for the selected encoding.
    /// </returns>
    public static bool EncodeColorsForMode(Rgba32 endpointLowRgba, Rgba32 endpointHighRgba, int maxValue, EndpointEncodingMode encodingMode, out ColorEndpointMode astcMode, List<int> values)
    {
        astcMode = ColorEndpointMode.LdrLumaDirect;
        EnsureValueSlots(values, encodingMode.GetValuesCount());

        switch (encodingMode)
        {
            case EndpointEncodingMode.DirectLuma:
                return EncodeColorsLuma(endpointLowRgba, endpointHighRgba, maxValue, out astcMode, values);
            case EndpointEncodingMode.DirectLumaAlpha:
                astcMode = ColorEndpointMode.LdrLumaAlphaDirect;
                EncodeLumaAlphaDirect(endpointLowRgba, endpointHighRgba, maxValue, values);
                return false;
            case EndpointEncodingMode.BaseScaleRgb:
            case EndpointEncodingMode.BaseScaleRgba:
                return EncodeRgbBaseScale(endpointLowRgba, endpointHighRgba, maxValue, withAlpha: encodingMode == EndpointEncodingMode.BaseScaleRgba, out astcMode, values);
            case EndpointEncodingMode.DirectRgb:
            case EndpointEncodingMode.DirectRgba:
                return EncodeColorsRGBA(endpointLowRgba, endpointHighRgba, maxValue, encodingMode == EndpointEncodingMode.DirectRgba, out astcMode, values);
            default:
                throw new InvalidOperationException("Unimplemented color encoding.");
        }
    }

    /// <summary>Pads <paramref name="values"/> with zeros so the caller can index up to <paramref name="slots"/>.</summary>
    private static void EnsureValueSlots(List<int> values, int slots)
    {
        for (int i = values.Count; i < slots; ++i)
        {
            values.Add(0);
        }
    }

    /// <summary>
    /// Encodes a luminance+alpha direct-mode endpoint pair (ASTC spec §C.2.14 mode 4):
    /// v0/v1 = low/high luma, v2/v3 = low/high alpha.
    /// </summary>
    private static void EncodeLumaAlphaDirect(Rgba32 low, Rgba32 high, int maxValue, List<int> values)
    {
        values[0] = Quantization.QuantizeCEValueToRange(low.GetAverage(), maxValue);
        values[1] = Quantization.QuantizeCEValueToRange(high.GetAverage(), maxValue);
        values[2] = Quantization.QuantizeCEValueToRange(low.GetChannel(3), maxValue);
        values[3] = Quantization.QuantizeCEValueToRange(high.GetChannel(3), maxValue);
    }

    /// <summary>
    /// Encodes an RGB base+scale (ASTC spec §C.2.14 mode 6) or the RGBA variant with two
    /// separate alpha values (mode 10). The high endpoint provides the base; the low endpoint
    /// becomes a scale factor inferred per-channel, then averaged. If two or more channels
    /// run in the wrong direction (high &lt; low), the endpoints are swapped and the caller is
    /// told to swap weights.
    /// </summary>
    private static bool EncodeRgbBaseScale(Rgba32 low, Rgba32 high, int maxValue, bool withAlpha, out ColorEndpointMode astcMode, List<int> values)
    {
        astcMode = ColorEndpointMode.LdrRgbBaseScale;

        Rgba32 baseColor = high;
        Rgba32 scaled = low;
        bool needsWeightSwap = false;
        if (ShouldSwapForBaseScale(low, high))
        {
            needsWeightSwap = true;
            (scaled, baseColor) = (baseColor, scaled);
        }

        int[] quantizedBase = QuantizeColorArray(baseColor, maxValue);
        int[] unquantizedBase = EndpointCodec.UnquantizeArray(quantizedBase, maxValue);

        values[0] = quantizedBase[0];
        values[1] = quantizedBase[1];
        values[2] = quantizedBase[2];
        values[3] = QuantizeAverageScale(scaled, unquantizedBase, maxValue);

        if (withAlpha)
        {
            values[4] = Quantization.QuantizeCEValueToRange(scaled.GetChannel(3), maxValue);
            values[5] = Quantization.QuantizeCEValueToRange(baseColor.GetChannel(3), maxValue);
            astcMode = ColorEndpointMode.LdrRgbBaseScaleTwoA;
        }

        return needsWeightSwap;
    }

    /// <summary>
    /// True when two or more of the R/G/B channels decrease from low to high, so the caller
    /// should treat the low endpoint as the base and invert the weight plane.
    /// </summary>
    private static bool ShouldSwapForBaseScale(Rgba32 low, Rgba32 high)
    {
        int channelsHighGe = 0;
        for (int i = 0; i < 3; ++i)
        {
            if (high.GetChannel(i) >= low.GetChannel(i))
            {
                channelsHighGe++;
            }
        }

        return channelsHighGe < 2;
    }

    /// <summary>
    /// Computes a per-channel scale factor (<c>scaled[c] * 256 / base[c]</c>), averages across
    /// channels with non-zero base, clamps to [0, 255] and quantizes.
    /// </summary>
    /// <returns>
    /// <paramref name="maxValue"/> (the maximum quantization slot) when no base channel was non-zero.
    /// </returns>
    private static int QuantizeAverageScale(Rgba32 scaled, int[] unquantizedBase, int maxValue)
    {
        int numSamples = 0;
        int scaleSum = 0;
        for (int i = 0; i < 3; ++i)
        {
            int baseChannel = unquantizedBase[i];
            if (baseChannel != 0)
            {
                ++numSamples;
                scaleSum += (scaled.GetChannel(i) * 256) / baseChannel;
            }
        }

        if (numSamples == 0)
        {
            return maxValue;
        }

        int avgScale = Math.Clamp(scaleSum / numSamples, 0, 255);
        return Quantization.QuantizeCEValueToRange(avgScale, maxValue);
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

    /// <summary>
    /// Encodes a luminance-only endpoint pair (ASTC spec §C.2.14 modes 0 and 1). Both the
    /// direct (mode 0) and base+offset (mode 1) encodings are tried; the one with lower
    /// reconstruction error is written into <paramref name="values"/>. When the high endpoint
    /// is dimmer than the low endpoint, the endpoints are swapped and the weight plane is
    /// inverted so the caller can use a single decode direction.
    /// </summary>
    private static bool EncodeColorsLuma(Rgba32 endpointLow, Rgba32 endpointHigh, int maxValue, out ColorEndpointMode astcMode, List<int> values)
    {
        astcMode = ColorEndpointMode.LdrLumaDirect;
        ArgumentOutOfRangeException.ThrowIfLessThan(values.Count, 2);

        (int avgLow, int avgHigh, bool needsWeightSwap) = SortedLumaAverages(endpointLow, endpointHigh);

        (int directLow, int directHigh) = QuantizeDirectLuma(avgLow, avgHigh, maxValue);
        (int offsetLow, int offsetHigh) = QuantizeBaseOffsetLuma(avgLow, avgHigh, maxValue);

        // Evaluate both candidates by decoding and measuring error against the original
        // endpoints in whatever order matches the post-swap encoding.
        Rgba32 originalLow = needsWeightSwap ? endpointHigh : endpointLow;
        Rgba32 originalHigh = needsWeightSwap ? endpointLow : endpointHigh;

        int directError = DecodeAndMeasureError(values, directLow, directHigh, maxValue, ColorEndpointMode.LdrLumaDirect, originalLow, originalHigh);
        int offsetError = DecodeAndMeasureError(values, offsetLow, offsetHigh, maxValue, ColorEndpointMode.LdrLumaBaseOffset, originalLow, originalHigh);

        if (directError <= offsetError)
        {
            values[0] = directLow;
            values[1] = directHigh;
            astcMode = ColorEndpointMode.LdrLumaDirect;
        }
        else
        {
            values[0] = offsetLow;
            values[1] = offsetHigh;
            astcMode = ColorEndpointMode.LdrLumaBaseOffset;
        }

        return needsWeightSwap;
    }

    /// <summary>
    /// Returns the average luma of the two endpoints sorted low → high, together with a flag
    /// indicating that the original ordering was reversed and the weight plane needs a swap.
    /// </summary>
    private static (int AvgLow, int AvgHigh, bool NeedsWeightSwap) SortedLumaAverages(Rgba32 endpointLow, Rgba32 endpointHigh)
    {
        int avgLow = endpointLow.GetAverage();
        int avgHigh = endpointHigh.GetAverage();
        if (avgLow <= avgHigh)
        {
            return (avgLow, avgHigh, false);
        }

        return (avgHigh, avgLow, true);
    }

    /// <summary>Quantizes two luma averages for LdrLumaDirect (§C.2.14 mode 0).</summary>
    private static (int Low, int High) QuantizeDirectLuma(int avgLow, int avgHigh, int maxValue)
        => (Quantization.QuantizeCEValueToRange(avgLow, maxValue),
            Quantization.QuantizeCEValueToRange(avgHigh, maxValue));

    /// <summary>
    /// Quantizes two luma averages into the base+offset form for LdrLumaBaseOffset
    /// (§C.2.14 mode 1). v0 carries the base low luma shifted left by 2; v1's high two bits
    /// carry the base top bits and the low six bits carry the offset.
    /// </summary>
    private static (int Low, int High) QuantizeBaseOffsetLuma(int avgLow, int avgHigh, int maxValue)
    {
        int offset = Math.Min(avgHigh - avgLow, 0x3F);
        return (Quantization.QuantizeCEValueToRange((avgLow & 0x3F) << 2, maxValue),
                Quantization.QuantizeCEValueToRange((avgLow & 0xC0) | offset, maxValue));
    }

    /// <summary>
    /// Writes two candidate values to <paramref name="values"/>, decodes them under
    /// <paramref name="mode"/>, and returns the sum-of-squared-channel error against the
    /// reference endpoints.
    /// </summary>
    private static int DecodeAndMeasureError(
        List<int> values,
        int v0,
        int v1,
        int maxValue,
        ColorEndpointMode mode,
        Rgba32 referenceLow,
        Rgba32 referenceHigh)
    {
        values[0] = v0;
        values[1] = v1;
        int[] unquantized = EndpointCodec.UnquantizeArray(values.ToArray(), maxValue);
        ColorEndpointPair decoded = EndpointCodec.Decode(unquantized, mode);
        return SquaredError(decoded.LdrLow, referenceLow) + SquaredError(decoded.LdrHigh, referenceHigh);
    }

    private static bool EncodeColorsRGBA(Rgba32 endpointLowRgba, Rgba32 endpointHighRgba, int maxValue, bool withAlpha, out ColorEndpointMode astcMode, List<int> values)
    {
        astcMode = ColorEndpointMode.LdrRgbDirect;

        // Blue-contract (ASTC spec §C.2.14) rewrites (R,G,B) such that B stays and R,G
        // shift toward 2R-B / 2G-B. The inverted form is fed back through decode to score
        // a candidate representation.
        Rgba32 invertedBcLow = endpointLowRgba.WithInvertedBlueContract();
        Rgba32 invertedBcHigh = endpointHighRgba.WithInvertedBlueContract();

        // Build base-offset candidates. The direct form (no BC) has two variants — normal
        // (low=base) and swapped (high=base) — which produce different encodings.
        // The blue-contract form has only one form: pre-swap the args at the call site so
        // the base-offset math runs against the BC-inverted high endpoint, matching ARM's
        // try_quantize_rgb_delta_blue_contract (which swaps internally).
        QuantizedEndpointPair offsetQuantized = BuildBaseOffsetPair(endpointLowRgba, endpointHighRgba, swapped: false, maxValue);
        QuantizedEndpointPair bcOffsetQuantized = BuildBaseOffsetPair(invertedBcHigh, invertedBcLow, swapped: false, maxValue);
        QuantizedEndpointPair offsetSwappedQuantized = BuildBaseOffsetPair(endpointLowRgba, endpointHighRgba, swapped: true, maxValue);

        QuantizedEndpointPair directQuantized = new(endpointLowRgba, endpointHighRgba, maxValue);
        QuantizedEndpointPair bcQuantized = new(invertedBcLow, invertedBcHigh, maxValue);

        // Rank five candidate encodings by reconstruction error; pack the first that fits.
        List<CEEncodingOption> candidates =
        [
            ScoreDirect(directQuantized, endpointLowRgba, endpointHighRgba, withAlpha),
            ScoreBlueContract(bcQuantized, endpointLowRgba, endpointHighRgba, withAlpha),
            ScoreBaseOffset(offsetQuantized, endpointLowRgba, endpointHighRgba, swapped: false, withAlpha),
            ScoreBaseOffsetBlueContract(bcOffsetQuantized, endpointLowRgba, endpointHighRgba, withAlpha),
            ScoreBaseOffset(offsetSwappedQuantized, endpointLowRgba, endpointHighRgba, swapped: true, withAlpha),
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

        // Fixed 4 ints each (16 bytes) — one per RGBA channel.
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
    /// reconstructed decoded endpoints. The caller has already pre-swapped the BC inputs in
    /// <see cref="BuildBaseOffsetPair"/>'s arg list, so after BC inversion <c>decodedLow</c>
    /// corresponds to <paramref name="originalHigh"/> and <c>decodedHigh</c> to
    /// <paramref name="originalLow"/>.
    /// </summary>
    private static CEEncodingOption ScoreBaseOffsetBlueContract(
        QuantizedEndpointPair pair,
        Rgba32 originalLow,
        Rgba32 originalHigh,
        bool withAlpha)
    {
        (Rgba32 decodedLow, Rgba32 decodedHigh) = ReconstructBaseOffset(pair, swapped: false);
        decodedLow = decodedLow.WithBlueContract();
        decodedHigh = decodedHigh.WithBlueContract();

        int error = ChannelError(decodedLow, originalHigh, withAlpha) + ChannelError(decodedHigh, originalLow, withAlpha);
        return new CEEncodingOption(error, pair, swapEndpoints: false, blueContract: true, useOffsetMode: true);
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
