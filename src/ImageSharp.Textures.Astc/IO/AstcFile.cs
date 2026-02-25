// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.IO;

/// <summary>
/// A very simple format consisting of a small header followed immediately
/// by the binary payload for a single image surface.
/// </summary>
/// <remarks>
/// See https://github.com/ARM-software/astc-encoder/blob/main/Docs/FileFormat.md
/// </remarks>
internal record AstcFile
{
    private readonly AstcFileHeader _header;
    private readonly byte[] _blocks;

    public ReadOnlySpan<byte> Blocks => _blocks;
    public Footprint Footprint { get; }
    public int Width => _header.ImageWidth;
    public int Height => _header.ImageHeight;
    public int Depth => _header.ImageDepth;

    internal AstcFile(AstcFileHeader header, byte[] blocks)
    {
        _header = header;
        _blocks = blocks;
        Footprint = GetFootprint();
    }

    public static AstcFile FromMemory(byte[] data)
    {
        var header = AstcFileHeader.FromMemory(data.AsSpan(0, AstcFileHeader.SizeInBytes));

        // Remaining bytes are blocks; C++ reference keeps them as string; here we keep as byte[]
        var blocks = new byte[data.Length - AstcFileHeader.SizeInBytes];
        Array.Copy(data, AstcFileHeader.SizeInBytes, blocks, 0, blocks.Length);

        return new AstcFile(header, blocks);
    }

    /// <summary>
    /// Map the block dimensions in the header to a Footprint, if possible.
    /// </summary>
    private Footprint GetFootprint() => (_header.BlockWidth, _header.BlockHeight) switch
    {
        (4, 4) => Footprint.FromFootprintType(FootprintType.Footprint4x4),
        (5, 4) => Footprint.FromFootprintType(FootprintType.Footprint5x4),
        (5, 5) => Footprint.FromFootprintType(FootprintType.Footprint5x5),
        (6, 5) => Footprint.FromFootprintType(FootprintType.Footprint6x5),
        (6, 6) => Footprint.FromFootprintType(FootprintType.Footprint6x6),
        (8, 5) => Footprint.FromFootprintType(FootprintType.Footprint8x5),
        (8, 6) => Footprint.FromFootprintType(FootprintType.Footprint8x6),
        (8, 8) => Footprint.FromFootprintType(FootprintType.Footprint8x8),
        (10, 5) => Footprint.FromFootprintType(FootprintType.Footprint10x5),
        (10, 6) => Footprint.FromFootprintType(FootprintType.Footprint10x6),
        (10, 8) => Footprint.FromFootprintType(FootprintType.Footprint10x8),
        (10, 10) => Footprint.FromFootprintType(FootprintType.Footprint10x10),
        (12, 10) => Footprint.FromFootprintType(FootprintType.Footprint12x10),
        (12, 12) => Footprint.FromFootprintType(FootprintType.Footprint12x12),
        _ => throw new ArgumentOutOfRangeException($"Unsupported block dimensions: {_header.BlockWidth}x{_header.BlockHeight}"),
    };
}
