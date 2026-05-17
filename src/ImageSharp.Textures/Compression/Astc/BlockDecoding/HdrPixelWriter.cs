// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;

/// <summary>
/// HDR <see cref="IPixelWriter{T}"/> — writes float RGBA. Handles both LDR and HDR endpoint
/// modes and the mode-14 LDR-alpha hybrid (ASTC spec §C.2.14, §C.2.15, §C.2.23).
/// </summary>
internal readonly struct HdrPixelWriter : IPixelWriter<float>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePixel(Span<float> buffer, int offset, in ColorEndpointPair endpoint, int weight)
        => WriteChannels(buffer.Slice(offset, 4), in endpoint, weight, dualPlane: null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePixelDualPlane(
        Span<float> buffer,
        int offset,
        in ColorEndpointPair endpoint,
        int primaryWeight,
        int dualPlaneChannel,
        int dualPlaneWeight)
        => WriteChannels(
            buffer.Slice(offset, 4),
            in endpoint,
            primaryWeight,
            dualPlane: new DualPlanePixel(dualPlaneChannel, dualPlaneWeight));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteChannels(
        Span<float> pixel,
        in ColorEndpointPair endpoint,
        int weight,
        DualPlanePixel? dualPlane)
    {
        if (endpoint.IsHdr)
        {
            WriteHdrChannels(pixel, in endpoint, weight, dualPlane);
        }
        else
        {
            WriteLdrAsHdrChannels(pixel, in endpoint, weight, dualPlane);
        }
    }

    /// <summary>
    /// Writes the four HDR-endpoint channels for a single pixel per ASTC spec §C.2.15: LNS →
    /// FP16 → float. Mode 14 alpha is LDR-as-UNORM16 (§C.2.14); HDR void-extent values are
    /// already FP16 bit patterns (§C.2.23) and skip the LNS conversion.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteHdrChannels(
        Span<float> pixel,
        in ColorEndpointPair endpoint,
        int weight,
        DualPlanePixel? dualPlane)
    {
        bool alphaIsLdr = endpoint.AlphaIsLdr;
        bool valuesAreLns = endpoint.ValuesAreLns;
        for (int channel = 0; channel < 4; ++channel)
        {
            int channelWeight = ChannelWeight(channel, weight, dualPlane);
            ushort interpolated = Interpolation.BlendWeightedAsUnorm16(
                endpoint.HdrLow.GetChannel(channel),
                endpoint.HdrHigh.GetChannel(channel),
                channelWeight);

            if (channel == 3 && alphaIsLdr)
            {
                // Mode 14 (spec §C.2.14): alpha is UNORM16, normalise directly.
                pixel[channel] = interpolated / 65535.0f;
            }
            else if (valuesAreLns)
            {
                // Normal HDR block (spec §C.2.15): LNS → FP16 → float.
                pixel[channel] = Fp16.LnsToFloat(interpolated);
            }
            else
            {
                // Void-extent HDR (spec §C.2.23): values are already FP16 bit patterns.
                pixel[channel] = Fp16.Fp16ToFloat(interpolated);
            }
        }
    }

    /// <summary>
    /// Writes the four LDR-endpoint channels for a single pixel as HDR floats: UNORM16 → [0,1].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteLdrAsHdrChannels(
        Span<float> pixel,
        in ColorEndpointPair endpoint,
        int weight,
        DualPlanePixel? dualPlane)
    {
        for (int channel = 0; channel < 4; ++channel)
        {
            int channelWeight = ChannelWeight(channel, weight, dualPlane);
            ushort unorm16 = Interpolation.BlendLdrReplicatedAsUnorm16(
                endpoint.LdrLow.GetChannel(channel),
                endpoint.LdrHigh.GetChannel(channel),
                channelWeight);
            pixel[channel] = unorm16 / 65535.0f;
        }
    }

    /// <summary>
    /// Returns <paramref name="primary"/> for ordinary channels and the dual-plane secondary
    /// weight only on the channel named in <paramref name="dualPlane"/>. Single-plane callers
    /// pass <c>null</c>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ChannelWeight(int channel, int primary, DualPlanePixel? dualPlane)
        => dualPlane is { } dp && channel == dp.Channel ? dp.Weight : primary;

    /// <summary>
    /// Per-pixel description of the dual-plane override for a single texel: the dual-plane
    /// channel index plus the secondary-plane weight. <c>null</c> means single-plane.
    /// </summary>
    private readonly record struct DualPlanePixel(int Channel, int Weight);
}
