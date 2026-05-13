// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

internal sealed class BoundedIntegerSequenceDecoder : BoundedIntegerSequenceCodec
{
    private static readonly BoundedIntegerSequenceDecoder?[] Cache = new BoundedIntegerSequenceDecoder?[256];

    public BoundedIntegerSequenceDecoder(int range)
        : base(range)
    {
    }

    public static BoundedIntegerSequenceDecoder GetCached(int range)
    {
        // Volatile.Read pairs with the implicit release on CompareExchange below to ensure
        // other threads observe a fully-constructed decoder. Decoders are immutable, so losing
        // the CAS race is harmless — the caller discards its own instance and uses the winner.
        BoundedIntegerSequenceDecoder? decoder = Volatile.Read(ref Cache[range]);
        if (decoder is null)
        {
            BoundedIntegerSequenceDecoder created = new(range);
            decoder = Interlocked.CompareExchange(ref Cache[range], created, null) ?? created;
        }

        return decoder;
    }

    /// <summary>
    /// Decode a sequence of bounded integers into a caller-provided span.
    /// </summary>
    /// <param name="valuesCount">The number of values to decode.</param>
    /// <param name="bitSource">The source of values to decode from.</param>
    /// <param name="result">The span to write decoded values into.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the encoded block size is too large.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there are not enough bits to decode.</exception>
    public void Decode(int valuesCount, ref BitStream bitSource, Span<int> result)
    {
        int totalBitCount = GetBitCount(this.Encoding, valuesCount, this.BitCount);
        int bitsPerBlock = this.GetEncodedBlockSize();
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitsPerBlock, 64);

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

            if (this.Encoding == BiseEncodingMode.BitEncoding)
            {
                if (resultIndex < valuesCount)
                {
                    result[resultIndex++] = (int)blockBits;
                }
            }
            else
            {
                int decoded = DecodeISEBlock(this.Encoding, blockBits, this.BitCount, blockResult);
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
    /// Decode a sequence of bounded integers. The number of bits read is dependent on the number
    /// of bits required to encode <paramref name="valuesCount"/> based on the calculation provided
    /// in Section C.2.22 of the ASTC specification.
    /// </summary>
    /// <param name="valuesCount">The number of values to decode.</param>
    /// <param name="bitSource">The source of values to decode from.</param>
    /// <returns>The decoded values. The collection always contains exactly <paramref name="valuesCount"/> elements.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the encoded block size is too large.</exception>
    /// <exception cref="InvalidOperationException">Thrown when there are not enough bits to decode.</exception>
    public int[] Decode(int valuesCount, ref BitStream bitSource)
    {
        int[] result = new int[valuesCount];
        this.Decode(valuesCount, ref bitSource, result);
        return result;
    }

    /// <summary>
    /// Decodes one trit/quint BISE block (ASTC spec §C.2.12) into <paramref name="result"/>.
    /// Returns the number of values written (5 for trits, 3 for quints). Uses direct bit
    /// extraction (no BitStream) and flat encoding tables for speed — this is on the hot path.
    /// </summary>
    public static int DecodeISEBlock(BiseEncodingMode mode, ulong encodedBlock, int encodedBitCount, Span<int> result)
    {
        ulong mantissaMask = (1UL << encodedBitCount) - 1;
        return mode == BiseEncodingMode.TritEncoding
            ? DecodeTritBlock(encodedBlock, encodedBitCount, mantissaMask, result)
            : DecodeQuintBlock(encodedBlock, encodedBitCount, mantissaMask, result);
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
        result[0] = (FlatTritEncodings[tritTableBase] << encodedBitCount) | mantissa0;
        result[1] = (FlatTritEncodings[tritTableBase + 1] << encodedBitCount) | mantissa1;
        result[2] = (FlatTritEncodings[tritTableBase + 2] << encodedBitCount) | mantissa2;
        result[3] = (FlatTritEncodings[tritTableBase + 3] << encodedBitCount) | mantissa3;
        result[4] = (FlatTritEncodings[tritTableBase + 4] << encodedBitCount) | mantissa4;
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
        result[0] = (FlatQuintEncodings[quintTableBase] << encodedBitCount) | mantissa0;
        result[1] = (FlatQuintEncodings[quintTableBase + 1] << encodedBitCount) | mantissa1;
        result[2] = (FlatQuintEncodings[quintTableBase + 2] << encodedBitCount) | mantissa2;
        return 3;
    }
}
