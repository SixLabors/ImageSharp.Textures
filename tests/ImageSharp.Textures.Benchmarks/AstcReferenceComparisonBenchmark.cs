// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;
using SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Reference;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

/// <summary>
/// Compares whole-image decode performance between this library and the ARM reference,
/// summed across an LDR file set and an HDR file set. One-shot framing: the ARM reference
/// allocates its decode context per invocation. See <see cref="AstcReferencePersistenceBenchmark"/>
/// for the steady-state comparison where ARM's context is hoisted out of the measured path.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class AstcReferenceComparisonBenchmark
{
    /// <summary>
    /// LDR test files spanning a range of footprints and content shapes:
    /// 4×4 RGB / 4×4 RGBA / 6×6 / 8×8.
    /// </summary>
    private static readonly string[] LdrFiles = ["rgba-4x4.astc", "rgba-6x6.astc", "rgba-8x8.astc", "rgb-4x4.astc"];

    /// <summary>
    /// HDR test files: pure-HDR mid-size, mixed LDR/HDR at 4×4, and mixed LDR/HDR at 8×8.
    /// </summary>
    private static readonly string[] HdrFiles =
        ["HdrPipeline/hdr-tile.astc", "HdrPipeline/mixed-256-4x4.astc", "HdrPipeline/mixed-256-8x8.astc"];

    private LdrInputs[] ldrInputs = [];
    private HdrInputs[] hdrInputs = [];

    [GlobalSetup]
    public void Setup()
    {
        this.ldrInputs = [.. LdrFiles.Select(file =>
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            return new LdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type),
                new byte[astc.Width * astc.Height * 4]);
        })];

        this.hdrInputs = [.. HdrFiles.Select(file =>
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            return new HdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type),
                new float[astc.Width * astc.Height * 4]);
        })];
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LDR")]
    public bool ImageSharp_Ldr()
    {
        bool ok = true;
        foreach (LdrInputs i in this.ldrInputs)
        {
            ok &= AstcDecoder.DecompressImage(i.Blocks, i.Width, i.Height, i.Footprint, i.Output);
        }

        return ok;
    }

    [Benchmark]
    [BenchmarkCategory("LDR")]
    public int Reference_Ldr()
    {
        int total = 0;
        foreach (LdrInputs i in this.ldrInputs)
        {
            total += AstcReferenceDecoder.DecompressLdr(i.Blocks, i.Width, i.Height, i.BlockDims.X, i.BlockDims.Y).Length;
        }

        return total;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HDR")]
    public bool ImageSharp_Hdr()
    {
        bool ok = true;
        foreach (HdrInputs i in this.hdrInputs)
        {
            ok &= AstcDecoder.DecompressHdrImage(i.Blocks, i.Width, i.Height, i.Footprint, i.Output);
        }

        return ok;
    }

    [Benchmark]
    [BenchmarkCategory("HDR")]
    public int Reference_Hdr()
    {
        int total = 0;
        foreach (HdrInputs i in this.hdrInputs)
        {
            total += AstcReferenceDecoder.DecompressHdr(i.Blocks, i.Width, i.Height, i.BlockDims.X, i.BlockDims.Y).Length;
        }

        return total;
    }

    private sealed record LdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, byte[] Output);

    private sealed record HdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, float[] Output);
}
