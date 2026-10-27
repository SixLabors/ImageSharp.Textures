// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.Uastc;

namespace SixLabors.ImageSharp.Textures.Compression.Astc;

/// <summary>
/// Decodes UASTC (Universal ASTC) LDR texture data to uncompressed RGBA32.
/// </summary>
/// <remarks>
/// <para>
/// UASTC is a Basis Universal encoding that is a constrained subset of LDR ASTC 4x4, blocks are
/// always 4x4 and 16 bytes.
/// </para>
/// <para>
/// The decoder returns raw decoded values and does not apply an sRGB-to-linear transform; see
/// <see cref="LdrDecodeMode"/>. All 19 UASTC LDR modes are supported. A block using the reserved
/// mode produces the error colour (magenta) for that block.
/// </para>
/// </remarks>
public static class UastcDecoder
{
    private const int BlockDim = 4;
    private const int DecodedBlockBytes = BlockDim * BlockDim * BlockInfo.ChannelsPerPixel;

    /// <summary>
    /// Decodes UASTC blocks read from <paramref name="source"/> and writes the RGBA32 result to <paramref name="destination"/>.
    /// </summary>
    /// <remarks>
    /// Only a single band of compressed blocks and decoded pixels is held in memory, so peak usage
    /// is independent of the image height.
    /// </remarks>
    /// <param name="source">The stream containing UASTC block data (16 bytes per 4x4 block, row-major).</param>
    /// <param name="destination">The stream to write RGBA32 pixels to, row-major.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="mode">LDR decode mode</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="destination"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="EndOfStreamException">
    /// Thrown if <paramref name="source"/> contains fewer blocks than the image requires.
    /// </exception>
    public static void DecompressImage(Stream source, Stream destination, int width, int height, LdrDecodeMode mode = LdrDecodeMode.Linear)
    {
        Guard.NotNull(source);
        Guard.NotNull(destination);
        ValidateArgs(width, height);

        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);
        int blocksWide = footprint.BlocksWide(width);
        int blocksHigh = footprint.BlocksHigh(height);
        int bandBlockBytes = blocksWide * BlockInfo.SizeInBytes;
        int bandPixelBytes = BlockDim * width * BlockInfo.ChannelsPerPixel;

        using IMemoryOwner<byte> bandBlocks = MemoryAllocator.Default.Allocate<byte>(bandBlockBytes);
        using IMemoryOwner<byte> bandPixels = MemoryAllocator.Default.Allocate<byte>(bandPixelBytes);
        using IMemoryOwner<byte> blockPixels = MemoryAllocator.Default.Allocate<byte>(DecodedBlockBytes);

        Span<byte> bandBlockSpan = bandBlocks.Memory.Span;
        Span<byte> bandPixelSpan = bandPixels.Memory.Span;
        Span<byte> blockPixelSpan = blockPixels.Memory.Span;

        for (int by = 0; by < blocksHigh; by++)
        {
            source.ReadExactly(bandBlockSpan);

            int bandHeight = Math.Min(BlockDim, height - (by * BlockDim));
            DecodeBandRow(bandBlockSpan, blocksWide, width, bandHeight, mode, footprint, bandPixelSpan, blockPixelSpan);

            int validBytes = bandHeight * width * BlockInfo.ChannelsPerPixel;
            destination.Write(bandPixelSpan[..validBytes]);
        }
    }

    /// <summary>
    /// Decodes UASTC blocks read from <paramref name="source"/> and writes the RGBA32 result to <paramref name="destination"/>.
    /// </summary>
    /// <remarks>
    /// Only a single band of compressed blocks and decoded pixels is held in memory, so peak usage
    /// is independent of the image height.
    /// </remarks>
    /// <param name="source">The stream containing UASTC block data (16 bytes per 4x4 block, row-major).</param>
    /// <param name="destination">The stream to write RGBA32 pixels to, row-major.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="mode">LDR decode mode</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the decode has finished.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="destination"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="EndOfStreamException">
    /// Thrown if <paramref name="source"/> contains fewer blocks than the image requires.
    /// </exception>
    public static async Task DecompressImageAsync(
        Stream source, Stream destination, int width, int height, LdrDecodeMode mode = LdrDecodeMode.Linear, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(source);
        Guard.NotNull(destination);
        ValidateArgs(width, height);

        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint4x4);
        int blocksWide = footprint.BlocksWide(width);
        int blocksHigh = footprint.BlocksHigh(height);
        int bandBlockBytes = blocksWide * BlockInfo.SizeInBytes;
        int bandPixelBytes = BlockDim * width * BlockInfo.ChannelsPerPixel;

        using IMemoryOwner<byte> bandBlocks = MemoryAllocator.Default.Allocate<byte>(bandBlockBytes);
        using IMemoryOwner<byte> bandPixels = MemoryAllocator.Default.Allocate<byte>(bandPixelBytes);
        using IMemoryOwner<byte> blockPixels = MemoryAllocator.Default.Allocate<byte>(DecodedBlockBytes);

        for (int by = 0; by < blocksHigh; by++)
        {
            await source.ReadExactlyAsync(bandBlocks.Memory, cancellationToken).ConfigureAwait(false);

            int bandHeight = Math.Min(BlockDim, height - (by * BlockDim));
            DecodeBandRow(bandBlocks.Memory.Span, blocksWide, width, bandHeight, mode, footprint, bandPixels.Memory.Span, blockPixels.Memory.Span);

            int validBytes = bandHeight * width * BlockInfo.ChannelsPerPixel;
            await destination.WriteAsync(bandPixels.Memory[..validBytes], cancellationToken).ConfigureAwait(false);
        }
    }

    private static void ValidateArgs(int width, int height)
    {
        Guard.MustBeGreaterThan(width, 0, nameof(width));
        Guard.MustBeGreaterThan(height, 0, nameof(height));

        long totalPixels = (long)width * height;
        Guard.MustBeLessThanOrEqualTo(totalPixels, (long)int.MaxValue / BlockInfo.ChannelsPerPixel, nameof(totalPixels));
    }

    private static void DecodeBandRow(
        ReadOnlySpan<byte> bandBlocks,
        int blocksWide,
        int width,
        int bandHeight,
        LdrDecodeMode mode,
        Footprint footprint,
        Span<byte> bandPixels,
        Span<byte> blockPixels)
    {
        for (int bx = 0; bx < blocksWide; bx++)
        {
            ReadOnlySpan<byte> block = bandBlocks.Slice(bx * BlockInfo.SizeInBytes, BlockInfo.SizeInBytes);
            DecodeBlock(block, blockPixels, mode);

            BlockDestination dest = BlockImageWriter.ComputeBlockDestination(bx, 0, footprint, width, bandHeight);
            BlockImageWriter.CopyBlockRect<byte>(
                blockPixels, bandPixels, BlockDim, dest.CopyWidth, dest.CopyHeight, dest.DstBaseX, dest.DstBaseY, width);
        }
    }

    private static void DecodeBlock(ReadOnlySpan<byte> block, Span<byte> blockPixels, LdrDecodeMode mode)
    {
        bool decoded = mode == LdrDecodeMode.Srgb
            ? UastcBlockDecoder.TryDecode<SrgbMode>(block, blockPixels)
            : UastcBlockDecoder.TryDecode<LinearMode>(block, blockPixels);

        if (!decoded)
        {
            BlockImageWriter.FillErrorColor(blockPixels);
        }
    }
}
