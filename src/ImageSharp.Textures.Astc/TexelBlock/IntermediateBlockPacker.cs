// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Astc.TexelBlock;

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
        UInt128 physicalBlockBits = 0;
        int expectedWeightsCount = data.WeightGridX * data.WeightGridY
            * (data.DualPlaneChannel.HasValue ? 2 : 1);
        int actualWeightsCount = data.WeightsCount > 0
            ? data.WeightsCount
            : (data.Weights?.Length ?? 0);
        if (actualWeightsCount != expectedWeightsCount)
        {
            return ("Incorrect number of weights!", 0);
        }

        BitStream bitSink = new(0UL, 0);

        // First we need to encode the block mode.
        string? errorMessage = PackBlockMode(data.WeightGridX, data.WeightGridY, data.WeightRange, data.DualPlaneChannel.HasValue, ref bitSink);
        if (errorMessage != null)
        {
            return (errorMessage, 0);
        }

        // number of partitions minus one
        int partitionCount = data.EndpointCount;
        bitSink.PutBits((uint)(partitionCount - 1), 2);

        if (partitionCount > 1)
        {
            int id = data.PartitionId ?? 0;
            ArgumentOutOfRangeException.ThrowIfLessThan(id, 0);
            bitSink.PutBits((uint)id, 10);
        }

        (BitStream weightSink, int weightBitsCount) = EncodeWeights(data);

        (string? error, int extraConfig) = EncodeColorEndpointModes(data, partitionCount, ref bitSink);
        if (error != null)
        {
            return (error, 0);
        }

        int colorValueRange = data.EndpointRange ?? IntermediateBlock.EndpointRangeForBlock(data);
        if (colorValueRange == -1)
        {
            throw new InvalidOperationException($"{nameof(colorValueRange)} must not be EndpointRangeInvalidWeightDimensions");
        }

        if (colorValueRange == -2)
        {
            return ("Intermediate block emits illegal color range", 0);
        }

        BoundedIntegerSequenceEncoder colorEncoder = new(colorValueRange);
        for (int i = 0; i < data.EndpointCount; i++)
        {
            IntermediateBlock.IntermediateEndpointData ep = data.Endpoints[i];
            for (int j = 0; j < ep.ColorCount; j++)
            {
                int color = ep.Colors[j];
                if (color > colorValueRange)
                {
                    return ("Color outside available color range!", 0);
                }

                colorEncoder.AddValue(color);
            }
        }

        colorEncoder.Encode(ref bitSink);

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

        // Flush out the bit writer
        if (!bitSink.TryGetBits(128 - weightBitsCount, out UInt128 astcBits))
        {
            throw new InvalidOperationException();
        }

        if (!weightSink.TryGetBits(weightBitsCount, out UInt128 revWeightBits))
        {
            throw new InvalidOperationException();
        }

        UInt128 combined = astcBits | UInt128Extensions.ReverseBits(revWeightBits);
        physicalBlockBits = combined;

        PhysicalBlock block = PhysicalBlock.Create(physicalBlockBits);
        string? illegal = block.IdentifyInvalidEncodingIssues();

        return (illegal, physicalBlockBits);
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
        (string? maybeErr, int[]? rangeValues) = GetEncodedWeightRange(range);
        if (maybeErr != null)
        {
            return maybeErr;
        }

        // Ensure top two bits of r1 and r2 not both zero per reference
        if ((rangeValues[1] | rangeValues[2]) <= 0)
        {
            throw new InvalidOperationException($"{nameof(rangeValues)}[1] | {nameof(rangeValues)}[2] must be > 0");
        }

        for (int mode = 0; mode < BlockModeInfoTable.Length; ++mode)
        {
            BlockModeInfo blockMode = BlockModeInfoTable[mode];
            bool isValidMode = true;
            isValidMode &= blockMode.MinWeightGridDimX <= dimX;
            isValidMode &= dimX <= blockMode.MaxWeightGridDimX;
            isValidMode &= blockMode.MinWeightGridDimY <= dimY;
            isValidMode &= dimY <= blockMode.MaxWeightGridDimY;
            isValidMode &= !(blockMode.RequireSinglePlaneLowPrec && dualPlane);
            isValidMode &= !(blockMode.RequireSinglePlaneLowPrec && highPrec);

            if (!isValidMode)
            {
                continue;
            }

            uint encodedMode = BlockModeMasks[mode];
            void SetBit(uint value, int offset)
            {
                if (offset < 0)
                {
                    return;
                }

                encodedMode = (encodedMode & ~(1u << offset)) | ((value & 1u) << offset);
            }

            SetBit((uint)rangeValues[0], blockMode.R0BitPos);
            SetBit((uint)rangeValues[1], blockMode.R1BitPos);
            SetBit((uint)rangeValues[2], blockMode.R2BitPos);

            int offsetX = dimX - blockMode.MinWeightGridDimX;
            int offsetY = dimY - blockMode.MinWeightGridDimY;

            if (blockMode.WeightGridXOffsetBitPos >= 0)
            {
                encodedMode |= (uint)(offsetX << blockMode.WeightGridXOffsetBitPos);
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(offsetX, 0);
            }

            if (blockMode.WeightGridYOffsetBitPos >= 0)
            {
                encodedMode |= (uint)(offsetY << blockMode.WeightGridYOffsetBitPos);
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(offsetY, 0);
            }

            if (!blockMode.RequireSinglePlaneLowPrec)
            {
                SetBit(highPrec ? 1u : 0u, 9);
                SetBit(dualPlane ? 1u : 0u, 10);
            }

            if (bitSink.Bits != 0)
            {
                throw new InvalidOperationException($"{nameof(bitSink)}.{nameof(bitSink.Bits)} must be 0");
            }

            bitSink.PutBits(encodedMode, 11);
            return null;
        }

        return "Could not find viable block mode";
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
                cemEncoder.PutBits(classSelectorBit, 1);
            }

            for (int i = 0; i < data.EndpointCount; i++)
            {
                int epMode = ((int)data.Endpoints[i].Mode) & 3;
                cemEncoder.PutBits(epMode, 2);
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
