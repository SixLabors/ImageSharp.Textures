# ImageSharp.Textures.FuzzTests

SharpFuzz-based fuzz harness for ImageSharp.Textures. Not part of CI — run manually when
investigating decoder robustness against adversarial input. Each fuzz target wraps a single
public entrypoint of the library; the harness is format-agnostic, and new targets can be
added for any decoder/parser surface.

## Built-in fuzz mode (default)

Drives the target with a deterministic per-seed RNG, no external fuzzer required. Defaults to
**10 seeds × 1,000,000 iterations = 10M inputs** per target.

```
dotnet run -c Release -- <target> [--seeds N] [--seed-start N] [--iterations N]
```

Examples:

```
# Quick smoke run, ~10k inputs, ~1 second.
dotnet run -c Release -- block-mode --seeds 1 --iterations 10000

# Full default sweep.
dotnet run -c Release -- decompress-block

# Continue from where a previous run stopped.
dotnet run -c Release -- decompress-image --seed-start 50 --seeds 50
```

When a target throws, the runner reports the seed, iteration, length, and a hex dump of the
first 64 bytes of the failing input — enough to reproduce by replaying the same seed up to
that iteration.

## libFuzzer mode (coverage-guided)

For real coverage-guided fuzzing on top of LLVM's libFuzzer engine.

### Run a fuzz session

```
pwsh tests/ImageSharp.Textures.FuzzTests/scripts/fuzz-libfuzzer.ps1 -Target block-mode
```

On first run the script downloads `libfuzzer-dotnet` (pinned release, cached
under `.tools/`), restores the SharpFuzz CLI from
`.config/dotnet-tools.json`, publishes and instruments the harness, and runs libFuzzer
against the per-target seed data (`SeedData/<target>/`). Findings (crashes / timeouts) are
written to `findings/<target>/`.

Subsequent runs skip the download — only publish + instrumentation + fuzz.

Parameters:
- `-Target <name>` — required; the name of a registered target.
- `-MaxTotalTime <seconds>` — wall-clock limit; default 300.
- `-Driver <path>` — override the libfuzzer-dotnet location (skips the auto-download).
  macOS contributors need this since upstream only ships Windows + Linux binaries; build
  via `clang -fsanitize=fuzzer libfuzzer-dotnet.cc -o libfuzzer-dotnet` and pass the
  resulting path.

### Triaging a crash

A crash file under `findings/<target>/` is the exact byte sequence that triggered the
exception. Replay it through the built-in mode by passing the file as a positional
argument — `Fuzzer.LibFuzzer.Run` falls back to reading a single input file when not
running under libFuzzer:

```
FUZZ_TARGET=block-mode dotnet run --project tests/ImageSharp.Textures.FuzzTests \
  -c Release -- findings/block-mode/crash-abc123
```

## Adding a new target

1. Drop a class in `Targets/` implementing `IFuzzTarget`. Give it a stable `Name` (used as
   the CLI argument / `FUZZ_TARGET` value) and a `Run(ReadOnlySpan<byte>)` that calls the
   decoder entrypoint under test. Catch and swallow exceptions that are part of the
   contract (e.g. format-rejection exceptions thrown intentionally by the decoder); let
   anything else propagate so the fuzzer flags it as a finding.
2. Append an instance to `FuzzTargetRegistry.All`.
3. Drop a few representative `*.bin` seeds under `SeedData/<target-name>/`. They should be
   plausibly-shaped inputs the decoder will accept (e.g. real compressed blocks, valid
   file headers) — libFuzzer mutates from these, so good seeds dramatically accelerate
   coverage discovery. The directory-local `SeedData/.gitattributes` routes them through
   git LFS automatically.
4. (Optional) extend the `-Target` `ValidateSet` in `scripts/fuzz-libfuzzer.ps1`.

`Program.cs` doesn't need to change — dispatch is by name through `FuzzTargetRegistry`.
