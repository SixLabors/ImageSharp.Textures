// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.IO;

/// <summary>
/// A very simple format consisting of a small header followed immediately
/// by the binary payload for a single image surface.
/// </summary>
/// <remarks>
/// See https://github.com/ARM-software/astc-encoder/blob/main/Docs/FileFormat.md
/// </remarks>
internal record AstcFile
{
    private readonly AstcFileHeader header;
    private readonly byte[] blocks;

    internal AstcFile(AstcFileHeader header, byte[] blocks)
    {
        this.header = header;
        this.blocks = blocks;
        this.Footprint = this.GetFootprint();
    }

    public ReadOnlySpan<byte> Blocks => this.blocks;

    public Footprint Footprint { get; }

    public int Width => this.header.ImageWidth;

    public int Height => this.header.ImageHeight;

    public int Depth => this.header.ImageDepth;

    public static AstcFile FromMemory(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, AstcFileHeader.SizeInBytes);

        AstcFileHeader header = AstcFileHeader.FromMemory(data.AsSpan(0, AstcFileHeader.SizeInBytes));

        int blockDataLength = data.Length - AstcFileHeader.SizeInBytes;
        ArgumentOutOfRangeException.ThrowIfNotEqual(blockDataLength % PhysicalBlock.SizeInBytes, 0);

        byte[] blocks = new byte[blockDataLength];
        Array.Copy(data, AstcFileHeader.SizeInBytes, blocks, 0, blocks.Length);

        return new AstcFile(header, blocks);
    }

    /// <summary>
    /// Map the block dimensions in the header to a Footprint, if possible.
    /// </summary>
    private Footprint GetFootprint() => (this.header.BlockWidth, this.header.BlockHeight) switch
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
        _ => throw new NotSupportedException($"Unsupported block dimensions: {this.header.BlockWidth}x{this.header.BlockHeight}"),
    };
}
