# UASTC (LDR 4×4) decoding

This directory implements decoding of **UASTC** texture blocks to RGBA8.

## What UASTC is

UASTC ("Universal ASTC") is a block format from [Basis Universal](https://github.com/BinomialLLC/basis_universal). It is a fixed-rate, 128-bit-per-block, 4×4-texel LDR format designed as a high-quality intermediate that can be transcoded losslessly to several GPU formats (ASTC, BC7, ETC, …) at load time. In KTX2 it is identified by `vkFormat = VK_FORMAT_UNDEFINED` together with a Data Format Descriptor whose `colorModel = KHR_DF_MODEL_UASTC (166)`, and the block payload is frequently Zstandard- or zlib-supercompressed on top.

## How it relates to ASTC

UASTC is a constrained subset of LDR ASTC 4×4. Every UASTC block maps to a legal ASTC 4×4 block, and the conversion is lossless. But the two are **not bit-compatible**:

- **Block layout differs.** A UASTC block begins with a variable-length Huffman *mode* prefix (one of 19 modes, plus a reserved mode 19), followed by transcode-hint fields, an optional partition-pattern index, an optional dual-plane component selector, endpoint data, and weights. ASTC instead starts with its own 11-bit block-mode field. The two encodings share neither field order nor bit packing.
- **Endpoints** use a non-interleaved trit/quint BISE packing (trit/quint bundle codes first, then the per-value low bits), unlike ASTC's interleaved BISE.
- **Weights** are stored LSB-first, the anchor texel of each subset stores one fewer bit.
- **Partitions** use UASTC's own small pattern tables (shared with BC7), not the ASTC partition hash function.

Because UASTC endpoints and weights are still ASTC-style values, this decoder reuses the existing ASTC back end for everything past the bit-parsing stage: endpoint/weight unquantization (`Quantization`), interpolation and pixel writing (`LogicalBlock.WriteDecodedLdr` → `LdrPixelWriter` → `Interpolation`). It decodes **directly to RGBA** — it does not transcode to an ASTC block first. Notably it does not apply ASTC blue-contraction, Basis direct-unpack path omits it.

## Modes

All 19 LDR modes are decoded: solid color (mode 8), single- and multi-subset RGB (CEM 8), RGBA (CEM 12) and luminance-alpha (CEM 4, output swizzled to `{L,L,L,A}`), including the dual-plane modes (6, 11, 13, 17). Mode 19 is reserved and yields the error colour (magenta).

## Sources

The mode tables, partition/anchor tables, Huffman codes, BISE range table, and the decode logic are ported from Basis Universal (Apache-2.0), primarily:

- `transcoder/basisu_transcoder_uastc.h` — table/struct declarations and constants.
- `transcoder/basisu_transcoder.cpp` — `unpack_uastc(...)` decode logic and table values.

The format is specified by the **UASTC LDR 4×4 Texture Specification** (Binomial), a copy is committed alongside this README at [`UASTC-LDR-4x4-Texture-Specification.md`](./UASTC-LDR-4x4-Texture-Specification.md) for reference. The canonical source is the [basis_universal wiki](https://github.com/BinomialLLC/basis_universal/wiki/UASTC-LDR-4x4-Texture-Specification). KTX2/DFD signalling of UASTC is defined by the [KTX2 specification](https://registry.khronos.org/KTX/specs/2.0/ktxspec.v2.html) and the Khronos Data Format Descriptor (`KHR_DF_MODEL_UASTC`). See the `NOTICE` file in this directory for attribution.
