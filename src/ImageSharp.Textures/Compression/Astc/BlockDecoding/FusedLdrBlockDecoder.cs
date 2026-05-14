// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// LDR pixel writers and entry points for the fused decode pipeline.
/// All methods handle single-partition, non-dual-plane blocks.
/// </summary>
internal static class FusedLdrBlockDecoder
{
    /// <summary>
    /// Fused LDR decode to a contiguous buffer.
    /// Only handles single-partition, non-dual-plane, LDR blocks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void DecompressBlockFusedLdr(UInt128 bits, in BlockInfo info, Footprint footprint, Span<byte> buffer)
        => DecompressBlock(
            bits,
            in info,
            footprint,
            buffer,
            dstBaseX: 0,
            dstBaseY: 0,
            dstRowStride: footprint.Width * BlockInfo.ChannelsPerPixel);

    /// <summary>
    /// Fused LDR decode writing directly to image buffer at strided positions.
    /// Only handles single-partition, non-dual-plane, LDR blocks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void DecompressBlockFusedLdrToImage(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        Span<byte> imageBuffer)
        => DecompressBlock(
            bits,
            in info,
            footprint,
            imageBuffer,
            dstBaseX,
            dstBaseY,
            dstRowStride: imageWidth * BlockInfo.ChannelsPerPixel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecompressBlock(
        UInt128 bits,
        in BlockInfo info,
        Footprint footprint,
        Span<byte> buffer,
        int dstBaseX,
        int dstBaseY,
        int dstRowStride)
    {
        // Up to 12×12 = 144 ints (576 bytes) for the largest 2D footprint per spec §C.2.4.
        Span<int> texelWeights = stackalloc int[footprint.PixelCount];
        ColorEndpointPair endpointPair = FusedBlockDecoder.DecodeFusedCore(bits, in info, footprint, texelWeights);
        WriteLdrPixels(buffer, footprint, dstBaseX, dstBaseY, dstRowStride, in endpointPair, texelWeights);
    }

    /// <summary>
    /// Writes a footprint-sized block of LDR pixels into <paramref name="buffer"/> at position
    /// (<paramref name="dstBaseX"/>, <paramref name="dstBaseY"/>) with the given row stride.
    /// Uses SIMD where hardware-accelerated; scalar otherwise.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrPixels(
        Span<byte> buffer,
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
            int dstRowOffset = ((dstBaseY + pixelY) * dstRowStride) + (dstBaseX * BlockInfo.ChannelsPerPixel);
            int srcRowBase = pixelY * footprintWidth;
            int pixelX = 0;

            if (Vector128.IsHardwareAccelerated)
            {
                int limit = footprintWidth - 3;
                for (; pixelX < limit; pixelX += 4)
                {
                    int texelIndex = srcRowBase + pixelX;
                    Vector128<int> weights = Vector128.Create(
                        texelWeights[texelIndex],
                        texelWeights[texelIndex + 1],
                        texelWeights[texelIndex + 2],
                        texelWeights[texelIndex + 3]);
                    SimdHelpers.Write4PixelLdr(
                        buffer,
                        dstRowOffset + (pixelX * BlockInfo.ChannelsPerPixel),
                        lowR,
                        lowG,
                        lowB,
                        lowA,
                        highR,
                        highG,
                        highB,
                        highA,
                        weights);
                }
            }

            for (; pixelX < footprintWidth; pixelX++)
            {
                SimdHelpers.WriteSinglePixelLdr(
                    buffer,
                    dstRowOffset + (pixelX * BlockInfo.ChannelsPerPixel),
                    lowR,
                    lowG,
                    lowB,
                    lowA,
                    highR,
                    highG,
                    highB,
                    highA,
                    texelWeights[srcRowBase + pixelX]);
            }
        }
    }
}
