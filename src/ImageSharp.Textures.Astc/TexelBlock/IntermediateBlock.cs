// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Astc.TexelBlock;

internal static class IntermediateBlock
{
    // From Table C.2.7 -- valid weight ranges
    public static readonly int[] ValidWeightRanges = [1, 2, 3, 4, 5, 7, 9, 11, 15, 19, 23, 31];

    // Returns the maximum endpoint value range or negative on error
    private const int EndpointRangeInvalidWeightDimensions = -1;
    private const int EndpointRangeNotEnoughColorBits = -2;

    public static IntermediateBlockData? UnpackIntermediateBlock(PhysicalBlock physicalBlock)
    {
        if (physicalBlock.IsIllegalEncoding || physicalBlock.IsVoidExtent)
            return null;

        var info = BlockInfo.Decode(physicalBlock.BlockBits);
        if (!info.IsValid || info.IsVoidExtent)
            return null;

        return UnpackIntermediateBlock(physicalBlock.BlockBits, in info);
    }

    /// <summary>
    /// Fast overload that uses pre-computed BlockInfo instead of calling PhysicalBlock getters.
    /// </summary>
    public static IntermediateBlockData? UnpackIntermediateBlock(UInt128 bits, in BlockInfo info)
    {
        if (!info.IsValid || info.IsVoidExtent) return null;

        var data = new IntermediateBlockData();

        // Use cached values from BlockInfo instead of PhysicalBlock getters
        var colorBitMask = UInt128Extensions.OnesMask(info.ColorBitCount);
        var colorBits = (bits >> info.ColorStartBit) & colorBitMask;
        var colorBitStream = new BitStream(colorBits, 128);

        var colorDecoder = BoundedIntegerSequenceDecoder.GetCached(info.ColorValuesRange);
        Span<int> colors = stackalloc int[info.ColorValuesCount];
        colorDecoder.Decode(info.ColorValuesCount, ref colorBitStream, colors);

        data.WeightGridX = info.GridWidth;
        data.WeightGridY = info.GridHeight;
        data.WeightRange = info.WeightRange;

        data.PartitionId = info.PartitionCount > 1
            ? (int)BitOperations.GetBits(bits.Low(), 13, 10)
            : null;

        data.DualPlaneChannel = info.IsDualPlane
            ? info.DualPlaneChannel
            : null;

        int colorIndex = 0;
        data.EndpointCount = info.PartitionCount;
        for (int i = 0; i < info.PartitionCount; ++i)
        {
            var mode = info.GetEndpointMode(i);
            int colorCount = mode.GetColorValuesCount();
            var ep = new IntermediateEndpointData { Mode = mode, ColorCount = colorCount };
            for (int j = 0; j < colorCount; ++j)
            {
                ep.Colors[j] = colors[colorIndex++];
            }
            data.Endpoints[i] = ep;
        }

        data.EndpointRange = info.ColorValuesRange;

        var weightBits = UInt128Extensions.ReverseBits(bits) & UInt128Extensions.OnesMask(info.WeightBitCount);
        var weightBitStream = new BitStream(weightBits, 128);

        var weightDecoder = BoundedIntegerSequenceDecoder.GetCached(data.WeightRange);
        int weightsCount = data.WeightGridX * data.WeightGridY;
        if (info.IsDualPlane) weightsCount *= 2;
        data.Weights = new int[weightsCount];
        data.WeightsCount = weightsCount;
        weightDecoder.Decode(weightsCount, ref weightBitStream, data.Weights);

        return data;
    }

    public static int EndpointRangeForBlock(in IntermediateBlockData data)
    {
        int dualPlaneMultiplier = data.DualPlaneChannel.HasValue
            ? 2
            : 1;
        if (BoundedIntegerSequenceCodec.GetBitCountForRange(data.WeightGridX * data.WeightGridY * dualPlaneMultiplier, data.WeightRange) > 96)
            return EndpointRangeInvalidWeightDimensions;

        int partitionCount = data.EndpointCount;
        int bitsWrittenCount = 11 + 2
            + ((partitionCount > 1) ? 10 : 0)
            + ((partitionCount == 1) ? 4 : 6);
        int availableColorBitsCount = ExtraConfigBitPosition(data) - bitsWrittenCount;

        int colorValuesCount = 0;
        for (int i = 0; i < data.EndpointCount; i++) colorValuesCount += data.Endpoints[i].Mode.GetColorValuesCount();

        int bitsNeededCount = (13 * colorValuesCount + 4) / 5;
        if (availableColorBitsCount < bitsNeededCount) return EndpointRangeNotEnoughColorBits;

        int colorValueRange = byte.MaxValue;
        for (; colorValueRange > 1; --colorValueRange)
        {
            int bitCountForRange = BoundedIntegerSequenceCodec.GetBitCountForRange(colorValuesCount, colorValueRange);
            if (bitCountForRange <= availableColorBitsCount) break;
        }
        return colorValueRange;
    }

    public static VoidExtentData? UnpackVoidExtent(PhysicalBlock physicalBlock)
    {
        var colorStartBit = physicalBlock.GetColorStartBit();
        var colorBitCount = physicalBlock.GetColorBitCount();
        if (physicalBlock.IsIllegalEncoding || !physicalBlock.IsVoidExtent || colorStartBit is null || colorBitCount is null)
            return null;

        var colorBits = (physicalBlock.BlockBits >> colorStartBit.Value) & UInt128Extensions.OnesMask(colorBitCount.Value);
        // We expect low 64 bits contain the 4x16-bit channels
        var low = colorBits.Low();

        var data = new VoidExtentData();
        // Bit 9 of the block mode indicates HDR (1) vs LDR (0) void extent
        data.IsHdr = (physicalBlock.BlockBits.Low() & (1UL << 9)) != 0;
        data.R = (ushort)((low >> 0) & 0xFFFF);
        data.G = (ushort)((low >> 16) & 0xFFFF);
        data.B = (ushort)((low >> 32) & 0xFFFF);
        data.A = (ushort)((low >> 48) & 0xFFFF);

        var coords = physicalBlock.GetVoidExtentCoordinates();
        data.Coords = new ushort[4];
        if (coords != null)
        {
            data.Coords[0] = (ushort)coords[0];
            data.Coords[1] = (ushort)coords[1];
            data.Coords[2] = (ushort)coords[2];
            data.Coords[3] = (ushort)coords[3];
        }
        else
        {
            ushort allOnes = (ushort)((1 << 13) - 1);
            for (int i = 0; i < 4; ++i) data.Coords[i] = allOnes;
        }

        return data;
    }

    /// <summary>
    /// Determines if all endpoint modes in the intermediate block data are the same
    /// </summary>
    internal static bool SharedEndpointModes(in IntermediateBlockData data)
    {
        if (data.EndpointCount == 0) return true;
        var first = data.Endpoints[0].Mode;
        for (int i = 1; i < data.EndpointCount; i++)
            if (data.Endpoints[i].Mode != first) return false;
        return true;
    }

    internal static int ExtraConfigBitPosition(in IntermediateBlockData data)
    {
        bool hasDualChannel = data.DualPlaneChannel.HasValue;
        int weightCount = data.WeightGridX * data.WeightGridY * (hasDualChannel ? 2 : 1);
        int weightBitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(weightCount, data.WeightRange);

        int extraConfigBitCount = 0;
        if (!SharedEndpointModes(data))
        {
            int encodedCemBitCount = 2 + data.EndpointCount * 3;
            extraConfigBitCount = encodedCemBitCount - 6;
        }

        if (hasDualChannel) extraConfigBitCount += 2;

        return 128 - weightBitCount - extraConfigBitCount;
    }

    internal struct VoidExtentData
    {
        public bool IsHdr;
        public ushort R;
        public ushort G;
        public ushort B;
        public ushort A;
        public ushort[] Coords; // length 4
    }

    [System.Runtime.CompilerServices.InlineArray(MaxColorValues)]
    internal struct EndpointColorValues
    {
        public const int MaxColorValues = 8;
#pragma warning disable CS0169, S1144 // Accessed by runtime via [InlineArray]
        private int _element0;
#pragma warning restore CS0169, S1144
    }

    internal struct IntermediateBlockData
    {
        public int WeightGridX;
        public int WeightGridY;
        public int WeightRange;

        public int[] Weights;
        public int WeightsCount;

        public int? PartitionId;
        public int? DualPlaneChannel;

        public IntermediateEndpointBuffer Endpoints;
        public int EndpointCount;

        public int? EndpointRange;
    }

    internal struct IntermediateEndpointData
    {
        public ColorEndpointMode Mode;
        public EndpointColorValues Colors;
        public int ColorCount;
    }

    [System.Runtime.CompilerServices.InlineArray(MaxPartitions)]
    internal struct IntermediateEndpointBuffer
    {
        public const int MaxPartitions = 4;
#pragma warning disable CS0169, S1144 // Accessed by runtime via [InlineArray]
        private IntermediateEndpointData _element0;
#pragma warning restore CS0169, S1144
    }
}
