// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="AstcDecoder.DecompressImage"/>. The fuzz input's first byte selects a
/// footprint and the remainder is treated as the raw block stream. Image dimensions are derived
/// to match the block count exactly, so the decoder runs through per-block code rather than
/// bailing on the size-mismatch precondition.
/// </summary>
internal sealed class DecompressImageTarget : IFuzzTarget
{
    // Worst-case output size: MaxInputBytes / 16 blocks × 12-wide × 12-high footprint × 4
    // channels. Reused across iterations to avoid per-call allocations.
    private readonly byte[] outputBuffer = new byte[
        (BuiltInFuzzRunner.MaxInputBytes / BlockInfo.SizeInBytes) * 12 * 12 * BlockInfo.ChannelsPerPixel];

    public string Name => "decompress-image";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 17)
        {
            return;
        }

        FootprintType footprintType = (FootprintType)(data[0] % 14);
        Footprint footprint;
        try
        {
            footprint = Footprint.FromFootprintType(footprintType);
        }
        catch (ArgumentOutOfRangeException)
        {
            return;
        }

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

        try
        {
            AstcDecoder.DecompressImage(blocks[..alignedLength], width, height, footprint, this.outputBuffer.AsSpan(0, outputLength));
        }
        catch (Common.Exceptions.TextureFormatException)
        {
            // Expected: LDR pipeline rejects HDR-content blocks per ASTC spec §C.2.19.
        }
    }
}
