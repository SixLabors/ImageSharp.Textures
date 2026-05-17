// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.FuzzTests;

/// <summary>
/// A single fuzz target. Each implementation maps to one entry in the
/// <c>FUZZ_TARGET</c> environment variable and exposes one decoder entrypoint to the fuzzer.
/// New targets are picked up automatically by <see cref="FuzzTargetRegistry"/> when their type
/// is referenced; no Program.cs change is required.
/// </summary>
internal interface IFuzzTarget
{
    /// <summary>
    /// Gets the value of <c>FUZZ_TARGET</c> that selects this target.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Invokes the decoder under test against one fuzz-supplied input. Implementations must not
    /// throw on adversarial input — any exception observed here is a bug in the decoder.
    /// </summary>
    void Run(ReadOnlySpan<byte> data);
}
