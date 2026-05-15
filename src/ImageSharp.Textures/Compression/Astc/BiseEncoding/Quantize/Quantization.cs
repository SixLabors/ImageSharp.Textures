// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding.Quantize;

internal static class Quantization
{
    public const int EndpointRangeMinValue = 5;
    public const int WeightRangeMaxValue = 31;

    private static readonly SortedDictionary<int, QuantizationMap> EndpointMaps = InitEndpointMaps();
    private static readonly SortedDictionary<int, QuantizationMap> WeightMaps = InitWeightMaps();

    // Flat lookup tables indexed by range value for O(1) access.
    // Each slot maps to the QuantizationMap for the greatest supported range <= that index.
    private static readonly QuantizationMap?[] EndpointMapByRange = BuildFlatLookup(EndpointMaps, 256);
    private static readonly QuantizationMap?[] WeightMapByRange = BuildFlatLookup(WeightMaps, 32);

    // Pre-computed flat tables for weight unquantization: entry[quantizedValue] = final unquantized weight.
    // Includes the dq > 32 -> dq + 1 adjustment. Indexed by weight range.
    // Valid ranges: 1, 2, 3, 4, 5, 7, 9, 11, 15, 19, 23, 31
    private static readonly int[]?[] UnquantizeWeightsFlat = InitializeUnquantizeWeightsFlat();

    // Pre-computed flat tables for endpoint unquantization.
    // Indexed by range value. Valid ranges: 5, 7, 9, 11, 15, 19, 23, 31, 39, 47, 63, 79, 95, 127, 159, 191, 255
    private static readonly int[]?[] UnquantizeEndpointsFlat = InitializeUnquantizeEndpointsFlat();

    public static int QuantizeCEValueToRange(int value, int rangeMaxValue)
    {
        Guard.MustBeBetweenOrEqualTo(rangeMaxValue, EndpointRangeMinValue, byte.MaxValue, nameof(rangeMaxValue));
        Guard.MustBeBetweenOrEqualTo(value, 0, byte.MaxValue, nameof(value));

        return GetQuantMapForValueRange(rangeMaxValue).Quantize(value);
    }

    public static int UnquantizeCEValueFromRange(int value, int rangeMaxValue)
    {
        Guard.MustBeBetweenOrEqualTo(rangeMaxValue, EndpointRangeMinValue, byte.MaxValue, nameof(rangeMaxValue));
        Guard.MustBeBetweenOrEqualTo(value, 0, rangeMaxValue, nameof(value));

        return GetQuantMapForValueRange(rangeMaxValue).Unquantize(value);
    }

    public static int QuantizeWeightToRange(int weight, int rangeMaxValue)
    {
        Guard.MustBeBetweenOrEqualTo(rangeMaxValue, 1, WeightRangeMaxValue, nameof(rangeMaxValue));
        Guard.MustBeBetweenOrEqualTo(weight, 0, 64, nameof(weight));

        // ASTC spec §C.2.18: weight slot 33 is unused; collapse 34..64 to 33..63 before
        // table lookup. The inverse (dequantized > 32 = +1) lives in UnquantizeWeightsFlat.
        if (weight > 33)
        {
            weight -= 1;
        }

        return GetQuantMapForWeightRange(rangeMaxValue).Quantize(weight);
    }

    public static int UnquantizeWeightFromRange(int weight, int rangeMaxValue)
    {
        Guard.MustBeBetweenOrEqualTo(rangeMaxValue, 1, WeightRangeMaxValue, nameof(rangeMaxValue));
        Guard.MustBeBetweenOrEqualTo(weight, 0, rangeMaxValue, nameof(weight));

        int dequantized = GetQuantMapForWeightRange(rangeMaxValue).Unquantize(weight);
        if (dequantized > 32)
        {
            dequantized += 1;
        }

        return dequantized;
    }

    /// <summary>
    /// Batch unquantize: uses pre-computed flat table for O(1) lookup per value.
    /// No per-call validation, no conditional branch per weight.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="range"/> has no associated unquantization table — would
    /// only happen on a malformed block that escaped <see cref="BlockDecoding.BlockModeDecoder"/>'s
    /// spec-bound checks.
    /// </exception>
    internal static void UnquantizeWeightsBatch(Span<int> weights, int range)
    {
        int[]? table = UnquantizeWeightsFlat[range];
        Guard.NotNull(table, nameof(range));

        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = table[weights[i]];
        }
    }

    /// <summary>
    /// Batch unquantize color endpoint values: uses pre-computed flat table.
    /// No per-call validation, single array lookup per value.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="rangeMaxValue"/> has no associated unquantization table —
    /// would only happen on a malformed block that escaped <see cref="BlockDecoding.BlockModeDecoder"/>'s
    /// spec-bound checks.
    /// </exception>
    internal static void UnquantizeCEValuesBatch(Span<int> values, int rangeMaxValue)
    {
        int[]? table = UnquantizeEndpointsFlat[rangeMaxValue];
        Guard.NotNull(table, nameof(rangeMaxValue));

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = table[values[i]];
        }
    }

    /// <summary>
    /// Allocating variant of <see cref="UnquantizeCEValueFromRange"/> over an array: returns
    /// a new array of unquantized values, leaving the input untouched. Used by the encoder
    /// where the quantized values are still needed alongside the unquantized ones. Tolerates
    /// non-canonical ranges (carries forward to the next-smaller canonical range).
    /// </summary>
    internal static int[] UnquantizeCEValuesArray(int[] values, int rangeMaxValue)
    {
        int[] result = new int[values.Length];
        for (int i = 0; i < values.Length; ++i)
        {
            result[i] = UnquantizeCEValueFromRange(values[i], rangeMaxValue);
        }

        return result;
    }

    private static SortedDictionary<int, QuantizationMap> InitEndpointMaps()
        => new()
        {
            { 5, TritQuantizationMap.Create(5, TritQuantizationMap.GetUnquantizedValue) },
            { 7, BitQuantizationMap.Create(7, 8) },
            { 9, QuintQuantizationMap.Create(9, QuintQuantizationMap.GetUnquantizedValue) },
            { 11, TritQuantizationMap.Create(11, TritQuantizationMap.GetUnquantizedValue) },
            { 15, BitQuantizationMap.Create(15, 8) },
            { 19, QuintQuantizationMap.Create(19, QuintQuantizationMap.GetUnquantizedValue) },
            { 23, TritQuantizationMap.Create(23, TritQuantizationMap.GetUnquantizedValue) },
            { 31, BitQuantizationMap.Create(31, 8) },
            { 39, QuintQuantizationMap.Create(39, QuintQuantizationMap.GetUnquantizedValue) },
            { 47, TritQuantizationMap.Create(47, TritQuantizationMap.GetUnquantizedValue) },
            { 63, BitQuantizationMap.Create(63, 8) },
            { 79, QuintQuantizationMap.Create(79, QuintQuantizationMap.GetUnquantizedValue) },
            { 95, TritQuantizationMap.Create(95, TritQuantizationMap.GetUnquantizedValue) },
            { 127, BitQuantizationMap.Create(127, 8) },
            { 159, QuintQuantizationMap.Create(159, QuintQuantizationMap.GetUnquantizedValue) },
            { 191, TritQuantizationMap.Create(191, TritQuantizationMap.GetUnquantizedValue) },
            { 255, BitQuantizationMap.Create(255, 8) },
        };

    private static SortedDictionary<int, QuantizationMap> InitWeightMaps()
        => new()
        {
            { 1, BitQuantizationMap.Create(1, 6) },
            { 2, TritQuantizationMap.Create(2, TritQuantizationMap.GetUnquantizedWeight) },
            { 3, BitQuantizationMap.Create(3, 6) },
            { 4, QuintQuantizationMap.Create(4, QuintQuantizationMap.GetUnquantizedWeight) },
            { 5, TritQuantizationMap.Create(5, TritQuantizationMap.GetUnquantizedWeight) },
            { 7, BitQuantizationMap.Create(7, 6) },
            { 9, QuintQuantizationMap.Create(9, QuintQuantizationMap.GetUnquantizedWeight) },
            { 11, TritQuantizationMap.Create(11, TritQuantizationMap.GetUnquantizedWeight) },
            { 15, BitQuantizationMap.Create(15, 6) },
            { 19, QuintQuantizationMap.Create(19, QuintQuantizationMap.GetUnquantizedWeight) },
            { 23, TritQuantizationMap.Create(23, TritQuantizationMap.GetUnquantizedWeight) },
            { 31, BitQuantizationMap.Create(31, 6) },
        };

    private static QuantizationMap?[] BuildFlatLookup(SortedDictionary<int, QuantizationMap> maps, int size)
    {
        QuantizationMap?[] flat = new QuantizationMap?[size];
        QuantizationMap? current = null;
        for (int i = 0; i < size; i++)
        {
            if (maps.TryGetValue(i, out QuantizationMap? map))
            {
                current = map;
            }

            flat[i] = current;
        }

        return flat;
    }

    /// <summary>
    /// Returns the endpoint <see cref="QuantizationMap"/> for the given range. Callers must
    /// have already validated that <paramref name="r"/> is within
    /// <c>[<see cref="EndpointRangeMinValue"/>, byte.MaxValue]</c>; the public methods on
    /// <see cref="Quantization"/> do this. Throws if the slot has no associated map.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="r"/> is outside the valid endpoint range.
    /// </exception>
    private static QuantizationMap GetQuantMapForValueRange(int r)
        => (uint)r < (uint)EndpointMapByRange.Length && EndpointMapByRange[r] is { } map
            ? map
            : throw new ArgumentOutOfRangeException(nameof(r), r, "No endpoint quantization map for this range");

    /// <summary>
    /// Returns the weight <see cref="QuantizationMap"/> for the given range. Callers must
    /// have already validated that <paramref name="r"/> is within
    /// <c>[1, <see cref="WeightRangeMaxValue"/>]</c>; the public methods on
    /// <see cref="Quantization"/> do this. Throws if the slot has no associated map.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="r"/> is outside the valid weight range.
    /// </exception>
    private static QuantizationMap GetQuantMapForWeightRange(int r)
        => (uint)r < (uint)WeightMapByRange.Length && WeightMapByRange[r] is { } map
            ? map
            : throw new ArgumentOutOfRangeException(nameof(r), r, "No weight quantization map for this range");

    private static int[]?[] InitializeUnquantizeWeightsFlat()
    {
        int[]?[] tables = new int[]?[WeightRangeMaxValue + 1];
        foreach (KeyValuePair<int, QuantizationMap> kvp in WeightMaps)
        {
            int range = kvp.Key;
            QuantizationMap map = kvp.Value;
            int[] table = new int[range + 1];
            for (int i = 0; i <= range; i++)
            {
                int dequantized = map.Unquantize(i);
                table[i] = dequantized > 32 ? dequantized + 1 : dequantized;
            }

            tables[range] = table;
        }

        return tables;
    }

    private static int[]?[] InitializeUnquantizeEndpointsFlat()
    {
        int[]?[] tables = new int[]?[256];
        foreach (KeyValuePair<int, QuantizationMap> kvp in EndpointMaps)
        {
            int range = kvp.Key;
            QuantizationMap map = kvp.Value;
            int[] table = new int[range + 1];
            for (int i = 0; i <= range; i++)
            {
                table[i] = map.Unquantize(i);
            }

            tables[range] = table;
        }

        return tables;
    }
}
