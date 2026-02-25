// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;

internal static class Quantization
{
    public const int EndpointRangeMinValue = 5;
    public const int WeightRangeMaxValue = 31;

    private static readonly SortedDictionary<int, QuantizationMap> EndpointMaps = InitEndpointMaps();
    private static readonly SortedDictionary<int, QuantizationMap> WeightMaps = InitWeightMaps();

    // Flat lookup tables indexed by range value for O(1) access.
    // Each slot maps to the QuantizationMap for the greatest supported range <= that index.
    private static readonly QuantizationMap?[] EndpointMapByRange = InitEndpointMapFlat();
    private static readonly QuantizationMap?[] WeightMapByRange = InitWeightMapFlat();

    // Pre-computed flat tables for weight unquantization: entry[quantizedValue] = final unquantized weight.
    // Includes the dq > 32 -> dq + 1 adjustment. Indexed by weight range.
    // Valid ranges: 1, 2, 3, 4, 5, 7, 9, 11, 15, 19, 23, 31
    private static readonly int[]?[] UnquantizeWeightsFlat = InitializeUnquantizeWeightsFlat();

    // Pre-computed flat tables for endpoint unquantization.
    // Indexed by range value. Valid ranges: 5, 7, 9, 11, 15, 19, 23, 31, 39, 47, 63, 79, 95, 127, 159, 191, 255
    private static readonly int[]?[] UnquantizeEndpointsFlat = InitializeUnquantizeEndpointsFlat();

    public static int QuantizeCEValueToRange(int value, int rangeMaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rangeMaxValue, EndpointRangeMinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rangeMaxValue, byte.MaxValue);
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, byte.MaxValue);

        var map = GetQuantMapForValueRange(rangeMaxValue);
        return map != null ? map.Quantize(value) : 0;
    }

    public static int UnquantizeCEValueFromRange(int value, int rangeMaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rangeMaxValue, EndpointRangeMinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rangeMaxValue, byte.MaxValue);
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, rangeMaxValue);

        var map = GetQuantMapForValueRange(rangeMaxValue);
        return map != null ? map.Unquantize(value) : 0;
    }

    public static int QuantizeWeightToRange(int weight, int rangeMaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rangeMaxValue, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rangeMaxValue, WeightRangeMaxValue);
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weight, 64);

        if (weight > 33)
        {
            weight -= 1;
        }

        var map = GetQuantMapForWeightRange(rangeMaxValue);
        return map != null ? map.Quantize(weight) : 0;
    }

    public static int UnquantizeWeightFromRange(int weight, int rangeMaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rangeMaxValue, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rangeMaxValue, WeightRangeMaxValue);
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weight, rangeMaxValue);

        var map = GetQuantMapForWeightRange(rangeMaxValue);
        int dequantized = map != null ? map.Unquantize(weight) : 0;
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
    internal static void UnquantizeWeightsBatch(Span<int> weights, int count, int range)
    {
        int[]? table = UnquantizeWeightsFlat[range];
        if (table == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            weights[i] = table[weights[i]];
        }
    }

    /// <summary>
    /// Batch unquantize color endpoint values: uses pre-computed flat table.
    /// No per-call validation, single array lookup per value.
    /// </summary>
    internal static void UnquantizeCEValuesBatch(Span<int> values, int count, int rangeMaxValue)
    {
        int[]? table = UnquantizeEndpointsFlat[rangeMaxValue];
        if (table == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            values[i] = table[values[i]];
        }
    }

    private static SortedDictionary<int, QuantizationMap> InitEndpointMaps()
    {
        var d = new SortedDictionary<int, QuantizationMap>
        {
            { 5, new TritQuantizationMap(5, TritQuantizationMap.GetUnquantizedValue) },
            { 7, new BitQuantizationMap(7, 8) },
            { 9, new QuintQuantizationMap(9, QuintQuantizationMap.GetUnquantizedValue) },
            { 11, new TritQuantizationMap(11, TritQuantizationMap.GetUnquantizedValue) },
            { 15, new BitQuantizationMap(15, 8) },
            { 19, new QuintQuantizationMap(19, QuintQuantizationMap.GetUnquantizedValue) },
            { 23, new TritQuantizationMap(23, TritQuantizationMap.GetUnquantizedValue) },
            { 31, new BitQuantizationMap(31, 8) },
            { 39, new QuintQuantizationMap(39, QuintQuantizationMap.GetUnquantizedValue) },
            { 47, new TritQuantizationMap(47, TritQuantizationMap.GetUnquantizedValue) },
            { 63, new BitQuantizationMap(63, 8) },
            { 79, new QuintQuantizationMap(79, QuintQuantizationMap.GetUnquantizedValue) },
            { 95, new TritQuantizationMap(95, TritQuantizationMap.GetUnquantizedValue) },
            { 127, new BitQuantizationMap(127, 8) },
            { 159, new QuintQuantizationMap(159, QuintQuantizationMap.GetUnquantizedValue) },
            { 191, new TritQuantizationMap(191, TritQuantizationMap.GetUnquantizedValue) },
            { 255, new BitQuantizationMap(255, 8) }
        };
        return d;
    }

    private static SortedDictionary<int, QuantizationMap> InitWeightMaps()
    {
        var d = new SortedDictionary<int, QuantizationMap>
        {
            { 1, new BitQuantizationMap(1, 6) },
            { 2, new TritQuantizationMap(2, TritQuantizationMap.GetUnquantizedWeight) },
            { 3, new BitQuantizationMap(3, 6) },
            { 4, new QuintQuantizationMap(4, QuintQuantizationMap.GetUnquantizedWeight) },
            { 5, new TritQuantizationMap(5, TritQuantizationMap.GetUnquantizedWeight) },
            { 7, new BitQuantizationMap(7, 6) },
            { 9, new QuintQuantizationMap(9, QuintQuantizationMap.GetUnquantizedWeight) },
            { 11, new TritQuantizationMap(11, TritQuantizationMap.GetUnquantizedWeight) },
            { 15, new BitQuantizationMap(15, 6) },
            { 19, new QuintQuantizationMap(19, QuintQuantizationMap.GetUnquantizedWeight) },
            { 23, new TritQuantizationMap(23, TritQuantizationMap.GetUnquantizedWeight) },
            { 31, new BitQuantizationMap(31, 6) }
        };
        return d;
    }

    private static QuantizationMap?[] BuildFlatLookup(SortedDictionary<int, QuantizationMap> maps, int size)
    {
        var flat = new QuantizationMap?[size];
        QuantizationMap? current = null;
        for (int i = 0; i < size; i++)
        {
            if (maps.TryGetValue(i, out var map))
            {
                current = map;
            }

            flat[i] = current;
        }

        return flat;
    }

    private static QuantizationMap?[] InitEndpointMapFlat()
        => BuildFlatLookup(InitEndpointMaps(), 256);

    private static QuantizationMap?[] InitWeightMapFlat()
        => BuildFlatLookup(InitWeightMaps(), 32);

    private static QuantizationMap? GetQuantMapForValueRange(int r)
    {
        if ((uint)r >= (uint)EndpointMapByRange.Length)
        {
            return null;
        }

        return EndpointMapByRange[r];
    }

    private static QuantizationMap? GetQuantMapForWeightRange(int r)
    {
        if ((uint)r >= (uint)WeightMapByRange.Length)
        {
            return null;
        }

        return WeightMapByRange[r];
    }

    private static int[]?[] InitializeUnquantizeWeightsFlat()
    {
        var tables = new int[]?[WeightRangeMaxValue + 1];
        foreach (KeyValuePair<int, QuantizationMap> kvp in WeightMaps)
        {
            int range = kvp.Key;
            var map = kvp.Value;
            var table = new int[range + 1];
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
        var tables = new int[]?[256];
        foreach (KeyValuePair<int, QuantizationMap> kvp in EndpointMaps)
        {
            int range = kvp.Key;
            var map = kvp.Value;
            var table = new int[range + 1];
            for (int i = 0; i <= range; i++)
            {
                table[i] = map.Unquantize(i);
            }

            tables[range] = table;
        }

        return tables;
    }
}
