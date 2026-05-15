# ImageSharp.Textures.FuzzTests

SharpFuzz harness for the ASTC decoder. Not part of CI — run manually when investigating
robustness against adversarial input.

## Targets

| Target name             | Entrypoint                                 |
|-------------------------|--------------------------------------------|
| `block-mode`            | `BlockModeDecoder.Decode`                  |
| `decompress-block`      | `AstcDecoder.DecompressBlock` (LDR)        |
| `decompress-hdr-block`  | `AstcDecoder.DecompressHdrBlock` (HDR)     |
| `decompress-image`      | `AstcDecoder.DecompressImage`              |

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

## libFuzzer mode

For coverage-guided fuzzing.

1. Install the SharpFuzz CLI:
   ```
   dotnet tool install --global SharpFuzz.CommandLine
   ```
2. Build and publish in Release:
   ```
   dotnet publish -c Release -o publish
   ```
3. Instrument the under-test assembly:
   ```
   sharpfuzz publish/SixLabors.ImageSharp.Textures.dll
   ```
4. Set `FUZZ_TARGET` and run libfuzzer-dotnet against the harness (libFuzzer consumes
   `args[0]` as the corpus path so target selection has to come from the environment):
   ```
   FUZZ_TARGET=block-mode libfuzzer-dotnet --target_path=publish/SixLabors.ImageSharp.Textures.FuzzTests -- corpus
   ```

## Adding a new target

1. Drop a new class in `Targets/` implementing `IFuzzTarget` — give it a stable `Name` (the
   CLI argument / `FUZZ_TARGET` value) and a `Run(ReadOnlySpan<byte>)` that calls the decoder
   entrypoint and swallows expected exceptions.
2. Append an instance to `FuzzTargetRegistry.All`.

`Program.cs` doesn't need to change.
