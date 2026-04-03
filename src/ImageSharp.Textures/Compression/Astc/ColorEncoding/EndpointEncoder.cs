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
        int numChannels = withAlpha ? 4 : 3;

        Rgba32 invertedBlueContractLow = endpointLowRgba.WithInvertedBlueContract();
        Rgba32 invertedBlueContractHigh = endpointHighRgba.WithInvertedBlueContract();

        int[] directBase = new int[4];
        int[] directOffset = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            directBase[i] = endpointLowRgba.GetChannel(i);
            directOffset[i] = Math.Clamp(endpointHighRgba.GetChannel(i) - endpointLowRgba.GetChannel(i), -32, 31);
            (directOffset[i], directBase[i]) = BitOperations.TransferPrecisionInverse(directOffset[i], directBase[i]);
        }

        int[] invertedBlueContractBase = new int[4];
        int[] invertedBlueContractOffset = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            invertedBlueContractBase[i] = invertedBlueContractHigh.GetChannel(i);
            invertedBlueContractOffset[i] = Math.Clamp(invertedBlueContractLow.GetChannel(i) - invertedBlueContractHigh.GetChannel(i), -32, 31);
            (invertedBlueContractOffset[i], invertedBlueContractBase[i]) = BitOperations.TransferPrecisionInverse(invertedBlueContractOffset[i], invertedBlueContractBase[i]);
        }

        int[] directBaseSwapped = new int[4];
        int[] directOffsetSwapped = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            directBaseSwapped[i] = endpointHighRgba.GetChannel(i);
            directOffsetSwapped[i] = Math.Clamp(endpointLowRgba.GetChannel(i) - endpointHighRgba.GetChannel(i), -32, 31);
            (directOffsetSwapped[i], directBaseSwapped[i]) = BitOperations.TransferPrecisionInverse(directOffsetSwapped[i], directBaseSwapped[i]);
        }

        int[] invertedBlueContractBaseSwapped = new int[4];
        int[] invertedBlueContractOffsetSwapped = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            invertedBlueContractBaseSwapped[i] = invertedBlueContractLow.GetChannel(i);
            invertedBlueContractOffsetSwapped[i] = Math.Clamp(invertedBlueContractHigh.GetChannel(i) - invertedBlueContractLow.GetChannel(i), -32, 31);
            (invertedBlueContractOffsetSwapped[i], invertedBlueContractBaseSwapped[i]) = BitOperations.TransferPrecisionInverse(invertedBlueContractOffsetSwapped[i], invertedBlueContractBaseSwapped[i]);
        }

        QuantizedEndpointPair directQuantized = new(endpointLowRgba, endpointHighRgba, maxValue);
        QuantizedEndpointPair bcQuantized = new(invertedBlueContractLow, invertedBlueContractHigh, maxValue);

        QuantizedEndpointPair offsetQuantized = new(ClampedRgba32(directBase[0], directBase[1], directBase[2], directBase[3]), ClampedRgba32(directOffset[0], directOffset[1], directOffset[2], directOffset[3]), maxValue);
        QuantizedEndpointPair bcOffsetQuantized = new(ClampedRgba32(invertedBlueContractBase[0], invertedBlueContractBase[1], invertedBlueContractBase[2], invertedBlueContractBase[3]), ClampedRgba32(invertedBlueContractOffset[0], invertedBlueContractOffset[1], invertedBlueContractOffset[2], invertedBlueContractOffset[3]), maxValue);

        QuantizedEndpointPair offsetSwappedQuantized = new(ClampedRgba32(directBaseSwapped[0], directBaseSwapped[1], directBaseSwapped[2], directBaseSwapped[3]), ClampedRgba32(directOffsetSwapped[0], directOffsetSwapped[1], directOffsetSwapped[2], directOffsetSwapped[3]), maxValue);
        QuantizedEndpointPair bcOffsetSwappedQuantized = new(ClampedRgba32(invertedBlueContractBaseSwapped[0], invertedBlueContractBaseSwapped[1], invertedBlueContractBaseSwapped[2], invertedBlueContractBaseSwapped[3]), ClampedRgba32(invertedBlueContractOffsetSwapped[0], invertedBlueContractOffsetSwapped[1], invertedBlueContractOffsetSwapped[2], invertedBlueContractOffsetSwapped[3]), maxValue);

        List<CEEncodingOption> errors = new(6);

        // 3.1 regular unquantized error
        {
            int[] rgbaLow = directQuantized.UnquantizedLow();
            int[] rgbaHigh = directQuantized.UnquantizedHigh();
            Rgba32 lowColor = ClampedRgba32(rgbaLow[0], rgbaLow[1], rgbaLow[2], rgbaLow[3]);
            Rgba32 highColor = ClampedRgba32(rgbaHigh[0], rgbaHigh[1], rgbaHigh[2], rgbaHigh[3]);
            int squaredRgbError = withAlpha
                ? SquaredError(lowColor, endpointLowRgba) + SquaredError(highColor, endpointHighRgba)
                : SquaredErrorRgb(lowColor, endpointLowRgba) + SquaredErrorRgb(highColor, endpointHighRgba);
            errors.Add(new CEEncodingOption(squaredRgbError, directQuantized, false, false, false));
        }

        // 3.2 blue-contract
        {
            int[] blueContractUnquantizedLow = bcQuantized.UnquantizedLow();
            int[] blueContractUnquantizedHigh = bcQuantized.UnquantizedHigh();
            Rgba32 blueContractLow = RgbaColorExtensions.WithBlueContract(blueContractUnquantizedLow[0], blueContractUnquantizedLow[1], blueContractUnquantizedLow[2], blueContractUnquantizedLow[3]);
            Rgba32 blueContractHigh = RgbaColorExtensions.WithBlueContract(blueContractUnquantizedHigh[0], blueContractUnquantizedHigh[1], blueContractUnquantizedHigh[2], blueContractUnquantizedHigh[3]);

            // TODO: How to handle alpha for this entire functions??
            int blueContractSquaredError = withAlpha
                ? SquaredError(blueContractLow, endpointLowRgba) + SquaredError(blueContractHigh, endpointHighRgba)
                : SquaredErrorRgb(blueContractLow, endpointLowRgba) + SquaredErrorRgb(blueContractHigh, endpointHighRgba);

            errors.Add(new CEEncodingOption(blueContractSquaredError, bcQuantized, swapEndpoints: false, blueContract: true, useOffsetMode: false));
        }

        // 3.3 base/offset
        void ComputeBaseOffsetError(QuantizedEndpointPair pair, bool swapped)
        {
            int[] baseArr = pair.UnquantizedLow();
            int[] offsetArr = pair.UnquantizedHigh();

            Rgba32 baseColor = ClampedRgba32(baseArr[0], baseArr[1], baseArr[2], baseArr[3]);
            Rgba32 offsetColor = ClampedRgba32(offsetArr[0], offsetArr[1], offsetArr[2], offsetArr[3]).AsOffsetFrom(baseColor);

            int baseOffsetError = 0;
            if (swapped)
            {
                baseOffsetError = withAlpha
                    ? SquaredError(baseColor, endpointHighRgba) + SquaredError(offsetColor, endpointLowRgba)
                    : SquaredErrorRgb(baseColor, endpointHighRgba) + SquaredErrorRgb(offsetColor, endpointLowRgba);
            }
            else
            {
                baseOffsetError = withAlpha
                    ? SquaredError(baseColor, endpointLowRgba) + SquaredError(offsetColor, endpointHighRgba)
                    : SquaredErrorRgb(baseColor, endpointLowRgba) + SquaredErrorRgb(offsetColor, endpointHighRgba);
            }

            errors.Add(new CEEncodingOption(baseOffsetError, pair, swapped, false, true));
        }

        ComputeBaseOffsetError(offsetQuantized, false);

        void ComputeBaseOffsetBlueContractError(QuantizedEndpointPair pair, bool swapped)
        {
            int[] baseArr = pair.UnquantizedLow();
            int[] offsetArr = pair.UnquantizedHigh();

            Rgba32 baseColor = ClampedRgba32(baseArr[0], baseArr[1], baseArr[2], baseArr[3]);
            Rgba32 offsetColor = ClampedRgba32(offsetArr[0], offsetArr[1], offsetArr[2], offsetArr[3]).AsOffsetFrom(baseColor);

            baseColor = baseColor.WithBlueContract();
            offsetColor = offsetColor.WithBlueContract();

            int squaredBlueContractError = 0;
            if (swapped)
            {
                squaredBlueContractError = withAlpha
                    ? SquaredError(baseColor, endpointLowRgba) + SquaredError(offsetColor, endpointHighRgba)
                    : SquaredErrorRgb(baseColor, endpointLowRgba) + SquaredErrorRgb(offsetColor, endpointHighRgba);
            }
            else
            {
                squaredBlueContractError = withAlpha
                    ? SquaredError(baseColor, endpointHighRgba) + SquaredError(offsetColor, endpointLowRgba)
                    : SquaredErrorRgb(baseColor, endpointHighRgba) + SquaredErrorRgb(offsetColor, endpointLowRgba);
            }

            errors.Add(new CEEncodingOption(squaredBlueContractError, pair, swapped, true, true));
        }

        ComputeBaseOffsetBlueContractError(bcOffsetQuantized, false);
        ComputeBaseOffsetError(offsetSwappedQuantized, true);
        ComputeBaseOffsetBlueContractError(bcOffsetSwappedQuantized, true);

        errors.Sort((a, b) => a.Error().CompareTo(b.Error()));

        foreach (CEEncodingOption measurement in errors)
        {
            bool needsWeightSwap = false;
            if (measurement.Pack(withAlpha, out ColorEndpointMode modeUnused, values, ref needsWeightSwap))
            {
                return needsWeightSwap;
            }
        }

        throw new InvalidOperationException("Shouldn't have reached this point");
    }

    private class QuantizedEndpointPair
    {
        private readonly Rgba32 originalLow;
        private readonly Rgba32 originalHigh;
        private readonly int[] quantizedLow;
        private readonly int[] quantizedHigh;
        private readonly int[] unquantizedLow;
        private readonly int[] unquantizedHigh;

        public QuantizedEndpointPair(Rgba32 low, Rgba32 high, int maxValue)
        {
            this.originalLow = low;
            this.originalHigh = high;
            this.quantizedLow = QuantizeColorArray(low, maxValue);
            this.quantizedHigh = QuantizeColorArray(high, maxValue);
            this.unquantizedLow = EndpointCodec.UnquantizeArray(this.quantizedLow, maxValue);
            this.unquantizedHigh = EndpointCodec.UnquantizeArray(this.quantizedHigh, maxValue);
        }

        public int[] QuantizedLow() => this.quantizedLow;

        public int[] QuantizedHigh() => this.quantizedHigh;

        public int[] UnquantizedLow() => this.unquantizedLow;

        public int[] UnquantizedHigh() => this.unquantizedHigh;

        public Rgba32 OriginalLow() => this.originalLow;

        public Rgba32 OriginalHigh() => this.originalHigh;
    }

    private class CEEncodingOption
    {
        private readonly int squaredError;
        private readonly QuantizedEndpointPair quantizedEndpoints;
        private readonly bool swapEndpoints;
        private readonly bool blueContract;
        private readonly bool useOffsetMode;

        public CEEncodingOption(
            int squaredError,
            QuantizedEndpointPair quantizedEndpoints,
            bool swapEndpoints,
            bool blueContract,
            bool useOffsetMode)
        {
            this.squaredError = squaredError;
            this.quantizedEndpoints = quantizedEndpoints;
            this.swapEndpoints = swapEndpoints;
            this.blueContract = blueContract;
            this.useOffsetMode = useOffsetMode;
        }

        public bool Pack(bool hasAlpha, out ColorEndpointMode endpointMode, List<int> values, ref bool needsWeightSwap)
        {
            endpointMode = ColorEndpointMode.LdrLumaDirect;
            int[] unquantizedLowOriginal = this.quantizedEndpoints.UnquantizedLow();
            int[] unquantizedHighOriginal = this.quantizedEndpoints.UnquantizedHigh();

            int[] unquantizedLow = (int[])unquantizedLowOriginal.Clone();
            int[] unquantizedHigh = (int[])unquantizedHighOriginal.Clone();

            if (this.useOffsetMode)
            {
                for (int i = 0; i < 4; ++i)
                {
                    (unquantizedHigh[i], unquantizedLow[i]) = BitOperations.TransferPrecision(unquantizedHigh[i], unquantizedLow[i]);
                }
            }

            int sum0 = 0, sum1 = 0;
            for (int i = 0; i < 3; ++i)
            {
                sum0 += unquantizedLow[i];
                sum1 += unquantizedHigh[i];
            }

            bool swapVals = false;
            if (this.useOffsetMode)
            {
                if (this.blueContract)
                {
                    swapVals = sum1 >= 0;
                }
                else
                {
                    swapVals = sum1 < 0;
                }

                if (swapVals)
                {
                    return false;
                }
            }
            else
            {
                if (this.blueContract)
                {
                    if (sum1 == sum0)
                    {
                        return false;
                    }

                    swapVals = sum1 > sum0;
                    needsWeightSwap = !needsWeightSwap;
                }
                else
                {
                    swapVals = sum1 < sum0;
                }
            }

            int[] quantizedLowOriginal = this.quantizedEndpoints.QuantizedLow();
            int[] quantizedHighOriginal = this.quantizedEndpoints.QuantizedHigh();

            int[] quantizedLow = (int[])quantizedLowOriginal.Clone();
            int[] quantizedHigh = (int[])quantizedHighOriginal.Clone();

            if (swapVals)
            {
                if (this.useOffsetMode)
                {
                    throw new InvalidOperationException();
                }

                (quantizedHigh, quantizedLow) = (quantizedLow, quantizedHigh);
                needsWeightSwap = !needsWeightSwap;
            }

            values[0] = quantizedLow[0];
            values[1] = quantizedHigh[0];
            values[2] = quantizedLow[1];
            values[3] = quantizedHigh[1];
            values[4] = quantizedLow[2];
            values[5] = quantizedHigh[2];

            if (this.useOffsetMode)
            {
                endpointMode = ColorEndpointMode.LdrRgbBaseOffset;
            }
            else
            {
                endpointMode = ColorEndpointMode.LdrRgbDirect;
            }

            if (hasAlpha)
            {
                values[6] = quantizedLow[3];
                values[7] = quantizedHigh[3];
                if (this.useOffsetMode)
                {
                    endpointMode = ColorEndpointMode.LdrRgbaBaseOffset;
                }
                else
                {
                    endpointMode = ColorEndpointMode.LdrRgbaDirect;
                }
            }

            if (this.swapEndpoints)
            {
                needsWeightSwap = !needsWeightSwap;
            }

            return true;
        }

        public bool BlueContract() => this.blueContract;

        public int Error() => this.squaredError;
    }
}
