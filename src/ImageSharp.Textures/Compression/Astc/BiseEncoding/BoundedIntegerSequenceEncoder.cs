// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

internal sealed class BoundedIntegerSequenceEncoder : BoundedIntegerSequenceCodec
{
    private readonly List<int> values = [];

    public BoundedIntegerSequenceEncoder(int range)
        : base(range)
    {
    }

    /// <summary>
    /// Adds a value to the encoding sequence.
    /// </summary>
    public void AddValue(int val) => this.values.Add(val);

    /// <summary>
    /// Encodes and writes the stored values encoding to the sink. Repeated calls  will produce the same result.
    /// </summary>
    public void Encode(ref BitStream bitSink)
    {
        int totalBitCount = GetBitCount(this.Encoding, this.values.Count, this.BitCount);

        int index = 0;
        int bitsWrittenCount = 0;
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
                    bitSink.PutBits((uint)this.values[index++], this.GetEncodedBlockSize());
                    break;
            }
        }
    }

    /// <summary>
    /// Clear the stored values.
    /// </summary>
    public void Reset() => this.values.Clear();

    private static void EncodeISEBlock(ReadOnlySpan<int> values, int bitsPerValue, ref BitStream bitSink, ref int bitsWritten, int totalBitCount)
    {
        int valueCount = values.Length;
        int valueRange = (valueCount == 3) ? 5 : 3;
        int[] interleavedBits = (valueRange == 5)
            ? InterleavedQuintBits
            : InterleavedTritBits;

        Span<int> nonBitComponents = stackalloc int[valueCount];
        Span<int> bitComponents = stackalloc int[valueCount];
        for (int i = 0; i < valueCount; ++i)
        {
            bitComponents[i] = values[i] & ((1 << bitsPerValue) - 1);
            nonBitComponents[i] = values[i] >> bitsPerValue;
        }

        // Determine how many interleaved bits for this block given the global
        // totalBitCount and how many bits have already been written.
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

        int[] encodings = valueRange == 5 ? FlatQuintEncodings : FlatTritEncodings;
        int stride = valueRange == 5 ? 3 : 5;
        int nonBitEncoding = -1;
        for (int j = (1 << encodedBitCount) - 1; j >= 0; --j)
        {
            bool matches = true;
            int rowBase = j * stride;
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
                nonBitEncoding = j;
                break;
            }
        }

        if (nonBitEncoding < 0)
        {
            throw new InvalidOperationException();
        }

        int nonBitEncodingCopy = nonBitEncoding;
        for (int i = 0; i < valueCount; ++i)
        {
            if (bitsWritten + bitsPerValue <= totalBitCount)
            {
                bitSink.PutBits((uint)bitComponents[i], bitsPerValue);
                bitsWritten += bitsPerValue;
            }

            int interleavedBitCount = interleavedBits[i];
            int interleavedBitsValue = nonBitEncodingCopy & ((1 << interleavedBitCount) - 1);
            if (bitsWritten + interleavedBitCount <= totalBitCount)
            {
                bitSink.PutBits((uint)interleavedBitsValue, interleavedBitCount);
                bitsWritten += interleavedBitCount;
                nonBitEncodingCopy >>= interleavedBitCount;
            }
        }
    }
}
