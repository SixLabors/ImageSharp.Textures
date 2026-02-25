// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding;

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
        while (index < this.values.Count)
        {
            switch (this.Encoding)
            {
                case BiseEncodingMode.TritEncoding:
                    var trits = new List<int>();
                    for (int i = 0; i < 5; ++i)
                    {
                        if (index < this.values.Count)
                        {
                            trits.Add(this.values[index++]);
                        }
                        else
                        {
                            trits.Add(0);
                        }
                    }

                    EncodeISEBlock<int>(trits, this.BitCount, ref bitSink, ref bitsWrittenCount, totalBitCount);
                    break;
                case BiseEncodingMode.QuintEncoding:
                    var quints = new List<int>();
                    for (int i = 0; i < 3; ++i)
                    {
                        var value = index < this.values.Count
                            ? this.values[index++]
                            : 0;
                        quints.Add(value);
                    }

                    EncodeISEBlock<int>(quints, this.BitCount, ref bitSink, ref bitsWrittenCount, totalBitCount);
                    break;
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

    private static void EncodeISEBlock<T>(List<int> values, int bitsPerValue, ref BitStream bitSink, ref int bitsWritten, int totalBitCount)
        where T : unmanaged
    {
        int valueCount = values.Count;
        int valueRange = (valueCount == 3) ? 5 : 3;
        int bitsPerBlock = (valueRange == 5) ? 7 : 8;
        int[] interleavedBits = (valueRange == 5)
            ? InterleavedQuintBits
            : InterleavedTritBits;

        var nonBitComponents = new int[valueCount];
        var bitComponents = new int[valueCount];
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

        int nonBitEncoding = -1;
        for (int j = (1 << encodedBitCount) - 1; j >= 0; --j)
        {
            bool matches = true;
            for (int i = 0; i < valueCount; ++i)
            {
                if (valueRange == 5)
                {
                    if (QuintEncodings[j][i] != nonBitComponents[i])
                    {
                        matches = false;
                        break;
                    }
                }
                else
                {
                    if (TritEncodings[j][i] != nonBitComponents[i])
                    {
                        matches = false;
                        break;
                    }
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
