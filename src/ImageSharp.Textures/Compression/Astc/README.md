Decoder for ASTC (Adaptive Scalable Texture Compression) textures, supporting both LDR and HDR content.

Originally developed as the standalone [AstcSharp](https://github.com/Erik-White/AstcSharp) library.

## Background

ASTC is a lossy block-based texture compression format developed by ARM and standardized by Khronos. It was designed as a single format to replace the patchwork of earlier GPU compression schemes (ETC, S3TC/DXT, PVRTC) that were each tied to specific hardware vendors or pixel formats.

### Key characteristics

- **Fixed block size** — Every compressed block is 128 bits (16 bytes), regardless of the footprint. This gives a constant memory bandwidth cost per block fetch on the GPU.
- **Variable footprint** — The block footprint ranges from 4x4 to 12x12 texels, giving bit rates from 8 bpp down to 0.89 bpp. Smaller footprints preserve more detail; larger footprints achieve higher compression.
- **LDR and HDR support** — The same format handles both standard 8-bit and high dynamic range content. HDR blocks use a different endpoint encoding that stores values as UNORM16 instead of UNORM8.
- **Partitions** — A single block can contain up to four partitions, each with its own pair of color endpoints. The decoder selects between partition layouts using a seed-based hash function.
- **Dual plane** — Blocks can optionally use a second set of interpolation weights for one color channel, improving quality for textures where one channel varies independently (e.g. alpha or normal maps).
- **Bounded Integer Sequence Encoding (BISE)** — Weights and color endpoint values are packed using a mixed radix encoding that combines bits with trits (base-3) or quints (base-5) to fill the 128-bit budget more efficiently than pure binary encoding.

### Block decoding overview

Decoding a single ASTC block involves:

1. Reading the block mode to determine the weight grid dimensions, quantization level, and whether the block is void-extent or standard
2. Unpacking the BISE-encoded interpolation weights and upsampling them to the texel grid
3. Decoding the color endpoints for each partition using the endpoint encoding mode (luminance, RGB, RGBA, or HDR variants)
4. For each texel, looking up its partition assignment, then interpolating between the two endpoints using the weight value

## Features

- Decode ASTC textures to RGBA32 (LDR) or RGBA float (HDR)
- All 2D block footprints from 4x4 to 12x12

## Decoding paths

The decoder employs three block decoding strategies:

1. **Direct decode** — Standard approach for normal blocks using batch unquantization without intermediate allocations
2. **Fused decode** — Accelerated path for single-partition, single-plane LDR blocks with combined decoding and interpolation
3. **Void extent** — Handles constant-color blocks

## Useful links

- [ASTC specification (Khronos Data Format Specification)](https://registry.khronos.org/DataFormat/specs/1.3/dataformat.1.3.html#ASTC)
- [ARM ASTC Encoder](https://github.com/ARM-software/astc-encoder)
- [Google astc-codec](https://github.com/google/astc-codec)
