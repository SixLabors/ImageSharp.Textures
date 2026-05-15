// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="AstcDecoder.DecompressHdrBlock"/>. Footprint is selected from the first
/// fuzz byte; the next 16 bytes drive a single HDR block decode. Exercises the HDR pipeline
/// (LNS conversion, mode-14 LDR-alpha hybrid, HDR void-extent FP16) across all 14 footprint
/// sizes.
/// </summary>
internal sealed class DecompressHdrBlockTarget : IFuzzTarget
{
    public string Name => "decompress-hdr-block";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 1 + BlockInfo.SizeInBytes)
        {
            return;
        }

        Footprint footprint = Footprint.FromFootprintType((FootprintType)(data[0] % 14));

        // Worst-case 12×12 footprint = 144 pixels × 4 channels = 576 floats = 2,304 bytes.
        Span<float> output = stackalloc float[footprint.PixelCount * BlockInfo.ChannelsPerPixel];
        AstcDecoder.DecompressHdrBlock(data.Slice(1, BlockInfo.SizeInBytes), footprint, output);
    }
}
