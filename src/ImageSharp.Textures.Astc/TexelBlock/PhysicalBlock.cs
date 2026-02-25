// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.TexelBlock;

/// <summary>
/// A physical ASTC texel block (128 bits).
/// Delegates all block mode decoding to <see cref="BlockInfo"/>.
/// </summary>
internal readonly struct PhysicalBlock
{
    public const int SizeInBytes = 16;
    private readonly BlockInfo _info;

    public UInt128 BlockBits { get; }

    public bool IsVoidExtent => _info.IsVoidExtent;

    public bool IsIllegalEncoding => !_info.IsValid;

    public bool IsDualPlane
        => _info.IsValid && !_info.IsVoidExtent && _info.IsDualPlane;

    private PhysicalBlock(UInt128 bits, BlockInfo info)
    {
        BlockBits = bits;
        _info = info;
    }

    /// <summary>
    /// Factory method to create a PhysicalBlock from raw bits
    /// </summary>
    public static PhysicalBlock Create(UInt128 bits)
        => new(bits, BlockInfo.Decode(bits));

    public static PhysicalBlock Create(ulong low) => Create((UInt128)low);

    public static PhysicalBlock Create(ulong low, ulong high) => Create(new UInt128(high, low));

    internal (int Width, int Height)? GetWeightGridDimensions()
        => _info.IsValid && !_info.IsVoidExtent
            ? (_info.GridWidth, _info.GridHeight)
            : null;

    internal int? GetWeightRange()
        => _info.IsValid && !_info.IsVoidExtent
            ? _info.WeightRange
            : null;

    internal int[]? GetVoidExtentCoordinates()
    {
        if (!_info.IsVoidExtent) return null;

        // If void extent coords are all 1's then these are not valid void extent coords
        ulong voidExtentMask = 0xFFFFFFFFFFFFFDFFUL;
        ulong constBlockMode = 0xFFFFFFFFFFFFFDFCUL;

        return _info.IsValid && (voidExtentMask & BlockBits.Low()) != constBlockMode
            ? DecodeVoidExtentCoordinates(BlockBits)
            : null;
    }

    /// <summary>
    /// Get the dual plane channel if dual plane is enabled
    /// </summary>
    /// <returns>The dual plane channel if enabled, otherwise null.</returns>
    internal int? GetDualPlaneChannel()
        => _info.IsValid && _info.IsDualPlane
            ? _info.DualPlaneChannel
            : null;

    internal string? IdentifyInvalidEncodingIssues()
    {
        if (_info.IsValid) return null;
        return _info.IsVoidExtent
            ? IdentifyVoidExtentIssues(BlockBits)
            : "Invalid block encoding";
    }

    internal int? GetWeightBitCount()
        => _info.IsValid && !_info.IsVoidExtent
            ? _info.WeightBitCount
            : null;

    internal int? GetWeightStartBit()
        => _info.IsValid && !_info.IsVoidExtent
            ? 128 - _info.WeightBitCount
            : null;

    internal int? GetPartitionsCount()
        => _info.IsValid && !_info.IsVoidExtent
            ? _info.PartitionCount
            : null;

    internal int? GetPartitionId()
    {
        if (!_info.IsValid || _info.IsVoidExtent || _info.PartitionCount == 1) return null;
        return (int)BitOperations.GetBits(BlockBits.Low(), 13, 10);
    }

    internal ColorEndpointMode? GetEndpointMode(int partition)
    {
        if (!_info.IsValid || _info.IsVoidExtent) return null;
        if (partition < 0 || partition >= _info.PartitionCount) return null;
        return _info.GetEndpointMode(partition);
    }

    internal int? GetColorStartBit()
    {
        if (_info.IsVoidExtent) return 64;
        return _info.IsValid
            ? _info.ColorStartBit
            : null;
    }

    internal int? GetColorValuesCount()
    {
        if (_info.IsVoidExtent) return 4;
        return _info.IsValid
            ? _info.ColorValuesCount
            : null;
    }

    internal int? GetColorBitCount()
    {
        if (_info.IsVoidExtent) return 64;
        return _info.IsValid
            ? _info.ColorBitCount
            : null;
    }

    internal int? GetColorValuesRange()
    {
        if (_info.IsVoidExtent) return (1 << 16) - 1;
        return _info.IsValid
            ? _info.ColorValuesRange
            : null;
    }

    internal static int[] DecodeVoidExtentCoordinates(UInt128 astcBits)
    {
        ulong lowBits = astcBits.Low();
        var coords = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            coords[i] = (int)BitOperations.GetBits(lowBits, 12 + 13 * i, 13);
        }
        return coords;
    }

    /// <summary>
    /// Full error-string version for void extent issues (used for error reporting)
    /// </summary>
    private static string? IdentifyVoidExtentIssues(UInt128 bits)
    {
        if (BitOperations.GetBits(bits, 10, 2).Low() != 0x3UL)
            return "Reserved bits set for void extent block";

        ulong lowBits = bits.Low();
        int c0 = (int)BitOperations.GetBits(lowBits, 12, 13);
        int c1 = (int)BitOperations.GetBits(lowBits, 25, 13);
        int c2 = (int)BitOperations.GetBits(lowBits, 38, 13);
        int c3 = (int)BitOperations.GetBits(lowBits, 51, 13);

        const int all1s = (1 << 13) - 1;
        bool coordsAll1s = c0 == all1s && c1 == all1s && c2 == all1s && c3 == all1s;

        if (!coordsAll1s && (c0 >= c1 || c2 >= c3))
            return "Void extent texture coordinates are invalid";

        return null;
    }
}
