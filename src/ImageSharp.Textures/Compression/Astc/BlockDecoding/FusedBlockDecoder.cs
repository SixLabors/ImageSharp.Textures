// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// Shared decode core for the fused (zero-allocation) ASTC block decode pipeline.
/// Contains BISE extraction and weight infill used by both LDR and HDR decoders.
/// </summary>
internal static class FusedBlockDecoder
{
    /// <summary>
    /// Shared decode core for the fused fast paths. Performs the per-block stages described
    /// in ASTC spec §C.2.7 (overall block decode procedure) in one inlined sweep:
    /// BISE decode the colour values (§C.2.12) and unquantize them (§C.2.13), decode the
    /// endpoint pair (§C.2.14), BISE decode the weights (§C.2.12), unquantize them (§C.2.17),
    /// and infill from the weight grid to the texel grid (§C.2.18). Populates
    /// <paramref name="texelWeights"/> and returns the decoded endpoint pair.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static ColorEndpointPair DecodeFusedCore(
        UInt128 bits, in BlockInfo info, Footprint footprint, Span<int> texelWeights)
    {
        // 1. BISE decode color endpoint values.
        // Single-partition fused path: up to 8 ints (32 bytes) — single-mode CEM caps values at 8.
        int colorCount = info.EndpointMode0.GetColorValuesCount();
        Span<int> colors = stackalloc int[colorCount];
        DecodeBiseValues(bits, info.Colors.StartBit, info.Colors.BitCount, info.Colors.Range, colorCount, colors);

        // 2. Batch unquantize color values, then decode endpoint pair
        Quantization.UnquantizeCEValuesBatch(colors, info.Colors.Range);
        ColorEndpointPair endpointPair = EndpointCodec.Decode(colors, info.EndpointMode0);

        // 3. BISE decode weights.
        // Up to 64 ints (256 bytes) — spec §C.2.11 caps single-plane gridSize at 64.
        int gridSize = info.Weights.Width * info.Weights.Height;
        Span<int> gridWeights = stackalloc int[gridSize];
        DecodeBiseWeights(bits, info.Weights.BitCount, info.Weights.Range, gridSize, gridWeights);

        // 4. Batch unquantize weights
        Quantization.UnquantizeWeightsBatch(gridWeights, info.Weights.Range);

        // 5. Infill weights from grid to texels (or pass through if identity mapping)
        if (info.Weights.Width == footprint.Width && info.Weights.Height == footprint.Height)
        {
            gridWeights[..footprint.PixelCount].CopyTo(texelWeights);
        }
        else
        {
            DecimationInfo decimationInfo = DecimationTable.Get(footprint, info.Weights.Width, info.Weights.Height);
            DecimationTable.InfillWeights(gridWeights, decimationInfo, texelWeights);
        }

        return endpointPair;
    }

    /// <summary>
    /// Decodes BISE-encoded (ASTC spec §C.2.12) colour endpoint values from the specified
    /// bit region of the block.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DecodeBiseValues(UInt128 bits, int startBit, int bitCount, int range, int valuesCount, Span<int> result)
    {
        UInt128 source = (bits >> startBit) & UInt128Extensions.OnesMask(bitCount);
        DecodeBiseSequence(source, range, valuesCount, result);
    }

    /// <summary>
    /// Decodes BISE-encoded (ASTC spec §C.2.12) weights from the reversed high-end of the
    /// block. Weight data is stored MSB-first at the top of the 128-bit block, so the bits
    /// are reversed before decode so the BISE reader can consume them in normal LSB-first
    /// order.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DecodeBiseWeights(UInt128 bits, int weightBitCount, int weightRange, int count, Span<int> result)
    {
        UInt128 source = UInt128Extensions.ReverseBits(bits) & UInt128Extensions.OnesMask(weightBitCount);
        DecodeBiseSequence(source, weightRange, count, result);
    }

    /// <summary>
    /// Decodes a BISE sequence from bits pre-normalised to start at bit 0.
    /// For bit-only encoding, extracts values directly via shifts (no BitStream).
    /// Trit/quint encodings fall back to the full BISE decoder.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeBiseSequence(UInt128 source, int range, int count, Span<int> result)
    {
        // Range is in [1, 255] by construction — BlockInfo's ColorValuesRange/WeightRange come
        // from BlockModeDecoder's spec-bound tables, so skip the redundant per-block bounds check.
        (BiseEncodingMode encMode, int bitsPerValue) = BoundedIntegerSequenceCodec.GetPackingModeBitCountUnchecked(range);

        if (encMode != BiseEncodingMode.BitEncoding)
        {
            BitStream stream = new(source, 128);
            BoundedIntegerSequenceDecoder.Decode(encMode, bitsPerValue, count, ref stream, result);
            return;
        }

        ulong mask = (1UL << bitsPerValue) - 1;
        ulong lowBits = source.Low();
        int totalBits = count * bitsPerValue;

        if (totalBits <= 64)
        {
            for (int i = 0; i < count; i++)
            {
                result[i] = (int)(lowBits & mask);
                lowBits >>= bitsPerValue;
            }

            return;
        }

        ulong highBits = source.High();
        int bitPos = 0;
        for (int i = 0; i < count; i++)
        {
            if (bitPos < 64)
            {
                ulong val = (lowBits >> bitPos) & mask;
                if (bitPos + bitsPerValue > 64)
                {
                    val |= (highBits << (64 - bitPos)) & mask;
                }

                result[i] = (int)val;
            }
            else
            {
                result[i] = (int)((highBits >> (bitPos - 64)) & mask);
            }

            bitPos += bitsPerValue;
        }
    }
}
