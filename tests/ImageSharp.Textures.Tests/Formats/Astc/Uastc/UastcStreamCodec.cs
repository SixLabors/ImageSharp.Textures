// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Uastc;

/// <summary>
/// Test adapter that bridges the streaming <see cref="UastcDecoder"/> API to materialized arrays.
/// </summary>
internal static class UastcStreamCodec
{
    public static byte[] DecodeUastc(ReadOnlySpan<byte> uastcData, int width, int height, LdrDecodeMode mode = LdrDecodeMode.Linear)
    {
        using MemoryStream source = new(uastcData.ToArray());
        using MemoryStream destination = new();
        UastcDecoder.DecompressImage(source, destination, width, height, mode);

        return destination.ToArray();
    }
}
