// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="AstcDecoder.DecompressHdrBlock"/>: same shape as
/// <see cref="DecompressBlockTarget"/> but exercises the HDR pipeline including LNS conversion,
/// the mode-14 LDR-alpha hybrid, and HDR void-extent FP16 handling.
/// </summary>
internal sealed class DecompressHdrBlockTarget : IFuzzTarget
{
    public string Name => "decompress-hdr-block";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 16)
        {
            return;
        }

        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);
        Span<float> output = stackalloc float[footprint.PixelCount * BlockInfo.ChannelsPerPixel];
        AstcDecoder.DecompressHdrBlock(data[..16], footprint, output);
    }
}
