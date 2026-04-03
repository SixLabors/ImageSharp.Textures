// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

[MemoryDiagnoser]
public class AstcDecodingBenchmark
{
    private AstcFile? astcFile;

    [GlobalSetup]
    public void Setup()
    {
        string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", "atlas_small_4x4.astc");
        byte[] astcData = File.ReadAllBytes(path);
        this.astcFile = AstcFile.FromMemory(astcData);
    }

    [Benchmark]
    public bool ParseBlock()
    {
        ReadOnlySpan<byte> blocks = this.astcFile!.Blocks;
        Span<byte> blockBytes = stackalloc byte[16];
        blocks[..16].CopyTo(blockBytes);
        ulong low = BitConverter.ToUInt64(blockBytes);
        ulong high = BitConverter.ToUInt64(blockBytes[8..]);
        PhysicalBlock physicalBlock = PhysicalBlock.Create((UInt128)low | ((UInt128)high << 64));

        return !physicalBlock.IsIllegalEncoding;
    }

    [Benchmark]
    public bool DecodeEndpoints()
    {
        ReadOnlySpan<byte> blocks = this.astcFile!.Blocks;
        Span<byte> blockBytes = stackalloc byte[16];
        blocks[..16].CopyTo(blockBytes);
        ulong low = BitConverter.ToUInt64(blockBytes);
        ulong high = BitConverter.ToUInt64(blockBytes[8..]);
        PhysicalBlock physicalBlock = PhysicalBlock.Create((UInt128)low | ((UInt128)high << 64));

        IntermediateBlock.IntermediateBlockData? blockData = IntermediateBlock.UnpackIntermediateBlock(physicalBlock);

        return blockData is not null;
    }

    [Benchmark]
    public bool Partitioning()
    {
        ReadOnlySpan<byte> blocks = this.astcFile!.Blocks;
        Span<byte> blockBytes = stackalloc byte[16];
        blocks[..16].CopyTo(blockBytes);
        ulong low = BitConverter.ToUInt64(blockBytes);
        ulong high = BitConverter.ToUInt64(blockBytes[8..]);
        UInt128 bits = (UInt128)low | ((UInt128)high << 64);
        BlockInfo info = BlockInfo.Decode(bits);
        LogicalBlock? logicalBlock = LogicalBlock.UnpackLogicalBlock(Footprint.Get4x4(), bits, in info)
            ?? throw new InvalidOperationException("Failed to unpack block");

        return logicalBlock is not null;
    }
}
