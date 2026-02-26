// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Astc.BlockDecoder;

/// <summary>
/// HDR pixel writers and entry points for the fused decode pipeline.
/// All methods handle single-partition, non-dual-plane blocks.
/// </summary>
internal static class FusedHdrBlockDecoder
{
    /// <summary>
    /// Fused HDR decode to contiguous float buffer.
    /// Handles single-partition, non-dual-plane blocks with both LDR and HDR endpoints.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void DecompressBlockFusedHdr(UInt128 bits, in BlockInfo info, Footprint footprint, Span<float> buffer)
    {
        Span<int> texelWeights = stackalloc int[footprint.PixelCount];
        ColorEndpointPair endpointPair = FusedBlockDecoder.DecodeFusedCore(bits, in info, footprint, texelWeights);
        WriteHdrOutputPixels(buffer, footprint.PixelCount, in endpointPair, texelWeights);
    }

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
    {
        Span<int> texelWeights = stackalloc int[footprint.PixelCount];
        ColorEndpointPair endpointPair = FusedBlockDecoder.DecodeFusedCore(bits, in info, footprint, texelWeights);
        WriteHdrOutputPixelsToImage(imageBuffer, footprint, dstBaseX, dstBaseY, imageWidth, in endpointPair, texelWeights);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrOutputPixels(
        Span<float> buffer, int pixelCount, in ColorEndpointPair endpointPair, Span<int> texelWeights)
    {
        if (endpointPair.IsHdr)
        {
            WriteHdrPixels(buffer, pixelCount, in endpointPair, texelWeights);
        }
        else
        {
            WriteLdrAsHdrPixels(buffer, pixelCount, in endpointPair, texelWeights);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrOutputPixelsToImage(
        Span<float> imageBuffer,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        in ColorEndpointPair endpointPair,
        Span<int> texelWeights)
    {
        if (endpointPair.IsHdr)
        {
            WriteHdrPixelsToImage(imageBuffer, footprint, dstBaseX, dstBaseY, imageWidth, in endpointPair, texelWeights);
        }
        else
        {
            WriteLdrAsHdrPixelsToImage(imageBuffer, footprint, dstBaseX, dstBaseY, imageWidth, in endpointPair, texelWeights);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrAsHdrPixels(Span<float> buffer, int pixelCount, in ColorEndpointPair endpointPair, Span<int> texelWeights)
    {
        int lowR = endpointPair.LdrLow.R, lowG = endpointPair.LdrLow.G, lowB = endpointPair.LdrLow.B, lowA = endpointPair.LdrLow.A;
        int highR = endpointPair.LdrHigh.R, highG = endpointPair.LdrHigh.G, highB = endpointPair.LdrHigh.B, highA = endpointPair.LdrHigh.A;

        for (int i = 0; i < pixelCount; i++)
        {
            int weight = texelWeights[i];
            int offset = i * 4;
            buffer[offset + 0] = InterpolateLdrAsFloat(lowR, highR, weight);
            buffer[offset + 1] = InterpolateLdrAsFloat(lowG, highG, weight);
            buffer[offset + 2] = InterpolateLdrAsFloat(lowB, highB, weight);
            buffer[offset + 3] = InterpolateLdrAsFloat(lowA, highA, weight);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrAsHdrPixelsToImage(
        Span<float> imageBuffer,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        in ColorEndpointPair endpointPair,
        Span<int> texelWeights)
    {
        int lowR = endpointPair.LdrLow.R, lowG = endpointPair.LdrLow.G, lowB = endpointPair.LdrLow.B, lowA = endpointPair.LdrLow.A;
        int highR = endpointPair.LdrHigh.R, highG = endpointPair.LdrHigh.G, highB = endpointPair.LdrHigh.B, highA = endpointPair.LdrHigh.A;

        const int channelsPerPixel = 4;
        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;
        int rowStride = imageWidth * channelsPerPixel;

        for (int pixelY = 0; pixelY < footprintHeight; pixelY++)
        {
            int dstRowOffset = ((dstBaseY + pixelY) * rowStride) + (dstBaseX * channelsPerPixel);
            int srcRowBase = pixelY * footprintWidth;

            for (int pixelX = 0; pixelX < footprintWidth; pixelX++)
            {
                int weight = texelWeights[srcRowBase + pixelX];
                int dstOffset = dstRowOffset + (pixelX * channelsPerPixel);
                imageBuffer[dstOffset + 0] = InterpolateLdrAsFloat(lowR, highR, weight);
                imageBuffer[dstOffset + 1] = InterpolateLdrAsFloat(lowG, highG, weight);
                imageBuffer[dstOffset + 2] = InterpolateLdrAsFloat(lowB, highB, weight);
                imageBuffer[dstOffset + 3] = InterpolateLdrAsFloat(lowA, highA, weight);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrPixels(Span<float> buffer, int pixelCount, in ColorEndpointPair endpointPair, Span<int> texelWeights)
    {
        bool alphaIsLdr = endpointPair.AlphaIsLdr;
        int lowR = endpointPair.HdrLow.R, lowG = endpointPair.HdrLow.G, lowB = endpointPair.HdrLow.B, lowA = endpointPair.HdrLow.A;
        int highR = endpointPair.HdrHigh.R, highG = endpointPair.HdrHigh.G, highB = endpointPair.HdrHigh.B, highA = endpointPair.HdrHigh.A;

        for (int i = 0; i < pixelCount; i++)
        {
            int weight = texelWeights[i];
            int offset = i * 4;
            buffer[offset + 0] = InterpolateHdrAsFloat(lowR, highR, weight);
            buffer[offset + 1] = InterpolateHdrAsFloat(lowG, highG, weight);
            buffer[offset + 2] = InterpolateHdrAsFloat(lowB, highB, weight);

            if (alphaIsLdr)
            {
                int interpolated = ((lowA * (64 - weight)) + (highA * weight) + 32) / 64;
                buffer[offset + 3] = (ushort)Math.Clamp(interpolated, 0, 0xFFFF) / 65535.0f;
            }
            else
            {
                buffer[offset + 3] = InterpolateHdrAsFloat(lowA, highA, weight);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrPixelsToImage(
        Span<float> imageBuffer,
        Footprint footprint,
        int dstBaseX,
        int dstBaseY,
        int imageWidth,
        in ColorEndpointPair endpointPair,
        Span<int> texelWeights)
    {
        bool alphaIsLdr = endpointPair.AlphaIsLdr;
        int lowR = endpointPair.HdrLow.R, lowG = endpointPair.HdrLow.G, lowB = endpointPair.HdrLow.B, lowA = endpointPair.HdrLow.A;
        int highR = endpointPair.HdrHigh.R, highG = endpointPair.HdrHigh.G, highB = endpointPair.HdrHigh.B, highA = endpointPair.HdrHigh.A;

        const int channelsPerPixel = 4;
        int footprintWidth = footprint.Width;
        int footprintHeight = footprint.Height;
        int rowStride = imageWidth * channelsPerPixel;

        for (int pixelY = 0; pixelY < footprintHeight; pixelY++)
        {
            int dstRowOffset = ((dstBaseY + pixelY) * rowStride) + (dstBaseX * channelsPerPixel);
            int srcRowBase = pixelY * footprintWidth;

            for (int pixelX = 0; pixelX < footprintWidth; pixelX++)
            {
                int weight = texelWeights[srcRowBase + pixelX];
                int dstOffset = dstRowOffset + (pixelX * channelsPerPixel);
                imageBuffer[dstOffset + 0] = InterpolateHdrAsFloat(lowR, highR, weight);
                imageBuffer[dstOffset + 1] = InterpolateHdrAsFloat(lowG, highG, weight);
                imageBuffer[dstOffset + 2] = InterpolateHdrAsFloat(lowB, highB, weight);

                if (alphaIsLdr)
                {
                    int interpolated = ((lowA * (64 - weight)) + (highA * weight) + 32) / 64;
                    imageBuffer[dstOffset + 3] = (ushort)Math.Clamp(interpolated, 0, 0xFFFF) / 65535.0f;
                }
                else
                {
                    imageBuffer[dstOffset + 3] = InterpolateHdrAsFloat(lowA, highA, weight);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float InterpolateLdrAsFloat(int p0, int p1, int weight)
    {
        int c0 = (p0 << 8) | p0;
        int c1 = (p1 << 8) | p1;
        int interpolated = ((c0 * (64 - weight)) + (c1 * weight) + 32) / 64;
        return Math.Clamp(interpolated, 0, 0xFFFF) / 65535.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float InterpolateHdrAsFloat(int p0, int p1, int weight)
    {
        int interpolated = ((p0 * (64 - weight)) + (p1 * weight) + 32) / 64;
        ushort clamped = (ushort)Math.Clamp(interpolated, 0, 0xFFFF);
        ushort halfFloatBits = LogicalBlock.LnsToSf16(clamped);
        return (float)BitConverter.UInt16BitsToHalf(halfFloatBits);
    }
}
