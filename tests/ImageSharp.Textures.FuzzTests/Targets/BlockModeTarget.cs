// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.FuzzTests.Targets;

/// <summary>
/// Fuzzes <see cref="BlockModeDecoder.Decode"/>. Any 128-bit input must be classified as either
/// valid or invalid without throwing — a throw here means a structurally malformed block bypasses
/// the <c>IsValid</c> gate that protects every downstream stage of the decoder.
/// </summary>
internal sealed class BlockModeTarget : IFuzzTarget
{
    public string Name => "block-mode";

    public void Run(ReadOnlySpan<byte> data)
    {
        if (data.Length < 16)
        {
            return;
        }

        UInt128 bits = BinaryPrimitives.ReadUInt128LittleEndian(data[..16]);
        BlockInfo info = BlockModeDecoder.Decode(bits);
        _ = info.IsValid;
    }
}
