// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp.Textures.Compression.Astc;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Benchmarks;

[MemoryDiagnoser]
public class AstcFullDecodeBenchmark
{
    private byte[] ldrBlocks = [];
    private int ldrWidth;
    private int ldrHeight;
    private Footprint ldrFootprint;
    private byte[] ldrOutput = [];

    private byte[] hdrBlocks = [];
    private int hdrWidth;
    private int hdrHeight;
    private Footprint hdrFootprint;
    private float[] hdrOutput = [];

    [GlobalSetup]
    public void Setup()
    {
        string ldrPath = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", "rgba_4x4.astc");
        AstcFile ldr = AstcFile.FromMemory(File.ReadAllBytes(ldrPath));
        this.ldrBlocks = ldr.Blocks.ToArray();
        this.ldrWidth = ldr.Width;
        this.ldrHeight = ldr.Height;
        this.ldrFootprint = ldr.Footprint;
        this.ldrOutput = new byte[ldr.Width * ldr.Height * 4];

        string hdrPath = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc", "HDR", "hdr-tile.astc");
        AstcFile hdr = AstcFile.FromMemory(File.ReadAllBytes(hdrPath));
        this.hdrBlocks = hdr.Blocks.ToArray();
        this.hdrWidth = hdr.Width;
        this.hdrHeight = hdr.Height;
        this.hdrFootprint = hdr.Footprint;
        this.hdrOutput = new float[hdr.Width * hdr.Height * 4];
    }

    [Benchmark]
    public bool DecompressLdrImage()
        => AstcDecoder.DecompressImage(this.ldrBlocks, this.ldrWidth, this.ldrHeight, this.ldrFootprint, this.ldrOutput);

    [Benchmark]
    public bool DecompressHdrImage()
        => AstcDecoder.DecompressHdrImage(this.hdrBlocks, this.hdrWidth, this.hdrHeight, this.hdrFootprint, this.hdrOutput);
}
