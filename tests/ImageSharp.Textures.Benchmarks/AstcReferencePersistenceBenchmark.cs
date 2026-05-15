// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AstcEncoder;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Reference;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

/// <summary>
/// Similar to <see cref="AstcReferenceComparisonBenchmark"/>, however this allocates a reference context
/// outside the benchmarked method and reuses it for all invocations with the same profile
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class AstcReferencePersistenceBenchmark
{
    public static IEnumerable<string> LdrFiles => ["rgba-4x4.astc", "rgba-6x6.astc", "rgba-8x8.astc", "rgb-4x4.astc"];

    public static IEnumerable<string> HdrFiles =>
        ["HdrPipeline/hdr-tile.astc", "HdrPipeline/ldr-tile.astc", "HdrPipeline/mixed-256-4x4.astc", "HdrPipeline/mixed-256-8x8.astc"];

    private readonly Dictionary<string, LdrInputs> ldrCache = [];
    private readonly Dictionary<string, HdrInputs> hdrCache = [];

    // One reference context per (profile, block-size) pair per input. Allocated in
    // GlobalSetup and freed in GlobalCleanup so the per-invocation cost is just the decode.
    private readonly Dictionary<(int X, int Y), AstcencContext> ldrContexts = [];
    private readonly Dictionary<(int X, int Y), AstcencContext> hdrContexts = [];

    [GlobalSetup]
    public void Setup()
    {
        foreach (string file in LdrFiles)
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            (int X, int Y) blockDims = AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type);
            this.ldrCache[file] = new LdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                blockDims,
                new byte[astc.Width * astc.Height * 4]);

            this.ldrContexts.TryAdd(blockDims, AstcReferenceDecoder.AllocDecodeContext(AstcencProfile.AstcencPrfLdr, blockDims.X, blockDims.Y));
        }

        foreach (string file in HdrFiles)
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            (int X, int Y) blockDims = AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type);
            int pixelCount = astc.Width * astc.Height;
            this.hdrCache[file] = new HdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                blockDims,
                new float[pixelCount * 4],
                new byte[pixelCount * 4 * sizeof(ushort)]);

            this.hdrContexts.TryAdd(blockDims, AstcReferenceDecoder.AllocDecodeContext(AstcencProfile.AstcencPrfHdr, blockDims.X, blockDims.Y));
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (AstcencContext context in this.ldrContexts.Values)
        {
            AstcReferenceDecoder.FreeContext(context);
        }

        foreach (AstcencContext context in this.hdrContexts.Values)
        {
            AstcReferenceDecoder.FreeContext(context);
        }

        this.ldrContexts.Clear();
        this.hdrContexts.Clear();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LDR")]
    [ArgumentsSource(nameof(LdrFiles))]
    public bool Ours_Ldr(string file)
    {
        LdrInputs i = this.ldrCache[file];
        return AstcDecoder.DecompressImage(i.Blocks, i.Width, i.Height, i.Footprint, i.Output);
    }

    [Benchmark]
    [BenchmarkCategory("LDR")]
    [ArgumentsSource(nameof(LdrFiles))]
    public void Reference_Ldr(string file)
    {
        LdrInputs i = this.ldrCache[file];
        AstcReferenceDecoder.DecompressLdrInto(this.ldrContexts[i.BlockDims], i.Blocks, i.Width, i.Height, i.Output);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HDR")]
    [ArgumentsSource(nameof(HdrFiles))]
    public bool Ours_Hdr(string file)
    {
        HdrInputs i = this.hdrCache[file];
        return AstcDecoder.DecompressHdrImage(i.Blocks, i.Width, i.Height, i.Footprint, i.Output);
    }

    [Benchmark]
    [BenchmarkCategory("HDR")]
    [ArgumentsSource(nameof(HdrFiles))]
    public void Reference_Hdr(string file)
    {
        HdrInputs i = this.hdrCache[file];
        AstcReferenceDecoder.DecompressHdrInto(this.hdrContexts[i.BlockDims], i.Blocks, i.Width, i.Height, i.OutputBytes);
    }

    private sealed record LdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, byte[] Output);

    private sealed record HdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, float[] Output, byte[] OutputBytes);
}
