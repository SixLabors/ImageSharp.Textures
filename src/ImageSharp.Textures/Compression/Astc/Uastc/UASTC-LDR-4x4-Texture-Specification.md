PUBLIC DOMAIN or CC0 (see below)

UASTC LDR 4x4 is a 19 mode 4x4 pixel LDR-only subset of the [ASTC](https://en.wikipedia.org/wiki/Adaptive_Scalable_Texture_Compression) GPU texture format with a simpler 128-bit block encoding. **UASTC is designed for efficient interchange of very high quality GPU texture data while being quickly transcodable to numerous other hardware GPU texture formats.** UASTC can be very quickly losslessly transcoded to the standard ASTC block format, quickly transcoded to BC7 with very low quality loss (within around .75-1.5 RGB dB PSNR relative to ASTC), or re-encoded to high quality ETC1/ETC2, ETC EAC, BC1-5, or PVRTC1 with a small amount of per-pixel work. **UASTC textures can be efficiently distributed and deployed to any GPU device using any graphics API on any platform**, making it unique compared to previous hardware-oriented GPU texture formats.

UASTC supports 9 opaque modes, 1 solid color mode, and 9 alpha modes. These UASTC modes each map to one of 6 BC7 modes (all except 0 and 4). The format supports the ASTC RGB, RGBA, and LA (Luminance-Alpha) Color Endpoint Modes (CEM's). Here's a good [high-level overview of ASTC](https://github.com/ARM-software/astc-encoder/blob/master/Docs/FormatOverview.md), and here's a [good introduction](http://www.reedbeta.com/blog/understanding-bcn-texture-compression-formats/) to the BCn GPU texture formats.

UASTC is the first high quality universal (or virtual) block-based texture format that supports block partitioning along with the ability to be efficiently transcoded to multiple GPU texture formats. Transcoding can either be done using the CPU or with a GPU compute shader. Transcoding UASTC to ASTC or BC7 involves only trivial per-texel level operations to convert each block to the desired encoded output format. Transcoding to ASTC or BC7 does not require unpacking the UASTC block to uncompressed 32-bpp RGBA pixels at all, because UASTC textures are encoded to a common subset of both 4x4 LDR ASTC and BC7.

UASTC's fields directly correspond to ASTC's fields whenever possible. A strong understanding of the BC7 and 4x4 LDR ASTC texture formats is assumed in this document.

The UASTC block format has hint bits to accelerate transcoding to various texture formats. There are up to two BC1 hint bits per block which direct the UASTC->BC1 transcoder to reuse the UASTC endpoint and/or weight indices (appropriately scaled) for faster real-time compression. On average ~60% of UASTC blocks don't need PCA, and ~30% don't need real-time BC1 encoding at all. There are also hint bits to accelerate transcoding to high quality ETC1 and ETC2 EAC A8. 

This document shows how each mode is laid out in a 128-bit UASTC block at the bit level. Bits are written starting from the beginning of the block (at the first byte's LSB) working "down" towards bit 128. The mode field is always first and is stored at bit 0 in the block (bit 0 of byte 0).

Quality-wise, the Basis Universal reference UASTC LDR 4x4 encoder is stronger than Intel's ispc_texcomp ASTC encoder (a bit shocking as ispc_texcomp has access to many more ASTC configurations than UASTC):

![Benchmark](uastc_quality_graph2.png "UASTC Quality Benchmark")

This is avg. RGBA (not RGB) PSNR across 34 RGB/RGBA test textures. Settings: UASTC encoder "slower" balanced profile, ispc_texcomp "astc_alpha_slow" profile, and for near-opt. BC7 we are using our SIMD BC7 compressor ([BC7E](https://github.com/richgel999/bc7enc_rdo)) in its "slow" profile (very similar to ispc_texcomp's BC7 "slower" profile).

Potential Future Additions
--------------------------

For now, as of 7/18/2020, no changes or additions are intended to the UASTC texture format, excluding critical changes required to fix bugs or security issues. This texture format is called either "UASTC" or "UASTC1". Any future additions will be called "UASTC2", etc.

Public Domain Declaration
-------------------------

The UASTC specification and block format is explicitly not copyrighted by any entity, and to our knowledge is patent free. It may be used for any purpose whatsoever, including commercial purposes. The author of this work hereby waives all claim of copyright (economic and moral) in this work and immediately places it in the public domain; it may be used, deployed, implemented or distributed in any manner whatsoever without further attribution or notice to the creator.

In jurisdictions where this declaration does not apply, the [The CC0 Public Domain Dedication](https://creativecommons.org/publicdomain/zero/1.0/) applies.

Note it is possible to encode high quality UASTC without using [Principle Component Analysis](https://en.wikipedia.org/wiki/Principal_component_analysis).

Visualizations of the Common ASTC/BC7 Partition Pattern Tables
==============================================================

UASTC supports the 2/3-subset partition patterns that BC7 and ASTC have in common. UASTC supports 60 of these common patterns, in three categories (2-subset, 3-subset, and 2-subset ASTC mapped to 3-subset BC7 with two endpoints set to equal vectors).  Here's a visualization of each category:

2-subset (30 patterns):

![Partition Patterns](astc_bc7_2subset_pats.png "ASTC_BC7_2SUBSETS")

3-subset (11 patterns):

![Partition Patterns](astc_bc7_3subset_pats.png "ASTC_BC7_3SUBSETS")

2 subset ASTC, 3 subset BC7 (19 patterns):

![Partition Patterns](bc7_3_astc2_2subset_pats.png "ASTC_BC7_3_ASTC2SUBSETS")

Another visualization of the 2/3-subset patterns, from a BC7 perspective (original pics from [here](https://rockets2000.wordpress.com/2015/05/19/bc7-partitions-subsets/)). The patterns supported by UASTC are circled in purple. Note this doesn't include the mixed 2-subset ASTC/3-subset BC7 patterns:

![Partition Patterns](bc7_2_subsets.png "BC7_2SUBSETS")

![Partition Patterns](bc7_3_subsets.png "BC7_3SUBSETS")

Field Definitions
=================

**MODE**: Huffman coded mode index (2-7 bits). The mode index range is [0,19]. One mode index (19) is saved for future expansion. The first bit of the Huffman code is the LSB, which is stored in bit 0 byte 0 of the UASTC block.

The Huffman codes and code lengths for each mode index are (this is a row major table, format is { code_value, code_len } ):
```
{ 0x1, 4 }, { 0x35, 6 }, { 0x1D, 5 }, { 0x3, 5 },
{ 0x13, 5 }, { 0xB, 5 }, { 0x1B, 5 }, { 0x7, 5 },
{ 0x17, 5 }, { 0xF, 5 }, { 0x2, 3 }, { 0x0, 2 },
{ 0x6, 3 }, { 0x1F, 5 }, { 0xD, 5 }, { 0x5, 7 },
{ 0x15, 6 }, { 0x25, 6 }, { 0x9, 4 }, { 0x45, 7 } 
```

Mode 0 uses a 4-bit Huffman code, mode 1 uses a 6-bit code, mode 2 uses a 5-bit code, etc.

The following Huffman decoding acceleration table (created from the list of Huffman codes), when accessed with the lowest 7 bits of the first byte in the UASTC block, contains the corresponding UASTC mode index:

```
static const uint8_t g_uastc_huff_modes[128] =
{ 
11,0,10,3,11,15,12,7,11,18,10,5,11,14,12,9,11,0,10,4,11,16,12,8,11,18,10,6,11,2,12,13,11,0,10,3,11,17,12,7,11,18,10,5,11,14,12,9,11,0,10,4,11,1,12,8,11,18,10,6,11,2,12,13,11,0,10,3,11,19,12,7,11,18,10,5,11,14,12,9,11,0,10,4,11,16,12,8,11,18,10,6,11,2,12,13,11,0,10,3,11,17,12,7,11,18,10,5,11,14,12,9,11,0,10,4,11,1,12,8,11,18,10,6,11,2,12,13
};
```

**BC1H0, BC1H1**: BC1 transcoding acceleration hints:

**BC1H0**: If set the transcoder can scale the first subset's UASTC endpoints to BC1 (5,6,5) endpoints, and then scale (or copy) the UASTC weight indices to BC1 2-bit weights. This skips the expensive PCA and least squares steps involved in real-time BC1 encoding.

**BC1H1**: If set the transcoder scales (or copies) the UASTC weight indices to BC1 2-bit weights. Least squares (1 or 2 iterations) can then immediately be used to compute the BC1 endpoints. This skips the expensive PCA step.

All modes (except for solid color) have BC1H0, and most modes have BC1H1.

**ETC1F, ETC1D, ETCI0, ETCI1**: 8-bits of ETC1 transcode hints (flipped subblocks flag bit, differential encoding flag bit, intensity table index 0, intensity table index 1).

These hints are used by the transcoder to quickly create ETC1 blocks from the unpacked UASTC texels. To use them, the transcoder computes each 4x2 or 2x4 subblock's average color, quantizes them to 555:333 or 444:444 bits, then computes the selectors in luma space. No other work is necessary (because all the hard work was done in the UASTC encoder).

**ETC1BIAS**: A 5-bit field indicating how to bias each ETC1's subset's computed block color. The encoder chooses the bias field which results in lowest overall ETC1 error. See the ETC1 bias helper function near the end of this document.

**ETC2TM**: 8-bits of ETC2 EAC A8 transcode hints (4-bit table, 4-bit multiplier)

This is similar to how ETC1 blocks are packed, except these hints are for the alpha portion of ETC2 EAC A8 blocks. These bits are only present in modes 9-17 (the alpha modes).

**ETQ**: Packed endpoint trits/quints values. A simplified form of BISE is used in UASTC, see:
https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#astc-integer-sequence-encoding

See the "UASTC BISE Endpoint Ranges table" below for the # of trits or quints for each endpoint range. Some of the ranges don't have trits or quints, so there will be no ETQ fields.

We store the trits/quints first, followed by each value's bits. The bit interleaving and trit/quint rearranging and preprocessing in section 18.2 aren't used. Instead the encoded trits/quints are stored in UASTC as-is.

For quints, each encoded value is up to 7-bits: quint2\*25+quint1\*5+quint0, and similar for trits except each encoded value is up to 8-bits. When the number of endpoint values isn't a multiple of 5 or 3 values, the size of the final code is the minimum # of bits necessary to represent the encoded value (to save bits).

**EBITS**: Endpoint bits (one set of bits per ASTC endpoint value). See the "UASTC BISE Endpoint Ranges table" below for the # of bits for each endpoint range. Endpoint order is the same as ASTC's: RL, RH, GL, GH, BL, BH, etc. Max of 18 values (RGB 3-subsets: 3\*2\*3).

To retrieve the endpoint values, you extract the trits/quints from the encoded ETQ values, shift each one left the appropriate number of bits (depending on the UASTC mode's endpoint range) and logically OR in the EBITS values.

Endpoint values are a sequence of integers that must be dequantized to [0,255] by following the ASTC spec in section 18.13, see:
https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#astc-endpoint-unquantization

**WEIGHTS**: Encoded weight indices. Just like BC7, the first weight of each subset's "anchor" texel index always has a MSB of 0, so these weights can be encoded with one less bit than the others. (UASTC doesn't use Blue Contraction so we can use this trick.)

Weights are always encoded as plain bits (no BISE necessary). Weight ordering is the same as ASTC's (raster order, left to right/top to bottom scanline). In dual plane mode, the ordering is also ASTC's: p0 p1, p0 p1, p0 p1, etc. (two weight indices per texel).

The weights are dequantized to 6-bit interpolation values in the same way as ASTC's:
https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#_weight_unquantization

And the endpoints are interpolated in the same way as ASTC's:
https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#astc_weight_application

**PAT**: Index into the common BC7/ASTC partition pattern table. This table contains BC7 pattern indices, ASTC pattern seeds, and permutation/flip flags which indicate how to map ASTC pattern subset indices to BC7's. There are three tables and 60 total partition patterns.

A UASTC decoder can either use ASTC's partition pattern generator or BC7's partition tables. To map ASTC's partition patterns to BC7's, the pattern subset indices are either used as-is, inverted, permuted, and/or combined to get BC7 partition pattern subset indices (see the tables/example code at the very bottom). These simple transformations correspond to changing the order of the encoded BC7 endpoints, or setting 2 endpoints in a 3-subset BC7 block to the same color/alpha values. Every ASTC pattern included in the below common tables maps to a BC7 pattern without loss (i.e. there is no subset "crosstalk" when mapping a UASTC to a BC7 pattern).

**COMPSEL**: ASTC's Color Component Selector (CCS) field. Only present in Dual Plane modes.
This maps to BC7 mode 5's 2-bit component rotation field. The CCS value must be remapped and the endpoint RGBA components reordered when transcoding to BC7.

ASTC and BC7 handle dual plane mode component rotations slightly differently. In ASTC, if the CCS field is 0 for red, the red component still appears in its usual position in the endpoint values, and the decoder uses the 2nd plane to separately interpolate the red (or green, or blue, etc.) channel. In BC7, if the component rotation field is 1 (red), the red component is swapped with alpha at encode time, the alpha component is always interpolated with the 2nd plane's weight indices by the decoder, and the components are then swapped after endpoint interpolation. To losslessly transcode ASTC dual plane modes to BC7, you have to swap the appropriate ASTC endpoint channel with the alpha channel.

Other Notes
-----------

- The number of color components is 2 for modes [15-17], 3 for modes [0-7] and 18, or 4 for modes [8-14].
- The number of subsets is [1-3].
- The total number of endpoint values is num_comps \* 2 \* num_subsets.
- The number of planes is either [1,2].
- The total number of weight values is either 16 (non-dual plane modes) or 32 (dual plane modes).
- Dual plane modes always have 1 subset in UASTC.
- For compatibility with BC7, BISE is not used at all for weight indices, only endpoints. Weight indices are always [1-5] bits.
- Various endpoint value ordering examples (UASTC and ASTC use the same endpoint orderings):

1 subset LA: LL0 LH0 AL0 AH0

1 subset RGB: RL0 RH0 GL0 GH0 BL0 BH0

1 subset RGBA: RL0 RH0 GL0 GH0 BL0 BH0 AL0 AH0

2 subset LA: LL0 LH0 AL0 AH0 LL1 LH1 AL1 AH1

2 subset RGB: RL0 RH0 GL0 GH0 BL0 BH0 RL1 RH1 GL1 GH1 BL1 BH1

2 subset RGBA: RL0 RH0 GL0 GH0 BL0 BH0 AL0 AH0 RL1 RH1 GL1 GH1 BL1 BH1 AL1 AH1

In dual plane mode, the UASTC components are NOT reordered like they would be in BC7. The COMPSEL field corresponds to the ASTC CCS field, which indicates which color component to separately interpolate with the 2nd plane weight indices:

Dual plane RGB: RL0 RH0 GL0 GH0 BL0 BH0

Dual plane RGBA: RL0 RH0 GL0 GH0 BL0 BH0 AL0 AH0

- Transcoding UASTC->ASTC is always a 100% lossless operation. The endpoints may need to be swapped (and the corresponding weight indices inverted) to disable blue contraction, but this is always a lossless transformation.
- The primary source of loss when transcoding UASTC->BC7 is mapping UASTC endpoints to BC7 endpoints. This is done using a simple scale with optional optimal p-bit computation. The UASTC weight indices are either copied as-is, or converted to the closest corresponding BC7 weight indices using a lookup table. The partition patterns are lossless, the weight tables are the same for 2/3-bits and very similar for 4-bits, and the endpoint interpolation method is nearly the same (16-bits in UASTC/ASTC, 8-bits with BC7, and both formats use [0,64] weights with rounding in the linear interpolation). 

If you implement your own independent UASTC->BC7 transcoder, you should implement the ASTC->BC7 endpoint/weight conversion process in the same exact way the Basis Universal encoder and transcoder does (which is documented below). Otherwise, you'll introduce more error than is necessary into your BC7 output. Ideally, it would be bit exact vs. what the Basis Universal transcoder outputs for BC7.

- Unlike ASTC, the weights are not stored in reverse bit order starting from the end of the block. Instead they are stored immediately following the endpoint bits in regular (LSB first) bit order.
- The ASTC CEM field(s) are either 4 (LA Direct) for modes 15-17, 8 (RGB Direct) for modes 0-7 and 18, or 12 (RGBA Direct) for modes 9-14. Blue Contraction isn't supported (i.e. the UASTC endpoints can be in arbitrary order, which we exploit to free up index bits like BC7 does). Mode 8 is void-extent.
- Weight index packing is similar to BC7's: Each subset's endpoints are swapped as necessary so the first weight index (that uses that subset) MSB is 0. If the mode uses a single subset, the first weight index MSB must be 0. For multiple subset modes, the first weight index written for each subset in the partition pattern must have an MSB of 0. The "anchor" texel indices for each pattern can be precomputed and stored in a table, like with BC7. This saves 1, 2, or 3 bits in the packed block which can be repurposed for other uses. In dual plane modes (which are always one subset), the first two weight indices must have an MSB of 0.
- We have not attempted to optimize the block format for efficient hardware RTL implementation.
- The LA Direct modes (15-17) must be transcoded to alpha BC7 modes using a "LLLA" swizzle.
- In the dual plane LA Direct mode (17), the CCS field is always 3.
- UASTC modes 4 and 7 are both 2 subset modes using 2-bit weights and endpoint range 12 (40 levels), so they have the same block encoding. Mode 4 uses the 2 subset partition pattern tables, and mode 7 uses the 2-subset ASTC/3-subset BC7 tables.
- UASTC doesn't use BC7-style p-bits. When transcoding UASTC to BC7, the transcoder must determine which p-bits to use on the fly. A fast example function to do this is below.
- We haven't fully documented here how to transcode UASTC to several formats, such as BC1 or ETC1, because doing so would be quite complex and take a lot of space. For those formats, it would be best to consult our transcoders in [`transcoder/basisu_transcoder.cpp`](https://github.com/BinomialLLC/basis_universal/blob/master/transcoder/basisu_transcoder.cpp) in the Basis Universal repo, which is fully open source.

List of UASTC Modes
===================

The format of "WeightRange" and "EndpointRange" is Range: Index (# of Quant Levels).
Format is "field: bit_offset num_bits"

<pre>
**** Mode: 0 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 8 (16), Subsets: 1, EndpointRange: 19 (192) - BC7 MODE 6 RGB

MODE: 0 4
BC1H0: 4 1
BC1H1: 5 1
ETC1F: 6 1
ETC1D: 7 1
ETC1I0: 8 3
ETC1I1: 11 3
ETC1BIAS: 14 5
ETQ: 19 8
ETQ: 27 2
EBITS: 29 6
EBITS: 35 6
EBITS: 41 6
EBITS: 47 6
EBITS: 53 6
EBITS: 59 6
WEIGHTS: 65 63
Total bits: 128, endpoint bits: 46, weight bits: 63

**** Mode: 1 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 1, EndpointRange: 20 (256) - BC7 MODE 3

MODE: 0 6
BC1H0: 6 1
BC1H1: 7 1
ETC1F: 8 1
ETC1D: 9 1
ETC1I0: 10 3
ETC1I1: 13 3
ETC1BIAS: 16 5
EBITS: 21 8
EBITS: 29 8
EBITS: 37 8
EBITS: 45 8
EBITS: 53 8
EBITS: 61 8
WEIGHTS: 69 31
Total bits: 100, endpoint bits: 48, weight bits: 31

**** Mode: 2 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 5 (8), Subsets: 2, EndpointRange: 8 (16) - BC7 MODE 1

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
PAT: 20 5
EBITS: 25 4
EBITS: 29 4
EBITS: 33 4
EBITS: 37 4
EBITS: 41 4
EBITS: 45 4
EBITS: 49 4
EBITS: 53 4
EBITS: 57 4
EBITS: 61 4
EBITS: 65 4
EBITS: 69 4
WEIGHTS: 73 46
Total bits: 119, endpoint bits: 48, weight bits: 46

**** Mode: 3 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 3, EndpointRange: 7 (12) - BC7 MODE 2

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
PAT: 20 4
ETQ: 24 8
ETQ: 32 8
ETQ: 40 8
ETQ: 48 5
EBITS: 53 2
EBITS: 55 2
EBITS: 57 2
EBITS: 59 2
EBITS: 61 2
EBITS: 63 2
EBITS: 65 2
EBITS: 67 2
EBITS: 69 2
EBITS: 71 2
EBITS: 73 2
EBITS: 75 2
EBITS: 77 2
EBITS: 79 2
EBITS: 81 2
EBITS: 83 2
EBITS: 85 2
EBITS: 87 2
WEIGHTS: 89 29
Total bits: 118, endpoint bits: 65, weight bits: 29

**** Mode: 4 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 2, EndpointRange: 12 (40) - BC7 MODE 3

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
PAT: 20 5
ETQ: 25 7
ETQ: 32 7
ETQ: 39 7
ETQ: 46 7
EBITS: 53 3
EBITS: 56 3
EBITS: 59 3
EBITS: 62 3
EBITS: 65 3
EBITS: 68 3
EBITS: 71 3
EBITS: 74 3
EBITS: 77 3
EBITS: 80 3
EBITS: 83 3
EBITS: 86 3
WEIGHTS: 89 30
Total bits: 119, endpoint bits: 64, weight bits: 30

**** Mode: 5 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 5 (8), Subsets: 1, EndpointRange: 20 (256) - BC7 MODE 6 RGB

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
EBITS: 20 8
EBITS: 28 8
EBITS: 36 8
EBITS: 44 8
EBITS: 52 8
EBITS: 60 8
WEIGHTS: 68 47
Total bits: 115, endpoint bits: 48, weight bits: 47

**** Mode: 6 (CEM 8 - RGB Direct)
DualPlane: 1, WeightRange: 2 (4), Subsets: 1, EndpointRange: 18 (160) - BC7 MODE 5 RGB

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
COMPSEL: 20 2
ETQ: 22 7
ETQ: 29 7
EBITS: 36 5
EBITS: 41 5
EBITS: 46 5
EBITS: 51 5
EBITS: 56 5
EBITS: 61 5
WEIGHTS: 66 62
Total bits: 128, endpoint bits: 44, weight bits: 62

**** Mode: 7 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 2, EndpointRange: 12 (40) - BC7 MODE 2

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
PAT: 20 5
ETQ: 25 7
ETQ: 32 7
ETQ: 39 7
ETQ: 46 7
EBITS: 53 3
EBITS: 56 3
EBITS: 59 3
EBITS: 62 3
EBITS: 65 3
EBITS: 68 3
EBITS: 71 3
EBITS: 74 3
EBITS: 77 3
EBITS: 80 3
EBITS: 83 3
EBITS: 86 3
WEIGHTS: 89 30
Total bits: 119, endpoint bits: 64, weight bits: 30

**** Mode: 8 (Void-Extent)
Void-Extent: Solid Color RGBA (BC7 MODE 5 or MODE 6)

MODE: 0 5
R: 5 8
G: 13 8
B: 21 8
A: 29 8
ETC1D: 37 1
ETC1I: 38 3
ETC1S: 41 2
ETC1R: 43 5
ETC1G: 48 5
ETC1B: 53 5
Total bits: 58

**** Mode: 9 (CEM 12 - RGBA Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 2, EndpointRange: 8 (16) - BC7 MODE 7

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
ETC2TM: 20 8
PAT: 28 5
EBITS: 33 4
EBITS: 37 4
EBITS: 41 4
EBITS: 45 4
EBITS: 49 4
EBITS: 53 4
EBITS: 57 4
EBITS: 61 4
EBITS: 65 4
EBITS: 69 4
EBITS: 73 4
EBITS: 77 4
EBITS: 81 4
EBITS: 85 4
EBITS: 89 4
EBITS: 93 4
WEIGHTS: 97 30
Total bits: 127, endpoint bits: 64, weight bits: 30

**** Mode: 10 (CEM 12 - RGBA Direct)
DualPlane: 0, WeightRange: 8 (16), Subsets: 1, EndpointRange: 13 (48) - BC7 MODE 6

MODE: 0 3
BC1H0: 3 1
ETC1F: 4 1
ETC1D: 5 1
ETC1I0: 6 3
ETC1I1: 9 3
ETC2TM: 12 8
ETQ: 20 8
ETQ: 28 5
EBITS: 33 4
EBITS: 37 4
EBITS: 41 4
EBITS: 45 4
EBITS: 49 4
EBITS: 53 4
EBITS: 57 4
EBITS: 61 4
WEIGHTS: 65 63
Total bits: 128, endpoint bits: 45, weight bits: 63

**** Mode: 11 (CEM 12 - RGBA Direct)
DualPlane: 1, WeightRange: 2 (4), Subsets: 1, EndpointRange: 13 (48) - BC7 MODE 5

MODE: 0 2
BC1H0: 2 1
ETC1F: 3 1
ETC1D: 4 1
ETC1I0: 5 3
ETC1I1: 8 3
ETC2TM: 11 8
COMPSEL: 19 2
ETQ: 21 8
ETQ: 29 5
EBITS: 34 4
EBITS: 38 4
EBITS: 42 4
EBITS: 46 4
EBITS: 50 4
EBITS: 54 4
EBITS: 58 4
EBITS: 62 4
WEIGHTS: 66 62
Total bits: 128, endpoint bits: 45, weight bits: 62

**** Mode: 12 (CEM 12 - RGBA Direct)
DualPlane: 0, WeightRange: 5 (8), Subsets: 1, EndpointRange: 19 (192) - BC7 MODE 6

MODE: 0 3
BC1H0: 3 1
ETC1F: 4 1
ETC1D: 5 1
ETC1I0: 6 3
ETC1I1: 9 3
ETC2TM: 12 8
ETQ: 20 8
ETQ: 28 5
EBITS: 33 6
EBITS: 39 6
EBITS: 45 6
EBITS: 51 6
EBITS: 57 6
EBITS: 63 6
EBITS: 69 6
EBITS: 75 6
WEIGHTS: 81 47
Total bits: 128, endpoint bits: 61, weight bits: 47

**** Mode: 13 (CEM 12 - RGBA Direct)
DualPlane: 1, WeightRange: 0 (2), Subsets: 1, EndpointRange: 20 (256) - BC7 MODE 5

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
ETC2TM: 20 8
COMPSEL: 28 2
EBITS: 30 8
EBITS: 38 8
EBITS: 46 8
EBITS: 54 8
EBITS: 62 8
EBITS: 70 8
EBITS: 78 8
EBITS: 86 8
WEIGHTS: 94 30
Total bits: 124, endpoint bits: 64, weight bits: 30

**** Mode: 14 (CEM 12 - RGBA Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 1, EndpointRange: 20 (256) - BC7 MODE 6

MODE: 0 5
BC1H0: 5 1
BC1H1: 6 1
ETC1F: 7 1
ETC1D: 8 1
ETC1I0: 9 3
ETC1I1: 12 3
ETC1BIAS: 15 5
ETC2TM: 20 8
EBITS: 28 8
EBITS: 36 8
EBITS: 44 8
EBITS: 52 8
EBITS: 60 8
EBITS: 68 8
EBITS: 76 8
EBITS: 84 8
WEIGHTS: 92 31
Total bits: 123, endpoint bits: 64, weight bits: 31

**** Mode: 15 (CEM 4 - LA Direct)
DualPlane: 0, WeightRange: 8 (16), Subsets: 1, EndpointRange: 20 (256) - BC7 MODE 6

MODE: 0 7
BC1H0: 7 1
BC1H1: 8 1
ETC1F: 9 1
ETC1D: 10 1
ETC1I0: 11 3
ETC1I1: 14 3
ETC1BIAS: 17 5
ETC2TM: 22 8
EBITS: 30 8
EBITS: 38 8
EBITS: 46 8
EBITS: 54 8
WEIGHTS: 62 63
Total bits: 125, endpoint bits: 32, weight bits: 63

**** Mode: 16 (CEM 4 - LA Direct)
DualPlane: 0, WeightRange: 2 (4), Subsets: 2, EndpointRange: 20 (256) - BC7 MODE 7

MODE: 0 6
BC1H0: 6 1
BC1H1: 7 1
ETC1F: 8 1
ETC1D: 9 1
ETC1I0: 10 3
ETC1I1: 13 3
ETC1BIAS: 16 5
ETC2TM: 21 8
PAT: 29 5
EBITS: 34 8
EBITS: 42 8
EBITS: 50 8
EBITS: 58 8
EBITS: 66 8
EBITS: 74 8
EBITS: 82 8
EBITS: 90 8
WEIGHTS: 98 30
Total bits: 128, endpoint bits: 64, weight bits: 30

**** Mode: 17 (CEM 4 - LA Direct)
DualPlane: 1, WeightRange: 2 (4), Subsets: 1, EndpointRange: 20 (256) - BC7 MODE 5

MODE: 0 6
BC1H0: 6 1
BC1H1: 7 1
ETC1F: 8 1
ETC1D: 9 1
ETC1I0: 10 3
ETC1I1: 13 3
ETC1BIAS: 16 5
ETC2TM: 21 8
EBITS: 29 8
EBITS: 37 8
EBITS: 45 8
EBITS: 53 8
WEIGHTS: 61 62
Total bits: 123, endpoint bits: 32, weight bits: 62

**** Mode: 18 (CEM 8 - RGB Direct)
DualPlane: 0, WeightRange: 11 (32), Subsets: 1, EndpointRange: 11 (32) - BC7 MODE 6

MODE: 0 4
BC1H0: 4 1
BC1H1: 5 1
ETC1F: 6 1
ETC1D: 7 1
ETC1I0: 8 3
ETC1I1: 11 3
ETC1BIAS: 14 5
EBITS: 19 5
EBITS: 24 5
EBITS: 29 5
EBITS: 34 5
EBITS: 39 5
EBITS: 44 5
WEIGHTS: 49 79
Total bits: 128, endpoint bits: 30, weight bits: 79
</pre>

UASTC/ASTC BISE Ranges Table
============================

<pre>
Range Bits Trits Quints Levels
0     1    0     0      0-1
1     0    1     0      0-2
2     2    0     0      0-3
                        
3     0    0     1      0-4
4     1    1     0      0-5
5     3    0     0      0-7
                        
6     1    0     1      0-9
7     2    1     0      0-11
8     4    0     0      0-15
                        
9     2    0     1      0-19
10    3    1     0      0-23
11    5    0     0      0-31
                        
12    3    0     1      0-39
13    4    1     0      0-47
14    6    0     0      0-63
                        
15    4    0     1      0-79
16    5    1     0      0-95
17    7    0     0      0-127
                        
18    5    0     1      0-159
19    6    1     0      0-191

20    8    0     0      0-255
</pre>

UASTC BISE Weight Index Ranges Table
====================================

<pre>
Range    Bits Trits Quints       UASTC Modes                  Levels
0        1                       13                           2
2        2                       1 3 4 6 7 6 9 11 14 16 17    4
5        3                       2 5 12                       8
8        4                       0 10 15                      16
11       5                       18                           32
</pre>

(UASTC weights do not use trits or quints.)

UASTC BISE Endpoint Ranges Table
================================

<pre>
Range    Bits Trits Quints       UASTC Modes           Quant. Levels
7        2    1                  3                     12
8        4                       2 9                   16
11       5                       18                    32
12       3          1            4 7                   40
13       4    1                  10 11                 48
18       5          1            6                     160
19       6    1                  0 12                  192
20       8                       1 5 13 14 15 16 17    256
</pre>

UASTC/BC7 2-Subset Partition Pattern Table
==========================================

```
uint32_t TOTAL_ASTC_BC7_COMMON_PARTITIONS2 = 30

struct
{
  int m_bc7_pattern;
  int m_astc_seed;
// if true, invert the BC7 pattern's subset index to match ASTC's subset index
  bool m_invert;
} g_uastc_bc7_common_partitions2[TOTAL_ASTC_BC7_COMMON_PARTITIONS2] =
{
  { 0, 28, false  }, { 1, 20, false }, { 2, 16, true }, { 3, 29, false },
  { 4, 91, true }, { 5, 9, false }, { 6, 107, true }, { 7, 72, true },
  { 8, 149, false }, { 9, 204, true }, { 10, 50, false }, { 11, 114, true },
  { 12, 496, true }, { 13, 17, true }, { 14, 78, false }, { 15, 39, true }, 
  { 17, 252, true }, { 18, 828, true }, { 19, 43, false }, { 20, 156, false }, 
  { 21, 116, false }, { 22, 210, true }, { 23, 476, true }, { 24, 273, false },
  { 25, 684, true }, { 26, 359, false }, { 29, 246, true }, { 32, 195, true },
  { 33, 694, true }, { 52, 524, true }
};

// UASTC pattern table for the 2-subset modes
const uint8_t g_uastc_patterns2[TOTAL_ASTC_BC7_COMMON_PARTITIONS2][16] =
{
   { 0,0,1,1,0,0,1,1,0,0,1,1,0,0,1,1 }, { 0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,1 }, 
   { 1,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0 }, { 0,0,0,1,0,0,1,1,0,0,1,1,0,1,1,1 },
   { 1,1,1,1,1,1,1,0,1,1,1,0,1,1,0,0 }, { 0,0,1,1,0,1,1,1,0,1,1,1,1,1,1,1 }, 
   { 1,1,1,0,1,1,0,0,1,0,0,0,0,0,0,0 }, { 1,1,1,1,1,1,1,0,1,1,0,0,1,0,0,0 },
   { 0,0,0,0,0,0,0,0,0,0,0,1,0,0,1,1 }, { 1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0 }, 
   { 0,0,0,0,0,0,0,1,0,1,1,1,1,1,1,1 }, { 1,1,1,1,1,1,1,1,1,1,1,0,1,0,0,0 },
   { 1,1,1,0,1,0,0,0,0,0,0,0,0,0,0,0 }, { 1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0 }, 
   { 0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1 }, { 1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0 },
   { 1,0,0,0,1,1,1,0,1,1,1,1,1,1,1,1 }, { 1,1,1,1,1,1,1,1,0,1,1,1,0,0,0,1 }, 
   { 0,1,1,1,0,0,1,1,0,0,0,1,0,0,0,0 }, { 0,0,1,1,0,0,0,1,0,0,0,0,0,0,0,0 },
   { 0,0,0,0,1,0,0,0,1,1,0,0,1,1,1,0 }, { 1,1,1,1,1,1,1,1,0,1,1,1,0,0,1,1 }, 
   { 1,0,0,0,1,1,0,0,1,1,0,0,1,1,1,0 }, { 0,0,1,1,0,0,0,1,0,0,0,1,0,0,0,0 },
   { 1,1,1,1,0,1,1,1,0,1,1,1,0,0,1,1 }, { 0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0 }, 
   { 1,1,1,1,0,0,0,0,0,0,0,0,1,1,1,1 }, { 1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0 },
   { 1,1,1,1,0,0,0,0,1,1,1,1,0,0,0,0 }, { 1,0,0,1,0,0,1,1,0,1,1,0,1,1,0,0 }

};
```

UASTC/BC7 3-Subset Partition Pattern Table
==========================================

```
uint32_t TOTAL_ASTC_BC7_COMMON_PARTITIONS3 = 11;

struct
{
  uint8_t m_bc7;
  uint16_t m_astc;
// maps ASTC to BC7 subset indices using g_astc_bc7_subset_index_perm_tables[][]
  uint8_t m_astc_to_bc7_perm;
} g_uastc_bc7_common_partitions3[TOTAL_ASTC_BC7_COMMON_PARTITIONS3] =
{
  { 4, 260, 0 },  { 8, 74, 5 },  { 9, 32, 5 },  { 10, 156, 2 },
  { 11, 183, 2 },  { 12, 15, 0 },  { 13, 745, 4 },  { 20, 0, 1 },
  { 35, 335, 1 },  { 36, 902, 5 },  { 57, 254, 0 }
};

const uint8_t g_astc_bc7_subset_index_perm_tables[6][3] = 
{
 { 0, 1, 2 }, { 1, 2, 0 }, { 2, 0, 1 }, { 2, 1, 0 }, { 0, 2, 1 }, { 1, 0, 2 }
};

// UASTC pattern table for the 3-subset modes
const uint8_t g_uastc_patterns3[TOTAL_ASTC_BC7_COMMON_PARTITIONS3][16] =
{
   { 0,0,0,0,0,0,0,0,1,1,2,2,1,1,2,2 }, { 1,1,1,1,1,1,1,1,0,0,0,0,2,2,2,2 }, 
   { 1,1,1,1,0,0,0,0,0,0,0,0,2,2,2,2 }, { 1,1,1,1,2,2,2,2,0,0,0,0,0,0,0,0 },
   { 1,1,2,0,1,1,2,0,1,1,2,0,1,1,2,0 }, { 0,1,1,2,0,1,1,2,0,1,1,2,0,1,1,2 }, 
   { 0,2,1,1,0,2,1,1,0,2,1,1,0,2,1,1 }, { 2,0,0,0,2,0,0,0,2,1,1,1,2,1,1,1 },
   { 2,0,1,2,2,0,1,2,2,0,1,2,2,0,1,2 }, { 1,1,1,1,0,0,0,0,2,2,2,2,1,1,1,1 }, 
   { 0,0,2,2,0,0,1,1,0,0,1,1,0,0,2,2 }

};
```

UASTC/BC7 2-Subset ASTC 3-Subset BC7 Partition Pattern Table
============================================================

This table is mapped to the BC7 3-subset patterns, but we only use 2 subsets from those patterns (by setting two of the BC7 endpoints/p-bits to the same values). It's only used in UASTC mode 7:

```
uint32_t TOTAL_BC73_ASTC2_COMMON_PARTITIONS = 19;

struct
{
 uint8_t m_bc73;
 uint16_t m_astc2;
// [0,5] - how to modify the BC7 3-subset pattern to match the ASTC pattern (LSB=invert). See convert_subset_index_3_to_2().
 uint8_t k;  
} g_bc73_uastc2_common_partitions[TOTAL_BC73_ASTC2_COMMON_PARTITIONS] =
{
 { 10, 36, 4 }, { 11, 48, 4 }, { 0, 61, 3 }, { 2, 137, 4 },
 { 8, 161, 5 }, { 13, 183, 4 }, { 1, 226, 2 }, { 33, 281, 2 },
 { 40, 302, 3 }, { 20, 307, 4 }, { 21, 479, 0 }, { 58, 495, 3 },
 { 3, 593, 0 }, { 32, 594, 2 }, { 59, 605, 1 }, { 34, 799, 3 },
 { 20, 812, 1 }, { 14, 988, 4 }, { 31, 993, 3 }
};

uint32_t convert_subset_index_3_to_2(uint32_t p, uint32_t k)
{
    assert(k < 6);
    switch (k >> 1)
    {
    case 0:
        if (p <= 1)
            p = 0;
        else 
            p = 1;
        break;
    case 1:
        if (p == 0)
            p = 0;
        else 
            p = 1;
        break;
    case 2:
        if ((p == 0) || (p == 2))
            p = 0;
        else 
            p = 1;
        break;
    }
    if (k & 1)
        p = 1 - p;
    return p;
}


// UASTC pattern table for UASTC mode 7 (2 subset UASTC, 3-subset BC7)
const uint8_t g_bc7_3_uastc2_patterns2[TOTAL_BC7_3_ASTC2_COMMON_PARTITIONS][16] =
{
   { 0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0 }, { 0,0,1,0,0,0,1,0,0,0,1,0,0,0,1,0 }, 
   { 1,1,0,0,1,1,0,0,1,0,0,0,0,0,0,0 }, { 0,0,0,0,0,0,0,1,0,0,1,1,0,0,1,1 },
   { 1,1,1,1,1,1,1,1,0,0,0,0,1,1,1,1 }, { 0,1,0,0,0,1,0,0,0,1,0,0,0,1,0,0 }, 
   { 0,0,0,1,0,0,1,1,1,1,1,1,1,1,1,1 }, { 0,1,1,1,0,0,1,1,0,0,1,1,0,0,1,1 },
   { 1,1,0,0,0,0,0,0,0,0,1,1,1,1,0,0 }, { 0,1,1,1,0,1,1,1,0,0,0,0,0,0,0,0 }, 
   { 0,0,0,0,0,0,0,0,1,1,1,0,1,1,1,0 }, { 1,1,0,0,0,0,0,0,0,0,0,0,1,1,0,0 },
   { 0,1,1,1,0,0,1,1,0,0,0,0,0,0,0,0 }, { 0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1 }, 
   { 1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,0 }, { 1,1,0,0,1,1,0,0,1,1,0,0,1,0,0,0 },
   { 1,1,1,1,1,1,1,1,1,0,0,0,1,0,0,0 }, { 0,0,1,1,0,1,1,0,1,1,0,0,1,0,0,0 }, 
   { 1,1,1,1,0,1,1,1,0,0,0,0,0,0,0,0 }
};
```

UASTC Weight Tables
===================

These 6-bit weight tables are used for endpoint interpolation in UASTC. They are the same as ASTC's.
```
const uint32_t g_astc_bc7_weights1[2] = { 0, 64 };
const uint32_t g_astc_bc7_weights2[4] = { 0, 21, 43, 64 };
const uint32_t g_astc_bc7_weights3[8] = { 0, 9, 18, 27, 37, 46, 55, 64 };
const uint32_t g_astc_weights4[16] = { 0, 4, 8, 12, 17, 21, 25, 29, 35, 39, 43, 47, 52, 56, 60, 64 };
const uint32_t g_astc_weights5[32] = { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62, 64 };
```

Note BC7 and ASTC use the same 2 and 3 bit weight tables, while the 4-bit tables are slightly different:

```
const uint32_t g_bc7_weights4[16] = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };
```

A UASTC encoder can work around this difference by evaluating the combined ASTC and transcoded BC7 error and choosing the mode/partition pattern/compsel/etc. configuration that minimizes the overall error.

### Decoding UASTC texture data to 32-bpp RGBA texels

UASTC is a subset of 4x4 LDR ASTC. UASTC can be decoded directly to a 4x4 block of 32-bit RGBA texels (with no intermediate transcode step). Alternately, it can be transcoded to ASTC and then unpacked from ASTC to RGBA using the EXT_texture_compression_astc_decode_mode extension with the decode mode set to GL_RGBA8. Both approaches are conceptually identical and should result in the same final output.

The UASTC and ASTC endpoint component interpolation function (see the next section) has an "srgb" option, which changes how dequantized 8-bit endpoint values are scaled to 16-bits before interpolation. This value should be set to the same value it was set to during encoding, otherwise slightly more error will be introduced into the decoded texture. (Currently, as of 8/25/2020 the Basis Universal reference encoder always sets this value to false.)

### UASTC Endpoint Interpolation

UASTC endpoints are interpolated [exactly like ASTC endpoints](https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#astc_weight_application):

```
inline uint32_t astc_interpolate(uint32_t l, uint32_t h, uint32_t w, bool srgb)
{
    if (srgb)
    {
        l = (l << 8) | 0x80;
        h = (h << 8) | 0x80;
    }
    else
    {
        l = (l << 8) | l;
        h = (h << 8) | h;
    }

    uint32_t k = (l * (64 - w) + h * w + 32) >> 6;

    return k >> 8;
}
```

Note: The "srgb" parameter is for hardware texture sampling implementations. It'll be true if sRGB conversion ("sRGB reads") are enabled when sampling the texture. Currently, the encoder always optimizes for lowest PSNR assuming the srgb flag in the interpolation function is false.

### UASTC Partition Pattern Anchor Index Tables

The texel indices in these tables indicate, for each common UASTC/BC7 pattern, which weight indices are stored with one less bit than normal. The texel indices are computed as x+y\*4. 

During encoding, the MSB's of weight indices stored with one less bit must be 0. If they aren't, the endpoints corresponding to that subset are swapped and that subset's weights are inverted before packing the UASTC block. BC7 uses a similar concept.

Anchor weight indices are applied to weight indices in both planes in dual plane UASTC modes.

The first texel is always an anchor. 

Decoders/transcoders must support anchor indices to properly decode the packed weights. (Without anchor indices, it wouldn't have been possible to pack some modes into 128-bits.) UASTC doesn't utilize ASTC's Blue Contraction method due to its utilization of anchor indices (because sometimes we must swap the endpoints to guarantee that the MSB's of the anchor weight indices are zero).

```
const uint8_t g_uastc_pattern2_anchors[TOTAL_ASTC_BC7_COMMON_PARTITIONS2][2] = 
{
   { 0, 2 }, { 0, 3 }, { 1, 0 }, { 0, 3 }, { 7, 0 }, { 0, 2 }, { 3, 0 }, 
   { 7, 0 }, { 0, 11 }, { 2, 0 }, { 0, 7 }, { 11, 0 }, { 3, 0 }, { 8, 0 }, 
   { 0, 4 }, { 12, 0 }, { 1, 0 }, { 8, 0 }, { 0, 1 }, { 0, 2 }, { 0, 4 }, 
   { 8, 0 }, { 1, 0 }, { 0, 2 }, { 4, 0 }, { 0, 1 }, { 4, 0 }, { 1, 0 }, 
   { 4, 0 }, { 1, 0 }
};

const uint8_t g_uastc_pattern3_anchors[TOTAL_ASTC_BC7_COMMON_PARTITIONS3][3] =
{
   { 0, 8, 10 },  { 8, 0, 12 }, { 4, 0, 12 }, { 8, 0, 4 }, { 3, 0, 2 }, 
   { 0, 1, 3 }, { 0, 2, 1 }, { 1, 9, 0 }, { 1, 2, 0 }, { 4, 0, 8 }, { 0, 6, 2 }
};

const uint8_t g_bc7_3_uastc2_patterns2_anchors[TOTAL_BC7_3_ASTC2_COMMON_PARTITIONS][2] =
{
   { 0, 4 }, { 0, 2 }, { 2, 0 }, { 0, 7 }, { 8, 0 }, { 0, 1 }, { 0, 3 }, 
   { 0, 1 }, { 2, 0 }, { 0, 1 }, { 0, 8 }, { 2, 0 }, { 0, 1 }, { 0, 7 }, 
   { 12, 0 }, { 2, 0 }, { 9, 0 }, { 0, 2 }, { 4, 0 }
};
```

UASTC Mode Description Tables
=============================

```
const uint32_t TOTAL_ASTC_MODES = 18;

const uint8_t g_uastc_mode_weight_bits[TOTAL_ASTC_MODES]          = { 4, 2, 3, 2, 2, 3, 2, 2,         0,  2, 4, 2, 3, 1, 2,         4, 2, 2,     5 };
const uint8_t g_uastc_mode_weight_ranges[TOTAL_ASTC_MODES]        = { 8, 2, 5, 2, 2, 5, 2, 2,         0,  2, 8, 2, 5, 0, 2,         8, 2, 2,     11 };
const uint8_t g_uastc_mode_endpoint_ranges[TOTAL_ASTC_MODES]      = { 19, 20, 8, 7, 12, 20, 18, 12,   0,  8, 13, 13, 19, 20, 20,    20, 20, 20,  11 };
const uint8_t g_uastc_mode_subsets[TOTAL_ASTC_MODES]              = { 1, 1, 2, 3, 2, 1, 1, 2,         0,  2, 1, 1, 1, 1, 1,         1, 2, 1,     1 };
const uint8_t g_uastc_mode_planes[TOTAL_ASTC_MODES]               = { 1, 1, 1, 1, 1, 1, 2, 1,         0,  1, 1, 2, 1, 2, 1,         1, 1, 2,     1 };
const uint8_t g_uastc_mode_comps[TOTAL_ASTC_MODES]                = { 3, 3, 3, 3, 3, 3, 3, 3,         4,  4, 4, 4, 4, 4, 4,         2, 2, 2,     3 };
const uint8_t g_uastc_mode_has_etc1_bias[TOTAL_ASTC_MODES]        = { 1, 1, 1, 1, 1, 1, 1, 1,         0,  1, 0, 0, 0, 1, 1,         1, 1, 1,     1 };
const uint8_t g_uastc_mode_has_bc1_hint0[TOTAL_ASTC_MODES]        = { 1, 1, 1, 1, 1, 1, 1, 1,         0,  1, 1, 1, 1, 1, 1,         1, 1, 1,     1 };
const uint8_t g_uastc_mode_has_bc1_hint1[TOTAL_ASTC_MODES]        = { 1, 1, 1, 1, 1, 1, 1, 1,         0,  1, 0, 0, 0, 1, 1,         1, 1, 1,     1 };
const uint8_t g_uastc_mode_cem[TOTAL_ASTC_MODES]                  = { 8, 8, 8, 8, 8, 8, 8, 8,         0,  12, 12, 12, 12, 12, 12,   4, 4, 4,     8 };
const uint8_t g_uastc_mode_has_alpha[TOTAL_ASTC_MODES]            = { 0, 0, 0, 0, 0, 0, 0, 0,         1,  1, 1, 1, 1, 1, 1,         1, 1, 1,     0 };
const uint8_t g_uastc_mode_is_la[TOTAL_ASTC_MODES]                = { 0, 0, 0, 0, 0, 0, 0, 0,         0,  0, 0, 0, 0, 0, 0,         1, 1, 1,     0 };
```

UASTC to BC7 Mode Conversion Table
==================================

<pre>
UASTC Mode   BC7 Mode    Implementation Notes
0            6           
1            3           set both endpoints to same colors/p-bits
2            1           
3            2
4            3
5            6           convert weights from 3 to 4 bits
6            5           swap CCS component with alpha
7            2           two endpoints will be set to same colors
8            5 or 6      choose mode & p-bits with lowest error (p-bits set to same values)
9            7
10           6
11           5           swap CCS component with alpha
12           6           convert weights from 3->4 bits
13           5           convert weights from 1->2 bits
14           6           convert weights from 2->4 bits
15           6           convert LA to RGBA(LLLA swizzle)
16           7           convert LA to RGBA (LLLA swizzle)
17           5           convert LA to RGBA (LLLA swizzle)
18           6           convert weights from 5->4 bits
</pre>

UASTC to BC7 Weight Index Conversion
====================================

If the UASTC mode's number of weight index bits matches the BC7 mode's index bits, then nothing needs to be done to convert UASTC weight indices to BC7 indices. (Note: When encoding the final output BC7 block data, you may need to invert one or more subset's indices if the subset's endpoints need to be swapped to account for the BC7 anchors. But otherwise, you can use the UASTC weights as-is.) In the cases that differ, use these tables to translate UASTC weight indices to BC7 indices:

<pre>
UASTC 1-bit to BC7 2-bit conversion table: { 0, 3 }
UASTC 2-bit to BC7 4-bit conversion table: { 0, 5, 10, 15 }
UASTC 3-bit to BC7 4-bit conversion table: { 0, 2, 4, 6, 9, 11, 13, 15 }
UASTC 5-bit to BC7 4-bit conversion table: { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6, 7, 8, 9, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15 };
</pre>

BC7 requires the "anchor" indices to have MSB's of 0. The UASTC->BC7 transcoder needs to check the anchor MSB's and swap the appropriate endpoints, because the UASTC anchor indices aren't the same as BC7's.

For UASTC mode 18, which uses 5-bit weights, the 5 to 4-bit weight index translation table is used.

UASTC mode 14, which uses 2-bit weights, the 2 to 4 table is used.

For UASTC modes 5 or 12, the 3 to 4 table is used.

For UASTC mode 13, the 1 to 2 table is used.

UASTC to BC7 Endpoint Conversion
================================

First dequantize the UASTC endpoints to [0,255] using the method outlined here:
https://www.khronos.org/registry/DataFormat/specs/1.3/dataformat.1.3.inline.html#astc-endpoint-unquantization

Alternately, you can use the dequantization tables included below, which inverts the procedure outlined in the ASTC spec.

The ASTC/UASTC endpoint ranges that use trits or quints must be decoded/dequantized using the parameters in Table 158. (Beware, in case you're not familiar with ASTC: The encoded values of ranges using trits/quints do not dequantize to [0,255] values in a monotonic order. This is why you need to apply the parameters in Table 158 to unquantize the encoded value. This may seem unintuitive, but this is supposed to simplify ASTC decoding hardware.)

Next, for modes with p-bits, divide the dequantized endpoints by 255.0 and compute the optimal quantized BC7 endpoint colors/p-bits using the helper functions included below. 

If the mode doesn't have p-bits, then scale the dequantized endpoint value to the desired number of BC7 endpoint bits, with rounding. To convert 8-bit endpoint components to 5-bits compute (value\*31+127)/255, and to convert 8-bits to 7 compute (value\*127+127)/255. 

The UASTC encoder assumes this is how the BC7 endpoints will be computed during transcoding. If you do something else the encoder's error computation won't be accurate, and the resulting BC7 texture will have more error than it should (and potentially noticeable artifacts on some textures).

Computing Optimal BC7 P-Bits
============================

To compute the optimal shared or unique BC7 p-bits, we use the following two helper functions. (This is tricky enough, and almost always messed up in the BC7 encoders that we've seen, that we're just including the exact code we use.) Our UASTC encoder assumes the p-bits are computed in this way.

Inputs:
total_comps: 3 or 4
comp_bits: bits in BC7 output color (7 for mode 6, etc.)
xl/xh: the normalized [0,1] input vectors (dequantize ASTC's endpoints and divide by 255.0)

Outputs: 
bestMinColor, bestMaxColor: quantized colors to pack into BC7 output
best_pbits[2]: p-bits to pack into BC7 output

```
// Determines the best shared pbits to use to encode xl/xh
static void determine_shared_pbits(
   uint32_t total_comps, uint32_t comp_bits, float xl[4], float xh[4],
   color_quad_u8& bestMinColor, color_quad_u8& bestMaxColor, uint32_t best_pbits[2])
{
   const uint32_t total_bits = comp_bits + 1;
   assert(total_bits >= 4 && total_bits <= 8);

   const int iscalep = (1 << total_bits) - 1;
   const float scalep = (float)iscalep;

   float best_err = 1e+9f;

   for (int p = 0; p < 2; p++)
   {
      color_quad_u8 xMinColor, xMaxColor;
      for (uint32_t c = 0; c < 4; c++)
      {
         xMinColor.m_c[c] = (uint8_t)(clampi(((int)((xl[c] * scalep - p) / 2.0f + .5f)) * 2 + p, p, iscalep - 1 + p));
         xMaxColor.m_c[c] = (uint8_t)(clampi(((int)((xh[c] * scalep - p) / 2.0f + .5f)) * 2 + p, p, iscalep - 1 + p));
      }

      color_quad_u8 scaledLow, scaledHigh;

      for (uint32_t i = 0; i < 4; i++)
      {
         scaledLow.m_c[i] = (xMinColor.m_c[i] << (8 - total_bits));
         scaledLow.m_c[i] |= (scaledLow.m_c[i] >> total_bits);
         assert(scaledLow.m_c[i] <= 255);

         scaledHigh.m_c[i] = (xMaxColor.m_c[i] << (8 - total_bits));
         scaledHigh.m_c[i] |= (scaledHigh.m_c[i] >> total_bits);
         assert(scaledHigh.m_c[i] <= 255);
      }

      float err = 0;
      for (uint32_t i = 0; i < total_comps; i++)
         err += squaref((scaledLow.m_c[i] / 255.0f) - xl[i]) + squaref((scaledHigh.m_c[i] / 255.0f) - xh[i]);

      if (err < best_err)
      {
         best_err = err;
         best_pbits[0] = p;
         best_pbits[1] = p;
         for (uint32_t j = 0; j < 4; j++)
         {
            bestMinColor.m_c[j] = xMinColor.m_c[j] >> 1;
            bestMaxColor.m_c[j] = xMaxColor.m_c[j] >> 1;
         }
      }
   }
}

// Determines the best unique pbits to use to encode xl/xh
static void determine_unique_pbits(
   uint32_t total_comps, uint32_t comp_bits, float xl[4], float xh[4], 
   color_quad_u8 &bestMinColor, color_quad_u8 &bestMaxColor, uint32_t best_pbits[2])
{
   const uint32_t total_bits = comp_bits + 1;
   const int iscalep = (1 << total_bits) - 1;
   const float scalep = (float)iscalep;

   float best_err0 = 1e+9f;
   float best_err1 = 1e+9f;

   for (int p = 0; p < 2; p++)
   {
      color_quad_u8 xMinColor, xMaxColor;

      for (uint32_t c = 0; c < 4; c++)
      {
         xMinColor.m_c[c] = (uint8_t)(clampi(((int)((xl[c] * scalep - p) / 2.0f + .5f)) * 2 + p, p, iscalep - 1 + p));
         xMaxColor.m_c[c] = (uint8_t)(clampi(((int)((xh[c] * scalep - p) / 2.0f + .5f)) * 2 + p, p, iscalep - 1 + p));
      }

      color_quad_u8 scaledLow, scaledHigh;
      for (uint32_t i = 0; i < 4; i++)
      {
         scaledLow.m_c[i] = (xMinColor.m_c[i] << (8 - total_bits));
         scaledLow.m_c[i] |= (scaledLow.m_c[i] >> total_bits);
         assert(scaledLow.m_c[i] <= 255);

         scaledHigh.m_c[i] = (xMaxColor.m_c[i] << (8 - total_bits));
         scaledHigh.m_c[i] |= (scaledHigh.m_c[i] >> total_bits);
         assert(scaledHigh.m_c[i] <= 255);
      }

      float err0 = 0, err1 = 0;
      for (uint32_t i = 0; i < total_comps; i++)
      {
         err0 += squaref(scaledLow.m_c[i] - xl[i] * 255.0f);
         err1 += squaref(scaledHigh.m_c[i] - xh[i] * 255.0f);
      }

      if (err0 < best_err0)
      {
         best_err0 = err0;
         best_pbits[0] = p;

         bestMinColor.m_c[0] = xMinColor.m_c[0] >> 1;
         bestMinColor.m_c[1] = xMinColor.m_c[1] >> 1;
         bestMinColor.m_c[2] = xMinColor.m_c[2] >> 1;
         bestMinColor.m_c[3] = xMinColor.m_c[3] >> 1;
      }

      if (err1 < best_err1)
      {
         best_err1 = err1;
         best_pbits[1] = p;

         bestMaxColor.m_c[0] = xMaxColor.m_c[0] >> 1;
         bestMaxColor.m_c[1] = xMaxColor.m_c[1] >> 1;
         bestMaxColor.m_c[2] = xMaxColor.m_c[2] >> 1;
         bestMaxColor.m_c[3] = xMaxColor.m_c[3] >> 1;
      }
   }
}
```

UASTC to BC7 Solid Block (Void Extent) Conversion
=================================================

UASTC supports a lossless solid color block mode (mode 8), but BC7 does not. To transcode these blocks to BC7 in the same way as the Basis Universal encoder and transcoders do (which is important to minimize BC7 error *and* output data entropy), the transcoder chooses between BC7 modes 5 or 6. For mode 6, it must choose which p-bit to use. It must choose the mode (and p-bit for BC7 mode 6) which results in the lowest error (MSE).

The UASTC->BC7 transcoder in Basis Universal prefers to use BC7 mode 6 over 5 to slightly lower the output entropy of the resulting BC7 data, because BC7 mode 6 is more commonly used, and solid color blocks are common. This is why it decides between mode 6 or 5. But if you don't care about the BC7 output data entropy (must use cases won't care at all), you can always use mode 5.

The transcoder uses a set of 1D precomputed tables for BC7 modes 5 and 6 (or just 5, if you don't care to use mode 6). These tables indicate, for each possible 8-bit [0,255] component value, the optimal BC7 low/high endpoint to use assuming the BC7 weight index is 1 (mode 5) or 5 (mode 6). (This method is commonly used in BC1 texture encoders to optimally encode solid color blocks.) These tables can be precomputed at startup, or stored in the executable. For mode 6, you will need two tables, one for p-bit 0 and another for p-bit 1.

The transcoder chooses the mode and p-bits which result in the least amount of quantization error. Solid color blocks are quite common, so it's important to do this in the same exact way as the BasisU encoder/transcoder do in order to minimize BC7 error. 

Example code:

```
const uint32_t BC7ENC_MODE_5_OPTIMAL_INDEX = 1;
const uint32_t BC7ENC_MODE_6_OPTIMAL_INDEX = 5;

case UASTC_MODE_INDEX_SOLID_COLOR:
{
    // UASTC Void-Extent: Solid Color RGBA (BC7 MODE5 or MODE6)
    const color32& solid_color = unpacked_src_blk.m_solid_color;

    // Compute the error from BC7 mode 6 p-bit 0
    uint32_t best_err0 = g_bc7_mode_6_optimal_endpoints[solid_color.r][0].m_error + g_bc7_mode_6_optimal_endpoints[solid_color.g][0].m_error +
    g_bc7_mode_6_optimal_endpoints[solid_color.b][0].m_error + g_bc7_mode_6_optimal_endpoints[solid_color.a][0].m_error;

    // Compute the error from BC7 mode 6 p-bit 1
    uint32_t best_err1 = g_bc7_mode_6_optimal_endpoints[solid_color.r][1].m_error + g_bc7_mode_6_optimal_endpoints[solid_color.g][1].m_error +
    g_bc7_mode_6_optimal_endpoints[solid_color.b][1].m_error + g_bc7_mode_6_optimal_endpoints[solid_color.a][1].m_error;

    // Is BC7 mode 6 not lossless? If so, use mode 5 instead.
    if (best_err0 > 0 && best_err1 > 0)
    {
        // Output BC7 mode 5
        dst_blk.m_mode = 5;

        // Convert the endpoints
        for (uint32_t c = 0; c < 3; c++)
        {
            dst_blk.m_low[0].m_c[c] = g_bc7_mode_5_optimal_endpoints[solid_color.c[c]].m_lo;
            dst_blk.m_high[0].m_c[c] = g_bc7_mode_5_optimal_endpoints[solid_color.c[c]].m_hi;
        }

        // Set the output BC7 selectors/weights to all 1's
        memset(dst_blk.m_selectors, BC7ENC_MODE_5_OPTIMAL_INDEX, 16);

        // Set the output alpha
        dst_blk.m_low[0].m_c[3] = solid_color.c[3];
        dst_blk.m_high[0].m_c[3] = solid_color.c[3];

        // Set the output BC7 alpha selectors/weights to all 0's
        memset(dst_blk.m_alpha_selectors, 0, 16);
    }
    else
    {
        // Output BC7 mode 6
        dst_blk.m_mode = 6;

        // Choose the p-bit with minimal error
        uint32_t best_p = 0;
        if (best_err1 < best_err0)
            best_p = 1;

        // Convert the components
        for (uint32_t c = 0; c < 4; c++)
        {
            dst_blk.m_low[0].m_c[c] = g_bc7_mode_6_optimal_endpoints[solid_color.c[c]][best_p].m_lo;
            dst_blk.m_high[0].m_c[c] = g_bc7_mode_6_optimal_endpoints[solid_color.c[c]][best_p].m_hi;
        }

        // Set the output p-bits
        dst_blk.m_pbits[0][0] = best_p;
        dst_blk.m_pbits[0][1] = best_p;
        
        // Set the output BC7 selectors/weights to all 5's
        memset(dst_blk.m_selectors, BC7ENC_MODE_6_OPTIMAL_INDEX, 16);
    }

    break;
}
```


ETC1 Transcoding
================

For solid color UASTC blocks: ETC1 transcode hints directly follow the 32-bit RGBA block color. These hints indicate which color mode (differential or individual), intensity table index, selector, and block color to use to encode the block with the lowest error.

For non-solid color blocks: The UASTC block must first be unpacked to pixels. After this, the only expensive work required to transcode to ETC1 is computing the subblock average colors and the texel indices. 

There are ETC1 hint fields which indicate how the ETC1 subblocks are flipped, which block color mode to use, each subblock's intensity table index, and how to bias the computed quantized subblock block colors. 

To compute each subblock's quantized block color: First compute each subblocks' average texel color. Then quantize the average subblock colors to 4 or 5-bits/component (depending on the ETC1 block color mode), with rounding. Next, apply the ETC1 bias indicated by the 5-bit UASTC ETC1BIAS field (see the apply_etc1_bias() function below) to each quantized subblock color. Finally, encode the quantized subblock colors in the ETC1 block. In differential mode the second subblock's differential color may need to be clamped to fit into 3-bits/component.

The last step is computing the texel indices. This step can be accelerated by computing the errors in a luma space with RGB component weights of (1,1,1), although other methods are possible too.

```
color_rgba apply_etc1_bias(color_rgba block_color, uint32_t bias, uint32_t limit, uint32_t subblock)
{
   for (uint32_t c = 0; c < 3; c++)
   {
      static const int s_divs[3] = { 1, 3, 9 };

      int delta = 0;

      switch (bias)
      {
      case 2: delta = subblock ? 0 : ((c == 0) ? -1 : 0); break;
      case 5: delta = subblock ? 0 : ((c == 1) ? -1 : 0); break;
      case 6: delta = subblock ? 0 : ((c == 2) ? -1 : 0); break;

      case 7: delta = subblock ? 0 : ((c == 0) ? 1 : 0); break;
      case 11: delta = subblock ? 0 : ((c == 1) ? 1 : 0); break;
      case 15: delta = subblock ? 0 : ((c == 2) ? 1 : 0); break;

      case 18: delta = subblock ? ((c == 0) ? -1 : 0) : 0; break;
      case 19: delta = subblock ? ((c == 1) ? -1 : 0) : 0; break;
      case 20: delta = subblock ? ((c == 2) ? -1 : 0) : 0; break;

      case 21: delta = subblock ? ((c == 0) ? 1 : 0) : 0; break;
      case 24: delta = subblock ? ((c == 1) ? 1 : 0) : 0; break;
      case 8: delta = subblock ? ((c == 2) ? 1 : 0) : 0; break;

      case 10: delta = -2; break;

      case 27: delta = subblock ? 0 : -1; break;
      case 28: delta = subblock ? -1 : 1; break;
      case 29: delta = subblock ? 1 : 0; break;
      case 30: delta = subblock ? -1 : 0; break;
      case 31: delta = subblock ? 0 : 1; break;

      default:
         delta = ((bias / s_divs[c]) % 3) - 1;
         break;
      }
      
      int v = block_color[c];
      if (v == 0)
      {
         if (delta == -2)
            v += 3;
         else
            v += delta + 1;
      }
      else if (v == (int)limit)
      {
         v += (delta - 1);
      }
      else
      {
         v += delta;
         if ((v < 0) || (v > (int)limit))
            v = (v - delta) - delta;
      }

      assert(v >= 0);
      assert(v <= (int)limit);

      block_color[c] = (uint8_t)v;
   }

   return block_color;
}
```

BISE Endpoint Encoding Quantization Tables
==========================================

These tables are used by UASTC encoders to convert endpoint components into integers which are then BISE coded. For example, if the encoder wants to pack component value 204 using endpoint range 4, it would encode the integer 3. These tables are compatible with ASTC's endpoint quantization, and were computed by inverting the procedure outlined in section 23.13.

The format is "{x,y}", where x is the integer to be BISE coded, and y is the unquantized [0,255] value. Only ranges with trits/quints are included here, because binary ranges use simple bit replication from the most significant bit of the value.

Note that some of these ranges aren't currently used in UASTC.

```
// Range: 4, Levels: 6, Bits: 1, Trits: 1, Quints: 0
{{0,0},{2,51},{4,102},{5,153},{3,204},{1,255}}
// Range: 6, Levels: 10, Bits: 1, Trits: 0, Quints: 1
{{0,0},{2,28},{4,56},{6,84},{8,113},{9,142},{7,171},{5,199},{3,227},{1,255}}
// Range: 7, Levels: 12, Bits: 2, Trits: 1, Quints: 0
{{0,0},{4,23},{8,46},{2,69},{6,92},{10,116},{11,139},{7,163},{3,186},{9,209},{5,232},{1,255}}
// Range: 9, Levels: 20, Bits: 2, Trits: 0, Quints: 1
{{0,0},{4,13},{8,27},{12,40},{16,54},{2,67},{6,80},{10,94},{14,107},{18,121},{19,134},{15,148},{11,161},{7,175},{3,188},{17,201},{13,215},{9,228},{5,242},{1,255}}
// Range: 10, Levels: 24, Bits: 3, Trits: 1, Quints: 0
{{0,0},{8,11},{16,22},{2,33},{10,44},{18,55},{4,66},{12,77},{20,88},{6,99},{14,110},{22,121},{23,134},{15,145},{7,156},{21,167},{13,178},{5,189},{19,200},{11,211},{3,222},{17,233},{9,244},{1,255}}
// Range: 12, Levels: 40, Bits: 3, Trits: 0, Quints: 1
{{0,0},{8,6},{16,13},{24,19},{32,26},{2,32},{10,39},{18,45},{26,52},{34,58},{4,65},{12,71},{20,78},{28,84},{36,91},{6,97},{14,104},{22,110},{30,117},{38,123},{39,132},{31,138},{23,145},{15,151},{7,158},{37,164},{29,171},{21,177},{13,184},{5,190},{35,197},{27,203},{19,210},{11,216},{3,223},{33,229},{25,236},{17,242},{9,249},{1,255}}
// Range: 13, Levels: 48, Bits: 4, Trits: 1, Quints: 0
{{0,0},{16,5},{32,11},{2,16},{18,21},{34,27},{4,32},{20,38},{36,43},{6,48},{22,54},{38,59},{8,65},{24,70},{40,76},{10,81},{26,86},{42,92},{12,97},{28,103},{44,108},{14,113},{30,119},{46,124},{47,131},{31,136},{15,142},{45,147},{29,152},{13,158},{43,163},{27,169},{11,174},{41,179},{25,185},{9,190},{39,196},{23,201},{7,207},{37,212},{21,217},{5,223},{35,228},{19,234},{3,239},{33,244},{17,250},{1,255}}
// Range: 15, Levels: 80, Bits: 4, Trits: 0, Quints: 1
{{0,0},{16,3},{32,6},{48,9},{64,13},{2,16},{18,19},{34,22},{50,25},{66,29},{4,32},{20,35},{36,38},{52,42},{68,45},{6,48},{22,51},{38,54},{54,58},{70,61},{8,64},{24,67},{40,71},{56,74},{72,77},{10,80},{26,83},{42,87},{58,90},{74,93},{12,96},{28,100},{44,103},{60,106},{76,109},{14,112},{30,116},{46,119},{62,122},{78,125},{79,130},{63,133},{47,136},{31,139},{15,143},{77,146},{61,149},{45,152},{29,155},{13,159},{75,162},{59,165},{43,168},{27,172},{11,175},{73,178},{57,181},{41,184},{25,188},{9,191},{71,194},{55,197},{39,201},{23,204},{7,207},{69,210},{53,213},{37,217},{21,220},{5,223},{67,226},{51,230},{35,233},{19,236},{3,239},{65,242},{49,246},{33,249},{17,252},{1,255}}
// Range: 16, Levels: 96, Bits: 5, Trits: 1, Quints: 0
{{0,0},{32,2},{64,5},{2,8},{34,10},{66,13},{4,16},{36,18},{68,21},{6,24},{38,26},{70,29},{8,32},{40,35},{72,37},{10,40},{42,43},{74,45},{12,48},{44,51},{76,53},{14,56},{46,59},{78,61},{16,64},{48,67},{80,70},{18,72},{50,75},{82,78},{20,80},{52,83},{84,86},{22,88},{54,91},{86,94},{24,96},{56,99},{88,102},{26,104},{58,107},{90,110},{28,112},{60,115},{92,118},{30,120},{62,123},{94,126},{95,129},{63,132},{31,135},{93,137},{61,140},{29,143},{91,145},{59,148},{27,151},{89,153},{57,156},{25,159},{87,161},{55,164},{23,167},{85,169},{53,172},{21,175},{83,177},{51,180},{19,183},{81,185},{49,188},{17,191},{79,194},{47,196},{15,199},{77,202},{45,204},{13,207},{75,210},{43,212},{11,215},{73,218},{41,220},{9,223},{71,226},{39,229},{7,231},{69,234},{37,237},{5,239},{67,242},{35,245},{3,247},{65,250},{33,253},{1,255}}
// Range: 18, Levels: 160, Bits: 5, Trits: 0, Quints: 1
{{0,0},{32,1},{64,3},{96,4},{128,6},{2,8},{34,9},{66,11},{98,12},{130,14},{4,16},{36,17},{68,19},{100,20},{132,22},{6,24},{38,25},{70,27},{102,28},{134,30},{8,32},{40,33},{72,35},{104,36},{136,38},{10,40},{42,41},{74,43},{106,44},{138,46},{12,48},{44,49},{76,51},{108,52},{140,54},{14,56},{46,57},{78,59},{110,60},{142,62},{16,64},{48,65},{80,67},{112,68},{144,70},{18,72},{50,73},{82,75},{114,76},{146,78},{20,80},{52,81},{84,83},{116,84},{148,86},{22,88},{54,89},{86,91},{118,92},{150,94},{24,96},{56,97},{88,99},{120,100},{152,102},{26,104},{58,105},{90,107},{122,108},{154,110},{28,112},{60,113},{92,115},{124,116},{156,118},{30,120},{62,121},{94,123},{126,124},{158,126},{159,129},{127,131},{95,132},{63,134},{31,135},{157,137},{125,139},{93,140},{61,142},{29,143},{155,145},{123,147},{91,148},{59,150},{27,151},{153,153},{121,155},{89,156},{57,158},{25,159},{151,161},{119,163},{87,164},{55,166},{23,167},{149,169},{117,171},{85,172},{53,174},{21,175},{147,177},{115,179},{83,180},{51,182},{19,183},{145,185},{113,187},{81,188},{49,190},{17,191},{143,193},{111,195},{79,196},{47,198},{15,199},{141,201},{109,203},{77,204},{45,206},{13,207},{139,209},{107,211},{75,212},{43,214},{11,215},{137,217},{105,219},{73,220},{41,222},{9,223},{135,225},{103,227},{71,228},{39,230},{7,231},{133,233},{101,235},{69,236},{37,238},{5,239},{131,241},{99,243},{67,244},{35,246},{3,247},{129,249},{97,251},{65,252},{33,254},{1,255}}
// Range: 19, Levels: 192, Bits: 6, Trits: 1, Quints: 0
{{0,0},{64,1},{128,2},{2,4},{66,5},{130,6},{4,8},{68,9},{132,10},{6,12},{70,13},{134,14},{8,16},{72,17},{136,18},{10,20},{74,21},{138,22},{12,24},{76,25},{140,26},{14,28},{78,29},{142,30},{16,32},{80,33},{144,34},{18,36},{82,37},{146,38},{20,40},{84,41},{148,42},{22,44},{86,45},{150,46},{24,48},{88,49},{152,50},{26,52},{90,53},{154,54},{28,56},{92,57},{156,58},{30,60},{94,61},{158,62},{32,64},{96,65},{160,66},{34,68},{98,69},{162,70},{36,72},{100,73},{164,74},{38,76},{102,77},{166,78},{40,80},{104,81},{168,82},{42,84},{106,85},{170,86},{44,88},{108,89},{172,90},{46,92},{110,93},{174,94},{48,96},{112,97},{176,98},{50,100},{114,101},{178,102},{52,104},{116,105},{180,106},{54,108},{118,109},{182,110},{56,112},{120,113},{184,114},{58,116},{122,117},{186,118},{60,120},{124,121},{188,122},{62,124},{126,125},{190,126},{191,129},{127,130},{63,131},{189,133},{125,134},{61,135},{187,137},{123,138},{59,139},{185,141},{121,142},{57,143},{183,145},{119,146},{55,147},{181,149},{117,150},{53,151},{179,153},{115,154},{51,155},{177,157},{113,158},{49,159},{175,161},{111,162},{47,163},{173,165},{109,166},{45,167},{171,169},{107,170},{43,171},{169,173},{105,174},{41,175},{167,177},{103,178},{39,179},{165,181},{101,182},{37,183},{163,185},{99,186},{35,187},{161,189},{97,190},{33,191},{159,193},{95,194},{31,195},{157,197},{93,198},{29,199},{155,201},{91,202},{27,203},{153,205},{89,206},{25,207},{151,209},{87,210},{23,211},{149,213},{85,214},{21,215},{147,217},{83,218},{19,219},{145,221},{81,222},{17,223},{143,225},{79,226},{15,227},{141,229},{77,230},{13,231},{139,233},{75,234},{11,235},{137,237},{73,238},{9,239},{135,241},{71,242},{7,243},{133,245},{69,246},{5,247},{131,249},{67,250},{3,251},{129,253},{65,254},{1,255}}
```

BISE Endpoint Decoding Dequantization Tables
============================================

These tables are the inverse of the previous encoding tables. They are used by decoders to convert BISE encoded integers back into endpoint values, and for dequantization to 8-bits. Alternately, you can use the decoding table and procedure outlined in section 23.13 in the ASTC specification.

Example: if the decoder receives the BISE encoded integer 2 in a UASTC mode which uses endpoint range 4, the resulting component value (which ranges from [0,5]) would be 1, which dequantizes to an 8-bit value of 51.

```
// Range: 4, Levels: 6, Bits: 1, Trits: 1, Quints: 0
{{0,0},{5,255},{1,51},{4,204},{2,102},{3,153}}
// Range: 6, Levels: 10, Bits: 1, Trits: 0, Quints: 1
{{0,0},{9,255},{1,28},{8,227},{2,56},{7,199},{3,84},{6,171},{4,113},{5,142}}
// Range: 7, Levels: 12, Bits: 2, Trits: 1, Quints: 0
{{0,0},{11,255},{3,69},{8,186},{1,23},{10,232},{4,92},{7,163},{2,46},{9,209},{5,116},{6,139}}
// Range: 9, Levels: 20, Bits: 2, Trits: 0, Quints: 1
{{0,0},{19,255},{5,67},{14,188},{1,13},{18,242},{6,80},{13,175},{2,27},{17,228},{7,94},{12,161},{3,40},{16,215},{8,107},{11,148},{4,54},{15,201},{9,121},{10,134}}
// Range: 10, Levels: 24, Bits: 3, Trits: 1, Quints: 0
{{0,0},{23,255},{3,33},{20,222},{6,66},{17,189},{9,99},{14,156},{1,11},{22,244},{4,44},{19,211},{7,77},{16,178},{10,110},{13,145},{2,22},{21,233},{5,55},{18,200},{8,88},{15,167},{11,121},{12,134}}
// Range: 12, Levels: 40, Bits: 3, Trits: 0, Quints: 1
{{0,0},{39,255},{5,32},{34,223},{10,65},{29,190},{15,97},{24,158},{1,6},{38,249},{6,39},{33,216},{11,71},{28,184},{16,104},{23,151},{2,13},{37,242},{7,45},{32,210},{12,78},{27,177},{17,110},{22,145},{3,19},{36,236},{8,52},{31,203},{13,84},{26,171},{18,117},{21,138},{4,26},{35,229},{9,58},{30,197},{14,91},{25,164},{19,123},{20,132}}
// Range: 13, Levels: 48, Bits: 4, Trits: 1, Quints: 0
{{0,0},{47,255},{3,16},{44,239},{6,32},{41,223},{9,48},{38,207},{12,65},{35,190},{15,81},{32,174},{18,97},{29,158},{21,113},{26,142},{1,5},{46,250},{4,21},{43,234},{7,38},{40,217},{10,54},{37,201},{13,70},{34,185},{16,86},{31,169},{19,103},{28,152},{22,119},{25,136},{2,11},{45,244},{5,27},{42,228},{8,43},{39,212},{11,59},{36,196},{14,76},{33,179},{17,92},{30,163},{20,108},{27,147},{23,124},{24,131}}
// Range: 15, Levels: 80, Bits: 4, Trits: 0, Quints: 1
{{0,0},{79,255},{5,16},{74,239},{10,32},{69,223},{15,48},{64,207},{20,64},{59,191},{25,80},{54,175},{30,96},{49,159},{35,112},{44,143},{1,3},{78,252},{6,19},{73,236},{11,35},{68,220},{16,51},{63,204},{21,67},{58,188},{26,83},{53,172},{31,100},{48,155},{36,116},{43,139},{2,6},{77,249},{7,22},{72,233},{12,38},{67,217},{17,54},{62,201},{22,71},{57,184},{27,87},{52,168},{32,103},{47,152},{37,119},{42,136},{3,9},{76,246},{8,25},{71,230},{13,42},{66,213},{18,58},{61,197},{23,74},{56,181},{28,90},{51,165},{33,106},{46,149},{38,122},{41,133},{4,13},{75,242},{9,29},{70,226},{14,45},{65,210},{19,61},{60,194},{24,77},{55,178},{29,93},{50,162},{34,109},{45,146},{39,125},{40,130}}
// Range: 16, Levels: 96, Bits: 5, Trits: 1, Quints: 0
{{0,0},{95,255},{3,8},{92,247},{6,16},{89,239},{9,24},{86,231},{12,32},{83,223},{15,40},{80,215},{18,48},{77,207},{21,56},{74,199},{24,64},{71,191},{27,72},{68,183},{30,80},{65,175},{33,88},{62,167},{36,96},{59,159},{39,104},{56,151},{42,112},{53,143},{45,120},{50,135},{1,2},{94,253},{4,10},{91,245},{7,18},{88,237},{10,26},{85,229},{13,35},{82,220},{16,43},{79,212},{19,51},{76,204},{22,59},{73,196},{25,67},{70,188},{28,75},{67,180},{31,83},{64,172},{34,91},{61,164},{37,99},{58,156},{40,107},{55,148},{43,115},{52,140},{46,123},{49,132},{2,5},{93,250},{5,13},{90,242},{8,21},{87,234},{11,29},{84,226},{14,37},{81,218},{17,45},{78,210},{20,53},{75,202},{23,61},{72,194},{26,70},{69,185},{29,78},{66,177},{32,86},{63,169},{35,94},{60,161},{38,102},{57,153},{41,110},{54,145},{44,118},{51,137},{47,126},{48,129}}
// Range: 18, Levels: 160, Bits: 5, Trits: 0, Quints: 1
{{0,0},{159,255},{5,8},{154,247},{10,16},{149,239},{15,24},{144,231},{20,32},{139,223},{25,40},{134,215},{30,48},{129,207},{35,56},{124,199},{40,64},{119,191},{45,72},{114,183},{50,80},{109,175},{55,88},{104,167},{60,96},{99,159},{65,104},{94,151},{70,112},{89,143},{75,120},{84,135},{1,1},{158,254},{6,9},{153,246},{11,17},{148,238},{16,25},{143,230},{21,33},{138,222},{26,41},{133,214},{31,49},{128,206},{36,57},{123,198},{41,65},{118,190},{46,73},{113,182},{51,81},{108,174},{56,89},{103,166},{61,97},{98,158},{66,105},{93,150},{71,113},{88,142},{76,121},{83,134},{2,3},{157,252},{7,11},{152,244},{12,19},{147,236},{17,27},{142,228},{22,35},{137,220},{27,43},{132,212},{32,51},{127,204},{37,59},{122,196},{42,67},{117,188},{47,75},{112,180},{52,83},{107,172},{57,91},{102,164},{62,99},{97,156},{67,107},{92,148},{72,115},{87,140},{77,123},{82,132},{3,4},{156,251},{8,12},{151,243},{13,20},{146,235},{18,28},{141,227},{23,36},{136,219},{28,44},{131,211},{33,52},{126,203},{38,60},{121,195},{43,68},{116,187},{48,76},{111,179},{53,84},{106,171},{58,92},{101,163},{63,100},{96,155},{68,108},{91,147},{73,116},{86,139},{78,124},{81,131},{4,6},{155,249},{9,14},{150,241},{14,22},{145,233},{19,30},{140,225},{24,38},{135,217},{29,46},{130,209},{34,54},{125,201},{39,62},{120,193},{44,70},{115,185},{49,78},{110,177},{54,86},{105,169},{59,94},{100,161},{64,102},{95,153},{69,110},{90,145},{74,118},{85,137},{79,126},{80,129}}
// Range: 19, Levels: 192, Bits: 6, Trits: 1, Quints: 0
{{0,0},{191,255},{3,4},{188,251},{6,8},{185,247},{9,12},{182,243},{12,16},{179,239},{15,20},{176,235},{18,24},{173,231},{21,28},{170,227},{24,32},{167,223},{27,36},{164,219},{30,40},{161,215},{33,44},{158,211},{36,48},{155,207},{39,52},{152,203},{42,56},{149,199},{45,60},{146,195},{48,64},{143,191},{51,68},{140,187},{54,72},{137,183},{57,76},{134,179},{60,80},{131,175},{63,84},{128,171},{66,88},{125,167},{69,92},{122,163},{72,96},{119,159},{75,100},{116,155},{78,104},{113,151},{81,108},{110,147},{84,112},{107,143},{87,116},{104,139},{90,120},{101,135},{93,124},{98,131},{1,1},{190,254},{4,5},{187,250},{7,9},{184,246},{10,13},{181,242},{13,17},{178,238},{16,21},{175,234},{19,25},{172,230},{22,29},{169,226},{25,33},{166,222},{28,37},{163,218},{31,41},{160,214},{34,45},{157,210},{37,49},{154,206},{40,53},{151,202},{43,57},{148,198},{46,61},{145,194},{49,65},{142,190},{52,69},{139,186},{55,73},{136,182},{58,77},{133,178},{61,81},{130,174},{64,85},{127,170},{67,89},{124,166},{70,93},{121,162},{73,97},{118,158},{76,101},{115,154},{79,105},{112,150},{82,109},{109,146},{85,113},{106,142},{88,117},{103,138},{91,121},{100,134},{94,125},{97,130},{2,2},{189,253},{5,6},{186,249},{8,10},{183,245},{11,14},{180,241},{14,18},{177,237},{17,22},{174,233},{20,26},{171,229},{23,30},{168,225},{26,34},{165,221},{29,38},{162,217},{32,42},{159,213},{35,46},{156,209},{38,50},{153,205},{41,54},{150,201},{44,58},{147,197},{47,62},{144,193},{50,66},{141,189},{53,70},{138,185},{56,74},{135,181},{59,78},{132,177},{62,82},{129,173},{65,86},{126,169},{68,90},{123,165},{71,94},{120,161},{74,98},{117,157},{77,102},{114,153},{80,106},{111,149},{83,110},{108,145},{86,114},{105,141},{89,118},{102,137},{92,122},{99,133},{95,126},{96,129}}
```

UASTC BISE Encoding Example
===========================

This section contains a low-level example of UASTC's BISE endpoint encoding method, which is similar to ASTC's.

In BISE coding, each integer value to be coded is divided up into bits and trits, or bits and quints. Trits range between [0,2] and quints [0,4]. 

Let's focus on UASTC mode 0. Mode 0 uses BISE range 19, which has 6 bits and 1 trit per endpoint value. Mode 0 is an opaque RGB mode with a single subset, so there are 6 total components (3 "low" and 3 "high" coordinates which define the single colorspace line's RGB endpoints). Each endpoint component has (2^6)\*3 or 192 unique values.

In ASTC/UASTC, endpoint values in ranges using trits or quints must first be converted into quantized integers before BISE packing. Either use the encoding tables above, or see section 23.12 and table 158 in the ASTC specification. Importantly, for trit and quint ranges, these quantized integers are not in monotonic order relative to the dequantized endpoint component values they represent, which can be confusing to developers new to ASTC.

In UASTC, the encoder first sends all the trit or quint values using one code per group, and then it sends the endpoint bit values using one code per endpoint value. This differs from ASTC, which interleaves the trit/quint group values and bits into packets.

Mode 0's BISE encoded endpoint integer values are packed into the 128-bit block like this:

<pre>
ETQ: 19 8
ETQ: 27 2
EBITS: 29 6
EBITS: 35 6
EBITS: 41 6
EBITS: 47 6
EBITS: 53 6
EBITS: 59 6
</pre>

Here's an explanation for what this means: The first ETQ ("endpoint-trit-quint") group value was packed into 8-bits at block bit position 19, and the second was packed into 2-bits at block bit position 27. The EBITS ("endpoint bits") values were each coded using 6-bits starting at block bit position 29.

The 6 endpoint components are packed in this order (the same order as ASTC):
<pre>
RL RH GL GH BL BH
</pre>

Let's call these 6 quantized integers to be coded E0, E1, E2, E3, E4, and E5, where E0=RL, E1=RH, etc.

First, the bits and trit values for each endpoint integer are computed:

<pre>
B0=E0 AND 63, T0=FLOOR(E0 / 64)
B1=E1 AND 63, T1=FLOOR(E1 / 64)
etc.
</pre>

The trit values T0, T1, etc. will all range between [0,2].

Now we have:
<pre>
0. B0 T0
1. B1 T1
2. B2 T2
3. B3 T3
4. B4 T4
5. B5 T5
</pre>

The encoder now combines the trits/quints values into groups. For trits, the total number of packed group codes is FLOOR((total_values+4)/5). For quints the total number of packed codes is FLOOR((total_values+2)/3).

Trits are packed in groups of 5 integers at a time into codes of up to 8-bits (because 3\*3\*3\*3\*3=243). Quints are packed in groups of 3 integers at a time into codes of up to 7-bits (because 5\*5\*5=125). 

If the last group is incomplete (like it is in mode 0), it gets packed into the fewest bits necessary to represent the final group value. (Note this differs from how ASTC's BISE works.) See the tables at the end of this section.

Mode 0's trit group values are computed like this:
<pre>
TRIT_GROUP_VAL0=T0+T1*3+T2*9+T3*27+T4*81
TRIT_GROUP_VAL1=T5
</pre>

(For quints the multipliers are 5 and 25.)

In mode 0, we only have 6 integers to pack, which corresponds to two groups. The first group has 5 values, and the second incomplete group only has 1 value. The first full group code is sent using 8-bits. The second incomplete group only uses 2-bits because it can only range between [0,2]:

<pre>
ETQ: 19 8 (TRIT_GROUP_VAL0)
ETQ: 27 2 (TRIT_GROUP_VAL1)
</pre>

Next, the encoder packs the bit values B0-B5:

<pre>
EBITS: 29 6 (B0)
EBITS: 35 6 (B1)
EBITS: 41 6 (B2)
EBITS: 47 6 (B3)
EBITS: 53 6 (B4)
EBITS: 59 6 (B5)
</pre>

The decoder inverts this encoding procedure. It first reads all the group codes, unpacks them into individual trit/quint values, then it combines these trit/quint values with the bit values to compute the endpoint integers.

This table shows the number of bits used to code each possible trit group size:

<pre>
Group Size       Number of Bits
1                2 
2                4 
3                5 
4                7
5                8
</pre>

And for quint groups:

<pre>
Group Size       Number of Bits
1                3
2                5
3                7
</pre>

Encoded UASTC Block Examples
============================

**1. Encoded UASTC block (mode 4):**

```{ 0xD3,0xFC,0xA6,0x0,0x0,0x0,0x20,0x9,0x82,0x20,0x48,0xFC,0x0,0x80,0x67,0x0 }```

Decoded block (as a 4x4 .PNG):
![Benchmark](unpacked_uastc_84.png "unpacked uastc texture")

**2. Encoded UASTC block (mode 4):**

```{ 0x13,0xE8,0x6,0x28,0x68,0x0,0x0,0xC7,0xF1,0xA6,0x69,0x6C,0x66,0x66,0x66,0x0 }```

Decoded block (as a 4x4 .PNG):
![Benchmark](unpacked_uastc_85.png "unpacked uastc texture")

**3. Encoded UASTC block (mode 0):**

```{ 0x61,0xA4,0xB6,0xA,0x29,0x9,0x1A,0xA0,0x0,0x40,0x30,0xE9,0xD8,0xFF,0xFF,0xFF }```

Decoded block (as a 4x4 .PNG):
![Benchmark](unpacked_uastc_87.png "unpacked uastc texture")

64 UASTC Block Examples
-----------------------

This random UASTC block data and what it decodes to is useful for developing and validating new UASTC decoders. A proper UASTC decoder must decode these UASTC blocks *exactly* to the indicated RGBA pixels.

First, here are the UASTC block bytes (16 bytes per block). Note a few blocks are purposely invalid.

```
{ 0x8, 0x18, 0x43, 0x67, 0x57, 0x4F, 0xB0, 0x8A, 0x5E, 0xB4, 0x62, 0x35, 0x42, 0xE2, 0xE, 0x22 },
{ 0xF1, 0xF0, 0x18, 0x8F, 0xE4, 0x6B, 0x3C, 0x3A, 0x90, 0xFB, 0x89, 0x5D, 0x56, 0x82, 0x9B, 0x6C },
{ 0x84, 0x9F, 0x81, 0x8D, 0xF5, 0xD3, 0x64, 0x4, 0x5B, 0xBB, 0x43, 0x87, 0x31, 0xAF, 0xC1, 0x3D },
{ 0xE, 0xB7, 0xC7, 0xFB, 0x50, 0x2, 0x79, 0x69, 0xDE, 0x93, 0xE2, 0x3F, 0xB5, 0x1B, 0x38, 0xEE },
{ 0x71, 0x9E, 0xB0, 0x2F, 0xA7, 0x6D, 0x26, 0xAC, 0x12, 0xC0, 0xB8, 0xA2, 0xB5, 0xCA, 0x11, 0x48 },
{ 0xD, 0x6A, 0x1E, 0x11, 0x35, 0x44, 0xB2, 0xE8, 0x5B, 0xDB, 0xD3, 0xB5, 0x4E, 0x0, 0xD, 0xB9 },
{ 0xCC, 0x6A, 0x83, 0x46, 0x83, 0xD4, 0xCF, 0x8A, 0xD7, 0xF6, 0xEF, 0xBC, 0x83, 0x69, 0xB0, 0xB4 },
{ 0x2B, 0x24, 0x5, 0x47, 0x26, 0xD6, 0x5E, 0xE2, 0xAA, 0x54, 0x5F, 0x72, 0xED, 0x77, 0x4C, 0x21 },
{ 0x3A, 0x66, 0xAA, 0x96, 0x0, 0xFA, 0xB7, 0xE2, 0x93, 0x35, 0xC4, 0xBE, 0x32, 0xC4, 0xA3, 0x97 },
{ 0xFD, 0x9, 0xB0, 0xF0, 0x15, 0x99, 0xB6, 0x6, 0xA5, 0x66, 0x7A, 0x74, 0xBA, 0x27, 0x6B, 0xDE },
{ 0x33, 0x1E, 0x79, 0x42, 0xF3, 0x98, 0xB7, 0x11, 0x2D, 0xFF, 0xE5, 0x59, 0xAD, 0x23, 0x10, 0xC },
{ 0x56, 0xD2, 0x6D, 0xEC, 0x43, 0xDC, 0xDF, 0x14, 0x8A, 0x8, 0xD9, 0xC8, 0x9E, 0x8C, 0xF5, 0x92 },
{ 0x5D, 0x77, 0x5C, 0x2C, 0xF5, 0x39, 0x7F, 0x0, 0x49, 0xE8, 0xB6, 0xDC, 0x42, 0x90, 0x85, 0xB3 },
{ 0x5A, 0x22, 0xC3, 0x5E, 0xCE, 0xB6, 0x4D, 0xD4, 0x81, 0xE, 0x73, 0x21, 0x94, 0xA8, 0x18, 0xDD },
{ 0xBA, 0x1C, 0xD2, 0xD2, 0x87, 0xDE, 0x2, 0x3F, 0x71, 0x82, 0xD7, 0x91, 0xED, 0xDA, 0xFA, 0xDD },
{ 0xE0, 0x4A, 0xB5, 0x1E, 0x43, 0x9C, 0xA5, 0x52, 0xE6, 0x91, 0x8A, 0x8D, 0x51, 0x75, 0x19, 0xC2 },
{ 0x8F, 0x6C, 0x7A, 0xB, 0x65, 0xEC, 0x26, 0x74, 0x16, 0x37, 0x39, 0x57, 0xB3, 0xB0, 0xF2, 0xFD },
{ 0xFA, 0xB5, 0x91, 0xC1, 0x2A, 0x5D, 0xAC, 0xFF, 0x90, 0x33, 0xB6, 0xB7, 0x53, 0xF6, 0x64, 0x2A },
{ 0xBD, 0xF8, 0xF6, 0x25, 0x70, 0xD0, 0xF5, 0x71, 0x63, 0x2, 0x3E, 0x63, 0xD1, 0x10, 0x46, 0x35 },
{ 0xE2, 0xE0, 0xAF, 0xB0, 0x89, 0xB1, 0x35, 0x6F, 0xA2, 0x83, 0x66, 0x8B, 0x31, 0x1D, 0x24, 0x5D },
{ 0xC3, 0x9A, 0x71, 0x34, 0xA4, 0xC9, 0x9F, 0x55, 0xF6, 0x5C, 0x73, 0x7E, 0x44, 0xD3, 0x77, 0x47 },
{ 0x3D, 0x5D, 0xF2, 0x24, 0x7B, 0x2A, 0xD, 0x30, 0xD2, 0x20, 0x26, 0x28, 0xF1, 0xB5, 0x5D, 0x8E },
{ 0x26, 0x74, 0xD6, 0xA5, 0x80, 0x68, 0xBB, 0x68, 0x65, 0x29, 0xAC, 0x93, 0xCC, 0x42, 0x8D, 0xF8 },
{ 0x57, 0x41, 0x44, 0xB6, 0x80, 0x57, 0x53, 0xA, 0xE9, 0x41, 0x82, 0xD7, 0xCC, 0x27, 0xF6, 0x24 },
{ 0x23, 0x63, 0x1A, 0x93, 0x84, 0x27, 0xD7, 0x4A, 0xCF, 0xDC, 0x34, 0xAC, 0x8D, 0xA1, 0x8F, 0x91 },
{ 0x58, 0x97, 0xEA, 0x3B, 0xF8, 0x8, 0xA8, 0x4D, 0x2F, 0xF7, 0xAD, 0x19, 0x70, 0x9F, 0xA, 0x26 },
{ 0xD0, 0xD7, 0xD4, 0x65, 0x3C, 0x91, 0xF, 0x55, 0x75, 0x93, 0x94, 0x83, 0xF6, 0x65, 0xCA, 0xBB },
{ 0xB, 0x70, 0x30, 0x86, 0xDA, 0x91, 0x69, 0xCC, 0xBA, 0xC8, 0xFB, 0x0, 0xB3, 0x79, 0x7B, 0x9E },
{ 0x6E, 0xD2, 0x1B, 0xEA, 0x46, 0xB5, 0xF4, 0x65, 0xA7, 0xB3, 0xE0, 0x2D, 0x27, 0x58, 0xB2, 0xAE },
{ 0x3C, 0xD0, 0xC5, 0x50, 0x79, 0x22, 0xD1, 0xD6, 0x10, 0x2F, 0x7F, 0x83, 0x77, 0x73, 0xCC, 0xD9 },
{ 0x5, 0xC2, 0x40, 0x2D, 0xBB, 0x89, 0xFC, 0x8D, 0x43, 0x7E, 0x4F, 0xF6, 0x52, 0x54, 0x4F, 0x8D },
{ 0x4B, 0x31, 0x58, 0xE5, 0x51, 0xD4, 0x27, 0x7C, 0x26, 0xA5, 0x58, 0x93, 0x9B, 0x15, 0x0, 0x49 },
{ 0xED, 0x81, 0x24, 0xB3, 0x21, 0x60, 0x56, 0xF9, 0x9A, 0x79, 0xFB, 0x56, 0xDC, 0x86, 0xDE, 0x83 },
{ 0x1C, 0x23, 0x9F, 0x13, 0xF7, 0xF2, 0xB4, 0xA9, 0x7A, 0xCE, 0x32, 0x2E, 0x47, 0x31, 0xB2, 0xE8 },
{ 0x4D, 0xBA, 0x4C, 0x63, 0x2, 0x79, 0xEB, 0x3D, 0x3C, 0x92, 0x23, 0x82, 0x37, 0x60, 0x3F, 0x46 },
{ 0x99, 0x7E, 0xDF, 0x6B, 0xB8, 0xBA, 0x52, 0x4C, 0xBC, 0x11, 0x21, 0xC2, 0xEB, 0xDE, 0x12, 0xE6 },
{ 0xEC, 0x42, 0xA4, 0xE5, 0x71, 0x7, 0xE5, 0x64, 0xF0, 0xBE, 0x81, 0xFF, 0xD5, 0x4D, 0xB9, 0x9E },
{ 0x5C, 0x90, 0xD1, 0x60, 0x14, 0xA3, 0x52, 0x17, 0xAA, 0xE6, 0x63, 0x7, 0x4D, 0x2F, 0x12, 0x2B },
{ 0x57, 0x5C, 0xFB, 0xD8, 0xD5, 0x1, 0xEB, 0xFA, 0xDE, 0xDC, 0x5B, 0x29, 0x96, 0x4D, 0xB0, 0x59 },
{ 0x6F, 0xBB, 0x89, 0xF2, 0xC5, 0x32, 0x3E, 0xAA, 0xC7, 0x31, 0xE, 0x5A, 0x1, 0x87, 0x3F, 0x78 },
{ 0x2F, 0x71, 0x68, 0xF2, 0xB9, 0xF9, 0x1A, 0x52, 0x90, 0x33, 0xEB, 0x57, 0xA, 0xC4, 0xA7, 0xEF },
{ 0x62, 0x80, 0x1, 0xD3, 0x84, 0x3A, 0xA9, 0x6, 0x25, 0x54, 0xD0, 0xCC, 0x25, 0x29, 0xFB, 0xA5 },
{ 0xC3, 0xAF, 0xA0, 0x20, 0xE8, 0x96, 0xE8, 0x1C, 0x38, 0x56, 0xA6, 0x81, 0xB1, 0xDD, 0x5B, 0xC7 },
{ 0x6D, 0x2C, 0xFB, 0xB8, 0xE, 0x5, 0x60, 0xDF, 0xB8, 0x1B, 0xE1, 0x5E, 0xE, 0xAA, 0x5C, 0xF5 },
{ 0x71, 0x57, 0x7C, 0x1D, 0x51, 0xB2, 0xE6, 0xEE, 0xC5, 0x7, 0x4, 0xD7, 0xC2, 0x13, 0x56, 0x52 },
{ 0x62, 0xB4, 0x53, 0x4C, 0x8B, 0x25, 0x4F, 0x7B, 0x89, 0xF1, 0xB, 0x1E, 0xC5, 0xC1, 0x33, 0x9A },
{ 0xB7, 0x73, 0x25, 0xF5, 0xF0, 0xD9, 0x24, 0x27, 0x52, 0x83, 0x21, 0xA5, 0x29, 0x35, 0x6, 0x70 },
{ 0xE, 0x7E, 0x12, 0x12, 0x7F, 0x5E, 0x93, 0x54, 0xC9, 0xF, 0xEC, 0x4D, 0x19, 0x2F, 0xA5, 0x39 },
{ 0x1A, 0x6E, 0xED, 0xBB, 0x35, 0xAE, 0x2F, 0x56, 0x71, 0x2D, 0xCE, 0xBC, 0xAC, 0x88, 0x5D, 0x87 },
{ 0xB5, 0x24, 0xC0, 0xE7, 0xA2, 0x4, 0x8C, 0xA8, 0x22, 0x5D, 0x7B, 0xE0, 0xF, 0xD2, 0x8C, 0x55 },
{ 0x97, 0x43, 0x6F, 0x3C, 0x4F, 0x89, 0x3F, 0xB7, 0x48, 0x90, 0xDB, 0x7F, 0x9A, 0xF6, 0x1B, 0x58 },
{ 0xE8, 0xA2, 0x97, 0xFB, 0x3A, 0xCA, 0xFB, 0x8C, 0xAC, 0xB6, 0xD3, 0x87, 0x64, 0xB3, 0x52, 0xA9 },
{ 0x1D, 0xFD, 0x4B, 0x46, 0xDF, 0x8E, 0x9, 0x4E, 0x96, 0x9B, 0x37, 0x82, 0x6F, 0x14, 0x7F, 0xE3 },
{ 0xAE, 0x3A, 0x7, 0x3C, 0x80, 0xC1, 0xC2, 0x14, 0xC0, 0x4E, 0x43, 0x88, 0x63, 0x8C, 0x7B, 0x5F },
{ 0x94, 0x25, 0xA8, 0x52, 0x1B, 0x8B, 0x47, 0x24, 0x2F, 0xBE, 0x66, 0x93, 0xDB, 0x26, 0xA3, 0x81 },
{ 0x5F, 0x2E, 0xE2, 0x34, 0x73, 0x5C, 0x8D, 0xF0, 0x96, 0xE9, 0xE7, 0xD7, 0x53, 0x4B, 0xF2, 0xB },
{ 0x1F, 0xA6, 0x31, 0x90, 0xE6, 0x2C, 0xF8, 0x7B, 0x23, 0x3A, 0x94, 0xE0, 0xA5, 0xD1, 0x3F, 0x3 },
{ 0x40, 0xC6, 0x80, 0x68, 0xBC, 0x90, 0xC7, 0xFE, 0xF6, 0x58, 0x3D, 0xEB, 0x59, 0x2A, 0x42, 0x3B },
{ 0xD0, 0x7C, 0x96, 0x1B, 0x35, 0x44, 0xA1, 0x5F, 0x55, 0x45, 0x64, 0x74, 0xDC, 0x9B, 0x24, 0x42 },
{ 0x4E, 0x47, 0x1, 0x69, 0x3F, 0x3B, 0xF3, 0x6F, 0xBB, 0x43, 0xDA, 0x5F, 0x16, 0x9A, 0xDC, 0x38 },
{ 0x2F, 0xB5, 0xB1, 0x51, 0x5D, 0x71, 0xCB, 0xFC, 0x36, 0x66, 0xBF, 0x19, 0x79, 0xF3, 0xA3, 0x7E },
{ 0xC1, 0xF5, 0xFA, 0xFC, 0x12, 0x1A, 0x6C, 0xEB, 0xF6, 0x47, 0xEE, 0x1F, 0x75, 0xC, 0x56, 0x23 },
{ 0xB8, 0xF8, 0x33, 0x54, 0xEA, 0xCE, 0x7B, 0x2, 0x6C, 0xD7, 0x15, 0xA0, 0xD0, 0x40, 0x4E, 0x91 },
{ 0xFB, 0x31, 0xEB, 0x91, 0x34, 0xFF, 0x84, 0x1F, 0x31, 0x99, 0x3D, 0xD0, 0x67, 0x81, 0x6D, 0xD }
```

Next, here is each UASTC block decoded to RGBA pixels. R is at bit position 0, G at 8, B at 16, and A at 24. We've purposely included some invalid UASTC blocks, which gets decoded to purple (255,0,255,255):

```
{ 0x255CF1C4, 0x255CF1C4, 0x1061EFC4, 0x5151F4AE, 0x3B56F3D9, 0x3B56F3C4, 0x255CF1C4, 0x5151F4D9, 0x3B56F3D9, 0x1061EFC4, 0x3B56F3D9, 0x3B56F398, 0x3B56F398, 0x1061EFD9, 0x3B56F3D9, 0x3B56F3D9 },
{ 0xFFDD964A, 0xFFEA8066, 0xFFED7C6C, 0xFFF27278, 0xFFEA8066, 0xFFE98263, 0xFFF07672, 0xFFE48A59, 0xFFE6885C, 0xFFE48A59, 0xFFE09250, 0xFFE98263, 0xFFED7C6C, 0xFFEA8066, 0xFFEE796F, 0xFFE6885C },
{ 0xFAB92099, 0xCAD06299, 0x67FFEA93, 0x67FFEA93, 0x67FFEA9E, 0xFAB92099, 0x67FFEA99, 0xFAB92093, 0xCAD0629E, 0x67FFEA9E, 0x67FFEA8E, 0x97E8A893, 0xCAD0629E, 0xFAB9208E, 0xCAD0628E, 0x67FFEA9E },
{ 0x3A6C2648, 0x2F726628, 0x2578A30A, 0x2578A30A, 0x33704F33, 0x376E3B3D, 0x2C747A1E, 0x2C747A1E, 0x33704F33, 0x33704F33, 0x3E6A1252, 0x2F726628, 0x33704F33, 0x2F726628, 0x33704F33, 0x2578A30A },
{ 0xFFC1678D, 0xFFC1678D, 0xFFBF6C8F, 0xFFD22770, 0xFFCC3E7A, 0xFFD02E73, 0xFFC2618A, 0xFFCF3375, 0xFFC75083, 0xFFD02E73, 0xFFCF3375, 0xFFD22770, 0xFFC1678D, 0xFFC1678D, 0xFFCC3E7A, 0xFFC55685 },
{ 0x47BB464C, 0x47BB464C, 0x47BB464C, 0x5DB58B43, 0x47BB464C, 0x52B86947, 0x3DBE2451, 0x3DBE2451, 0x3DBE2451, 0x3DBE2451, 0x52B86947, 0x52B86947, 0x47BB464C, 0x3DBE2451, 0x52B86947, 0x3DBE2451 },
{ 0x3BD6BB05, 0x3BD6BB1B, 0x5CBCA10C, 0x7CA3881B, 0x7CA3881B, 0x5CBCA11B, 0x1BEFD41B, 0x7CA38813, 0x7CA38805, 0x1BEFD413, 0x3BD6BB13, 0x5CBCA10C, 0x1BEFD405, 0x7CA38813, 0x1BEFD40C, 0x7CA38813 },
{ 0xFF4B896D, 0xFF4B896D, 0xFF4B896D, 0xFF87C667, 0xFF9BDA66, 0xFFAEED64, 0xFF4B896D, 0xFF4B896D, 0xFF9BDA66, 0xFF87C667, 0xFF9BDA66, 0xFF9BDA66, 0xFFAEED64, 0xFF87C667, 0xFF5F9D6B, 0xFF74B369 },
{ 0xF8AD9305, 0xB7CA9008, 0xD8BB9106, 0xE9B39206, 0xE0B89206, 0x9ED58F09, 0x8FDC8E0A, 0xA8D18F09, 0xF1B09205, 0xE9B39206, 0xE0B89206, 0x9ED58F09, 0xE9B39206, 0xAFCE9009, 0xC9C29107, 0xB7CA9008 },
{ 0xFF375790, 0xFF3F6480, 0xFF293FAD, 0xFF467072, 0xFF558855, 0xFF2233BB, 0xFF467072, 0xFF375790, 0xFF304B9E, 0xFF558855, 0xFF4E7C63, 0xFF939CBA, 0xFF3F6480, 0xFF7D96CD, 0xFF6A92DE, 0xFF578DEF },
{ 0xFF54977B, 0xFF78A081, 0xFF669B7E, 0xFF669B7E, 0xFF669B7E, 0xFF8AA484, 0xFF78A081, 0xFF7527AB, 0xFF8AA484, 0xFF669B7E, 0xFF7527AB, 0xFF622FC5, 0xFF54977B, 0xFF7527AB, 0xFF4D38E0, 0xFF7527AB },
{ 0x9167DBF, 0x561D9E9E, 0x561D9E9E, 0x7220AA91, 0x7220AA91, 0x8C23B586, 0xBF28CA70, 0x7220AA91, 0x7220AA91, 0x221888B4, 0xA625BF7B, 0x3C1B93A9, 0xBF28CA70, 0x8C23B586, 0x7220AA91, 0x7220AA91 },
{ 0xFFCCAA66, 0xFFA7E78B, 0xFF3539A9, 0xFF267550, 0xFFA0F392, 0xFF3A26C6, 0xFF267550, 0xFF2B626C, 0xFFA7E78B, 0xFF4400FF, 0xFF3A26C6, 0xFF4400FF, 0xFF3A26C6, 0xFF267550, 0xFF3A26C6, 0xFF4400FF },
{ 0x563BA3C4, 0x65299A73, 0x6F1D9439, 0x563BA3C4, 0x5B35A0A8, 0x622C9C81, 0x5839A2BB, 0x5937A1B2, 0x5D329F9D, 0x6627996A, 0x65299A73, 0x68259860, 0x65299A73, 0x5839A2BB, 0x6E1F9542, 0x6E1F9542 },
{ 0x8EFF83EF, 0xA4AE6091, 0x94E979D5, 0xA89D597E, 0xA4AE6091, 0xB8623F3A, 0x91F47EE2, 0xAB925471, 0xB8623F3A, 0xBB573B2D, 0xAE874F64, 0xB8623F3A, 0xAE874F64, 0xBE4C3620, 0xB8623F3A, 0xB8623F3A },
{ 0x57BE9B5B, 0x865164AF, 0x57BE9B5B, 0x57759B5B, 0x867564AF, 0x2B75CF0B, 0x57519B5B, 0x2B75CF0B, 0x57BE9B5B, 0x579A9B5B, 0x579A9B5B, 0xB39A30FF, 0x57759B5B, 0x57BE9B5B, 0x86BE64AF, 0x2B51CF0B },
{ 0xB6BBB0AA, 0xAA336622, 0x5A1C712D, 0x83286C27, 0xB6BBB0AA, 0xBBCCBBBB, 0xB0AAA499, 0x83286C27, 0xB6BBB0AA, 0xB6BBB0AA, 0xB0AAA499, 0xAA999988, 0xAA999988, 0xB0AAA499, 0xAA999988, 0xAA999988 },
{ 0x88307CD9, 0xB0733AC2, 0x944468D2, 0x944468D2, 0xA25B52CA, 0xB8812CBD, 0xA6624BC8, 0xB8812CBD, 0x944468D2, 0x9D5458CD, 0xA25B52CA, 0xC99E10B3, 0x994D5FCF, 0xA25B52CA, 0xB47A33C0, 0x903D6ED5 },
{ 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF }, // invalid UASTC block
{ 0xD1584B29, 0xE59A814D, 0xD5655631, 0xE18D7646, 0xDC7C683D, 0xDC7C683D, 0xE7A18751, 0xE18D7646, 0xD1584B29, 0xD5655631, 0xEBB19359, 0xD1584B29, 0xD86E5D35, 0xD35E512D, 0xEBB19359, 0xDA756339 },
{ 0xFF6DAB5C, 0xFF3D6C3D, 0xFF2E5C8B, 0xFF2E5C8B, 0xFFA3D15C, 0xFF356465, 0xFF457417, 0xFF356465, 0xFF35825C, 0xFF64C294, 0xFF45BABA, 0xFF64C294, 0xFF005C5C, 0xFFA3D145, 0xFF64C294, 0xFFA3D145 },
{ 0xFF998866, 0xFF7C432B, 0xFF998866, 0xFF83563B, 0xFF8B6649, 0xFF998866, 0xFF8B6649, 0xFF927758, 0xFF927758, 0xFF6D210E, 0xFF661100, 0xFF8B6649, 0xFF729554, 0xFF8F4B88, 0xFF8F4B88, 0xFF866378 },
{ 0xEF51CD3E, 0xF32CD1A0, 0xF520D2BF, 0xED5DCC1F, 0xED5DCC1F, 0xED5DCC1F, 0xF045CE5E, 0xF520D2BF, 0xEF51CD3E, 0xEB69CA00, 0xF32CD1A0, 0xF520D2BF, 0xEB69CA00, 0xED5DCC1F, 0xF520D2BF, 0xF615D3DE },
{ 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A, 0x5B2220A },
{ 0xFF455C8B, 0xFFA3E8D1, 0xFF84BABA, 0xFF84BABA, 0xFF648AA2, 0xFFA3E8D1, 0xFF455C8B, 0xFF84BABA, 0xFF54E17A, 0xFF5CE845, 0xFF5CE845, 0xFF54E17A, 0xFFAB548A, 0xFF2E74E8, 0xFF2E74E8, 0xFFE8455C },
{ 0xD54F0E9B, 0xB94210C0, 0x9E360EE4, 0x9E360BE4, 0xD54F0B9B, 0xB9420CC0, 0xD54F0C9B, 0xD54F109B, 0xEF5C1077, 0x9E360EE4, 0x9E360BE4, 0xD54F0C9B, 0xB9420CC0, 0xEF5C1077, 0xB9420EC0, 0xB94210C0 },
{ 0xDBEF4466, 0xDFAB7720, 0xDFEF7720, 0xDB644466, 0xD9AB2B88, 0xDB644466, 0xDFEF7720, 0xD9642B88, 0xDDAB5E42, 0xDF207720, 0xDBAB4466, 0xDDAB5E42, 0xDD645E42, 0xD9202B88, 0xDF647720, 0xDF647720 },
{ 0xFFBB5180, 0xFFBF4076, 0xFFB7658B, 0xFFB7658B, 0xFFAC99A8, 0xFFB0889E, 0xFFAC99A8, 0xFFC61D63, 0xFFC61D63, 0xFFB7658B, 0xFFC32E6D, 0xFFBB5180, 0xFFBB5180, 0xFFB0889E, 0xFFBB5180, 0xFFBB5180 },
{ 0xC665B5B9, 0x6D3C9580, 0x2D1E7E56, 0x42288664, 0x9B51A69D, 0x42288664, 0xB15BADAB, 0xB15BADAB, 0xC665B5B9, 0x85479E8F, 0xB15BADAB, 0xB15BADAB, 0x85479E8F, 0x58328E72, 0x85479E8F, 0x58328E72 },
{ 0xDF264C7C, 0xE3513F7C, 0xEAA926B3, 0xE77E327C, 0xEAA926B3, 0xEAA9268E, 0xEAA9267C, 0xDF264CA1, 0xEAA9268E, 0xEAA9268E, 0xEAA9267C, 0xEAA9268E, 0xDF264CB3, 0xDF264CB3, 0xE3513FA1, 0xE3513FB3 },
{ 0xA99F9F9F, 0xE7E0E0E0, 0xDBD4D4D4, 0x37262626, 0x695A5A5A, 0x9D929292, 0xDBD4D4D4, 0x695A5A5A, 0x9D929292, 0x80737373, 0xDBD4D4D4, 0x74676767, 0x74676767, 0x9D929292, 0x74676767, 0xA99F9F9F },
{ 0xFFA95545, 0xFF8D6535, 0xFFA95545, 0xFFA95545, 0xFFB54D4D, 0xFF747525, 0xFFA95545, 0xFF9C5C3E, 0xFFA95545, 0xFF747525, 0xFF816D2D, 0xFFB54D4D, 0xFF9C5C3E, 0xFF816D2D, 0xFFC24555, 0xFFC24555 },
{ 0x9FA87512, 0x87A0860A, 0xB7AF661B, 0x87A0860A, 0x6F999502, 0x87A0860A, 0x9FA87512, 0x6F999502, 0xB7AF661B, 0xB7AF661B, 0x9FA87512, 0x6F999502, 0x6F999502, 0x87A0860A, 0x6F999502, 0x9FA87512 },
{ 0x5C986C9E, 0x5C30E4A9, 0x5C52BDA6, 0x5C986C9E, 0x5C52BDA6, 0x5C30E4A9, 0x5C52BDA6, 0x5C52BDA6, 0x5C30E4A9, 0x5C986C9E, 0x5C7693A2, 0x5C30E4A9, 0x5C52BDA6, 0x5C30E4A9, 0x5C986C9E, 0x5C52BDA6 },
{ 0x39C3B726, 0x39C3B726, 0x2223DE90, 0x2223DE90, 0x2957D26D, 0x318FC449, 0x39C3B726, 0x39C3B726, 0x39C3B726, 0x2223DE90, 0x2957D26D, 0x2223DE90, 0x2223DE90, 0x318FC449, 0x39C3B726, 0x2223DE90 },
{ 0xFF9631B9, 0xFFA81DD6, 0xFF79518C, 0xFF6C5F77, 0xFF64686A, 0xFF982EBD, 0xFFA323CE, 0xFFA323CE, 0xFFA81DD6, 0xFF5C705E, 0xFF67656F, 0xFF5F6D62, 0xFF8B3DA9, 0xFF9631B9, 0xFF6C5F77, 0xFF626A66 },
{ 0xB3BEF461, 0xFAEF269E, 0xE3DF6A9E, 0xFAEF268A, 0xCBCEB161, 0xB3BEF48A, 0xFAEF269E, 0xFAEF269E, 0xCBCEB175, 0xCBCEB19E, 0xCBCEB19E, 0xB3BEF475, 0xCBCEB18A, 0xFAEF268A, 0xE3DF6A9E, 0xCBCEB18A },
{ 0xD94941DF, 0x73754C92, 0x73494C92, 0x739E4C92, 0x4120516C, 0x73494C92, 0x4149516C, 0xD92041DF, 0xA79E46B9, 0xD94941DF, 0x419E516C, 0x73204C92, 0x73204C92, 0xA72046B9, 0x4175516C, 0x73204C92 },
{ 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2, 0xAEC7DAE2 },
{ 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF }, // invalid UASTC block
{ 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF, 0xFFFF00FF }, // invalid UASTC block
{ 0xD6379812, 0xD6379812, 0xBF519D15, 0xB45DA017, 0xEA209310, 0x5AC2B423, 0x65B7B222, 0x65B7B222, 0xB45DA017, 0xD6379812, 0x8691AA1D, 0xD6379812, 0x72A8AF20, 0x46D9B926, 0xB45DA017, 0x7C9CAD1E },
{ 0xFF00458B, 0xFF00458B, 0xFFD1A35C, 0xFFD1A35C, 0xFF17A3E8, 0xFF00458B, 0xFFBAD12E, 0xFF742EA3, 0xFF0F84CA, 0xFF0764AA, 0xFF742EA3, 0xFFA39C54, 0xFF17A3E8, 0xFF17A3E8, 0xFFC2543D, 0xFFCA7D4D },
{ 0x599C51B8, 0xA6ACA583, 0x118D00EB, 0xEEBBF650, 0x599C51B8, 0x118D00EB, 0x118D00EB, 0x599C51B8, 0x599C51B8, 0x599C51B8, 0x599C51B8, 0xA6ACA583, 0xEEBBF650, 0xA6ACA583, 0xA6ACA583, 0xA6ACA583 },
{ 0xFF90D111, 0xFF89E314, 0xFF8DDA12, 0xFF91CE11, 0xFF8ED512, 0xFF91CE11, 0xFF8DDA12, 0xFF88E414, 0xFF90D111, 0xFF89E314, 0xFF8FD311, 0xFF90D011, 0xFF8DD812, 0xFF8ED712, 0xFF90D111, 0xFF8ED712 },
{ 0x9FA93DB9, 0xA18B6C9B, 0x9EBE1ACE, 0xA35CB96C, 0xA2788C88, 0x9EC410D4, 0xA362AF72, 0x9EBE1ACE, 0xA0A247B2, 0xA26F9980, 0x9EBE1ACE, 0xA26F9980, 0x9FB12FC1, 0x9FB12FC1, 0xA17E828E, 0xA1857795 },
{ 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D, 0x87A92B9D },
{ 0xA2472F80, 0xDA33297D, 0xFE26257A, 0xEC2C277B, 0xC8392B7E, 0xA2472F80, 0xEC2C277B, 0x7E543483, 0xFE26257A, 0xDA33297D, 0xC8392B7E, 0xA2472F80, 0xA2472F80, 0xB4402D7F, 0xEC2C277B, 0x904D3282 },
{ 0xAEC4CF56, 0x84DFB6A3, 0x5CF89FEA, 0xA3CCC86B, 0x57FC9CF5, 0x62F4A2E0, 0x62F4A2E0, 0x6AF0A7D2, 0x62F4A2E0, 0x6FECAAC8, 0x7BE5B1B3, 0x7BE5B1B3, 0x5CF89FEA, 0x90D8BD8E, 0x84DFB6A3, 0x7BE5B1B3 },
{ 0xFF343831, 0xFF44253E, 0xFF343831, 0xFF156017, 0xFF343831, 0xFF343831, 0xFF156017, 0xFF244C23, 0xFF156017, 0xFF343831, 0xFF44253E, 0xFF44253E, 0xFF244C23, 0xFF156017, 0xFF156017, 0xFF156017 },
{ 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C, 0x79E37A1C },
{ 0xA69E3567, 0x5FC25D56, 0x5F9E5D56, 0x1BC28346, 0x1B7C8346, 0xA6E43567, 0x1B9E8346, 0xEAC21077, 0xEA9E1077, 0x5F9E5D56, 0x1B7C8346, 0x1BC28346, 0x5F7C5D56, 0xA69E3567, 0xA6C23567, 0x5FC25D56 },
{ 0xFFBE6B3A, 0xFFC25327, 0xFFCA2E09, 0xFFC25327, 0xFFC25327, 0xFFC5461C, 0xFFBB7744, 0xFF77FF33, 0xFFCC2200, 0xFFC25327, 0xFFC25327, 0xFFA8A778, 0xFFC05F31, 0xFFC5461C, 0xFFCC66AA, 0xFFCC66AA },
{ 0x76122C23, 0x6D162C00, 0x76122C23, 0x93092F91, 0x6D162C00, 0xAF0031FA, 0x6D162C00, 0x890C2E69, 0x93092F91, 0x76122C23, 0xA60330D7, 0x9D062FB4, 0xAF0031FA, 0xA60330D7, 0xAF0031FA, 0x800F2D46 },
{ 0x9CF93749, 0x83F61558, 0x83F67C58, 0x6CF45A67, 0x83F63758, 0x83F63758, 0x6CF41567, 0x9CF95A49, 0x6CF45A67, 0x9CF97C49, 0x83F63758, 0x83F61558, 0x6CF41567, 0x83F65A58, 0x9CF91549, 0xB3FA5A3B },
{ 0x9F5B35CC, 0x5FA6C271, 0x5FA6C271, 0x9F5B35CC, 0x9FA6C271, 0x9FA6C271, 0x5FA6C271, 0x5F5B35CC, 0x9F5B35CC, 0x9FA6C271, 0x5F5B35CC, 0x9F5B35CC, 0x5FA6C271, 0x5FA6C271, 0x5FA6C271, 0x5F5B35CC },
{ 0x508DE09A, 0x82E8EFB3, 0x82E8E0B3, 0x82E8E0B3, 0x508DEF9A, 0x508DEF9A, 0x82E8E0B3, 0x508DE09A, 0x82E8E0B3, 0x82E8EFB3, 0x82E8EFB3, 0x82E8EFB3, 0x82E8EFB3, 0x508DE09A, 0x82E8EFB3, 0x508DE09A },
{ 0x95E54283, 0xAEAE7C15, 0x88FF2639, 0x95E5425F, 0x95E54215, 0xAEAE7C83, 0xAEAE7C39, 0xA2C96015, 0x95E54239, 0x95E5425F, 0xA2C96039, 0xA2C96083, 0xA2C96083, 0x88FF265F, 0xAEAE7C39, 0xAEAE7C83 },
{ 0xCD4CF168, 0xCD5CF168, 0xCD5CF168, 0xC45CFF98, 0xC45CFF98, 0xD65CE235, 0xC45CFF98, 0xDF5CD405, 0xC47CFF98, 0xCD7CF168, 0xDF6CD405, 0xCD6CF168, 0xC45CFF98, 0xD64CE235, 0xD64CE235, 0xC45CFF98 },
{ 0xAD8DC3B7, 0x7A82AC9E, 0x126D7E6C, 0x126D7E6C, 0x44779584, 0x5E7DA091, 0x44779584, 0xC692CEC3, 0x9388B8AB, 0x7A82AC9E, 0x9388B8AB, 0x2B728978, 0x44779584, 0xAD8DC3B7, 0x2B728978, 0xAD8DC3B7 },
{ 0xCCFF33BB, 0x9EE98E49, 0x88DDBB11, 0xB6F46083, 0x88DDBB11, 0xCCFF33BB, 0x88DDBB11, 0x88DDBB11, 0xC75A99D8, 0xB6F46083, 0xCCFF33BB, 0xB6F46083, 0xC75A99D8, 0x7766BBAA, 0x88DDBB11, 0x88DDBB11 },
{ 0xFFCAC4AD, 0xFFC73206, 0xFFC99476, 0xFFC9B69D, 0xFFC73D12, 0xFFC73D12, 0xFFC73206, 0xFFCADAC6, 0xFFC9AA90, 0xFFC99476, 0xFFC8532C, 0xFFCAE5D3, 0xFFC99F83, 0xFFC9AA90, 0xFFCAC4AD, 0xFFCACFBA },
{ 0x38EC473, 0x78EA38C, 0xB8E83A3, 0x3BEC473, 0x38EC473, 0x377C473, 0x77E45C, 0x7A7A38C, 0x77E45C, 0x3BEC473, 0x77E45C, 0x8EE45C, 0x7BEA38C, 0x8EE45C, 0x377C473, 0x3A7C473 },
{ 0xFF84B4B6, 0xFF844181, 0xFF418EA5, 0xFF418EA5, 0xFF218EA5, 0xFF844181, 0xFF84B4B6, 0xFF218EA5, 0xFF634181, 0xFF636792, 0xFF848EA5, 0xFF41B4B6, 0xFF218EA5, 0xFF636792, 0xFF218EA5, 0xFF84B4B6 }
```

External References
===================

https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#ASTC

https://docs.microsoft.com/en-us/windows/win32/direct3d11/bc7-format-mode-reference

https://rockets2000.wordpress.com/2015/05/19/bc7-partitions-subsets/

https://github.com/KhronosGroup/OpenGL-Registry/blob/master/extensions/EXT/EXT_texture_compression_bptc.txt

https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#ETC1

https://www.khronos.org/registry/DataFormat/specs/1.1/dataformat.1.1.html#ETC2

https://docs.microsoft.com/en-us/windows/win32/direct3d10/d3d10-graphics-programming-guide-resources-block-compression#bc1

https://github.com/ARM-software/astc-encoder/blob/master/Docs/FormatOverview.md

https://www.khronos.org/registry/OpenGL/extensions/EXT/EXT_texture_compression_astc_decode_mode.txt

----------------

This non-copyrighted, Public Domain document was written by Richard Geldreich Jr., Binomial LLC. See the "Public Domain Declaration" above.

