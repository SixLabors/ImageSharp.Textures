// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="AstcDecoder.DecompressBlock"/>: a 16-byte fuzz input drives a fixed-footprint
/// LDR single-block decode. Exercises endpoint decode, weight infill, and interpolation through
/// the LDR pipeline.
/// </summary>
internal sealed class DecompressBlockTarget : IFuzzTarget
{
    public string Name => "decompress-block";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 16)
        {
            return;
        }

        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);
        Span<byte> output = stackalloc byte[footprint.PixelCount * BlockInfo.ChannelsPerPixel];
        try
        {
            AstcDecoder.DecompressBlock(data[..16], footprint, output);
        }
        catch (Common.Exceptions.TextureFormatException)
        {
            // Expected: LDR pipeline rejects HDR-content blocks per ASTC spec §C.2.19.
        }
    }
}
