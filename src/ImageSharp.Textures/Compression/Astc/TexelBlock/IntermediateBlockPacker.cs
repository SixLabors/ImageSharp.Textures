// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

internal static class IntermediateBlockPacker
{
    private static readonly BlockModeInfo[] BlockModeInfoTable = [
        new BlockModeInfo { MinWeightGridDimX = 4, MaxWeightGridDimX = 7, MinWeightGridDimY = 2, MaxWeightGridDimY = 5, R0BitPos = 4, R1BitPos = 0, R2BitPos = 1, WeightGridXOffsetBitPos = 7, WeightGridYOffsetBitPos = 5, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 8, MaxWeightGridDimX = 11, MinWeightGridDimY = 2, MaxWeightGridDimY = 5, R0BitPos = 4, R1BitPos = 0, R2BitPos = 1, WeightGridXOffsetBitPos = 7, WeightGridYOffsetBitPos = 5, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 2, MaxWeightGridDimX = 5, MinWeightGridDimY = 8, MaxWeightGridDimY = 11, R0BitPos = 4, R1BitPos = 0, R2BitPos = 1, WeightGridXOffsetBitPos = 5, WeightGridYOffsetBitPos = 7, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 2, MaxWeightGridDimX = 5, MinWeightGridDimY = 6, MaxWeightGridDimY = 7, R0BitPos = 4, R1BitPos = 0, R2BitPos = 1, WeightGridXOffsetBitPos = 5, WeightGridYOffsetBitPos = 7, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 2, MaxWeightGridDimX = 3, MinWeightGridDimY = 2, MaxWeightGridDimY = 5, R0BitPos = 4, R1BitPos = 0, R2BitPos = 1, WeightGridXOffsetBitPos = 7, WeightGridYOffsetBitPos = 5, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 12, MaxWeightGridDimX = 12, MinWeightGridDimY = 2, MaxWeightGridDimY = 5, R0BitPos = 4, R1BitPos = 2, R2BitPos = 3, WeightGridXOffsetBitPos = -1, WeightGridYOffsetBitPos = 5, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 2, MaxWeightGridDimX = 5, MinWeightGridDimY = 12, MaxWeightGridDimY = 12, R0BitPos = 4, R1BitPos = 2, R2BitPos = 3, WeightGridXOffsetBitPos = 5, WeightGridYOffsetBitPos = -1, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 6, MaxWeightGridDimX = 6, MinWeightGridDimY = 10, MaxWeightGridDimY = 10, R0BitPos = 4, R1BitPos = 2, R2BitPos = 3, WeightGridXOffsetBitPos = -1, WeightGridYOffsetBitPos = -1, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 10, MaxWeightGridDimX = 10, MinWeightGridDimY = 6, MaxWeightGridDimY = 6, R0BitPos = 4, R1BitPos = 2, R2BitPos = 3, WeightGridXOffsetBitPos = -1, WeightGridYOffsetBitPos = -1, RequireSinglePlaneLowPrec = false },
        new BlockModeInfo { MinWeightGridDimX = 6, MaxWeightGridDimX = 9, MinWeightGridDimY = 6, MaxWeightGridDimY = 9, R0BitPos = 4, R1BitPos = 2, R2BitPos = 3, WeightGridXOffsetBitPos = 5, WeightGridYOffsetBitPos = 9, RequireSinglePlaneLowPrec = true }
    ];

    private static readonly uint[] BlockModeMasks = [0x0u, 0x4u, 0x8u, 0xCu, 0x10Cu, 0x0u, 0x80u, 0x180u, 0x1A0u, 0x100u];

    public static (string? Error, UInt128 PhysicalBlockBits) Pack(in IntermediateBlock.IntermediateBlockData data)
    {
        if (!ValidateWeightCount(data))
        {
            return ("Incorrect number of weights!", 0);
        }

        BitStream bitSink = new(0UL, 0);

        // Block mode (ASTC spec §C.2.8 Table 24) — weight-grid dimensions, weight range,
        // and dual-plane flag.
        string? modeError = PackBlockMode(data.WeightGridX, data.WeightGridY, data.WeightRange, data.DualPlaneChannel.HasValue, ref bitSink);
        if (modeError != null)
        {
            return (modeError, 0);
        }

        WritePartitionFields(data, ref bitSink);

        (BitStream weightSink, int weightBitsCount) = EncodeWeights(data);

        (string? cemError, int extraConfig) = EncodeColorEndpointModes(data, data.EndpointCount, ref bitSink);
        if (cemError != null)
        {
            return (cemError, 0);
        }

        if (!TryResolveColorRange(data, out int colorValueRange, out string? rangeError))
        {
            return (rangeError, 0);
        }

        if (!TryEncodeColorValues(data, colorValueRange, ref bitSink, out string? colorError))
        {
            return (colorError, 0);
        }

        WriteExtraConfigAndPadding(data, weightBitsCount, extraConfig, ref bitSink);

        return ComposePhysicalBlock(ref bitSink, ref weightSink, weightBitsCount);
    }

    /// <summary>
    /// Validates that the caller-supplied weight count matches the weight-grid dimensions
    /// (doubled for dual-plane blocks per ASTC spec §C.2.20).
    /// </summary>
    private static bool ValidateWeightCount(in IntermediateBlock.IntermediateBlockData data)
    {
        int expectedWeightsCount = data.WeightGridX * data.WeightGridY
            * (data.DualPlaneChannel.HasValue ? 2 : 1);
        int actualWeightsCount = data.WeightsCount > 0
            ? data.WeightsCount
            : (data.Weights?.Length ?? 0);
        return actualWeightsCount == expectedWeightsCount;
    }

    /// <summary>
    /// Writes the 2-bit partition count (encoded as count − 1) and, for multi-partition
    /// blocks, the 10-bit partition id (ASTC spec §C.2.10).
    /// </summary>
    private static void WritePartitionFields(in IntermediateBlock.IntermediateBlockData data, ref BitStream bitSink)
    {
        int partitionCount = data.EndpointCount;
        bitSink.PutBits((uint)(partitionCount - 1), 2);

        if (partitionCount > 1)
        {
            int id = data.PartitionId ?? 0;
            ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
            bitSink.PutBits((uint)id, 10);
        }
    }

    /// <summary>
    /// Chooses the BISE color-value range (ASTC spec §C.2.16). <paramref name="error"/> is
    /// non-null only when the block is structurally legal but no BISE range fits.
    /// </summary>
    private static bool TryResolveColorRange(
        in IntermediateBlock.IntermediateBlockData data,
        out int colorValueRange,
        out string? error)
    {
        colorValueRange = data.EndpointRange ?? IntermediateBlock.EndpointRangeForBlock(data);
        error = null;

        if (colorValueRange == -1)
        {
            throw new InvalidOperationException("Color value range must not be EndpointRangeInvalidWeightDimensions");
        }

        if (colorValueRange == -2)
        {
            error = "Intermediate block emits illegal color range";
            return false;
        }

        return true;
    }

    /// <summary>
    /// BISE-encodes the per-partition color endpoint values (ASTC spec §C.2.14).
    /// Returns false if any value exceeds the selected BISE range.
    /// </summary>
    private static bool TryEncodeColorValues(
        in IntermediateBlock.IntermediateBlockData data,
        int colorValueRange,
        ref BitStream bitSink,
        out string? error)
    {
        BoundedIntegerSequenceEncoder colorEncoder = new(colorValueRange);
        for (int i = 0; i < data.EndpointCount; i++)
        {
            IntermediateBlock.IntermediateEndpointData ep = data.Endpoints[i];
            for (int j = 0; j < ep.ColorCount; j++)
            {
                int color = ep.Colors[j];
                if (color > colorValueRange)
                {
                    error = "Color outside available color range!";
                    return false;
                }

                colorEncoder.AddValue(color);
            }
        }

        colorEncoder.Encode(ref bitSink);
        error = null;
        return true;
    }

    /// <summary>
    /// Pads the bitstream out to the extra-CEM bit position with zero bits, then writes the
    /// extra-CEM configuration bits if any (ASTC spec §C.2.11).
    /// </summary>
    private static void WriteExtraConfigAndPadding(
        in IntermediateBlock.IntermediateBlockData data,
        int weightBitsCount,
        int extraConfig,
        ref BitStream bitSink)
    {
        int extraConfigBitPosition = IntermediateBlock.ExtraConfigBitPosition(data);
        int extraConfigBits = 128 - weightBitsCount - extraConfigBitPosition;

        ArgumentOutOfRangeException.ThrowIfNegative(extraConfigBits);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(extraConfig, 1 << extraConfigBits);

        int bitsToSkip = extraConfigBitPosition - (int)bitSink.Bits;
        ArgumentOutOfRangeException.ThrowIfNegative(bitsToSkip);
        while (bitsToSkip > 0)
        {
            int skipping = Math.Min(32, bitsToSkip);
            bitSink.PutBits(0u, skipping);
            bitsToSkip -= skipping;
        }

        if (extraConfigBits > 0)
        {
            bitSink.PutBits((uint)extraConfig, extraConfigBits);
        }

        ArgumentOutOfRangeException.ThrowIfNotEqual(bitSink.Bits, 128u - weightBitsCount);
    }

    /// <summary>
    /// Assembles the 128-bit physical block. The forward stream occupies the low bits and
    /// the reversed weight stream (per ASTC spec §C.2.5 — weights are written from the top
    /// of the block down) occupies the high bits.
    /// </summary>
    private static (string? Error, UInt128 PhysicalBlockBits) ComposePhysicalBlock(
        ref BitStream bitSink,
        ref BitStream weightSink,
        int weightBitsCount)
    {
        if (!bitSink.TryGetBits(128 - weightBitsCount, out UInt128 astcBits))
        {
            throw new InvalidOperationException();
        }

        if (!weightSink.TryGetBits(weightBitsCount, out UInt128 revWeightBits))
        {
            throw new InvalidOperationException();
        }

        UInt128 physicalBlockBits = astcBits | UInt128Extensions.ReverseBits(revWeightBits);
        PhysicalBlock block = PhysicalBlock.Create(physicalBlockBits);
        return (block.IdentifyInvalidEncodingIssues(), physicalBlockBits);
    }

    public static (string? Error, UInt128 PhysicalBlockBits) Pack(IntermediateBlock.VoidExtentData data)
    {
        // Pack void extent
        // Assemble the 128-bit value explicitly: low 64 bits = RGBA (4x16)
        // high 64 bits = 12-bit header (0xDFC) followed by four 13-bit coords.
        ulong high64 = ((ulong)data.A << 48) | ((ulong)data.B << 32) | ((ulong)data.G << 16) | data.R;
        ulong low64 = 0UL;

        // Header occupies lowest 12 bits of the high word
        low64 |= 0xDFCu;
        for (int i = 0; i < 4; ++i)
        {
            low64 |= ((ulong)(data.Coords[i] & 0x1FFF)) << (12 + (13 * i));
        }

        UInt128 physicalBlockBits;

        // Decide representation: if the RGBA low word is zero we emit the
        // compact single-ulong representation (low word = header+coords,
        // high word = 0) to match the reference tests. Otherwise the
        // low word holds RGBA and the high word holds header+coords.
        if (high64 == 0UL)
        {
            physicalBlockBits = (UInt128)low64;

            // using compact void extent representation
        }
        else
        {
            physicalBlockBits = new UInt128(high64, low64);

            // using full void extent representation
        }

        PhysicalBlock block = PhysicalBlock.Create(physicalBlockBits);
        string? illegal = block.IdentifyInvalidEncodingIssues();
        if (illegal is not null)
        {
            throw new InvalidOperationException($"{nameof(Pack)}(void extent) produced illegal encoding");
        }

        return (illegal, physicalBlockBits);
    }

    private static (string? Error, int[] Range) GetEncodedWeightRange(int range)
    {
        int[][] validRangeEncodings = [
            [0, 1, 0], [1, 1, 0], [0, 0, 1], [1, 0, 1], [0, 1, 1], [1, 1, 1],
            [0, 1, 0], [1, 1, 0], [0, 0, 1], [1, 0, 1], [0, 1, 1], [1, 1, 1]
        ];

        int smallestRange = IntermediateBlock.ValidWeightRanges.First();
        int largestRange = IntermediateBlock.ValidWeightRanges.Last();
        if (range < smallestRange || largestRange < range)
        {
            return ($"Could not find block mode. Invalid weight range: {range} not in [{smallestRange}, {largestRange}]", new int[3]);
        }

        int index = Array.FindIndex(IntermediateBlock.ValidWeightRanges, v => v >= range);
        if (index < 0)
        {
            index = IntermediateBlock.ValidWeightRanges.Length - 1;
        }

        int[] encoding = validRangeEncodings[index];
        return (null, [encoding[0], encoding[1], encoding[2]]);
    }

    private static string? PackBlockMode(int dimX, int dimY, int range, bool dualPlane, ref BitStream bitSink)
    {
        bool highPrec = range > 7;
        (string? rangeError, int[]? rangeValues) = GetEncodedWeightRange(range);
        if (rangeError != null)
        {
            return rangeError;
        }

        // ASTC spec §C.2.9: the top two bits of r1 and r2 cannot both be zero, so at least
        // one of them must be set in any encoded mode.
        if ((rangeValues[1] | rangeValues[2]) <= 0)
        {
            throw new InvalidOperationException("rangeValues[1] | rangeValues[2] must be > 0");
        }

        // Try each block-mode layout in BlockModeInfoTable; the first one whose grid bounds
        // and precision constraints fit produces a valid encoding.
        for (int modeIndex = 0; modeIndex < BlockModeInfoTable.Length; ++modeIndex)
        {
            BlockModeInfo blockMode = BlockModeInfoTable[modeIndex];
            if (!IsBlockModeCompatible(in blockMode, dimX, dimY, dualPlane, highPrec))
            {
                continue;
            }

            uint encodedMode = AssembleEncodedMode(in blockMode, BlockModeMasks[modeIndex], rangeValues, dimX, dimY, dualPlane, highPrec);

            if (bitSink.Bits != 0)
            {
                throw new InvalidOperationException("bitSink must be empty before writing the block mode");
            }

            bitSink.PutBits(encodedMode, 11);
            return null;
        }

        return "Could not find viable block mode";
    }

    /// <summary>
    /// Returns true if the given block-mode layout supports the requested weight grid size
    /// and precision/dual-plane combination (ASTC spec §C.2.8 Table 24).
    /// </summary>
    private static bool IsBlockModeCompatible(in BlockModeInfo blockMode, int dimX, int dimY, bool dualPlane, bool highPrec)
        => blockMode.MinWeightGridDimX <= dimX
           && dimX <= blockMode.MaxWeightGridDimX
           && blockMode.MinWeightGridDimY <= dimY
           && dimY <= blockMode.MaxWeightGridDimY
           && !(blockMode.RequireSinglePlaneLowPrec && (dualPlane || highPrec));

    /// <summary>
    /// Builds the 11-bit encoded block mode by placing the range selector bits, grid size
    /// offset bits, and (for layouts that support them) the precision and dual-plane bits
    /// into the template mask from <see cref="BlockModeMasks"/>.
    /// </summary>
    private static uint AssembleEncodedMode(
        in BlockModeInfo blockMode,
        uint template,
        int[] rangeValues,
        int dimX,
        int dimY,
        bool dualPlane,
        bool highPrec)
    {
        uint encodedMode = template;
        SetBit(ref encodedMode, (uint)rangeValues[0], blockMode.R0BitPos);
        SetBit(ref encodedMode, (uint)rangeValues[1], blockMode.R1BitPos);
        SetBit(ref encodedMode, (uint)rangeValues[2], blockMode.R2BitPos);

        int offsetX = dimX - blockMode.MinWeightGridDimX;
        int offsetY = dimY - blockMode.MinWeightGridDimY;
        PlaceGridOffset(ref encodedMode, offsetX, blockMode.WeightGridXOffsetBitPos);
        PlaceGridOffset(ref encodedMode, offsetY, blockMode.WeightGridYOffsetBitPos);

        if (!blockMode.RequireSinglePlaneLowPrec)
        {
            SetBit(ref encodedMode, highPrec ? 1u : 0u, 9);
            SetBit(ref encodedMode, dualPlane ? 1u : 0u, 10);
        }

        return encodedMode;
    }

    /// <summary>Sets bit <paramref name="offset"/> of <paramref name="encoded"/> from the LSB of <paramref name="value"/>. No-op if offset &lt; 0.</summary>
    private static void SetBit(ref uint encoded, uint value, int offset)
    {
        if (offset < 0)
        {
            return;
        }

        encoded = (encoded & ~(1u << offset)) | ((value & 1u) << offset);
    }

    /// <summary>
    /// Shifts the grid-dimension offset into place. When the layout has no offset bits
    /// (offsetBitPos &lt; 0) the caller must supply offset 0, per the fixed-grid rows in
    /// ASTC spec §C.2.8 Table 24.
    /// </summary>
    private static void PlaceGridOffset(ref uint encoded, int offset, int offsetBitPos)
    {
        if (offsetBitPos >= 0)
        {
            encoded |= (uint)(offset << offsetBitPos);
        }
        else
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(offset, 0);
        }
    }

    private static (BitStream WeightSink, int WeightBitsCount) EncodeWeights(in IntermediateBlock.IntermediateBlockData data)
    {
        BitStream weightSink = new(0UL, 0);
        BoundedIntegerSequenceEncoder weightsEncoder = new(data.WeightRange);
        int weightCount = data.WeightsCount > 0
            ? data.WeightsCount
            : (data.Weights?.Length ?? 0);
        if (data.Weights is null)
        {
            throw new InvalidOperationException($"{nameof(data.Weights)} is null in {nameof(EncodeWeights)}");
        }

        for (int i = 0; i < weightCount; i++)
        {
            weightsEncoder.AddValue(data.Weights[i]);
        }

        weightsEncoder.Encode(ref weightSink);

        int weightBitsCount = (int)weightSink.Bits;
        if ((int)weightSink.Bits != BoundedIntegerSequenceCodec.GetBitCountForRange(weightCount, data.WeightRange))
        {
            throw new InvalidOperationException($"{nameof(weightSink)}.{nameof(weightSink.Bits)} does not match expected bit count");
        }

        return (weightSink, weightBitsCount);
    }

    private static (string? Error, int ExtraConfig) EncodeColorEndpointModes(in IntermediateBlock.IntermediateBlockData data, int partitionCount, ref BitStream bitSink)
    {
        int extraConfig = 0;
        bool sharedEndpointMode = IntermediateBlock.SharedEndpointModes(data);

        if (sharedEndpointMode)
        {
            if (partitionCount > 1)
            {
                bitSink.PutBits(0u, 2);
            }

            bitSink.PutBits((uint)data.Endpoints[0].Mode, 4);
        }
        else
        {
            // compute min_class, max_class
            int minClass = 2;
            int maxClass = 0;
            for (int i = 0; i < data.EndpointCount; i++)
            {
                int endpointModeClass = ((int)data.Endpoints[i].Mode) >> 2;
                minClass = Math.Min(minClass, endpointModeClass);
                maxClass = Math.Max(maxClass, endpointModeClass);
            }

            if (maxClass - minClass > 1)
            {
                return ("Endpoint modes are invalid", 0);
            }

            BitStream cemEncoder = new(0UL, 0);
            cemEncoder.PutBits((uint)(minClass + 1), 2);

            for (int i = 0; i < data.EndpointCount; i++)
            {
                int endpointModeClass = ((int)data.Endpoints[i].Mode) >> 2;
                int classSelectorBit = endpointModeClass - minClass;
                cemEncoder.PutBits((ulong)classSelectorBit, 1);
            }

            for (int i = 0; i < data.EndpointCount; i++)
            {
                int epMode = ((int)data.Endpoints[i].Mode) & 3;
                cemEncoder.PutBits((ulong)epMode, 2);
            }

            int cemBits = 2 + (partitionCount * 3);
            if (!cemEncoder.TryGetBits(cemBits, out uint encodedCem))
            {
                throw new InvalidOperationException();
            }

            extraConfig = (int)(encodedCem >> 6);

            bitSink.PutBits(encodedCem, Math.Min(6, cemBits));
        }

        // dual plane channel
        if (data.DualPlaneChannel.HasValue)
        {
            int channel = data.DualPlaneChannel.Value;
            ArgumentOutOfRangeException.ThrowIfLessThan(channel, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(channel, 3);
            extraConfig = (extraConfig << 2) | channel;
        }

        return (null, extraConfig);
    }

    private struct BlockModeInfo
    {
        public int MinWeightGridDimX;
        public int MaxWeightGridDimX;
        public int MinWeightGridDimY;
        public int MaxWeightGridDimY;
        public int R0BitPos;
        public int R1BitPos;
        public int R2BitPos;
        public int WeightGridXOffsetBitPos;
        public int WeightGridYOffsetBitPos;
        public bool RequireSinglePlaneLowPrec;
    }
}
