// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

/// <summary>
/// Parallel LDR-decode benchmark. Runs N concurrent <see cref="AstcDecoder.DecompressImage"/>
/// calls on a logical-path-heavy file. Designed to surface GC contention from per-block
/// allocations: parallel decodes that all churn Gen0 should scale worse than parallel decodes
/// that don't allocate at all.
/// </summary>
[MemoryDiagnoser]
public class AstcParallelDecodeBenchmark
{
    /// <summary>
    /// Number of concurrent decode tasks
    /// </summary>
    [Params(1, 4, 8, 16)]
    public int Parallelism { get; set; }

    // rgb-4x4.astc is ~75% logical-path per the BlockPathProfiler, so any per-block
    // allocation pressure shows up clearly here.
    private const string TestFile = "rgb-4x4.astc";

    private byte[] blocks = [];
    private int width;
    private int height;
    private Footprint footprint;
    private byte[][] outputs = [];

    [GlobalSetup]
    public void Setup()
    {
        string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", TestFile);
        AstcFile file = AstcFile.FromMemory(File.ReadAllBytes(path));
        this.blocks = file.Blocks.ToArray();
        this.width = file.Width;
        this.height = file.Height;
        this.footprint = file.Footprint;

        // Pre-allocate one output buffer per concurrent task so the benchmark measures only
        // the decode itself, not output-buffer allocation.
        this.outputs = new byte[this.Parallelism][];
        for (int i = 0; i < this.Parallelism; i++)
        {
            this.outputs[i] = new byte[file.Width * file.Height * 4];
        }
    }

    [Benchmark]
    public void DecompressParallel()
        => Parallel.For(0, this.Parallelism, i =>
            AstcDecoder.DecompressImage(this.blocks, this.width, this.height, this.footprint, this.outputs[i]));
}
