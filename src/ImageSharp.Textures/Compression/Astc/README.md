# ASTC decoder

A managed C# decoder for [ASTC](https://registry.khronos.org/DataFormat/specs/1.3/dataformat.1.3.html#ASTC) (Adaptive Scalable Texture Compression) textures. Supports LDR and HDR content, all 14 two-dimensional block footprints from 4×4 to 12×12, and decodes to `Rgba32` (LDR) or `Rgba128Float` (HDR).

Originally developed as the standalone [AstcSharp](https://github.com/Erik-White/AstcSharp) library.

## Format overview

ASTC was designed by ARM and standardised by Khronos as a single replacement for the patchwork of earlier GPU compression schemes (S3TC/DXT, ETC, PVRTC). A few properties that shape the decoder's structure:

- **Fixed block size.** Every compressed block is 128 bits (16 bytes) regardless of the footprint. Larger footprints mean fewer bits per texel (bitrates range from 8 bpp at 4×4 down to 0.89 bpp at 12×12).
- **Variable footprint.** The 14 footprints share identical decoding logic — the footprint only affects weight grid sizing and texel-to-block mapping.
- **LDR / HDR content lives in the same container.** The container format (`VK_FORMAT_ASTC_*_UNORM_BLOCK`, `_SRGB_BLOCK`, `_SFLOAT_BLOCK`) declares the decode profile. HDR blocks use different endpoint encoding modes (2, 3, 7, 11, 14, 15) and emit UNORM16 rather than UNORM8 endpoints.
- **Up to four partitions.** A single block can contain up to four partitions, each with its own pair of colour endpoints. Partition assignment per texel is computed from a 10-bit seed via the spec's hash function (§C.2.21).
- **Dual plane.** Blocks can carry a second weight plane for one channel, useful when a channel varies independently (alpha, normal-map components). Spec §C.2.20.
- **Bounded Integer Sequence Encoding (BISE).** Weights and colour endpoint values are packed with a mixed-radix encoding that combines plain bits with trits (base 3) or quints (base 5), to fit more values in the 128-bit budget than plain binary encoding would allow. Spec §C.2.12.

## Code organisation

The code is organised by decoder concern rather than by spec chapter. `AstcDecoder` is the public entry point. Below it, `BlockDecoding` holds every per-block decode pipeline — the fused fast paths plus the general-purpose `LogicalBlock` pipeline — and the `IBlockPipeline<T>` dispatch strategy that routes LDR and HDR blocks through a shared loop. `ColorEncoding` and `BiseEncoding` isolate the two tricky encodings the spec defines for endpoint and weight values respectively, and `BiseEncoding` also owns the `BitStream` primitive used by the BISE codecs. `Core` holds the shared block-structure primitives — `BlockInfo` (the single-pass block-mode parser), footprints, decimation tables, partition, `UInt128` helpers, SIMD primitives, and scalar blend/FP16 helpers. `IO` covers `.astc` file parsing only.

This grouping makes it easier to change one decoder feature at a time: BISE changes stay inside `BiseEncoding`, endpoint-mode additions stay inside `ColorEncoding`, and the fused paths can be tuned without touching the general pipeline.

## Decoding pipelines

### Why three pipelines?

A straightforward ASTC decoder can get away with a single pipeline: read the 128-bit block, parse the mode, decompose into an intermediate representation (endpoint pairs + weight grid + partition map), then iterate texels and interpolate. That's what the spec describes and what `LogicalBlock` implements. It's correct, readable, and handles every ASTC feature.

It's also slow at scale. A 2048×2048 4×4 texture contains 262,144 blocks. Each block through the generic pipeline allocates a `LogicalBlock` (a reference type holding endpoint pairs, a weight array, and a partition map), plus intermediate arrays for the BISE-decoded values — all with GC pressure proportional to image size, and with memory traffic reading each intermediate back out on the pixel-write pass. So the decoder has fast paths for the cases that cover the overwhelming majority of real-world blocks.

The split is gated on three flags from `BlockModeDecoder.Decode`:

```csharp
!info.IsVoidExtent && info.PartitionCount == 1 && !info.IsDualPlane
```

Real-world ASTC content is overwhelmingly single-partition, single-plane, non-void-extent. The fast paths handle that; everything else falls through to the generic path.

### 1. Fused LDR fast path — `BlockDecoding/FusedLdrBlockDecoder.cs`

Used when all three gate conditions hold and the endpoint mode is LDR (modes 0, 1, 4, 5, 6, 8, 9, 10, 12, 13).

Instead of building a `LogicalBlock`, the fused path does this in one sweep per block:

1. **BISE-decode the colour endpoint values and weight values.** The shared helper `FusedBlockDecoder.DecodeBiseValues` / `DecodeBiseWeights` handles three BISE encoding modes (pure bits / trits / quints — spec §C.2.12) by extracting directly from the 128-bit block as a `UInt128`, bypassing the general `BitStream`. Pure-bit ranges that fit in 64 bits skip a `BitStream` entirely.
2. **Batch-unquantise both sequences.** Precomputed per-range maps (`BiseEncoding/Quantize/TritQuantizationMap.cs`, `QuintQuantizationMap.cs`, `BitQuantizationMap.cs`) convert the raw BISE values to endpoint/weight values in a single pass. These tables are built once at type-load time.
3. **Infill weights from the grid to the texel array** using the precomputed `DecimationTable.Get(footprint, gridW, gridH)` entry. For full-grid blocks (weight grid matches footprint), this is an identity pass.
4. **Write pixels directly into the destination image buffer.** No intermediate per-block scratch allocation. A `Vector128` SIMD path (`Core/SimdHelpers.cs`) interpolates and writes four pixels at a time when hardware acceleration is available; the scalar fallback produces byte-identical output.

Two sub-entry-points exist: `DecompressBlockFusedLdrToImage` writes straight to image-buffer coordinates for full-footprint interior blocks; `DecompressBlockFusedLdr` writes to a small scratch span for edge blocks that need cropping before the copy-out. Both share `FusedBlockDecoder.DecodeFusedCore`.

### 2. Fused HDR fast path — `BlockDecoding/FusedHdrBlockDecoder.cs`

Same structural shape as the LDR fast path: same gate, same `DecodeFusedCore`, same no-allocation discipline. Differences:

- Endpoint decoding goes through `ColorEncoding/HdrEndpointDecoder.cs` (spec §C.2.14) instead of the LDR decoder. HDR endpoint modes (2, 3, 7, 11, 14, 15) emit UNORM16 endpoints rather than UNORM8.
- Interpolation produces `float` RGBA rather than `byte` RGBA.
- Output target is `Rgba128Float` (4 × float32 per pixel) so the destination buffer stride differs.

HDR mode 14 (`HdrRgbDirectLdrAlpha`) is a hybrid — RGB is HDR but alpha is LDR. `FusedHdrBlockDecoder` handles that by branching on `endpointPair.AlphaIsLdr` and doing an LDR-style alpha interpolation with an 8-bit-to-float conversion, alongside the HDR RGB interpolation.

### 3. General (logical-block) path — `BlockDecoding/LogicalBlock.cs`

Everything else goes here. That includes:

- **Multi-partition blocks** (2, 3, or 4 partitions). The partition index for each texel is computed from a 10-bit seed plus the block position via the spec's hash function (`ColorEncoding/Partition.cs`, spec §C.2.21). Each partition has its own endpoint pair, so interpolation picks the endpoints based on the assigned partition per texel.
- **Dual-plane blocks.** A second weight grid drives one channel independently (spec §C.2.20). `LogicalBlock` stack-allocates a secondary-weight span and passes it (with the dual-plane channel index) to a dedicated dual-plane writer. Interpolation uses the dual-plane weight for the designated channel and the regular weight for the other three.
- **Void-extent blocks.** The entire block is a single constant colour (LDR UNORM16 or HDR FP16, distinguished by bit 9 — see design decisions below). Handled by a short-circuit branch in `LogicalBlock.DecodeSinglePlane` that reads the constant from the high half of the block and skips BISE decode entirely.
- **Mixed LDR/HDR blocks.** Any block where individual partitions use different LDR/HDR endpoint modes (legal per spec).

This path still decodes BISE, unquantises, computes partition assignments, and upsamples weights — the same work the fast paths fuse. The difference is that every intermediate result materialises in a stack-local `DecodedBlockState` (per-partition endpoint pairs + weight span + partition-assignment map), and the pixel write is a separate iteration that reads back from that state. The generic `WriteAllPixels<TWriter,T>` / `WriteAllPixelsDualPlane<TWriter,T>` loops dispatch through an `IPixelWriter<T>` (`LdrPixelWriter`/`HdrPixelWriter`) so the JIT specialises per output type, but per-pixel they still pay for partition lookup and (in the dual-plane variant) per-channel weight selection. More branches and more memory traffic than the fused paths; but it handles every ASTC feature the spec defines without hundreds of lines of specialised code per feature combination.

### Dispatching

`AstcDecoder.DecompressImage` and `DecompressBlock` read each 128-bit block, parse its mode via `BlockModeDecoder.Decode` (`BlockDecoding/BlockModeDecoder.cs`), check the fast-path gate, and route. The parser is a single pass over spec Tables 17–24: block mode classification, weight grid dimensions, partition count, CEM (colour endpoint mode) extraction, dual-plane flag, colour value count, reserved-configuration rejection — all in one pass with no allocations. It returns a `BlockInfo` (`Core/BlockInfo.cs`) struct the caller inspects for dispatch.

`BlockInfo.IsValid == false` means the block is reserved or illegal per spec. The decoder writes the spec-mandated error colour (magenta) into the corresponding image region rather than throwing or leaving zeros. `BlockInfo.IsHdr` covers both HDR endpoint modes (§C.2.14) and HDR void-extent blocks (§C.2.23, dynamic-range flag set); `IBlockPipeline.IsBlockLegal` returns false for HDR-mode blocks in the LDR pipeline so they get the same magenta treatment per §C.2.25.

## Design decisions

### Illegal blocks emit the spec-mandated error colour

Per spec §C.2.19, §C.2.24, §C.2.25 a decoder must emit the error colour (magenta `0xFFFF00FF` in LDR; `(1, 0, 1, 1)` floats in HDR) for every texel of:

* a reserved or illegal block encoding (e.g. reserved block-mode bits, weight count > 64, weight bits outside [24, 96], malformed void-extent);
* an HDR endpoint-mode block when decoded under the LDR profile.

This decoder emits magenta for both cases. ARM `astcenc` differs in two ways: it returns `ASTCENC_ERR_BAD_DECODE_MODE` from the API on the first HDR block in LDR mode (we don't — the spec describes per-texel behaviour, and a single bad block shouldn't fail the whole image), and its current build emits `(0, 0, 0, 1)` for some illegal-encoding cases. The spec text prescribes the error colour for both, which is what we do — so a real-world scenario like "one corrupt block in a 100MB texture" produces a mostly-correct image with visible magenta artefacts where the bad block lives, rather than a thrown exception or silent zeroes. Callers who need HDR values use `DecompressHdrImage` / `DecompressHdrBlock`; the same illegal-block rule applies, with the float error colour.

### LDR UNORM8 reduction takes the top 8 bits

Per spec §C.2.19 (Weight Application), the LDR-mode UNORM8 output for each channel is the **top 8 bits** of the UNORM16 interpolation result `C = floor((C0*(64-i) + C1*i + 32)/64)` — i.e. `byte = (C >> 8) & 0xFF`, not a "fair" UNORM16→UNORM8 round like `((C * 255) + 32767) / 65536`. The two formulas differ by 1 LSB at many `C` values, so the spec-mandated truncation is what `SimdHelpers.InterpolateChannelScalar` and `Interpolate4ChannelPixels` use. This matches ARM's `astcenc` (`lerp_color_int` in `astcenc_decompress_symbolic.cpp`) bit-exactly, which is what the comparison tests in `tests/.../Astc/Reference/` enforce.

### sRGB is not applied at decode time

Any `VK_FORMAT_ASTC_*_SRGB_BLOCK` container decodes to the raw UNORM8 values without an sRGB→linear transform. This matches the library-wide convention for `BC7` and friends — callers who need linear RGB apply the transform downstream. The sRGB *colour-space* tag is purely informational and passes through unchanged.

### Void-extent HDR flag convention

Bit 9 of the block-mode low bits distinguishes LDR (`= 1`, stored as UNORM16) from HDR (`= 0`, stored as FP16) for void-extent blocks. This matches ARM's reference decoder (`astcenc_symbolic_physical.cpp`: `if (block_mode & 0x200) SYM_BTYPE_CONST_F16`). A plausible inverse reading exists elsewhere online; we've verified this one against ARM.

### Thread-safe lazy caches

`DecimationTable.Table` (14 footprints × 11 × 11 grid cells) is lazy-initialised on first access and shared across threads. Publication uses `Volatile.Read` + `Interlocked.CompareExchange`. The cached objects are immutable, so a losing CAS race just drops the duplicate and returns the winner. No lock is held during `Compute`, so concurrent decoders don't serialise on first-use.

### Scratch buffers via `MemoryAllocator`

The image-level LDR and HDR entry points allocate their per-block scratch (and, for the `Stream` overloads, the staging buffer for the compressed payload) through `MemoryAllocator.Default.Allocate<T>`, returned as `IMemoryOwner<T>` and disposed with `using`. This routes through the same allocator ImageSharp uses elsewhere, gives us pool reuse without manual rent/return discipline, and removes the need for a `try`/`finally` to avoid leaking a rented buffer on exception. Inside individual block decoders, weight grids and per-partition endpoint buffers are `stackalloc`'d — the spec caps both at sizes (≤ 144 ints for weights, 4 endpoint pairs) that comfortably fit in a stack frame.

### `BitStream` shift boundaries

The 128-bit bit buffer (`BiseEncoding/BitStream.cs`) special-cases `count == 0` and `count >= 128` in `ShiftBuffer`. C# masks shift amounts to the operand width, so `ulong << 64` is `<< 0` (identity) rather than zero. Without the explicit guards, a zero-bit read would OR the high half into the low half, corrupting every subsequent read.

### Single-pass block mode decode

`BlockModeDecoder.Decode` parses the entire block mode, weight grid dimensions, partition count, CEM (colour endpoint mode) layout, dual-plane flag, and colour value count in one pass over the 128-bit block, rejecting reserved configurations inline.

## Decimation

A weight grid can be smaller than the texel grid (e.g. a 4×4 weight grid driving an 8×8 footprint). Each texel's weight is then a bilinear blend of up to four neighbouring grid weights. The precomputed index + factor tables live in `Core/DecimationTable.cs`, keyed by `(footprint, gridWidth, gridHeight)`. One table is shared across every block that uses that combination.

## Known limitations

- **2D only.** 3D ASTC footprints (`VK_FORMAT_ASTC_3x3x3_*_BLOCK` and relatives) are rejected at `AstcFileHeader.FromMemory`. The decoder's arithmetic and tables are 2D-only; adding 3D support would be a substantial rework of the decimation and partition paths.
- **Supercompressed KTX2 containers (ZSTD, ZLIB, BasisLZ).** Rejected at the KTX2 decoder level with `NotSupportedException` before reaching this decoder.

## Useful links

The `§C.2.X` spec citations throughout the source code (e.g. `§C.2.19` for Weight Application) are section numbers from the **OpenGL `KHR_texture_compression_astc_hdr` extension**. A copy of that document is kept alongside this README at [`KHR_texture_compression_astc_hdr.txt`](./KHR_texture_compression_astc_hdr.txt) for reference; the canonical source is at [registry.khronos.org](https://registry.khronos.org/OpenGL/extensions/KHR/KHR_texture_compression_astc_hdr.txt).

A secondary reference is the **Khronos Data Format Specification** (chapter 23), which covers the same ASTC content with a different numbering system. The PDF is committed at [`dataformat.1.3.pdf`](./dataformat.1.3.pdf) the HTML version is at [registry.khronos.org/DataFormat/specs/1.3](https://registry.khronos.org/DataFormat/specs/1.3/dataformat.1.3.html#ASTC). Section numbers do **not** match between the two documents — `§C.2.X` references in the code map to the OpenGL extension only.

- [ARM ASTC Encoder](https://github.com/ARM-software/astc-encoder) — the reference implementation; `astcenc_symbolic_physical.cpp` and `astcenc_decompress_symbolic.cpp` are the canonical read for decoder behaviour.
- [Google astc-codec](https://github.com/google/astc-codec) — a second reference; useful cross-check for bit-layout corner cases.
