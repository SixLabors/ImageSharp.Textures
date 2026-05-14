// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

internal sealed class BoundedIntegerSequenceEncoder
{
    private readonly List<int> values = [];

    public BoundedIntegerSequenceEncoder(int range)
    {
        (BiseEncodingMode encoding, int bitCount) = BoundedIntegerSequenceCodec.GetPackingModeBitCount(range);
        this.Encoding = encoding;
        this.BitCount = bitCount;
    }

    private BiseEncodingMode Encoding { get; }

    private int BitCount { get; }

    /// <summary>
    /// Adds a value to the encoding sequence.
    /// </summary>
    public void AddValue(int val) => this.values.Add(val);

    /// <summary>
    /// Encodes and writes the stored values encoding to the sink. Repeated calls  will produce the same result.
    /// </summary>
    public void Encode(ref BitStream bitSink)
    {
        int totalBitCount = BoundedIntegerSequenceCodec.GetBitCount(this.Encoding, this.values.Count, this.BitCount);

        int index = 0;
        int bitsWrittenCount = 0;

        // Fixed 5 ints (20 bytes) — one BISE block holds at most 5 trits or 3 quints (spec §C.2.22).
        Span<int> block = stackalloc int[5];
        while (index < this.values.Count)
        {
            switch (this.Encoding)
            {
                case BiseEncodingMode.TritEncoding:
                case BiseEncodingMode.QuintEncoding:
                {
                    int blockLength = this.Encoding == BiseEncodingMode.TritEncoding ? 5 : 3;
                    Span<int> slice = block[..blockLength];
                    for (int i = 0; i < blockLength; ++i)
                    {
                        slice[i] = index < this.values.Count ? this.values[index++] : 0;
                    }

                    EncodeISEBlock(slice, this.BitCount, ref bitSink, ref bitsWrittenCount, totalBitCount);
                    break;
                }

                case BiseEncodingMode.BitEncoding:
                    bitSink.PutBits((uint)this.values[index++], BoundedIntegerSequenceCodec.GetEncodedBlockSize(this.Encoding, this.BitCount));
                    break;
            }
        }
    }

    /// <summary>
    /// Clear the stored values.
    /// </summary>
    public void Reset() => this.values.Clear();

    /// <summary>
    /// Encodes one BISE trit or quint block (ASTC spec §C.2.12). Splits each input into its
    /// plain bit mantissa and its trit/quint component, looks up the compact 7- or 8-bit
    /// selector for the combined trit/quint tuple, and interleaves the selector bits with
    /// the mantissas into the output stream.
    /// </summary>
    private static void EncodeISEBlock(ReadOnlySpan<int> values, int bitsPerValue, ref BitStream bitSink, ref int bitsWritten, int totalBitCount)
    {
        int valueCount = values.Length;
        int valueRange = valueCount == 3 ? 5 : 3;
        int[] interleavedBits = valueRange == 5 ? BoundedIntegerSequenceCodec.InterleavedQuintBits : BoundedIntegerSequenceCodec.InterleavedTritBits;

        // Up to 5 ints each (20 bytes) — one BISE block holds 5 trits or 3 quints (spec §C.2.22).
        Span<int> mantissas = stackalloc int[valueCount];
        Span<int> nonBitComponents = stackalloc int[valueCount];
        SplitComponents(values, bitsPerValue, mantissas, nonBitComponents);

        int encodedBitCount = ComputeEncodedBitCount(valueCount, bitsPerValue, interleavedBits, bitsWritten, totalBitCount);
        int selector = FindEncodingSelector(nonBitComponents, valueRange, encodedBitCount);
        if (selector < 0)
        {
            throw new InvalidOperationException("No BISE encoding found for the supplied trit/quint values");
        }

        WriteInterleavedBits(mantissas, interleavedBits, selector, bitsPerValue, ref bitSink, ref bitsWritten, totalBitCount);
    }

    /// <summary>Splits each source value into its plain-bit mantissa and its trit/quint component.</summary>
    private static void SplitComponents(ReadOnlySpan<int> values, int bitsPerValue, Span<int> mantissas, Span<int> nonBit)
    {
        int mask = (1 << bitsPerValue) - 1;
        for (int i = 0; i < values.Length; ++i)
        {
            mantissas[i] = values[i] & mask;
            nonBit[i] = values[i] >> bitsPerValue;
        }
    }

    /// <summary>
    /// Returns the number of interleaved selector bits that fit in the remaining budget.
    /// Called before lookup so the selector search only considers encodings whose selector
    /// fits without exceeding <paramref name="totalBitCount"/>.
    /// </summary>
    private static int ComputeEncodedBitCount(int valueCount, int bitsPerValue, int[] interleavedBits, int bitsWritten, int totalBitCount)
    {
        int tempBitsAdded = bitsWritten;
        int encodedBitCount = 0;
        for (int i = 0; i < valueCount; ++i)
        {
            tempBitsAdded += bitsPerValue;
            if (tempBitsAdded >= totalBitCount)
            {
                break;
            }

            encodedBitCount += interleavedBits[i];
            tempBitsAdded += interleavedBits[i];
            if (tempBitsAdded >= totalBitCount)
            {
                break;
            }
        }

        return encodedBitCount;
    }

    /// <summary>
    /// Walks the flat trit/quint encoding table in descending index order and returns the
    /// first row whose entries match <paramref name="nonBitComponents"/>. -1 if no row
    /// matches within the selector budget.
    /// </summary>
    private static int FindEncodingSelector(ReadOnlySpan<int> nonBitComponents, int valueRange, int encodedBitCount)
    {
        int[] encodings = valueRange == 5 ? BoundedIntegerSequenceCodec.FlatQuintEncodings : BoundedIntegerSequenceCodec.FlatTritEncodings;
        int stride = valueRange == 5 ? 3 : 5;
        int valueCount = nonBitComponents.Length;

        for (int selector = (1 << encodedBitCount) - 1; selector >= 0; --selector)
        {
            int rowBase = selector * stride;
            bool matches = true;
            for (int i = 0; i < valueCount; ++i)
            {
                if (encodings[rowBase + i] != nonBitComponents[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return selector;
            }
        }

        return -1;
    }

    /// <summary>
    /// Writes the alternating sequence of mantissa bits and selector bits. The selector is
    /// consumed low-bit-first, one <c>interleavedBits[i]</c>-sized chunk per value.
    /// </summary>
    private static void WriteInterleavedBits(
        ReadOnlySpan<int> mantissas,
        int[] interleavedBits,
        int selector,
        int bitsPerValue,
        ref BitStream bitSink,
        ref int bitsWritten,
        int totalBitCount)
    {
        int remainingSelector = selector;
        for (int i = 0; i < mantissas.Length; ++i)
        {
            if (bitsWritten + bitsPerValue <= totalBitCount)
            {
                bitSink.PutBits((uint)mantissas[i], bitsPerValue);
                bitsWritten += bitsPerValue;
            }

            int interleavedBitCount = interleavedBits[i];
            int interleavedBitsValue = remainingSelector & ((1 << interleavedBitCount) - 1);
            if (bitsWritten + interleavedBitCount <= totalBitCount)
            {
                bitSink.PutBits((uint)interleavedBitsValue, interleavedBitCount);
                bitsWritten += interleavedBitCount;
                remainingSelector >>= interleavedBitCount;
            }
        }
    }
}
