// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Concurrent;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

internal sealed class Partition
{
    private static readonly ConcurrentDictionary<(FootprintType, int, int), Partition> PartitionCache = new();
    private static readonly ConcurrentDictionary<FootprintType, Partition> SinglePartitionCache = new();

    private Partition(int[] assignment) => this.Assignment = assignment;

    /// <summary>
    /// Gets the per-texel partition-subset map (length <see cref="Footprint.PixelCount"/>).
    /// Cached and shared across blocks that resolve to the same partition;
    /// <b>callers must not mutate</b>. Same convention as <see cref="DecimationInfo.WeightIndices"/>.
    /// </summary>
    public int[] Assignment { get; }

    /// <summary>
    /// Returns the shared single-partition assignment for the given footprint. Every texel is
    /// assigned to subset 0, so one zero-filled <see cref="Assignment"/> array is reused across
    /// all callers (void-extent blocks and single-partition logical-path blocks).
    /// </summary>
    public static Partition GetSinglePartition(Footprint footprint)
        => SinglePartitionCache.GetOrAdd(
            footprint.Type,
            static (_, fp) => new Partition(new int[fp.PixelCount]),
            footprint);

    public static Partition GetASTCPartition(Footprint footprint, int partitionCount, int partitionId)
        => PartitionCache.GetOrAdd(
            (footprint.Type, partitionCount, partitionId),
            static (key, fp) => Build(fp, key.Item2, key.Item3),
            footprint);

    private static Partition Build(Footprint footprint, int partitionCount, int partitionId)
    {
        int w = footprint.Width;
        int h = footprint.Height;
        int[] assignment = new int[w * h];
        int idx = 0;
        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                assignment[idx++] = SelectASTCPartition(partitionId, x, y, 0, partitionCount, footprint.PixelCount);
            }
        }

        return new Partition(assignment);
    }

    /// <summary>
    /// Computes the partition index (0..<paramref name="partitionCount"/>-1) for a texel at
    /// (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>) given the block's
    /// 10-bit partition <paramref name="seed"/>. Implements ASTC spec §C.2.21's partition
    /// selection hash: a PRNG scrambles the seed, then 12 small seeds weight the texel
    /// coordinates into four candidate values whose largest wins.
    /// </summary>
    private static int SelectASTCPartition(int seed, int x, int y, int z, int partitionCount, int pixelCount)
    {
        if (partitionCount <= 1)
        {
            return 0;
        }

        // Small footprints (< 31 texels) have all coordinates doubled so neighbouring texels
        // spread further through the hash and avoid degenerate single-partition patterns.
        if (pixelCount < 31)
        {
            x <<= 1;
            y <<= 1;
            z <<= 1;
        }

        uint randomNumber = ScrambleSeed(seed, partitionCount);

        // Fixed 12 uints (48 bytes) — partition hash uses 12 4-bit sub-seeds per spec §C.2.21.
        Span<uint> subseeds = stackalloc uint[12];
        ExtractSubSeeds(randomNumber, subseeds);
        ShiftSubSeeds(subseeds, seed, partitionCount);

        (int a, int b, int c, int d) = MixSubSeedsWithCoords(subseeds, randomNumber, x, y, z);
        return SelectPartitionFromCandidates(a, b, c, d, partitionCount);
    }

    /// <summary>
    /// Applies the 10-step PRNG scramble from ASTC spec §C.2.21 Listing 11 to the 10-bit
    /// seed offset by <paramref name="partitionCount"/>.
    /// </summary>
    private static uint ScrambleSeed(int seed, int partitionCount)
    {
        uint random = (uint)(seed + ((partitionCount - 1) * 1024));
        random ^= random >> 15;
        random -= random << 17;
        random += random << 7;
        random += random << 4;
        random ^= random >> 5;
        random += random << 16;
        random ^= random >> 7;
        random ^= random >> 3;
        random ^= random << 6;
        random ^= random >> 17;
        return random;
    }

    /// <summary>
    /// Extracts the 12 4-bit sub-seeds from the scrambled number per ASTC spec §C.2.21
    /// and squares each. The squaring biases the distribution so small values stay small
    /// and large values become dominant.
    /// </summary>
    private static void ExtractSubSeeds(uint random, Span<uint> subseeds)
    {
        subseeds[0] = random & 0xF;
        subseeds[1] = (random >> 4) & 0xF;
        subseeds[2] = (random >> 8) & 0xF;
        subseeds[3] = (random >> 12) & 0xF;
        subseeds[4] = (random >> 16) & 0xF;
        subseeds[5] = (random >> 20) & 0xF;
        subseeds[6] = (random >> 24) & 0xF;
        subseeds[7] = (random >> 28) & 0xF;
        subseeds[8] = (random >> 18) & 0xF;
        subseeds[9] = (random >> 22) & 0xF;
        subseeds[10] = (random >> 26) & 0xF;
        subseeds[11] = ((random >> 30) | (random << 2)) & 0xF;

        for (int i = 0; i < 12; ++i)
        {
            subseeds[i] *= subseeds[i];
        }
    }

    /// <summary>
    /// Right-shifts each sub-seed by one of three mode-dependent shift amounts (sh1, sh2, sh3)
    /// per ASTC spec §C.2.21. The shift choice is driven by low-order bits of the original
    /// seed together with the partition count.
    /// </summary>
    private static void ShiftSubSeeds(Span<uint> subseeds, int seed, int partitionCount)
    {
        int sh1, sh2;
        if ((seed & 1) != 0)
        {
            sh1 = (seed & 2) != 0 ? 4 : 5;
            sh2 = partitionCount == 3 ? 6 : 5;
        }
        else
        {
            sh1 = partitionCount == 3 ? 6 : 5;
            sh2 = (seed & 2) != 0 ? 4 : 5;
        }

        int sh3 = (seed & 0x10) != 0 ? sh1 : sh2;

        subseeds[0] >>= sh1;
        subseeds[1] >>= sh2;
        subseeds[2] >>= sh1;
        subseeds[3] >>= sh2;
        subseeds[4] >>= sh1;
        subseeds[5] >>= sh2;
        subseeds[6] >>= sh1;
        subseeds[7] >>= sh2;
        subseeds[8] >>= sh3;
        subseeds[9] >>= sh3;
        subseeds[10] >>= sh3;
        subseeds[11] >>= sh3;
    }

    /// <summary>
    /// Computes the four candidate values a, b, c, d as weighted combinations of the texel
    /// coordinates with sub-seeds as weights, plus the scrambled-number shifted by a
    /// candidate-specific amount. Low six bits are retained per ASTC spec §C.2.21.
    /// </summary>
    private static (int A, int B, int C, int D) MixSubSeedsWithCoords(ReadOnlySpan<uint> subseeds, uint random, int x, int y, int z)
    {
        int a = (int)((subseeds[0] * x) + (subseeds[1] * y) + (subseeds[10] * z) + (random >> 14));
        int b = (int)((subseeds[2] * x) + (subseeds[3] * y) + (subseeds[11] * z) + (random >> 10));
        int c = (int)((subseeds[4] * x) + (subseeds[5] * y) + (subseeds[8] * z) + (random >> 6));
        int d = (int)((subseeds[6] * x) + (subseeds[7] * y) + (subseeds[9] * z) + (random >> 2));
        return (a & 0x3F, b & 0x3F, c & 0x3F, d & 0x3F);
    }

    /// <summary>
    /// Returns the index of the largest of a, b, c, d after zeroing the unused ones based on
    /// <paramref name="partitionCount"/>. Ties prefer the lower index (matches ASTC spec
    /// §C.2.21's cascade of ≥ comparisons).
    /// </summary>
    private static int SelectPartitionFromCandidates(int a, int b, int c, int d, int partitionCount)
    {
        if (partitionCount <= 3)
        {
            d = 0;
        }

        if (partitionCount <= 2)
        {
            c = 0;
        }

        if (a >= b && a >= c && a >= d)
        {
            return 0;
        }

        if (b >= c && b >= d)
        {
            return 1;
        }

        return c >= d ? 2 : 3;
    }
}
