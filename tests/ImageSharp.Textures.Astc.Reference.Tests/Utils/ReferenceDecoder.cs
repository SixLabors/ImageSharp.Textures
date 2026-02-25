// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.InteropServices;
using AstcEncoder;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.Reference.Tests.Utils;

/// <summary>
/// Wrapper around the ARM reference ASTC encoder/decoder (AstcEncoderCSharp package)
/// for use as a comparison baseline in tests.
/// </summary>
internal static class ReferenceDecoder
{
    private static readonly AstcencSwizzle IdentitySwizzle = new()
    {
        r = AstcencSwz.AstcencSwzR,
        g = AstcencSwz.AstcencSwzG,
        b = AstcencSwz.AstcencSwzB,
        a = AstcencSwz.AstcencSwzA,
    };

    /// <summary>
    /// Decompress ASTC blocks to RGBA8 (LDR) using the ARM reference decoder.
    /// </summary>
    public static byte[] DecompressLdr(ReadOnlySpan<byte> blocks, int w, int h, int blockX, int blockY)
    {
        var error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfLdr,
            (uint)blockX, (uint)blockY, 1,
            Astcenc.AstcencPreFastest,
            AstcencFlags.DecompressOnly,
            out var config);
        ThrowOnError(error, "ConfigInit(LDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out var context);
        ThrowOnError(error, "ContextAlloc(LDR)");

        try
        {
            int pixelCount = w * h;
            var outputBytes = new byte[pixelCount * 4]; // RGBA8

            var image = new AstcencImage
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeU8,
                data = outputBytes,
            };

            // We need a mutable copy of blocks for the Span<byte> parameter
            var blocksCopy = blocks.ToArray();
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
        var error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfHdr,
            (uint)blockX, (uint)blockY, 1,
            Astcenc.AstcencPreFastest,
            AstcencFlags.DecompressOnly,
            out var config);
        ThrowOnError(error, "ConfigInit(HDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out var context);
        ThrowOnError(error, "ContextAlloc(HDR)");

        try
        {
            int pixelCount = w * h;
            var outputHalves = new Half[pixelCount * 4]; // RGBA FP16
            var outputBytes = MemoryMarshal.AsBytes(outputHalves.AsSpan()).ToArray();

            var image = new AstcencImage
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeF16,
                data = outputBytes,
            };

            var blocksCopy = blocks.ToArray();
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
    /// Compress RGBA8 pixel data to ASTC using the ARM reference encoder (LDR).
    /// </summary>
    public static byte[] CompressLdr(byte[] pixels, int w, int h, int blockX, int blockY)
    {
        var error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfLdr,
            (uint)blockX, (uint)blockY, 1,
            Astcenc.AstcencPreMedium,
            0,
            out var config);
        ThrowOnError(error, "ConfigInit(CompressLDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out var context);
        ThrowOnError(error, "ContextAlloc(CompressLDR)");

        try
        {
            var image = new AstcencImage
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeU8,
                data = pixels,
            };

            int blocksWide = (w + blockX - 1) / blockX;
            int blocksHigh = (h + blockY - 1) / blockY;
            var compressedData = new byte[blocksWide * blocksHigh * 16];

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
        var error = Astcenc.AstcencConfigInit(
            AstcencProfile.AstcencPrfHdr,
            (uint)blockX, (uint)blockY, 1,
            Astcenc.AstcencPreMedium,
            0,
            out var config);
        ThrowOnError(error, "ConfigInit(CompressHDR)");

        error = Astcenc.AstcencContextAlloc(ref config, 1, out var context);
        ThrowOnError(error, "ContextAlloc(CompressHDR)");

        try
        {
            var pixelBytes = MemoryMarshal.AsBytes(pixels.AsSpan()).ToArray();

            var image = new AstcencImage
            {
                dimX = (uint)w,
                dimY = (uint)h,
                dimZ = 1,
                dataType = AstcencType.AstcencTypeF16,
                data = pixelBytes,
            };

            int blocksWide = (w + blockX - 1) / blockX;
            int blocksHigh = (h + blockY - 1) / blockY;
            var compressedData = new byte[blocksWide * blocksHigh * 16];

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
    /// Map a FootprintType to its (blockX, blockY) dimensions.
    /// </summary>
    public static (int blockX, int blockY) ToBlockDimensions(FootprintType footprint) => footprint switch
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
            var message = Astcenc.GetErrorString(error) ?? error.ToString();
            throw new InvalidOperationException($"ARM ASTC encoder {operation} failed: {message}");
        }
    }
}
