// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Diagnostics;

namespace SixLabors.ImageSharp.Textures.FuzzTests;

/// <summary>
/// Self-driven fuzz runner. Runs a target through a deterministic per-seed RNG for
/// <see cref="Options.Seeds"/> seeds × <see cref="Options.IterationsPerSeed"/> iterations.
/// </summary>
internal static class BuiltInFuzzRunner
{
    /// <summary>
    /// Capped input size. Block-level targets early-return on inputs &lt; 16 bytes; 4 KB still
    /// gives the image target up to ~256 blocks of arbitrary content while keeping the per-call
    /// allocation cost low.
    /// </summary>
    public const int MaxInputBytes = 4 * 1024;

    /// <summary>
    /// Runs <paramref name="target"/> through the configured seed × iteration loop, reporting
    /// progress to stderr. Returns 0 on success, 1 if the target throws.
    /// </summary>
    public static int Run(IFuzzTarget target, Options options)
    {
        byte[] buffer = new byte[MaxInputBytes];
        long total = (long)options.Seeds * options.IterationsPerSeed;

        Console.Error.WriteLine(
            $"Built-in fuzz: target={target.Name}, seeds=[{options.SeedStart}..{options.SeedStart + options.Seeds}), iterations/seed={options.IterationsPerSeed:N0}, total={total:N0}");

        Stopwatch sw = Stopwatch.StartNew();
        long completed = 0;
        long progressMark = Math.Max(1, total / 20); // ~20 progress lines

        for (int seedOffset = 0; seedOffset < options.Seeds; seedOffset++)
        {
            int seed = options.SeedStart + seedOffset;
            Random rng = new(seed);
            for (int iter = 0; iter < options.IterationsPerSeed; iter++)
            {
                int length = rng.Next(0, MaxInputBytes + 1);
                rng.NextBytes(buffer.AsSpan(0, length));
                try
                {
                    target.Run(buffer.AsSpan(0, length));
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    Console.Error.WriteLine();
                    Console.Error.WriteLine($"FAIL: seed={seed} iter={iter} length={length}");
                    int hexCap = Math.Min(64, length);
                    Console.Error.WriteLine($"Input (first {hexCap} bytes): {Convert.ToHexString(buffer.AsSpan(0, hexCap))}");
                    Console.Error.WriteLine(ex);
                    return 1;
                }

                completed++;
                if (completed % progressMark == 0)
                {
                    double rate = completed / sw.Elapsed.TotalSeconds;
                    Console.Error.WriteLine($"  {completed:N0}/{total:N0} ({100.0 * completed / total:0.0}%) at {rate:N0}/sec");
                }
            }
        }

        sw.Stop();
        double finalRate = total / sw.Elapsed.TotalSeconds;
        Console.Error.WriteLine($"OK: {total:N0} iterations in {sw.Elapsed} ({finalRate:N0}/sec)");
        return 0;
    }

    /// <summary>
    /// Loop-shape parameters
    /// </summary>
    public readonly record struct Options(int Seeds, int SeedStart, int IterationsPerSeed)
    {
        public static Options Default { get; } = new(Seeds: 10, SeedStart: 0, IterationsPerSeed: 1_000_000);
    }
}
