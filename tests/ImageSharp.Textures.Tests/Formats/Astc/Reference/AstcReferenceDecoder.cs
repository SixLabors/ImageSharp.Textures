// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.InteropServices;
using AstcEncoder;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Reference;

/// <summary>
/// Wrapper around the ARM reference ASTC encoder/decoder for use as a comparison baseline in tests.
/// </summary>
internal static class AstcReferenceDecoder
{
    private static readonly AstcencSwizzle IdentitySwizzle = new()
    {
        r = AstcencSwz.AstcencSwzR,
        g = AstcencSwz.AstcencSwzG,
        b = AstcencSwz.AstcencSwzB,
        a = AstcencSwz.AstcencSwzA,
    };

    /// <summary>
    /// Decompress ASTC blocks to RGBA32 (LDR) using the ARM reference decoder.
    /// </summary>
    public static byte[] DecompressLdr(ReadOnlySpan<byte> blocks, int w, int h, int blockX, int blockY)
    {
        AstcencError error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfLdr,
            (uint)blockX,
            (uint)blockY,
            1,
            Astcenc.AstcencPreFastest,
            AstcencFlags.DecompressOnly,
            out AstcencConfig config);
        ThrowOnError(error, "ConfigInit(LDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out AstcencContext context);
        ThrowOnError(error, "ContextAlloc(LDR)");

        try
        {
            int pixelCount = w * h;
            byte[] outputBytes = new byte[pixelCount * 4]; // RGBA32

            AstcencImage image = new()
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeU8,
                data = outputBytes,
            };

            // We need a mutable copy of blocks for the Span<byte> parameter
            byte[] blocksCopy = blocks.ToArray();
            error = Astcenc.AstcencDecompressImage(context, blocksCopy, ref image, IdentitySwizzle, 0);
            ThrowOnError(error, "DecompressImage(LDR)");

            return outputBytes;
        }
        finally
        {
            Astcenc.AstcencContextFree(context);
        }
    }

    /// <summary>
    /// Decompress ASTC blocks to FP16 RGBA (HDR) using the ARM reference decoder.
    /// </summary>
    public static Half[] DecompressHdr(ReadOnlySpan<byte> blocks, int w, int h, int blockX, int blockY)
    {
        AstcencError error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfHdr,
            (uint)blockX,
            (uint)blockY,
            1,
            Astcenc.AstcencPreFastest,
            AstcencFlags.DecompressOnly,
            out AstcencConfig config);
        ThrowOnError(error, "ConfigInit(HDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out AstcencContext context);
        ThrowOnError(error, "ContextAlloc(HDR)");

        try
        {
            int pixelCount = w * h;
            Half[] outputHalves = new Half[pixelCount * 4]; // RGBA FP16
            byte[] outputBytes = MemoryMarshal.AsBytes(outputHalves.AsSpan()).ToArray();

            AstcencImage image = new()
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeF16,
                data = outputBytes,
            };

            byte[] blocksCopy = blocks.ToArray();
            error = Astcenc.AstcencDecompressImage(context, blocksCopy, ref image, IdentitySwizzle, 0);
            ThrowOnError(error, "DecompressImage(HDR)");

            // Copy the decompressed bytes back into the Half array
            MemoryMarshal.AsBytes(outputHalves.AsSpan()).Clear();
            outputBytes.AsSpan().CopyTo(MemoryMarshal.AsBytes(outputHalves.AsSpan()));

            return outputHalves;
        }
        finally
        {
            Astcenc.AstcencContextFree(context);
        }
    }

    /// <summary>
    /// Compress RGBA32 pixel data to ASTC using the ARM reference encoder (LDR).
    /// </summary>
    public static byte[] CompressLdr(byte[] pixels, int w, int h, int blockX, int blockY)
    {
        AstcencError error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfLdr,
            (uint)blockX,
            (uint)blockY,
            1,
            Astcenc.AstcencPreMedium,
            0,
            out AstcencConfig config);
        ThrowOnError(error, "ConfigInit(CompressLDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out AstcencContext context);
        ThrowOnError(error, "ContextAlloc(CompressLDR)");

        try
        {
            AstcencImage image = new()
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeU8,
                data = pixels,
            };

            int blocksWide = (w + blockX - 1) / blockX;
            int blocksHigh = (h + blockY - 1) / blockY;
            byte[] compressedData = new byte[blocksWide * blocksHigh * 16];

            error = Astcenc.AstcencCompressImage(context, ref image, IdentitySwizzle, compressedData, 0);
            ThrowOnError(error, "CompressImage(LDR)");

            return compressedData;
        }
        finally
        {
            Astcenc.AstcencContextFree(context);
        }
    }

    /// <summary>
    /// Compress FP16 RGBA pixel data to ASTC using the ARM reference encoder (HDR).
    /// </summary>
    public static byte[] CompressHdr(Half[] pixels, int w, int h, int blockX, int blockY)
    {
        AstcencError error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfHdr,
            (uint)blockX,
            (uint)blockY,
            1,
            Astcenc.AstcencPreMedium,
            0,
            out AstcencConfig config);
        ThrowOnError(error, "ConfigInit(CompressHDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out AstcencContext context);
        ThrowOnError(error, "ContextAlloc(CompressHDR)");

        try
        {
            byte[] pixelBytes = MemoryMarshal.AsBytes(pixels.AsSpan()).ToArray();

            AstcencImage image = new()
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeF16,
                data = pixelBytes,
            };

            int blocksWide = (w + blockX - 1) / blockX;
            int blocksHigh = (h + blockY - 1) / blockY;
            byte[] compressedData = new byte[blocksWide * blocksHigh * 16];

            error = Astcenc.AstcencCompressImage(context, ref image, IdentitySwizzle, compressedData, 0);
            ThrowOnError(error, "CompressImage(HDR)");

            return compressedData;
        }
        finally
        {
            Astcenc.AstcencContextFree(context);
        }
    }

    /// <summary>
    /// Allocates a reusable astcenc decode context for the given profile and block size. The
    /// caller owns the context and must release it via <see cref="FreeContext"/>.
    /// </summary>
    public static AstcencContext AllocDecodeContext(AstcencProfile profile, int blockX, int blockY)
    {
        AstcencError error = Astcenc.AstcencConfigInit(
            profile,
            (uint)blockX,
            (uint)blockY,
            1,
            Astcenc.AstcencPreFastest,
            AstcencFlags.DecompressOnly,
            out AstcencConfig config);
        ThrowOnError(error, $"ConfigInit({profile})");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out AstcencContext context);
        ThrowOnError(error, $"ContextAlloc({profile})");
        return context;
    }

    /// <summary>
    /// Releases a context previously returned from <see cref="AllocDecodeContext"/>.
    /// </summary>
    public static void FreeContext(AstcencContext context) => Astcenc.AstcencContextFree(context);

    /// <summary>
    /// Decompress LDR ASTC blocks into a caller-supplied <paramref name="output"/> buffer using
    /// a pre-allocated <paramref name="context"/>. The block buffer is passed straight through
    /// to the underlying decoder, and the output must be sized to <c>w * h * 4</c> bytes (RGBA32).
    /// </summary>
    public static void DecompressLdrInto(AstcencContext context, byte[] blocks, int w, int h, byte[] output)
    {
        AstcencImage image = new()
        {
            dimX = (uint)w,
            dimY = (uint)h,
            dimZ = 1,
            dataType = AstcencType.AstcencTypeU8,
            data = output,
        };
        AstcencError error = Astcenc.AstcencDecompressImage(context, blocks, ref image, IdentitySwizzle, 0);
        ThrowOnError(error, "DecompressImage(LDR/Into)");
    }

    /// <summary>
    /// Decompress HDR ASTC blocks into a caller-supplied <paramref name="output"/> byte buffer
    /// (FP16 RGBA bit-pattern, sized to <c>w * h * 4 * sizeof(ushort)</c>) using a pre-allocated
    /// <paramref name="context"/>.
    /// </summary>
    public static void DecompressHdrInto(AstcencContext context, byte[] blocks, int w, int h, byte[] output)
    {
        AstcencImage image = new()
        {
            dimX = (uint)w,
            dimY = (uint)h,
            dimZ = 1,
            dataType = AstcencType.AstcencTypeF16,
            data = output,
        };
        AstcencError error = Astcenc.AstcencDecompressImage(context, blocks, ref image, IdentitySwizzle, 0);
        ThrowOnError(error, "DecompressImage(HDR/Into)");
    }

    /// <summary>
    /// Map a FootprintType to its (blockX, blockY) dimensions.
    /// </summary>
    public static (int BlockX, int BlockY) ToBlockDimensions(FootprintType footprint) => footprint switch
    {
        FootprintType.Footprint4x4 => (4, 4),
        FootprintType.Footprint5x4 => (5, 4),
        FootprintType.Footprint5x5 => (5, 5),
        FootprintType.Footprint6x5 => (6, 5),
        FootprintType.Footprint6x6 => (6, 6),
        FootprintType.Footprint8x5 => (8, 5),
        FootprintType.Footprint8x6 => (8, 6),
        FootprintType.Footprint8x8 => (8, 8),
        FootprintType.Footprint10x5 => (10, 5),
        FootprintType.Footprint10x6 => (10, 6),
        FootprintType.Footprint10x8 => (10, 8),
        FootprintType.Footprint10x10 => (10, 10),
        FootprintType.Footprint12x10 => (12, 10),
        FootprintType.Footprint12x12 => (12, 12),
        _ => throw new ArgumentOutOfRangeException(nameof(footprint)),
    };

    private static void ThrowOnError(AstcencError error, string operation)
    {
        if (error != AstcencError.AstcencSuccess)
        {
            string message = Astcenc.GetErrorString(error) ?? error.ToString();
            throw new InvalidOperationException($"ARM ASTC encoder {operation} failed: {message}");
        }
    }
}
