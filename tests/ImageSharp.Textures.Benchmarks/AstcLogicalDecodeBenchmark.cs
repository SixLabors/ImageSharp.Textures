// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

/// <summary>
/// LDR full-image decode benchmark across a set of test files spanning the full range of
/// fused-vs-logical-path proportions. Indicates how changes to the general (logical) decode
/// path affect content where that path dominates.
/// </summary>
/// <remarks>
/// Block path distribution per file:
/// rgb_12x12.astc — 90% fused / 10% logical (best case)
/// rgba_8x8.astc  — 63% fused / 37% logical
/// rgba_4x4.astc  — 46% fused / 54% logical
/// rgb_4x4.astc   — 25% fused / 75% logical (worst case)
/// </remarks>
[MemoryDiagnoser]
public class AstcLogicalDecodeBenchmark
{
    [ParamsSource(nameof(Files))]
    public string File { get; set; } = string.Empty;

    public static IEnumerable<string> Files =>
    [
        "rgb_12x12.astc",
        "rgba_8x8.astc",
        "rgba_4x4.astc",
        "rgb_4x4.astc",
    ];

    private byte[] blocks = [];
    private int width;
    private int height;
    private Footprint footprint;
    private byte[] output = [];

    [GlobalSetup]
    public void Setup()
    {
        string path = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", this.File);
        AstcFile file = AstcFile.FromMemory(System.IO.File.ReadAllBytes(path));
        this.blocks = file.Blocks.ToArray();
        this.width = file.Width;
        this.height = file.Height;
        this.footprint = file.Footprint;
        this.output = new byte[file.Width * file.Height * 4];
    }

    [Benchmark]
    public bool Decompress()
        => AstcDecoder.DecompressImage(this.blocks, this.width, this.height, this.footprint, this.output);
}
