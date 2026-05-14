// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

[MemoryDiagnoser]
public class AstcDecodingBenchmark
{
    private AstcFile? astcFile;

    [GlobalSetup]
    public void Setup()
    {
        string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", "rgba_4x4.astc");
        byte[] astcData = File.ReadAllBytes(path);
        this.astcFile = AstcFile.FromMemory(astcData);
    }

    [Benchmark]
    public bool DecodeBlockInfo()
    {
        ReadOnlySpan<byte> blocks = this.astcFile!.Blocks;
        Span<byte> blockBytes = stackalloc byte[16];
        blocks[..16].CopyTo(blockBytes);
        ulong low = BitConverter.ToUInt64(blockBytes);
        ulong high = BitConverter.ToUInt64(blockBytes[8..]);
        UInt128 bits = (UInt128)low | ((UInt128)high << 64);

        BlockInfo info = BlockModeDecoder.Decode(bits);

        return info.IsValid;
    }

    [Benchmark]
    public int Partitioning()
    {
        ReadOnlySpan<byte> blocks = this.astcFile!.Blocks;
        Span<byte> blockBytes = stackalloc byte[16];
        blocks[..16].CopyTo(blockBytes);
        ulong low = BitConverter.ToUInt64(blockBytes);
        ulong high = BitConverter.ToUInt64(blockBytes[8..]);
        UInt128 bits = (UInt128)low | ((UInt128)high << 64);
        BlockInfo info = BlockModeDecoder.Decode(bits);
        Footprint footprint = Footprint.Get4x4();
        Span<byte> pixels = stackalloc byte[footprint.PixelCount * 4];
        LogicalBlock.DecodeToBytes(bits, in info, footprint, pixels);
        return pixels[0];
    }
}
