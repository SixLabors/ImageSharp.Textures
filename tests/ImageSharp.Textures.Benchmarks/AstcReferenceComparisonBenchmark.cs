// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Reference;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

/// <summary>
/// Compares whole-image decode performance between this library and the ARM reference
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class AstcReferenceComparisonBenchmark
{
    /// <summary>
    /// Gets LDR test files. The set covers a range of footprints and content shapes:
    /// <list type="bullet">
    /// <item><description><c>rgba_4x4.astc</c> — high-quality 4×4 LDR; logical-block-heavy.</description></item>
    /// <item><description><c>rgba_6x6.astc</c> — mid-range LDR.</description></item>
    /// <item><description><c>rgba_8x8.astc</c> — large-footprint LDR; fewer blocks per image.</description></item>
    /// <item><description><c>rgb_4x4.astc</c> — dual-plane-heavy LDR.</description></item>
    /// </list>
    /// </summary>
    public static IEnumerable<string> LdrFiles => ["rgba_4x4.astc", "rgba_6x6.astc", "rgba_8x8.astc", "rgb_4x4.astc"];

    /// <summary>
    /// Gets HDR test files. <c>hdr-tile.astc</c> uses HDR endpoint modes; <c>ldr-tile.astc</c> is
    /// LDR content decoded through the HDR pipeline.
    /// </summary>
    public static IEnumerable<string> HdrFiles => ["HDR/hdr-tile.astc", "HDR/ldr-tile.astc"];

    private readonly Dictionary<string, LdrInputs> ldrCache = [];
    private readonly Dictionary<string, HdrInputs> hdrCache = [];

    [GlobalSetup]
    public void Setup()
    {
        foreach (string file in LdrFiles)
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            this.ldrCache[file] = new LdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type),
                new byte[astc.Width * astc.Height * 4]);
        }

        foreach (string file in HdrFiles)
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            this.hdrCache[file] = new HdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type),
                new float[astc.Width * astc.Height * 4]);
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("LDR")]
    [ArgumentsSource(nameof(LdrFiles))]
    public bool Ours_Ldr(string file)
    {
        LdrInputs i = this.ldrCache[file];
        return AstcDecoder.DecompressImage(i.Blocks, i.Width, i.Height, i.Footprint, i.Output);
    }

    [Benchmark, BenchmarkCategory("LDR")]
    [ArgumentsSource(nameof(LdrFiles))]
    public byte[] Reference_Ldr(string file)
    {
        LdrInputs i = this.ldrCache[file];
        return AstcReferenceDecoder.DecompressLdr(i.Blocks, i.Width, i.Height, i.BlockDims.X, i.BlockDims.Y);
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
    public Half[] Reference_Hdr(string file)
    {
        HdrInputs i = this.hdrCache[file];
        return AstcReferenceDecoder.DecompressHdr(i.Blocks, i.Width, i.Height, i.BlockDims.X, i.BlockDims.Y);
    }

    private sealed record LdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, byte[] Output);

    private sealed record HdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, float[] Output);
}
