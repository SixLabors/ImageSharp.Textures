// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;

/// <summary>
/// The Bounded Integer Sequence Encoding (BISE) allows storage of character sequences using
/// arbitrary alphabets of up to 256 symbols. Each alphabet size is encoded in the most
/// space-efficient choice of bits, trits, and quints.
/// </summary>
internal partial class BoundedIntegerSequenceCodec
{
    /// <summary>
    /// The maximum number of bits needed to encode an ISE value.
    /// </summary>
    /// <remarks>
    /// The ASTC specification does not give a maximum number, however unquantized color
    /// values have a maximum range of 255, meaning that we can't feasibly have more
    /// than eight bits per value.
    /// </remarks>
    private const int Log2MaxRangeForBits = 8;

    /// <summary>
    /// The number of bits used after each value to store the interleaved quint block.
    /// </summary>
    protected static readonly int[] InterleavedQuintBits = [3, 2, 2];

    /// <summary>
    /// The number of bits used after each value to store the interleaved trit block.
    /// </summary>
    protected static readonly int[] InterleavedTritBits = [2, 2, 1, 2, 1];

    /// <summary>
    /// Trit encodings for BISE blocks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These tables are used to decode the blocks of values encoded using the ASTC
    /// integer sequence encoding. The theory is that five trits (values that can
    /// take any number in the range [0, 2]) can take on a total of 3^5 = 243 total
    /// values, which can be stored in eight bits. These eight bits are used to
    /// decode the five trits based on the ASTC specification in Section C.2.12.
    /// </para>
    /// <para>
    /// For simplicity, we have stored a look-up table here so that we don't need
    /// to implement the decoding logic. Similarly, seven bits are used to decode
    /// three quints.
    /// </para>
    /// </remarks>
    protected static readonly int[][] TritEncodings =
    [
        [0, 0, 0, 0, 0], [1, 0, 0, 0, 0], [2, 0, 0, 0, 0], [0, 0, 2, 0, 0], [0, 1, 0, 0, 0], [1, 1, 0, 0, 0], [2, 1, 0, 0, 0], [1, 0, 2, 0, 0], [0, 2, 0, 0, 0],
        [1, 2, 0, 0, 0], [2, 2, 0, 0, 0], [2, 0, 2, 0, 0], [0, 2, 2, 0, 0], [1, 2, 2, 0, 0], [2, 2, 2, 0, 0], [2, 0, 2, 0, 0], [0, 0, 1, 0, 0], [1, 0, 1, 0, 0],
        [2, 0, 1, 0, 0], [0, 1, 2, 0, 0], [0, 1, 1, 0, 0], [1, 1, 1, 0, 0], [2, 1, 1, 0, 0], [1, 1, 2, 0, 0], [0, 2, 1, 0, 0], [1, 2, 1, 0, 0], [2, 2, 1, 0, 0],
        [2, 1, 2, 0, 0], [0, 0, 0, 2, 2], [1, 0, 0, 2, 2], [2, 0, 0, 2, 2], [0, 0, 2, 2, 2], [0, 0, 0, 1, 0], [1, 0, 0, 1, 0], [2, 0, 0, 1, 0], [0, 0, 2, 1, 0],
        [0, 1, 0, 1, 0], [1, 1, 0, 1, 0], [2, 1, 0, 1, 0], [1, 0, 2, 1, 0], [0, 2, 0, 1, 0], [1, 2, 0, 1, 0], [2, 2, 0, 1, 0], [2, 0, 2, 1, 0], [0, 2, 2, 1, 0],
        [1, 2, 2, 1, 0], [2, 2, 2, 1, 0], [2, 0, 2, 1, 0], [0, 0, 1, 1, 0], [1, 0, 1, 1, 0], [2, 0, 1, 1, 0], [0, 1, 2, 1, 0], [0, 1, 1, 1, 0], [1, 1, 1, 1, 0],
        [2, 1, 1, 1, 0], [1, 1, 2, 1, 0], [0, 2, 1, 1, 0], [1, 2, 1, 1, 0], [2, 2, 1, 1, 0], [2, 1, 2, 1, 0], [0, 1, 0, 2, 2], [1, 1, 0, 2, 2], [2, 1, 0, 2, 2],
        [1, 0, 2, 2, 2], [0, 0, 0, 2, 0], [1, 0, 0, 2, 0], [2, 0, 0, 2, 0], [0, 0, 2, 2, 0], [0, 1, 0, 2, 0], [1, 1, 0, 2, 0], [2, 1, 0, 2, 0], [1, 0, 2, 2, 0],
        [0, 2, 0, 2, 0], [1, 2, 0, 2, 0], [2, 2, 0, 2, 0], [2, 0, 2, 2, 0], [0, 2, 2, 2, 0], [1, 2, 2, 2, 0], [2, 2, 2, 2, 0], [2, 0, 2, 2, 0], [0, 0, 1, 2, 0],
        [1, 0, 1, 2, 0], [2, 0, 1, 2, 0], [0, 1, 2, 2, 0], [0, 1, 1, 2, 0], [1, 1, 1, 2, 0], [2, 1, 1, 2, 0], [1, 1, 2, 2, 0], [0, 2, 1, 2, 0], [1, 2, 1, 2, 0],
        [2, 2, 1, 2, 0], [2, 1, 2, 2, 0], [0, 2, 0, 2, 2], [1, 2, 0, 2, 2], [2, 2, 0, 2, 2], [2, 0, 2, 2, 2], [0, 0, 0, 0, 2], [1, 0, 0, 0, 2], [2, 0, 0, 0, 2],
        [0, 0, 2, 0, 2], [0, 1, 0, 0, 2], [1, 1, 0, 0, 2], [2, 1, 0, 0, 2], [1, 0, 2, 0, 2], [0, 2, 0, 0, 2], [1, 2, 0, 0, 2], [2, 2, 0, 0, 2], [2, 0, 2, 0, 2],
        [0, 2, 2, 0, 2], [1, 2, 2, 0, 2], [2, 2, 2, 0, 2], [2, 0, 2, 0, 2], [0, 0, 1, 0, 2], [1, 0, 1, 0, 2], [2, 0, 1, 0, 2], [0, 1, 2, 0, 2], [0, 1, 1, 0, 2],
        [1, 1, 1, 0, 2], [2, 1, 1, 0, 2], [1, 1, 2, 0, 2], [0, 2, 1, 0, 2], [1, 2, 1, 0, 2], [2, 2, 1, 0, 2], [2, 1, 2, 0, 2], [0, 2, 2, 2, 2], [1, 2, 2, 2, 2],
        [2, 2, 2, 2, 2], [2, 0, 2, 2, 2], [0, 0, 0, 0, 1], [1, 0, 0, 0, 1], [2, 0, 0, 0, 1], [0, 0, 2, 0, 1], [0, 1, 0, 0, 1], [1, 1, 0, 0, 1], [2, 1, 0, 0, 1],
        [1, 0, 2, 0, 1], [0, 2, 0, 0, 1], [1, 2, 0, 0, 1], [2, 2, 0, 0, 1], [2, 0, 2, 0, 1], [0, 2, 2, 0, 1], [1, 2, 2, 0, 1], [2, 2, 2, 0, 1], [2, 0, 2, 0, 1],
        [0, 0, 1, 0, 1], [1, 0, 1, 0, 1], [2, 0, 1, 0, 1], [0, 1, 2, 0, 1], [0, 1, 1, 0, 1], [1, 1, 1, 0, 1], [2, 1, 1, 0, 1], [1, 1, 2, 0, 1], [0, 2, 1, 0, 1],
        [1, 2, 1, 0, 1], [2, 2, 1, 0, 1], [2, 1, 2, 0, 1], [0, 0, 1, 2, 2], [1, 0, 1, 2, 2], [2, 0, 1, 2, 2], [0, 1, 2, 2, 2], [0, 0, 0, 1, 1], [1, 0, 0, 1, 1],
        [2, 0, 0, 1, 1], [0, 0, 2, 1, 1], [0, 1, 0, 1, 1], [1, 1, 0, 1, 1], [2, 1, 0, 1, 1], [1, 0, 2, 1, 1], [0, 2, 0, 1, 1], [1, 2, 0, 1, 1], [2, 2, 0, 1, 1],
        [2, 0, 2, 1, 1], [0, 2, 2, 1, 1], [1, 2, 2, 1, 1], [2, 2, 2, 1, 1], [2, 0, 2, 1, 1], [0, 0, 1, 1, 1], [1, 0, 1, 1, 1], [2, 0, 1, 1, 1], [0, 1, 2, 1, 1],
        [0, 1, 1, 1, 1], [1, 1, 1, 1, 1], [2, 1, 1, 1, 1], [1, 1, 2, 1, 1], [0, 2, 1, 1, 1], [1, 2, 1, 1, 1], [2, 2, 1, 1, 1], [2, 1, 2, 1, 1], [0, 1, 1, 2, 2],
        [1, 1, 1, 2, 2], [2, 1, 1, 2, 2], [1, 1, 2, 2, 2], [0, 0, 0, 2, 1], [1, 0, 0, 2, 1], [2, 0, 0, 2, 1], [0, 0, 2, 2, 1], [0, 1, 0, 2, 1], [1, 1, 0, 2, 1],
        [2, 1, 0, 2, 1], [1, 0, 2, 2, 1], [0, 2, 0, 2, 1], [1, 2, 0, 2, 1], [2, 2, 0, 2, 1], [2, 0, 2, 2, 1], [0, 2, 2, 2, 1], [1, 2, 2, 2, 1], [2, 2, 2, 2, 1],
        [2, 0, 2, 2, 1], [0, 0, 1, 2, 1], [1, 0, 1, 2, 1], [2, 0, 1, 2, 1], [0, 1, 2, 2, 1], [0, 1, 1, 2, 1], [1, 1, 1, 2, 1], [2, 1, 1, 2, 1], [1, 1, 2, 2, 1],
        [0, 2, 1, 2, 1], [1, 2, 1, 2, 1], [2, 2, 1, 2, 1], [2, 1, 2, 2, 1], [0, 2, 1, 2, 2], [1, 2, 1, 2, 2], [2, 2, 1, 2, 2], [2, 1, 2, 2, 2], [0, 0, 0, 1, 2],
        [1, 0, 0, 1, 2], [2, 0, 0, 1, 2], [0, 0, 2, 1, 2], [0, 1, 0, 1, 2], [1, 1, 0, 1, 2], [2, 1, 0, 1, 2], [1, 0, 2, 1, 2], [0, 2, 0, 1, 2], [1, 2, 0, 1, 2],
        [2, 2, 0, 1, 2], [2, 0, 2, 1, 2], [0, 2, 2, 1, 2], [1, 2, 2, 1, 2], [2, 2, 2, 1, 2], [2, 0, 2, 1, 2], [0, 0, 1, 1, 2], [1, 0, 1, 1, 2], [2, 0, 1, 1, 2],
        [0, 1, 2, 1, 2], [0, 1, 1, 1, 2], [1, 1, 1, 1, 2], [2, 1, 1, 1, 2], [1, 1, 2, 1, 2], [0, 2, 1, 1, 2], [1, 2, 1, 1, 2], [2, 2, 1, 1, 2], [2, 1, 2, 1, 2],
        [0, 2, 2, 2, 2], [1, 2, 2, 2, 2], [2, 2, 2, 2, 2], [2, 1, 2, 2, 2]
    ];

    /// <summary>
    /// Quint encodings for BISE blocks.
    /// </summary>
    /// <remarks>
    /// See <see cref="TritEncodings"/> for more details.
    /// </remarks>
    protected static readonly int[][] QuintEncodings =
    [
        [0, 0, 0], [1, 0, 0], [2, 0, 0], [3, 0, 0], [4, 0, 0], [0, 4, 0], [4, 4, 0], [4, 4, 4], [0, 1, 0], [1, 1, 0], [2, 1, 0], [3, 1, 0], [4, 1, 0],
        [1, 4, 0], [4, 4, 1], [4, 4, 4], [0, 2, 0], [1, 2, 0], [2, 2, 0], [3, 2, 0], [4, 2, 0], [2, 4, 0], [4, 4, 2], [4, 4, 4], [0, 3, 0], [1, 3, 0],
        [2, 3, 0], [3, 3, 0], [4, 3, 0], [3, 4, 0], [4, 4, 3], [4, 4, 4], [0, 0, 1], [1, 0, 1], [2, 0, 1], [3, 0, 1], [4, 0, 1], [0, 4, 1], [4, 0, 4],
        [0, 4, 4], [0, 1, 1], [1, 1, 1], [2, 1, 1], [3, 1, 1], [4, 1, 1], [1, 4, 1], [4, 1, 4], [1, 4, 4], [0, 2, 1], [1, 2, 1], [2, 2, 1], [3, 2, 1],
        [4, 2, 1], [2, 4, 1], [4, 2, 4], [2, 4, 4], [0, 3, 1], [1, 3, 1], [2, 3, 1], [3, 3, 1], [4, 3, 1], [3, 4, 1], [4, 3, 4], [3, 4, 4], [0, 0, 2],
        [1, 0, 2], [2, 0, 2], [3, 0, 2], [4, 0, 2], [0, 4, 2], [2, 0, 4], [3, 0, 4], [0, 1, 2], [1, 1, 2], [2, 1, 2], [3, 1, 2], [4, 1, 2], [1, 4, 2],
        [2, 1, 4], [3, 1, 4], [0, 2, 2], [1, 2, 2], [2, 2, 2], [3, 2, 2], [4, 2, 2], [2, 4, 2], [2, 2, 4], [3, 2, 4], [0, 3, 2], [1, 3, 2], [2, 3, 2],
        [3, 3, 2], [4, 3, 2], [3, 4, 2], [2, 3, 4], [3, 3, 4], [0, 0, 3], [1, 0, 3], [2, 0, 3], [3, 0, 3], [4, 0, 3], [0, 4, 3], [0, 0, 4], [1, 0, 4],
        [0, 1, 3], [1, 1, 3], [2, 1, 3], [3, 1, 3], [4, 1, 3], [1, 4, 3], [0, 1, 4], [1, 1, 4], [0, 2, 3], [1, 2, 3], [2, 2, 3], [3, 2, 3], [4, 2, 3],
        [2, 4, 3], [0, 2, 4], [1, 2, 4], [0, 3, 3], [1, 3, 3], [2, 3, 3], [3, 3, 3], [4, 3, 3], [3, 4, 3], [0, 3, 4], [1, 3, 4]
    ];

    /// <summary>
    /// The maximum ranges for BISE encoding.
    /// </summary>
    /// <remarks>
    /// These are the numbers between 1 and <see cref="byte.MaxValue"/>
    /// that can be represented exactly as a number in the ranges
    /// <c>[0, 2^k)</c>, <c>[0, 3 * 2^k)</c>, and <c>[0, 5 * 2^k)</c>.
    /// </remarks>
    internal static readonly int[] MaxRanges = [1, 2, 3, 4, 5, 7, 9, 11, 15, 19, 23, 31, 39, 47, 63, 79, 95, 127, 159, 191, 255];

    // Flat encoding tables: eliminates jagged array indirection
    protected static readonly int[] FlatTritEncodings = FlattenEncodings(TritEncodings, 5);
    protected static readonly int[] FlatQuintEncodings = FlattenEncodings(QuintEncodings, 3);

    private static readonly (BiseEncodingMode Mode, int BitCount)[] PackingModeCache = InitPackingModeCache();

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundedIntegerSequenceCodec"/> class.
    /// operate on sequences of integers and produce bit patterns that pack the
    /// integers based on the encoding scheme specified in the ASTC specification
    /// Section C.2.12.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The resulting bit pattern is a sequence of encoded blocks.
    /// All blocks in a sequence are one of the following encodings:
    /// </para>
    /// <list type="bullet">
    /// <item>Bit encoding: one encoded value of the form 2^k</item>
    /// <item>Trit encoding: five encoded values of the form 3*2^k</item>
    /// <item>Quint encoding: three encoded values of the form 5*2^k</item>
    /// </list>
    /// The layouts of each block are designed such that the blocks can be truncated
    /// during encoding in order to support variable length input sequences (i.e. a
    /// sequence of values that are encoded using trit encoded blocks does not
    /// need to have a multiple-of-five length).
    /// </remarks>
    /// <param name="range">Creates a decoder that decodes values within [0, <paramref name="range"/>] (inclusive).</param>
    protected BoundedIntegerSequenceCodec(int range)
    {
        (BiseEncodingMode encodingMode, int bitCount) = GetPackingModeBitCount(range);
        this.Encoding = encodingMode;
        this.BitCount = bitCount;
    }

    protected BiseEncodingMode Encoding { get; }

    protected int BitCount { get; }

    /// <summary>
    /// The number of bits needed to encode the given number of values with respect to the
    /// number of trits, quints, and bits specified by <see cref="BiseEncodingMode"/>.
    /// </summary>
    public static (BiseEncodingMode Mode, int BitCount) GetPackingModeBitCount(int range)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(range, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(range, 1 << Log2MaxRangeForBits);

        return PackingModeCache[range];
    }

    /// <summary>
    /// Returns the overall bit count for a range of values encoded
    /// </summary>
    public static int GetBitCount(BiseEncodingMode encodingMode, int valuesCount, int bitCount)
    {
        int encodingBitCount = encodingMode switch
        {
            BiseEncodingMode.TritEncoding => ((valuesCount * 8) + 4) / 5,
            BiseEncodingMode.QuintEncoding => ((valuesCount * 7) + 2) / 3,
            BiseEncodingMode.BitEncoding => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(encodingMode), "Invalid encoding mode"),
        };
        int baseBitCount = valuesCount * bitCount;

        return encodingBitCount + baseBitCount;
    }

    /// <summary>
    /// The number of bits needed to encode a given number of values within the range [0, <paramref name="range"/>] (inclusive).
    /// </summary>
    public static int GetBitCountForRange(int valuesCount, int range)
    {
        (BiseEncodingMode mode, int bitCount) = GetPackingModeBitCount(range);

        return GetBitCount(mode, valuesCount, bitCount);
    }

    /// <summary>
    /// The size of a single ISE block in bits
    /// </summary>
    protected int GetEncodedBlockSize()
    {
        (int blockSize, int extraBlockSize) = this.Encoding switch
        {
            BiseEncodingMode.TritEncoding => (5, 8),
            BiseEncodingMode.QuintEncoding => (3, 7),
            BiseEncodingMode.BitEncoding => (1, 0),
            _ => (0, 0),
        };

        return extraBlockSize + (blockSize * this.BitCount);
    }

    private static int[] FlattenEncodings(int[][] jagged, int stride)
    {
        int[] flat = new int[jagged.Length * stride];
        for (int i = 0; i < jagged.Length; i++)
        {
            for (int j = 0; j < stride; j++)
            {
                flat[(i * stride) + j] = jagged[i][j];
            }
        }

        return flat;
    }

    private static (BiseEncodingMode, int)[] InitPackingModeCache()
    {
        (BiseEncodingMode, int)[] cache = new (BiseEncodingMode, int)[1 << Log2MaxRangeForBits];

        // Precompute for all valid ranges [1, 255]
        for (int range = 1; range < cache.Length; range++)
        {
            int index = -1;
            for (int i = 0; i < MaxRanges.Length; i++)
            {
                if (MaxRanges[i] >= range)
                {
                    index = i;
                    break;
                }
            }

            int maxValue = index < 0
                ? MaxRanges[^1] + 1
                : MaxRanges[index] + 1;

            // Check QuintEncoding (5), TritEncoding (3), BitEncoding (1) in descending order
            BiseEncodingMode encodingMode = BiseEncodingMode.Unknown;
            ReadOnlySpan<BiseEncodingMode> modes = [BiseEncodingMode.QuintEncoding, BiseEncodingMode.TritEncoding, BiseEncodingMode.BitEncoding];
            foreach (BiseEncodingMode em in modes)
            {
                if (maxValue % (int)em == 0 && int.IsPow2(maxValue / (int)em))
                {
                    encodingMode = em;
                    break;
                }
            }

            if (encodingMode == BiseEncodingMode.Unknown)
            {
                throw new InvalidOperationException($"Invalid range for BISE encoding: {range}");
            }

            cache[range] = (encodingMode, int.Log2(maxValue / (int)encodingMode));
        }

        return cache;
    }
}
