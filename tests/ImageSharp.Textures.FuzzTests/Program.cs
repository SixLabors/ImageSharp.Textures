// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SharpFuzz;

namespace SixLabors.ImageSharp.Textures.FuzzTests;

/// <summary>
/// Fuzz harness entrypoint. Two modes:
/// <list type="bullet">
/// <item><description>
///   <b>Built-in (default)</b>: <c>dotnet run -- &lt;target&gt; [--seeds N] [--seed-start N]
///   [--iterations N]</c> drives the target with a deterministic per-seed RNG, no external
///   fuzzer required.
/// </description></item>
/// <item><description>
///   <b>libFuzzer</b>: set <c>FUZZ_TARGET=&lt;target&gt;</c> in the environment and invoke the
///   harness through libfuzzer-dotnet. libFuzzer consumes <c>args[0]</c> as the test data path.
/// </description></item>
/// </list>
/// New targets are added by registering them in <see cref="FuzzTargetRegistry.All"/>.
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        string? envTarget = Environment.GetEnvironmentVariable("FUZZ_TARGET");
        if (!string.IsNullOrEmpty(envTarget))
        {
            return RunWithLibFuzzer(envTarget);
        }

        return RunBuiltIn(args);
    }

    private static int RunWithLibFuzzer(string targetName)
    {
        IFuzzTarget? target = FuzzTargetRegistry.TryGet(targetName);
        if (target is null)
        {
            Console.Error.WriteLine($"Unknown FUZZ_TARGET: {targetName}");
            PrintAvailableTargets();
            return 1;
        }

        Fuzzer.LibFuzzer.Run(target.Run);
        return 0;
    }

    private static int RunBuiltIn(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        IFuzzTarget? target = FuzzTargetRegistry.TryGet(args[0]);
        if (target is null)
        {
            Console.Error.WriteLine($"Unknown target: {args[0]}");
            PrintAvailableTargets();
            return 1;
        }

        if (!TryParseOptions(args.AsSpan(1), out BuiltInFuzzRunner.Options options, out string? error))
        {
            Console.Error.WriteLine(error);
            PrintUsage();
            return 1;
        }

        return BuiltInFuzzRunner.Run(target, options);
    }

    private static bool TryParseOptions(ReadOnlySpan<string> args, out BuiltInFuzzRunner.Options options, out string? error)
    {
        options = BuiltInFuzzRunner.Options.Default;
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--seeds":
                    if (!TryConsumeIntArg(args, ref i, "--seeds", out int seeds, out error))
                    {
                        return false;
                    }

                    options = options with { Seeds = seeds };
                    break;
                case "--seed-start":
                    if (!TryConsumeIntArg(args, ref i, "--seed-start", out int seedStart, out error))
                    {
                        return false;
                    }

                    options = options with { SeedStart = seedStart };
                    break;
                case "--iterations":
                    if (!TryConsumeIntArg(args, ref i, "--iterations", out int iterations, out error))
                    {
                        return false;
                    }

                    options = options with { IterationsPerSeed = iterations };
                    break;
                default:
                    error = $"Unknown argument: {args[i]}";
                    return false;
            }
        }

        error = null;
        return true;
    }

    private static bool TryConsumeIntArg(ReadOnlySpan<string> args, ref int i, string flag, out int value, out string? error)
    {
        if (i + 1 >= args.Length)
        {
            value = 0;
            error = $"{flag} requires a value";
            return false;
        }

        if (!int.TryParse(args[i + 1], out value) || value < 0)
        {
            error = $"{flag} requires a non-negative integer; got '{args[i + 1]}'";
            return false;
        }

        i++;
        error = null;
        return true;
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: dotnet run -- <target> [--seeds N] [--seed-start N] [--iterations N]");
        Console.Error.WriteLine($"  Defaults: --seeds {BuiltInFuzzRunner.Options.Default.Seeds}, --seed-start {BuiltInFuzzRunner.Options.Default.SeedStart}, --iterations {BuiltInFuzzRunner.Options.Default.IterationsPerSeed}");
        PrintAvailableTargets();
    }

    private static void PrintAvailableTargets()
    {
        Console.Error.Write("Targets: ");
        Console.Error.WriteLine(string.Join(" | ", FuzzTargetRegistry.All.Select(t => t.Name)));
    }
}
