// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Compression.Astc.BlockDecoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc;

/// <summary>
/// Decodes ASTC-compressed texture data into uncompressed pixel formats.
/// </summary>
/// <remarks>
/// Image data is streamed from a source of ASTC blocks to a destination <see cref="Stream"/> of pixels, one
/// block-row band at a time, so peak memory is independent of the image height.
/// The decoder returns raw decoded values and does not apply an sRGB-to-linear transform.
/// </remarks>
public static class AstcDecoder
{
    /// <summary>
    /// Serialises a decoded pixel band of element type <typeparamref name="TElement"/> into a
    /// destination byte buffer. Used by the stream-to-stream decode paths to choose the output
    /// byte layout (raw bytes or little-endian float) while the band loop stays generic.
    /// </summary>
    /// <typeparam name="TElement">Decoded pixel element type — <see cref="byte"/> for LDR, <see cref="float"/> for HDR.</typeparam>
    private interface IBandSerializer<TElement>
        where TElement : unmanaged
    {
        /// <summary>
        /// Gets the number of bytes emitted per decoded element (RGBA channel).
        /// </summary>
        public int BytesPerElement { get; }

        /// <summary>
        /// Writes <paramref name="source"/> (<paramref name="elementCount"/> decoded elements)
        /// into <paramref name="destination"/> as little-endian bytes.
        /// </summary>
        /// <param name="source">The decoded pixel band.</param>
        /// <param name="elementCount">The number of valid elements at the start of <paramref name="source"/>.</param>
        /// <param name="destination">The byte buffer to write the serialised elements to.</param>
        public void Serialize(ReadOnlySpan<TElement> source, int elementCount, Span<byte> destination);
    }

    /// <summary>
    /// Decodes ASTC blocks read from <paramref name="source"/> and writes the RGBA32 result to
    /// <paramref name="destination"/>, one block-row band at a time.
    /// </summary>
    /// <param name="source">The stream containing ASTC-compressed block data.</param>
    /// <param name="destination">The stream to write RGBA32 pixels to, row-major.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="footprint">The ASTC block footprint.</param>
    /// <param name="mode">LDR decode mode — linear (default) or sRGB endpoint expansion.</param>
    /// <exception cref="EndOfStreamException">
    /// Thrown if <paramref name="source"/> contains fewer bytes than the footprint requires.
    /// </exception>
    public static void DecompressImage(Stream source, Stream destination, int width, int height, Footprint footprint, LdrDecodeMode mode = LdrDecodeMode.Linear)
    {
        Guard.NotNull(source);
        Guard.NotNull(destination);
        ValidateStreamDecodeArgs(width, height);

        if (mode == LdrDecodeMode.Srgb)
        {
            DecodeToStream<LdrPipeline<SrgbMode>, byte, ByteBandSerializer>(source, destination, width, height, footprint);
        }
        else
        {
            DecodeToStream<LdrPipeline<LinearMode>, byte, ByteBandSerializer>(source, destination, width, height, footprint);
        }
    }

    /// <summary>
    /// Asynchronously decodes ASTC blocks read from <paramref name="source"/> and writes the
    /// RGBA32 result to <paramref name="destination"/>, one block-row band at a time.
    /// </summary>
    /// <param name="source">The stream containing ASTC-compressed block data.</param>
    /// <param name="destination">The stream to write RGBA32 pixels to, row-major.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="footprint">The ASTC block footprint.</param>
    /// <param name="mode">LDR decode mode — linear (default) or sRGB endpoint expansion.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the decode has finished.</returns>
    /// <exception cref="EndOfStreamException">
    /// Thrown if <paramref name="source"/> contains fewer bytes than the footprint requires.
    /// </exception>
    public static Task DecompressImageAsync(
        Stream source, Stream destination, int width, int height, Footprint footprint, LdrDecodeMode mode = LdrDecodeMode.Linear, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(source);
        Guard.NotNull(destination);
        ValidateStreamDecodeArgs(width, height);

        return mode == LdrDecodeMode.Srgb
            ? DecodeToStreamAsync<LdrPipeline<SrgbMode>, byte, ByteBandSerializer>(source, destination, width, height, footprint, cancellationToken)
            : DecodeToStreamAsync<LdrPipeline<LinearMode>, byte, ByteBandSerializer>(source, destination, width, height, footprint, cancellationToken);
    }

    /// <summary>
    /// Decodes ASTC blocks read from <paramref name="source"/> and writes the RGBA float result
    /// to <paramref name="destination"/> as little-endian IEEE-754 values, one block-row band at
    /// a time. For HDR content, values may exceed 1.0.
    /// </summary>
    /// <param name="source">The stream containing ASTC-compressed block data.</param>
    /// <param name="destination">The stream to write little-endian RGBA float pixels to, row-major.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="footprint">The ASTC block footprint.</param>
    /// <exception cref="EndOfStreamException">
    /// Thrown if <paramref name="source"/> contains fewer bytes than the footprint requires.
    /// </exception>
    public static void DecompressHdrImage(Stream source, Stream destination, int width, int height, Footprint footprint)
    {
        Guard.NotNull(source);
        Guard.NotNull(destination);
        ValidateStreamDecodeArgs(width, height);

        DecodeToStream<HdrPipeline, float, FloatBandSerializer>(source, destination, width, height, footprint);
    }

    /// <summary>
    /// Asynchronously decodes ASTC blocks read from <paramref name="source"/> and writes the RGBA
    /// float result to <paramref name="destination"/> as little-endian IEEE-754 values, one
    /// block-row band at a time.
    /// </summary>
    /// <param name="source">The stream containing ASTC-compressed block data.</param>
    /// <param name="destination">The stream to write little-endian RGBA float pixels to, row-major.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="footprint">The ASTC block footprint.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that completes when the decode has finished.</returns>
    /// <exception cref="EndOfStreamException">
    /// Thrown if <paramref name="source"/> contains fewer bytes than the footprint requires.
    /// </exception>
    public static Task DecompressHdrImageAsync(
        Stream source, Stream destination, int width, int height, Footprint footprint, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(source);
        Guard.NotNull(destination);
        ValidateStreamDecodeArgs(width, height);

        return DecodeToStreamAsync<HdrPipeline, float, FloatBandSerializer>(source, destination, width, height, footprint, cancellationToken);
    }

    /// <summary>
    /// Decodes one block-row (a horizontal band of <paramref name="blocksWide"/> blocks) into
    /// <paramref name="destination"/>, a single-band pixel buffer whose first
    /// <paramref name="destinationHeight"/> rows are valid (the band is clipped to the image
    /// height at the bottom edge). <paramref name="bandBlocks"/> holds exactly the band's blocks,
    /// indexed from 0; the per-block decode writes through <paramref name="decodedPixels"/> scratch
    /// for blocks the fused fast path cannot place directly.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeBlockRow<TPipeline, T>(
        ReadOnlySpan<byte> bandBlocks,
        int blocksWide,
        Footprint footprint,
        int destinationWidth,
        int destinationHeight,
        Span<T> destination,
        Span<T> decodedPixels)
        where TPipeline : struct, IBlockPipeline<T>
        where T : unmanaged
    {
        TPipeline pipeline = default;

        for (int blockX = 0; blockX < blocksWide; blockX++)
        {
            UInt128 blockBits = ReadBlockBits(bandBlocks, blockX);

            BlockInfo info = BlockModeDecoder.Decode(blockBits);
            BlockDestination dest = BlockImageWriter.ComputeBlockDestination(blockX, 0, footprint, destinationWidth, destinationHeight);

            // Spec §C.2.19, §C.2.24, §C.2.25: illegal block encodings, and HDR endpoint modes
            // in the LDR profile, must produce the error colour (magenta) for every texel.
            if (!info.IsValid || !pipeline.IsBlockLegal(in info))
            {
                pipeline.WriteErrorColorClipped(footprint, dest.DstBaseX, dest.DstBaseY, dest.CopyWidth, dest.CopyHeight, destinationWidth, destination);
                continue;
            }

            DecodeBlock<TPipeline, T>(blockBits, in info, footprint, dest, destinationWidth, destination, decodedPixels);
        }
    }

    /// <summary>
    /// Routes a single block to the best available path. Single-partition, single-plane,
    /// non-void-extent blocks (the common shape per ASTC spec §C.2.10, §C.2.20, §C.2.23) take
    /// the fused fast path — directly to the band buffer when the block fits entirely inside
    /// the band, or to a scratch buffer at edges that need cropping. Everything else
    /// (void-extent, multi-partition, dual-plane) falls through to the general <see cref="LogicalBlock"/> pipeline.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeBlock<TPipeline, T>(
        UInt128 blockBits,
        in BlockInfo info,
        Footprint footprint,
        BlockDestination dest,
        int imageWidth,
        Span<T> imageBuffer,
        Span<T> decodedPixels)
        where TPipeline : struct, IBlockPipeline<T>
        where T : unmanaged
    {
        TPipeline pipeline = default;

        if (info.IsFusable && dest.IsFullInterior)
        {
            pipeline.FusedToImage(blockBits, in info, footprint, dest.DstBaseX, dest.DstBaseY, imageWidth, imageBuffer);
            return;
        }

        if (info.IsFusable)
        {
            pipeline.FusedToScratch(blockBits, in info, footprint, decodedPixels);
        }
        else
        {
            pipeline.LogicalWrite(blockBits, in info, footprint, decodedPixels);
        }

        BlockImageWriter.CopyBlockRect(decodedPixels, imageBuffer, footprint.Width, dest.CopyWidth, dest.CopyHeight, dest.DstBaseX, dest.DstBaseY, imageWidth);
    }

    /// <summary>
    /// Validates that <paramref name="width"/> and <paramref name="height"/> are positive and
    /// that <c>width × height × 4</c> does not overflow <see cref="int.MaxValue"/>. The
    /// stream-to-stream paths never materialise the whole image, but the per-band buffer offsets
    /// are computed with <see cref="int"/> arithmetic, so the total element count must still fit.
    /// </summary>
    private static void ValidateStreamDecodeArgs(int width, int height)
    {
        Guard.MustBeGreaterThan(width, 0, nameof(width));
        Guard.MustBeGreaterThan(height, 0, nameof(height));

        long totalPixels = (long)width * height;
        Guard.MustBeLessThanOrEqualTo(totalPixels, (long)int.MaxValue / BlockInfo.ChannelsPerPixel, nameof(totalPixels));
    }

    /// <summary>
    /// Streams a decode from <paramref name="source"/> to <paramref name="destination"/> one
    /// block-row band at a time, serialising each band with <typeparamref name="TSerializer"/>.
    /// Peak memory is one band of compressed blocks, one band of decoded pixels, one per-block
    /// scratch buffer, and one band of serialised output - all independent of the image height.
    /// </summary>
    private static void DecodeToStream<TPipeline, TElement, TSerializer>(
        Stream source, Stream destination, int width, int height, Footprint footprint)
        where TPipeline : struct, IBlockPipeline<TElement>
        where TElement : unmanaged
        where TSerializer : struct, IBandSerializer<TElement>
    {
        int blocksWide = footprint.BlocksWide(width);
        int blocksHigh = footprint.BlocksHigh(height);
        int bandBlockBytes = blocksWide * BlockInfo.SizeInBytes;
        int bandPixelElements = footprint.Height * width * BlockInfo.ChannelsPerPixel;
        int scratchSize = footprint.PixelCount * BlockInfo.ChannelsPerPixel;

        TSerializer serializer = default;
        using IMemoryOwner<byte> bandBlocks = MemoryAllocator.Default.Allocate<byte>(bandBlockBytes);
        using IMemoryOwner<TElement> bandPixels = MemoryAllocator.Default.Allocate<TElement>(bandPixelElements);
        using IMemoryOwner<TElement> scratch = MemoryAllocator.Default.Allocate<TElement>(scratchSize);
        using IMemoryOwner<byte> outputBand = MemoryAllocator.Default.Allocate<byte>(bandPixelElements * serializer.BytesPerElement);

        Span<byte> bandSpan = bandBlocks.Memory.Span;
        Span<TElement> bandPixelSpan = bandPixels.Memory.Span;
        Span<TElement> scratchSpan = scratch.Memory.Span;
        Span<byte> outputSpan = outputBand.Memory.Span;

        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            source.ReadExactly(bandSpan);
            int bandHeight = Math.Min(footprint.Height, height - (blockY * footprint.Height));
            DecodeBlockRow<TPipeline, TElement>(bandSpan, blocksWide, footprint, width, bandHeight, bandPixelSpan, scratchSpan);

            int validElements = bandHeight * width * BlockInfo.ChannelsPerPixel;
            int outputBytes = validElements * serializer.BytesPerElement;
            serializer.Serialize(bandPixelSpan, validElements, outputSpan);
            destination.Write(outputSpan[..outputBytes]);
        }
    }

    /// <summary>
    /// Asynchronous counterpart to <see cref="DecodeToStream{TPipeline, TElement, TSerializer}"/>.
    /// The block decode itself is synchronous (CPU-bound, span-based); only the source read and
    /// destination write are awaited, so the buffers must persist across awaits — hence the
    /// <see cref="IMemoryOwner{T}"/>-backed <see cref="Memory{T}"/> rather than spans.
    /// </summary>
    private static async Task DecodeToStreamAsync<TPipeline, TElement, TSerializer>(
        Stream source, Stream destination, int width, int height, Footprint footprint, CancellationToken cancellationToken)
        where TPipeline : struct, IBlockPipeline<TElement>
        where TElement : unmanaged
        where TSerializer : struct, IBandSerializer<TElement>
    {
        int blocksWide = footprint.BlocksWide(width);
        int blocksHigh = footprint.BlocksHigh(height);
        int bandBlockBytes = blocksWide * BlockInfo.SizeInBytes;
        int bandPixelElements = footprint.Height * width * BlockInfo.ChannelsPerPixel;
        int scratchSize = footprint.PixelCount * BlockInfo.ChannelsPerPixel;

        TSerializer serializer = default;
        using IMemoryOwner<byte> bandBlocks = MemoryAllocator.Default.Allocate<byte>(bandBlockBytes);
        using IMemoryOwner<TElement> bandPixels = MemoryAllocator.Default.Allocate<TElement>(bandPixelElements);
        using IMemoryOwner<TElement> scratch = MemoryAllocator.Default.Allocate<TElement>(scratchSize);
        using IMemoryOwner<byte> outputBand = MemoryAllocator.Default.Allocate<byte>(bandPixelElements * serializer.BytesPerElement);

        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            await source.ReadExactlyAsync(bandBlocks.Memory, cancellationToken).ConfigureAwait(false);

            int bandHeight = Math.Min(footprint.Height, height - (blockY * footprint.Height));
            DecodeBlockRow<TPipeline, TElement>(
                bandBlocks.Memory.Span,
                blocksWide,
                footprint,
                width,
                bandHeight,
                bandPixels.Memory.Span,
                scratch.Memory.Span);

            int validElements = bandHeight * width * BlockInfo.ChannelsPerPixel;
            int outputBytes = validElements * serializer.BytesPerElement;
            serializer.Serialize(bandPixels.Memory.Span, validElements, outputBand.Memory.Span);
            await destination.WriteAsync(outputBand.Memory[..outputBytes], cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reads the 16 bytes of the ASTC block at <paramref name="blockIndex"/> into a
    /// <see cref="UInt128"/> (little-endian). The caller is responsible for ensuring
    /// <paramref name="astcData"/> contains the requested block.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 ReadBlockBits(ReadOnlySpan<byte> astcData, int blockIndex)
    {
        int offset = blockIndex * BlockInfo.SizeInBytes;
        return BinaryPrimitives.ReadUInt128LittleEndian(astcData.Slice(offset, BlockInfo.SizeInBytes));
    }

    private readonly struct ByteBandSerializer : IBandSerializer<byte>
    {
        public int BytesPerElement => sizeof(byte);

        public void Serialize(ReadOnlySpan<byte> source, int elementCount, Span<byte> destination)
            => source[..elementCount].CopyTo(destination);
    }

    private readonly struct FloatBandSerializer : IBandSerializer<float>
    {
        public int BytesPerElement => sizeof(float);

        public void Serialize(ReadOnlySpan<float> source, int elementCount, Span<byte> destination)
        {
            for (int i = 0; i < elementCount; i++)
            {
                BinaryPrimitives.WriteSingleLittleEndian(destination.Slice(i * sizeof(float), sizeof(float)), source[i]);
            }
        }
    }
}
