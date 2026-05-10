// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoder;

/// <summary>
/// HDR pixel writers and entry points for the fused decode pipeline.
/// All methods handle single-partition, non-dual-plane blocks.
/// </summary>
internal static class FusedHdrBlockDecoder
{
    private const int ChannelsPerPixel = 4;

    /// <summary>
    /// Fused HDR decode to a contiguous float buffer.
    /// Handles single-partition, non-dual-plane blocks with both LDR and HDR endpoints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void DecompressBlockFusedHdr(UInt128 bits, in BlockInfo info, Footprint footprint, Span<float> buffer)
        => DecompressBlock(
            bits,
            in info,
            footprint,
            buffer,
            dstBaseX: 0,
            dstBaseY: 0,
            dstRowStride: footprint.Width * ChannelsPerPixel);

    /// <summary>
    /// Fused HDR decode writing directly to image buffer at strided positions.
    /// Handles single-partition, non-dual-plane blocks with both LDR and HDR endpoints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void DecompressBlockFusedHdrToImage(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        Span<float> imageBuffer)
        => DecompressBlock(
            bits,
            in info,
            footprint,
            imageBuffer,
            dstBaseX,
            dstBaseY,
            dstRowStride: imageWidth * ChannelsPerPixel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecompressBlock(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        Span<float> buffer,
        int dstBaseX,
        int dstBaseY,
        int dstRowStride)
    {
        Span<int> texelWeights = stackalloc int[footprint.PixelCount];
        ColorEndpointPair endpointPair = FusedBlockDecoder.DecodeFusedCore(bits, in info, footprint, texelWeights);

        if (endpointPair.IsHdr)
        {
            WriteHdrPixels(buffer, footprint, dstBaseX, dstBaseY, dstRowStride, in endpointPair, texelWeights);
        }
        else
        {
            WriteLdrAsHdrPixels(buffer, footprint, dstBaseX, dstBaseY, dstRowStride, in endpointPair, texelWeights);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrAsHdrPixels(
        Span<float> buffer,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int dstRowStride,
        in ColorEndpointPair endpointPair,
        Span<int> texelWeights)
    {
        int lowR = endpointPair.LdrLow.R, lowG = endpointPair.LdrLow.G, lowB = endpointPair.LdrLow.B, lowA = endpointPair.LdrLow.A;
        int highR = endpointPair.LdrHigh.R, highG = endpointPair.LdrHigh.G, highB = endpointPair.LdrHigh.B, highA = endpointPair.LdrHigh.A;

        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;

        for (int pixelY = 0; pixelY < footprintHeight; pixelY++)
        {
            int dstRowOffset = ((dstBaseY + pixelY) * dstRowStride) + (dstBaseX * ChannelsPerPixel);
            int srcRowBase = pixelY * footprintWidth;

            for (int pixelX = 0; pixelX < footprintWidth; pixelX++)
            {
                int weight = texelWeights[srcRowBase + pixelX];
                int dstOffset = dstRowOffset + (pixelX * ChannelsPerPixel);
                buffer[dstOffset + 0] = InterpolateLdrAsFloat(lowR, highR, weight);
                buffer[dstOffset + 1] = InterpolateLdrAsFloat(lowG, highG, weight);
                buffer[dstOffset + 2] = InterpolateLdrAsFloat(lowB, highB, weight);
                buffer[dstOffset + 3] = InterpolateLdrAsFloat(lowA, highA, weight);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrPixels(
        Span<float> buffer,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int dstRowStride,
        in ColorEndpointPair endpointPair,
        Span<int> texelWeights)
    {
        bool alphaIsLdr = endpointPair.AlphaIsLdr;
        int lowR = endpointPair.HdrLow.R, lowG = endpointPair.HdrLow.G, lowB = endpointPair.HdrLow.B, lowA = endpointPair.HdrLow.A;
        int highR = endpointPair.HdrHigh.R, highG = endpointPair.HdrHigh.G, highB = endpointPair.HdrHigh.B, highA = endpointPair.HdrHigh.A;

        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;

        for (int pixelY = 0; pixelY < footprintHeight; pixelY++)
        {
            int dstRowOffset = ((dstBaseY + pixelY) * dstRowStride) + (dstBaseX * ChannelsPerPixel);
            int srcRowBase = pixelY * footprintWidth;

            for (int pixelX = 0; pixelX < footprintWidth; pixelX++)
            {
                int weight = texelWeights[srcRowBase + pixelX];
                int dstOffset = dstRowOffset + (pixelX * ChannelsPerPixel);
                buffer[dstOffset + 0] = InterpolateHdrAsFloat(lowR, highR, weight);
                buffer[dstOffset + 1] = InterpolateHdrAsFloat(lowG, highG, weight);
                buffer[dstOffset + 2] = InterpolateHdrAsFloat(lowB, highB, weight);

                if (alphaIsLdr)
                {
                    // Mode 14 (ASTC spec §C.2.14): alpha is a UNORM16 value interpolated like LDR.
                    buffer[dstOffset + 3] = Interpolation.Unorm16ToFloat(Interpolation.BlendWeighted(lowA, highA, weight));
                }
                else
                {
                    buffer[dstOffset + 3] = InterpolateHdrAsFloat(lowA, highA, weight);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float InterpolateLdrAsFloat(int p0, int p1, int weight)
        => Interpolation.Unorm16ToFloat(Interpolation.BlendLdrReplicated(p0, p1, weight));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float InterpolateHdrAsFloat(int p0, int p1, int weight)
    {
        int interpolated = Interpolation.BlendWeighted(p0, p1, weight);
        return Fp16.LnsToFloat(Math.Clamp(interpolated, 0, 0xFFFF));
    }
}
