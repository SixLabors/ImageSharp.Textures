// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

/// <summary>
/// Quantizes an RGBA endpoint pair to a given BISE range and pre-computes the dequantized
/// values. Used by the endpoint encoder to score candidate encodings against the original
/// input (ASTC spec §C.2.16).
/// </summary>
internal sealed class QuantizedEndpointPair
{
    public QuantizedEndpointPair(Rgba32 low, Rgba32 high, int maxValue)
    {
        this.QuantizedLow = QuantizeChannels(low, maxValue);
        this.QuantizedHigh = QuantizeChannels(high, maxValue);
        this.UnquantizedLow = Unquantize(this.QuantizedLow, maxValue);
        this.UnquantizedHigh = Unquantize(this.QuantizedHigh, maxValue);
    }

    /// <summary>Gets the quantized low endpoint, RGBA order.</summary>
    public int[] QuantizedLow { get; }

    /// <summary>Gets the quantized high endpoint, RGBA order.</summary>
    public int[] QuantizedHigh { get; }

    /// <summary>Gets the dequantized low endpoint, RGBA order.</summary>
    public int[] UnquantizedLow { get; }

    /// <summary>Gets the dequantized high endpoint, RGBA order.</summary>
    public int[] UnquantizedHigh { get; }

    private static int[] QuantizeChannels(Rgba32 color, int maxValue)
    {
        int[] array = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            array[i] = Quantization.QuantizeCEValueToRange(color.GetChannel(i), maxValue);
        }

        return array;
    }

    private static int[] Unquantize(int[] quantized, int maxValue)
        => Quantization.UnquantizeCEValuesArray(quantized, maxValue);
}
