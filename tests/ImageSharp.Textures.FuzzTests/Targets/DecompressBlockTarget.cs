// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="AstcDecoder.DecompressBlock"/>. Footprint is selected from the first
/// fuzz byte (one of the 14 spec-supported 2D footprints); the next 16 bytes drive a single
/// LDR block decode. Exercises endpoint decode, weight infill, and interpolation across all
/// footprint sizes (the previous fixed-4×4 target only hit one weight-grid shape).
/// </summary>
internal sealed class DecompressBlockTarget : IFuzzTarget
{
    public string Name => "decompress-block";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 1 + BlockInfo.SizeInBytes)
        {
            return;
        }

        Footprint footprint = Footprint.FromFootprintType((FootprintType)(data[0] % 14));

        // Worst-case 12×12 footprint = 144 pixels × 4 channels = 576 bytes; safe to stackalloc.
        Span<byte> output = stackalloc byte[footprint.PixelCount * BlockInfo.ChannelsPerPixel];
        try
        {
            AstcDecoder.DecompressBlock(data.Slice(1, BlockInfo.SizeInBytes), footprint, output);
        }
        catch (Common.Exceptions.TextureFormatException)
        {
            // Expected: LDR pipeline rejects HDR-content blocks per ASTC spec §C.2.19.
        }
    }
}
