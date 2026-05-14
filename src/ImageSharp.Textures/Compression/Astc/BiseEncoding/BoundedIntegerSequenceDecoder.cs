// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

/// <summary>
/// BISE decoder (ASTC spec §C.2.22) for bounded integer sequences. Stateless: callers pass
/// the BISE encoding mode and mantissa bit count directly (both typically already on hand
/// from <see cref="BoundedIntegerSequenceCodec.GetPackingModeBitCount"/>).
/// </summary>
internal static class BoundedIntegerSequenceDecoder
{
    /// <summary>
    /// Decodes a sequence of bounded integers into a caller-provided span.
    /// </summary>
    /// <param name="encoding">The BISE encoding mode (bits, trits, or quints).</param>
    /// <param name="bitCount">The number of mantissa bits per value (from the BISE packing).</param>
    /// <param name="valuesCount">The number of values to decode.</param>
    /// <param name="bitSource">The source of values to decode from.</param>
    /// <param name="result">The span to write decoded values into.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the encoded block size is too large.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there are not enough bits to decode.</exception>
    public static void Decode(BiseEncodingMode encoding, int bitCount, int valuesCount, ref BitStream bitSource, Span<int> result)
    {
        int totalBitCount = BoundedIntegerSequenceCodec.GetBitCount(encoding, valuesCount, bitCount);
        int bitsPerBlock = GetEncodedBlockSize(encoding, bitCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitsPerBlock, 64);

        // Fixed 5 ints (20 bytes) — one BISE block holds at most 5 trits or 3 quints (spec §C.2.22).
        Span<int> blockResult = stackalloc int[5];
        int resultIndex = 0;
        int bitsRemaining = totalBitCount;

        while (bitsRemaining > 0)
        {
            int bitsToRead = Math.Min(bitsRemaining, bitsPerBlock);
            if (!bitSource.TryGetBits(bitsToRead, out ulong blockBits))
            {
                throw new InvalidOperationException("Not enough bits in BitStream to decode BISE block");
            }

            if (encoding == BiseEncodingMode.BitEncoding)
            {
                if (resultIndex < valuesCount)
                {
                    result[resultIndex++] = (int)blockBits;
                }
            }
            else
            {
                int decoded = DecodeISEBlock(encoding, blockBits, bitCount, blockResult);
                for (int i = 0; i < decoded && resultIndex < valuesCount; ++i)
                {
                    result[resultIndex++] = blockResult[i];
                }
            }

            bitsRemaining -= bitsPerBlock;
        }

        if (resultIndex < valuesCount)
        {
            throw new InvalidOperationException("Decoded fewer values than expected from BISE block");
        }
    }

    /// <summary>
    /// Decodes one trit/quint BISE block (ASTC spec §C.2.12) into <paramref name="result"/>.
    /// Returns the number of values written (5 for trits, 3 for quints). Uses direct bit
    /// extraction (no BitStream) and flat encoding tables for speed.
    /// </summary>
    private static int DecodeISEBlock(BiseEncodingMode mode, ulong encodedBlock, int encodedBitCount, Span<int> result)
    {
        ulong mantissaMask = (1UL << encodedBitCount) - 1;
        return mode == BiseEncodingMode.TritEncoding
            ? DecodeTritBlock(encodedBlock, encodedBitCount, mantissaMask, result)
            : DecodeQuintBlock(encodedBlock, encodedBitCount, mantissaMask, result);
    }

    /// <summary>
    /// The size of a single ISE block in bits — the inverse of the packing computed by
    /// <see cref="BoundedIntegerSequenceCodec.GetPackingModeBitCount"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetEncodedBlockSize(BiseEncodingMode encoding, int bitCount)
    {
        (int blockSize, int extraBlockSize) = encoding switch
        {
            BiseEncodingMode.TritEncoding => (5, 8),
            BiseEncodingMode.QuintEncoding => (3, 7),
            BiseEncodingMode.BitEncoding => (1, 0),
            _ => (0, 0),
        };

        return extraBlockSize + (blockSize * bitCount);
    }

    /// <summary>
    /// Decodes a five-value trit block. The ASTC spec §C.2.12 layout interleaves mantissas
    /// and an 8-bit packed trit selector as [m0, t0(2), m1, t1(2), m2, t2(1), m3, t3(2), m4, t4(1)].
    /// The 8 selector bits look up a row in the pre-flattened trit encoding table.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeTritBlock(ulong encodedBlock, int encodedBitCount, ulong mantissaMask, Span<int> result)
    {
        int bitPosition = 0;
        int mantissa0 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        bitPosition += encodedBitCount;
        ulong encodedTrits = (encodedBlock >> bitPosition) & 0x3;
        bitPosition += 2;
        int mantissa1 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        bitPosition += encodedBitCount;
        encodedTrits |= ((encodedBlock >> bitPosition) & 0x3) << 2;
        bitPosition += 2;
        int mantissa2 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        bitPosition += encodedBitCount;
        encodedTrits |= ((encodedBlock >> bitPosition) & 0x1) << 4;
        bitPosition += 1;
        int mantissa3 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        bitPosition += encodedBitCount;
        encodedTrits |= ((encodedBlock >> bitPosition) & 0x3) << 5;
        bitPosition += 2;
        int mantissa4 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        encodedTrits |= ((encodedBlock >> (bitPosition + encodedBitCount)) & 0x1) << 7;

        int tritTableBase = (int)encodedTrits * 5;
        result[0] = (BoundedIntegerSequenceCodec.FlatTritEncodings[tritTableBase] << encodedBitCount) | mantissa0;
        result[1] = (BoundedIntegerSequenceCodec.FlatTritEncodings[tritTableBase + 1] << encodedBitCount) | mantissa1;
        result[2] = (BoundedIntegerSequenceCodec.FlatTritEncodings[tritTableBase + 2] << encodedBitCount) | mantissa2;
        result[3] = (BoundedIntegerSequenceCodec.FlatTritEncodings[tritTableBase + 3] << encodedBitCount) | mantissa3;
        result[4] = (BoundedIntegerSequenceCodec.FlatTritEncodings[tritTableBase + 4] << encodedBitCount) | mantissa4;
        return 5;
    }

    /// <summary>
    /// Decodes a three-value quint block (ASTC spec §C.2.12). The 7-bit packed quint
    /// selector is interleaved as [m0, q0(3), m1, q1(2), m2, q2(2)] and indexes a row in
    /// the pre-flattened quint encoding table.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeQuintBlock(ulong encodedBlock, int encodedBitCount, ulong mantissaMask, Span<int> result)
    {
        int bitPosition = 0;
        int mantissa0 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        bitPosition += encodedBitCount;
        ulong encodedQuints = (encodedBlock >> bitPosition) & 0x7;
        bitPosition += 3;
        int mantissa1 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        bitPosition += encodedBitCount;
        encodedQuints |= ((encodedBlock >> bitPosition) & 0x3) << 3;
        bitPosition += 2;
        int mantissa2 = (int)((encodedBlock >> bitPosition) & mantissaMask);
        encodedQuints |= ((encodedBlock >> (bitPosition + encodedBitCount)) & 0x3) << 5;

        int quintTableBase = (int)encodedQuints * 3;
        result[0] = (BoundedIntegerSequenceCodec.FlatQuintEncodings[quintTableBase] << encodedBitCount) | mantissa0;
        result[1] = (BoundedIntegerSequenceCodec.FlatQuintEncodings[quintTableBase + 1] << encodedBitCount) | mantissa1;
        result[2] = (BoundedIntegerSequenceCodec.FlatQuintEncodings[quintTableBase + 2] << encodedBitCount) | mantissa2;
        return 3;
    }
}
