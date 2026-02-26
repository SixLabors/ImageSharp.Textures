// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Astc.BlockDecoder;

/// <summary>
/// LDR pixel writers and entry points for the fused decode pipeline.
/// All methods handle single-partition, non-dual-plane blocks.
/// </summary>
internal static class FusedLdrBlockDecoder
{
    private const int BytesPerPixelUnorm8 = 4;

    /// <summary>
    /// Fused LDR decode to contiguous buffer.
    /// Only handles single-partition, non-dual-plane, LDR blocks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void DecompressBlockFusedLdr(UInt128 bits, in BlockInfo info, Footprint footprint, Span<byte> buffer)
    {
        Span<int> texelWeights = stackalloc int[footprint.PixelCount];
        ColorEndpointPair endpointPair = FusedBlockDecoder.DecodeFusedCore(bits, in info, footprint, texelWeights);
        WriteLdrPixels(buffer, footprint.PixelCount, in endpointPair, texelWeights);
    }

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
    {
        Span<int> texelWeights = stackalloc int[footprint.PixelCount];
        ColorEndpointPair endpointPair = FusedBlockDecoder.DecodeFusedCore(bits, in info, footprint, texelWeights);
        WriteLdrPixelsToImage(imageBuffer, footprint, dstBaseX, dstBaseY, imageWidth, in endpointPair, texelWeights);
    }

    /// <summary>
    /// Writes all pixels for a single-partition LDR block using SIMD where possible.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrPixels(Span<byte> buffer, int pixelCount, in ColorEndpointPair endpointPair, Span<int> texelWeights)
    {
        int lowR = endpointPair.LdrLow.R, lowG = endpointPair.LdrLow.G, lowB = endpointPair.LdrLow.B, lowA = endpointPair.LdrLow.A;
        int highR = endpointPair.LdrHigh.R, highG = endpointPair.LdrHigh.G, highB = endpointPair.LdrHigh.B, highA = endpointPair.LdrHigh.A;

        int i = 0;
        if (Vector128.IsHardwareAccelerated)
        {
            int limit = pixelCount - 3;
            for (; i < limit; i += 4)
            {
                Vector128<int> weights = Vector128.Create(
                    texelWeights[i],
                    texelWeights[i + 1],
                    texelWeights[i + 2],
                    texelWeights[i + 3]);
                SimdHelpers.Write4PixelLdr(
                    buffer,
                    i * 4,
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

        for (; i < pixelCount; i++)
        {
            SimdHelpers.WriteSinglePixelLdr(
                buffer,
                i * 4,
                lowR,
                lowG,
                lowB,
                lowA,
                highR,
                highG,
                highB,
                highA,
                texelWeights[i]);
        }
    }

    /// <summary>
    /// Writes LDR pixels directly to image buffer at strided positions.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrPixelsToImage(
        Span<byte> imageBuffer,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        in ColorEndpointPair endpointPair,
        Span<int> texelWeights)
    {
        int lowR = endpointPair.LdrLow.R, lowG = endpointPair.LdrLow.G, lowB = endpointPair.LdrLow.B, lowA = endpointPair.LdrLow.A;
        int highR = endpointPair.LdrHigh.R, highG = endpointPair.LdrHigh.G, highB = endpointPair.LdrHigh.B, highA = endpointPair.LdrHigh.A;

        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;
        int rowStride = imageWidth * BytesPerPixelUnorm8;

        for (int pixelY = 0; pixelY < footprintHeight; pixelY++)
        {
            int dstRowOffset = ((dstBaseY + pixelY) * rowStride) + (dstBaseX * BytesPerPixelUnorm8);
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
                        imageBuffer,
                        dstRowOffset + (pixelX * BytesPerPixelUnorm8),
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
                    imageBuffer,
                    dstRowOffset + (pixelX * BytesPerPixelUnorm8),
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
