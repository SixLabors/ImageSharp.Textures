// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Astc.IO;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

[MemoryDiagnoser]
public class AstcImageDecodeBenchmark
{
    private AstcFile? astcFile;

    [GlobalSetup]
    public void Setup()
    {
        string path = BenchmarkTestDataLocator.FindAstcTestData(Path.Combine("Input", "atlas_small_4x4.astc"));
        byte[] astcData = File.ReadAllBytes(path);
        this.astcFile = AstcFile.FromMemory(astcData);
    }

    [Benchmark]
    public void ImageDecode()
    {
        ReadOnlySpan<byte> blocks = this.astcFile!.Blocks;
        int numBlocks = blocks.Length / 16;
        Span<byte> blockBytes = stackalloc byte[16];
        for (int i = 0; i < numBlocks; ++i)
        {
            blocks.Slice(i * 16, 16).CopyTo(blockBytes);
            ulong low = BitConverter.ToUInt64(blockBytes);
            ulong high = BitConverter.ToUInt64(blockBytes.Slice(8));
            PhysicalBlock block = PhysicalBlock.Create((UInt128)low | ((UInt128)high << 64));
            IntermediateBlock.IntermediateBlockData? _ = IntermediateBlock.UnpackIntermediateBlock(block);
        }
    }
}
