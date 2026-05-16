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
/// Steady-state counterpart to <see cref="AstcReferenceComparisonBenchmark"/>. The ARM
/// reference's per-call context allocation is hoisted into <see cref="GlobalSetup"/>, so the
/// per-invocation cost is just the decode.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class AstcReferencePersistenceBenchmark
{
    private static readonly string[] LdrFiles = ["rgba-4x4.astc", "rgba-6x6.astc", "rgba-8x8.astc", "rgb-4x4.astc"];

    private static readonly string[] HdrFiles =
        ["HdrPipeline/hdr-tile.astc", "HdrPipeline/mixed-256-4x4.astc", "HdrPipeline/mixed-256-8x8.astc"];

    private LdrInputs[] ldrInputs = [];
    private HdrInputs[] hdrInputs = [];

    // One reference context per (profile, block-size) pair. Allocated in GlobalSetup and freed
    // in GlobalCleanup so the per-invocation cost is just the decode.
    private readonly Dictionary<(int X, int Y), AstcencContext> ldrContexts = [];
    private readonly Dictionary<(int X, int Y), AstcencContext> hdrContexts = [];

    [GlobalSetup]
    public void Setup()
    {
        this.ldrInputs = [.. LdrFiles.Select(file =>
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            (int X, int Y) blockDims = AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type);
            this.ldrContexts.TryAdd(blockDims, AstcReferenceDecoder.AllocDecodeContext(AstcencProfile.AstcencPrfLdr, blockDims.X, blockDims.Y));
            return new LdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                blockDims,
                new byte[astc.Width * astc.Height * 4]);
        })];

        this.hdrInputs = [.. HdrFiles.Select(file =>
        {
            string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", file);
            AstcFile astc = AstcFile.FromMemory(File.ReadAllBytes(path));
            (int X, int Y) blockDims = AstcReferenceDecoder.ToBlockDimensions(astc.Footprint.Type);
            int pixelCount = astc.Width * astc.Height;
            this.hdrContexts.TryAdd(blockDims, AstcReferenceDecoder.AllocDecodeContext(AstcencProfile.AstcencPrfHdr, blockDims.X, blockDims.Y));
            return new HdrInputs(
                astc.Blocks.ToArray(),
                astc.Width,
                astc.Height,
                astc.Footprint,
                blockDims,
                new float[pixelCount * 4],
                new byte[pixelCount * 4 * sizeof(ushort)]);
        })];
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
    public void Reference_Ldr()
    {
        foreach (LdrInputs i in this.ldrInputs)
        {
            AstcReferenceDecoder.DecompressLdrInto(this.ldrContexts[i.BlockDims], i.Blocks, i.Width, i.Height, i.Output);
        }
    }

    [Benchmark]
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

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HDR")]
    public void Reference_Hdr()
    {
        foreach (HdrInputs i in this.hdrInputs)
        {
            AstcReferenceDecoder.DecompressHdrInto(this.hdrContexts[i.BlockDims], i.Blocks, i.Width, i.Height, i.OutputBytes);
        }
    }

    [Benchmark]
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

    private sealed record LdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, byte[] Output);

    private sealed record HdrInputs(byte[] Blocks, int Width, int Height, Footprint Footprint, (int X, int Y) BlockDims, float[] Output, byte[] OutputBytes);
}
