// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding;

internal sealed class BoundedIntegerSequenceDecoder : BoundedIntegerSequenceCodec
{

    private static readonly BoundedIntegerSequenceDecoder?[] _cache = new BoundedIntegerSequenceDecoder?[256];


    public BoundedIntegerSequenceDecoder(int range) : base(range) { }


    public static BoundedIntegerSequenceDecoder GetCached(int range)
    {
        var decoder = _cache[range];
        if (decoder is null)
        {
            decoder = new BoundedIntegerSequenceDecoder(range);
            _cache[range] = decoder;
        }
        return decoder;
    }

    /// <summary>
    /// Decode a sequence of bounded integers into a caller-provided span.
    /// </summary>
    /// <param name="valuesCount">The number of values to decode.</param>
    /// <param name="bitSource">The source of values to decode from.</param>
    /// <param name="result">The span to write decoded values into.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void Decode(int valuesCount, ref BitStream bitSource, Span<int> result)
    {
        int totalBitCount = GetBitCount(_encoding, valuesCount, _bitCount);
        int bitsPerBlock = GetEncodedBlockSize();
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitsPerBlock, 64);

        Span<int> blockResult = stackalloc int[5];
        int resultIndex = 0;
        int bitsRemaining = totalBitCount;

        while (bitsRemaining > 0)
        {
            int bitsToRead = Math.Min(bitsRemaining, bitsPerBlock);
            if (!bitSource.TryGetBits(bitsToRead, out ulong blockBits))
                throw new InvalidOperationException("Not enough bits in BitStream to decode BISE block");

            if (_encoding == BiseEncodingMode.BitEncoding)
            {
                if (resultIndex < valuesCount)
                    result[resultIndex++] = (int)blockBits;
            }
            else
            {
                int decoded = DecodeISEBlock(_encoding, blockBits, _bitCount, blockResult);
                for (int i = 0; i < decoded && resultIndex < valuesCount; ++i)
                    result[resultIndex++] = blockResult[i];
            }

            bitsRemaining -= bitsPerBlock;
        }

        if (resultIndex < valuesCount)
            throw new InvalidOperationException("Decoded fewer values than expected from BISE block");
    }

    /// <summary>
    /// Decode a sequence of bounded integers. The number of bits read is dependent on the number
    /// of bits required to encode <paramref name="valuesCount"/> based on the calculation provided
    /// in Section C.2.22 of the ASTC specification.
    /// </summary>
    /// <param name="valuesCount">The number of values to decode.</param>
    /// <param name="bitSource">The source of values to decode from.</param>
    /// <returns>The decoded values. The collection always contains exactly <paramref name="valuesCount"/> elements.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int[] Decode(int valuesCount, ref BitStream bitSource)
    {
        var result = new int[valuesCount];
        Decode(valuesCount, ref bitSource, result);
        return result;
    }

    /// <summary>
    /// Decode a trit/quint block into a caller-provided span.
    /// Returns the number of values written.
    /// Uses direct bit extraction (no BitStream) and flat encoding tables.
    /// </summary>
    public static int DecodeISEBlock(BiseEncodingMode mode, ulong encodedBlock, int encodedBitCount, Span<int> result)
    {
        ulong mantissaMask = (1UL << encodedBitCount) - 1;

        if (mode == BiseEncodingMode.TritEncoding)
        {
            // 5 values, interleaved bits = [2, 2, 1, 2, 1] = 8 bits total
            int bitPosition = 0;
            int mantissa0 = (int)((encodedBlock >> bitPosition) & mantissaMask); bitPosition += encodedBitCount;
            ulong encodedTrits = (encodedBlock >> bitPosition) & 0x3; bitPosition += 2;
            int mantissa1 = (int)((encodedBlock >> bitPosition) & mantissaMask); bitPosition += encodedBitCount;
            encodedTrits |= ((encodedBlock >> bitPosition) & 0x3) << 2; bitPosition += 2;
            int mantissa2 = (int)((encodedBlock >> bitPosition) & mantissaMask); bitPosition += encodedBitCount;
            encodedTrits |= ((encodedBlock >> bitPosition) & 0x1) << 4; bitPosition += 1;
            int mantissa3 = (int)((encodedBlock >> bitPosition) & mantissaMask); bitPosition += encodedBitCount;
            encodedTrits |= ((encodedBlock >> bitPosition) & 0x3) << 5; bitPosition += 2;
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
        else
        {
            // 3 values, interleaved bits = [3, 2, 2] = 7 bits total
            int bitPosition = 0;
            int mantissa0 = (int)((encodedBlock >> bitPosition) & mantissaMask); bitPosition += encodedBitCount;
            ulong encodedQuints = (encodedBlock >> bitPosition) & 0x7; bitPosition += 3;
            int mantissa1 = (int)((encodedBlock >> bitPosition) & mantissaMask); bitPosition += encodedBitCount;
            encodedQuints |= ((encodedBlock >> bitPosition) & 0x3) << 3; bitPosition += 2;
            int mantissa2 = (int)((encodedBlock >> bitPosition) & mantissaMask);
            encodedQuints |= ((encodedBlock >> (bitPosition + encodedBitCount)) & 0x3) << 5;

            int quintTableBase = (int)encodedQuints * 3;
            result[0] = (FlatQuintEncodings[quintTableBase] << encodedBitCount) | mantissa0;
            result[1] = (FlatQuintEncodings[quintTableBase + 1] << encodedBitCount) | mantissa1;
            result[2] = (FlatQuintEncodings[quintTableBase + 2] << encodedBitCount) | mantissa2;
            return 3;
        }
    }
}
