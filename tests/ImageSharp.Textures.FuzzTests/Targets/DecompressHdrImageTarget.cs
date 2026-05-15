// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="AstcDecoder.DecompressHdrImage"/>. The fuzz input's first byte selects a footprint
/// and the remainder is treated as the raw block stream; image dimensions are derived to consume every
/// supplied block. Exercises the whole-image dispatch through the HDR pipeline including the
/// fused fast path, edge-cropping copy, and the LDR-as-HDR / HDR / mode-14 branches per block.
/// </summary>
internal sealed class DecompressHdrImageTarget : IFuzzTarget
{
    // Worst-case output size: MaxInputBytes / 16 blocks × 12-wide × 12-high footprint × 4
    // channels. Reused across iterations to avoid per-call allocations.
    private readonly float[] outputBuffer = new float[
        (BuiltInFuzzRunner.MaxInputBytes / BlockInfo.SizeInBytes) * 12 * 12 * BlockInfo.ChannelsPerPixel];

    public string Name => "decompress-hdr-image";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 17)
        {
            return;
        }

        Footprint footprint = Footprint.FromFootprintType((FootprintType)(data[0] % 14));

        ReadOnlySpan<byte> blocks = data[1..];
        int alignedLength = blocks.Length - (blocks.Length % BlockInfo.SizeInBytes);
        if (alignedLength == 0)
        {
            return;
        }

        int blockCount = alignedLength / BlockInfo.SizeInBytes;
        int width = blockCount * footprint.Width;
        int height = footprint.Height;
        int outputLength = width * height * BlockInfo.ChannelsPerPixel;

        AstcDecoder.DecompressHdrImage(blocks[..alignedLength], width, height, footprint, this.outputBuffer.AsSpan(0, outputLength));
    }
}
