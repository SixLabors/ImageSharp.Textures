// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.BiseEncoding.Quantize;
using SixLabors.ImageSharp.Textures.Astc.BlockDecoder;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.TexelBlock;

internal sealed class LogicalBlock
{
    private ColorEndpointPair[] _endpoints;
    private int _endpointCount;
    private int[] _weights;
    private Partition _partition;
    private DualPlaneData? _dualPlane;

    public LogicalBlock(Footprint footprint)
    {
        _endpoints = [ColorEndpointPair.Ldr(RgbaColor.Empty, RgbaColor.Empty)];
        _endpointCount = 1;
        _weights = new int[footprint.PixelCount];
        _partition = new Partition(footprint, 1, 0)
        {
            Assignment = new int[footprint.PixelCount]
        };
    }

    public LogicalBlock(Footprint footprint, in IntermediateBlock.IntermediateBlockData block)
    {
        _endpoints = new ColorEndpointPair[block.EndpointCount];
        _endpointCount = DecodeEndpoints(in block, _endpoints);
        _partition = ComputePartition(footprint, in block);
        _weights = new int[footprint.PixelCount];
        CalculateWeights(footprint, in block);
    }

    public LogicalBlock(Footprint footprint, IntermediateBlock.VoidExtentData block)
    {
        _endpoints = new ColorEndpointPair[1];
        _endpointCount = DecodeEndpoints(block, _endpoints);
        _partition = ComputePartition(footprint, block);
        _weights = new int[footprint.PixelCount];
    }

    /// <summary>
    /// Direct-decode constructor: decodes directly from raw bits + BlockInfo,
    /// bypassing IntermediateBlock and using batch unquantize operations.
    /// </summary>
    private LogicalBlock(Footprint footprint, UInt128 bits, in BlockInfo info)
    {
        // --- BISE decode + batch unquantize color endpoint values ---
        Span<int> colors = stackalloc int[info.ColorValuesCount];
        FusedBlockDecoder.DecodeBiseValues(bits, info.ColorStartBit, info.ColorBitCount,
            info.ColorValuesRange, info.ColorValuesCount, colors);
        Quantization.UnquantizeCEValuesBatch(colors, info.ColorValuesCount, info.ColorValuesRange);

        // --- Decode endpoints per partition ---
        _endpointCount = info.PartitionCount;
        _endpoints = new ColorEndpointPair[_endpointCount];
        int colorIndex = 0;
        for (int i = 0; i < _endpointCount; i++)
        {
            var mode = info.GetEndpointMode(i);
            int colorCount = mode.GetColorValuesCount();
            ReadOnlySpan<int> slice = colors.Slice(colorIndex, colorCount);
            _endpoints[i] = EndpointCodec.DecodeColorsForModePolymorphicUnquantized(slice, mode);
            colorIndex += colorCount;
        }

        // --- Set up partition ---
        _partition = info.PartitionCount > 1
            ? Partition.GetASTCPartition(footprint, info.PartitionCount,
                (int)BitOperations.GetBits(bits.Low(), 13, 10))
            : GenerateSinglePartition(footprint);

        // --- BISE decode + unquantize + infill weights ---
        int gridSize = info.GridWidth * info.GridHeight;
        bool isDualPlane = info.IsDualPlane;
        int totalWeights = isDualPlane ? gridSize * 2 : gridSize;

        Span<int> rawWeights = stackalloc int[totalWeights];
        FusedBlockDecoder.DecodeBiseWeights(bits, info.WeightBitCount, info.WeightRange,
            totalWeights, rawWeights);

        var decimationInfo = DecimationTable.Get(footprint, info.GridWidth, info.GridHeight);
        _weights = new int[footprint.PixelCount];

        if (!isDualPlane)
        {
            Quantization.UnquantizeWeightsBatch(rawWeights, gridSize, info.WeightRange);
            DecimationTable.InfillWeights(rawWeights[..gridSize], decimationInfo, _weights);
        }
        else
        {
            // De-interleave: even indices -> plane0, odd indices -> plane1
            Span<int> plane0 = stackalloc int[gridSize];
            Span<int> plane1 = stackalloc int[gridSize];
            for (int i = 0; i < gridSize; i++)
            {
                plane0[i] = rawWeights[i * 2];
                plane1[i] = rawWeights[i * 2 + 1];
            }

            Quantization.UnquantizeWeightsBatch(plane0, gridSize, info.WeightRange);
            Quantization.UnquantizeWeightsBatch(plane1, gridSize, info.WeightRange);

            DecimationTable.InfillWeights(plane0, decimationInfo, _weights);

            _dualPlane = new DualPlaneData
            {
                Channel = info.DualPlaneChannel,
                Weights = new int[footprint.PixelCount]
            };
            DecimationTable.InfillWeights(plane1, decimationInfo, _dualPlane.Weights);
        }
    }

    public Footprint GetFootprint() => _partition.Footprint;

    public void SetWeightAt(int x, int y, int weight)
    {
        if (weight < 0 || weight > 64)
            throw new ArgumentOutOfRangeException(nameof(weight));

        _weights[y * GetFootprint().Width + x] = weight;
    }

    public int WeightAt(int x, int y) => _weights[y * GetFootprint().Width + x];

    public void SetDualPlaneWeightAt(int channel, int x, int y, int weight)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channel);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weight, 64);

        if (!IsDualPlane())
            throw new InvalidOperationException("Not a dual plane block");

        if (_dualPlane is not null && _dualPlane.Channel == channel)
            _dualPlane.Weights[y * GetFootprint().Width + x] = weight;
        else
            SetWeightAt(x, y, weight);
    }

    public int DualPlaneWeightAt(int channel, int x, int y)
    {
        if (!IsDualPlane())
            return WeightAt(x, y);

        return _dualPlane is not null && _dualPlane.Channel == channel
            ? _dualPlane.Weights[y * GetFootprint().Width + x]
            : WeightAt(x, y);
    }

    public RgbaColor ColorAt(int x, int y)
    {
        var footprint = GetFootprint();

        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, footprint.Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, footprint.Height);

        int index = y * footprint.Width + x;
        int part = _partition.Assignment[index];
        ref var endpoint = ref _endpoints[part];

        int weight = _weights[index];
        if (!endpoint.IsHdr)
        {
            if (_dualPlane is not null)
                return SimdHelpers.InterpolateColorLdrDualPlane(
                    endpoint.LdrLow, endpoint.LdrHigh, weight, _dualPlane.Channel, _dualPlane.Weights[index]);
            return SimdHelpers.InterpolateColorLdr(endpoint.LdrLow, endpoint.LdrHigh, weight);
        }
        else
        {
            if (_dualPlane is not null)
            {
                int dualPlaneChannel = _dualPlane.Channel;
                int dualPlaneWeight = _dualPlane.Weights[index];
                return new RgbaColor(
                    r: InterpolateChannelHdr(endpoint.HdrLow[0], endpoint.HdrHigh[0],
                        dualPlaneChannel == 0 ? dualPlaneWeight : weight) >> 8,
                    g: InterpolateChannelHdr(endpoint.HdrLow[1], endpoint.HdrHigh[1],
                        dualPlaneChannel == 1 ? dualPlaneWeight : weight) >> 8,
                    b: InterpolateChannelHdr(endpoint.HdrLow[2], endpoint.HdrHigh[2],
                        dualPlaneChannel == 2 ? dualPlaneWeight : weight) >> 8,
                    a: InterpolateChannelHdr(endpoint.HdrLow[3], endpoint.HdrHigh[3],
                        dualPlaneChannel == 3 ? dualPlaneWeight : weight) >> 8);
            }
            return new RgbaColor(
                r: InterpolateChannelHdr(endpoint.HdrLow[0], endpoint.HdrHigh[0], weight) >> 8,
                g: InterpolateChannelHdr(endpoint.HdrLow[1], endpoint.HdrHigh[1], weight) >> 8,
                b: InterpolateChannelHdr(endpoint.HdrLow[2], endpoint.HdrHigh[2], weight) >> 8,
                a: InterpolateChannelHdr(endpoint.HdrLow[3], endpoint.HdrHigh[3], weight) >> 8);
        }
    }

    /// <summary>
    /// Returns the HDR color at the specified pixel position.
    /// </summary>
    /// <remarks>
    /// For HDR endpoints, returns full 16-bit precision (0-65535) per channel.
    /// For LDR endpoints, upscales to HDR range.
    /// </remarks>
    public RgbaHdrColor ColorAtHdr(int x, int y)
    {
        var footprint = GetFootprint();

        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, footprint.Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, footprint.Height);

        int index = y * footprint.Width + x;
        int part = _partition.Assignment[index];
        ref var endpoint = ref _endpoints[part];

        int weight = _weights[index];
        if (endpoint.IsHdr)
        {
            if (_dualPlane != null)
            {
                int dualPlaneChannel = _dualPlane.Channel;
                int dualPlaneWeight = _dualPlane.Weights[index];
                return new RgbaHdrColor(
                    InterpolateChannelHdr(endpoint.HdrLow[0], endpoint.HdrHigh[0],
                        dualPlaneChannel == 0 ? dualPlaneWeight : weight),
                    InterpolateChannelHdr(endpoint.HdrLow[1], endpoint.HdrHigh[1],
                        dualPlaneChannel == 1 ? dualPlaneWeight : weight),
                    InterpolateChannelHdr(endpoint.HdrLow[2], endpoint.HdrHigh[2],
                        dualPlaneChannel == 2 ? dualPlaneWeight : weight),
                    InterpolateChannelHdr(endpoint.HdrLow[3], endpoint.HdrHigh[3],
                        dualPlaneChannel == 3 ? dualPlaneWeight : weight));
            }
            return new RgbaHdrColor(
                InterpolateChannelHdr(endpoint.HdrLow[0], endpoint.HdrHigh[0], weight),
                InterpolateChannelHdr(endpoint.HdrLow[1], endpoint.HdrHigh[1], weight),
                InterpolateChannelHdr(endpoint.HdrLow[2], endpoint.HdrHigh[2], weight),
                InterpolateChannelHdr(endpoint.HdrLow[3], endpoint.HdrHigh[3], weight));
        }
        else
        {
            if (_dualPlane != null)
            {
                int dualPlaneChannel = _dualPlane.Channel;
                int dualPlaneWeight = _dualPlane.Weights[index];
                return new RgbaHdrColor(
                    (ushort)(InterpolateChannel(endpoint.LdrLow.R, endpoint.LdrHigh.R,
                        dualPlaneChannel == 0 ? dualPlaneWeight : weight) * 257),
                    (ushort)(InterpolateChannel(endpoint.LdrLow.G, endpoint.LdrHigh.G,
                        dualPlaneChannel == 1 ? dualPlaneWeight : weight) * 257),
                    (ushort)(InterpolateChannel(endpoint.LdrLow.B, endpoint.LdrHigh.B,
                        dualPlaneChannel == 2 ? dualPlaneWeight : weight) * 257),
                    (ushort)(InterpolateChannel(endpoint.LdrLow.A, endpoint.LdrHigh.A,
                        dualPlaneChannel == 3 ? dualPlaneWeight : weight) * 257));
            }
            return new RgbaHdrColor(
                (ushort)(InterpolateChannel(endpoint.LdrLow.R, endpoint.LdrHigh.R, weight) * 257),
                (ushort)(InterpolateChannel(endpoint.LdrLow.G, endpoint.LdrHigh.G, weight) * 257),
                (ushort)(InterpolateChannel(endpoint.LdrLow.B, endpoint.LdrHigh.B, weight) * 257),
                (ushort)(InterpolateChannel(endpoint.LdrLow.A, endpoint.LdrHigh.A, weight) * 257));
        }
    }

    /// <summary>
    /// Writes the HDR float values for the pixel at (x, y) into the output span.
    /// </summary>
    /// <remarks>
    /// For HDR endpoints, values are in LNS (Log-Normalized Space). After interpolation
    /// in LNS, the result is converted to FP16 via <see cref="LnsToSf16"/> then widened to float.
    /// For Mode 14 (HDR RGB + LDR Alpha), the alpha channel is UNORM16 instead of LNS.
    /// For LDR endpoints, the interpolated UNORM16 value is normalized to 0.0-1.0.
    /// </remarks>
    public void WriteHdrPixel(int x, int y, Span<float> output)
    {
        var footprint = GetFootprint();

        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, footprint.Width);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, footprint.Height);

        int index = y * footprint.Width + x;
        int part = _partition.Assignment[index];
        ref var endpoint = ref _endpoints[part];

        int weight = _weights[index];
        int dualPlaneChannel = _dualPlane?.Channel ?? -1;
        int dualPlaneWeight = _dualPlane?.Weights[index] ?? weight;

        if (endpoint.IsHdr)
        {
            for (int channel = 0; channel < RgbaColor.BytesPerPixel; ++channel)
            {
                int channelWeight = (channel == dualPlaneChannel)
                    ? dualPlaneWeight
                    : weight;
                ushort interpolated = InterpolateChannelHdr(endpoint.HdrLow[channel], endpoint.HdrHigh[channel], channelWeight);

                if (channel == 3 && endpoint.AlphaIsLdr)
                {
                    // Mode 14: alpha is UNORM16, normalize directly
                    output[channel] = interpolated / 65535.0f;
                }
                else if (endpoint.ValuesAreLns)
                {
                    // Normal HDR block: convert from LNS to FP16, then to float
                    ushort halfFloatBits = LnsToSf16(interpolated);
                    output[channel] = (float)BitConverter.UInt16BitsToHalf(halfFloatBits);
                }
                else
                {
                    // Void extent HDR: values are already FP16 bit patterns
                    output[channel] = (float)BitConverter.UInt16BitsToHalf(interpolated);
                }
            }
        }
        else
        {
            for (int channel = 0; channel < RgbaColor.BytesPerPixel; ++channel)
            {
                int channelWeight = (channel == dualPlaneChannel)
                    ? dualPlaneWeight
                    : weight;
                int p0 = channel switch { 0 => endpoint.LdrLow.R, 1 => endpoint.LdrLow.G, 2 => endpoint.LdrLow.B, _ => endpoint.LdrLow.A };
                int p1 = channel switch { 0 => endpoint.LdrHigh.R, 1 => endpoint.LdrHigh.G, 2 => endpoint.LdrHigh.B, _ => endpoint.LdrHigh.A };
                ushort unorm16 = InterpolateLdrAsUnorm16(p0, p1, channelWeight);
                output[channel] = unorm16 / 65535.0f;
            }
        }
    }

    /// <summary>
    /// Writes all pixels in the block directly to the output buffer in RGBA byte format.
    /// Avoids per-pixel method call overhead, type dispatch, and RgbaColor allocation.
    /// </summary>
    public void WriteAllPixelsLdr(Footprint footprint, Span<byte> buffer)
    {
        ref var endpoint0 = ref _endpoints[0];

        if (!endpoint0.IsHdr && _partition.PartitionCount == 1)
        {
            // Fast path: single-partition LDR block (most common case)
            int lowR = endpoint0.LdrLow.R, lowG = endpoint0.LdrLow.G, lowB = endpoint0.LdrLow.B, lowA = endpoint0.LdrLow.A;
            int highR = endpoint0.LdrHigh.R, highG = endpoint0.LdrHigh.G, highB = endpoint0.LdrHigh.B, highA = endpoint0.LdrHigh.A;

            if (_dualPlane == null)
            {
                WriteLdrSinglePartition(buffer, footprint, lowR, lowG, lowB, lowA, highR, highG, highB, highA);
            }
            else
            {
                int dualPlaneChannel = _dualPlane.Channel;
                var dpWeights = _dualPlane.Weights;
                int pixelCount = footprint.PixelCount;
                for (int i = 0; i < pixelCount; i++)
                {
                    SimdHelpers.WriteSinglePixelLdrDualPlane(
                        buffer, i * 4,
                        lowR, lowG, lowB, lowA, highR, highG, highB, highA,
                        _weights[i], dualPlaneChannel, dpWeights[i]);
                }
            }
        }
        else
        {
            // General path: multi-partition or HDR blocks
            WriteAllPixelsGeneral(footprint, buffer);
        }
    }

    public void SetPartition(Partition p)
    {
        if (!p.Footprint.Equals(_partition.Footprint))
            throw new InvalidOperationException("New partitions may not be for a different footprint");
        _partition = p;
        if (_endpointCount < p.PartitionCount)
        {
            var newEndpoints = new ColorEndpointPair[p.PartitionCount];
            Array.Copy(_endpoints, newEndpoints, _endpointCount);
            for (int i = _endpointCount; i < p.PartitionCount; i++)
                newEndpoints[i] = ColorEndpointPair.Ldr(RgbaColor.Empty, RgbaColor.Empty);
            _endpoints = newEndpoints;
        }
        _endpointCount = p.PartitionCount;
    }

    public void SetEndpoints(RgbaColor firstEndpoint, RgbaColor secondEndpoint, int subset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subset);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subset, _partition.PartitionCount);

        _endpoints[subset] = ColorEndpointPair.Ldr(firstEndpoint, secondEndpoint);
    }

    public void SetDualPlaneChannel(int channel)
    {
        if (channel < 0) { _dualPlane = null; }
        else if (_dualPlane != null) { _dualPlane.Channel = channel; }
        else { _dualPlane = new DualPlaneData { Channel = channel, Weights = (int[])_weights.Clone() }; }
    }

    public bool IsDualPlane() => _dualPlane is not null;

    public static LogicalBlock? UnpackLogicalBlock(Footprint footprint, UInt128 bits, in BlockInfo info)
    {
        if (!info.IsValid) return null;

        if (info.IsVoidExtent)
        {
            // Void extent blocks are rare; fall back to existing PhysicalBlock path
            var pb = PhysicalBlock.Create(bits);
            var voidExtentData = IntermediateBlock.UnpackVoidExtent(pb);
            if (voidExtentData is null) return null;

            return new LogicalBlock(footprint, voidExtentData.Value);
        }
        else
        {
            return new LogicalBlock(footprint, bits, in info);
        }
    }

    /// <summary>
    /// Converts a 16-bit LNS (Log-Normalized Space) value to a 16-bit SF16 (FP16) bit pattern.
    /// </summary>
    /// <remarks>
    /// The LNS value encodes a 5-bit exponent in the upper bits and an 11-bit mantissa
    /// in the lower bits. The mantissa is transformed using a piecewise linear function
    /// before being combined with the exponent to form the FP16 result.
    /// </remarks>
    internal static ushort LnsToSf16(int lns)
    {
        int mantissaComponent = lns & 0x7FF;       // Lower 11 bits: mantissa component
        int exponentComponent = (lns >> 11) & 0x1F; // Upper 5 bits: exponent component

        int mantissaTransformed;
        if (mantissaComponent < 512)
            mantissaTransformed = mantissaComponent * 3;
        else if (mantissaComponent < 1536)
            mantissaTransformed = mantissaComponent * 4 - 512;
        else
            mantissaTransformed = mantissaComponent * 5 - 2048;

        int result = (exponentComponent << 10) | (mantissaTransformed >> 3);
        return (ushort)Math.Min(result, 0x7BFF); // Clamp to max finite FP16
    }

    private static int DecodeEndpoints(in IntermediateBlock.IntermediateBlockData block, ColorEndpointPair[] endpointPair)
    {
        int endpointRange = block.EndpointRange ?? IntermediateBlock.EndpointRangeForBlock(block);
        if (endpointRange <= 0) throw new InvalidOperationException("Invalid endpoint range");
        for (int i = 0; i < block.EndpointCount; i++)
        {
            var ed = block.Endpoints[i];
            ReadOnlySpan<int> colorSpan = ((ReadOnlySpan<int>)ed.Colors)[..ed.ColorCount];
            endpointPair[i] = EndpointCodec.DecodeColorsForModePolymorphic(colorSpan, endpointRange, ed.Mode);
        }
        return block.EndpointCount;
    }

    private static int DecodeEndpoints(IntermediateBlock.VoidExtentData block, ColorEndpointPair[] endpointPair)
    {
        if (block.IsHdr)
        {
            // HDR void extent: ushort values are FP16 bit patterns (not LNS)
            var hdrColor = new RgbaHdrColor(block.R, block.G, block.B, block.A);
            endpointPair[0] = ColorEndpointPair.Hdr(hdrColor, hdrColor, valuesAreLns: false);
        }
        else
        {
            // LDR void extent: ushort values are UNORM16, convert to byte range
            var ldrColor = new RgbaColor(
                (byte)(block.R >> 8),
                (byte)(block.G >> 8),
                (byte)(block.B >> 8),
                (byte)(block.A >> 8));
            endpointPair[0] = ColorEndpointPair.Ldr(ldrColor, ldrColor);
        }
        return 1;
    }

    private static Partition GenerateSinglePartition(Footprint footprint)
    {
        return new Partition(footprint, 1, 0)
        {
            Assignment = new int[footprint.PixelCount]
        };
    }

    private static Partition ComputePartition(Footprint footprint, in IntermediateBlock.IntermediateBlockData block)
        => block.PartitionId.HasValue
            ? Partition.GetASTCPartition(footprint, block.EndpointCount, block.PartitionId.Value)
            : GenerateSinglePartition(footprint);

    private static Partition ComputePartition(Footprint footprint, IntermediateBlock.VoidExtentData block)
        => GenerateSinglePartition(footprint);

    private void CalculateWeights(Footprint footprint, in IntermediateBlock.IntermediateBlockData block)
    {
        int gridSize = block.WeightGridX * block.WeightGridY;
        int weightFrequency = block.DualPlaneChannel.HasValue ? 2 : 1;

        // Get decimation info once for both planes
        var decimationInfo = DecimationTable.Get(footprint, block.WeightGridX, block.WeightGridY);

        // stackalloc avoids per-block heap allocation (max 12×12 = 144 ints = 576 bytes)
        Span<int> unquantized = stackalloc int[gridSize];
        for (int i = 0; i < gridSize; ++i)
        {
            unquantized[i] = Quantization.UnquantizeWeightFromRange(
                block.Weights[i * weightFrequency], block.WeightRange);
        }
        DecimationTable.InfillWeights(unquantized, decimationInfo, _weights);

        if (block.DualPlaneChannel.HasValue)
        {
            var dualPlane = new DualPlaneData();
            dualPlane.Channel = block.DualPlaneChannel.Value;
            dualPlane.Weights = new int[footprint.PixelCount];
            _dualPlane = dualPlane;
            for (int i = 0; i < gridSize; ++i)
            {
                unquantized[i] = Quantization.UnquantizeWeightFromRange(
                    block.Weights[i * weightFrequency + 1], block.WeightRange);
            }
            DecimationTable.InfillWeights(unquantized, decimationInfo, _dualPlane.Weights);
        }
    }

    private static int InterpolateChannel(int p0, int p1, int weight)
    {
        int c0 = (p0 << 8) | p0;
        int c1 = (p1 << 8) | p1;
        int c = (c0 * (64 - weight) + c1 * weight + 32) / 64;
        int quantized = ((c * byte.MaxValue) + short.MaxValue) / (ushort.MaxValue + 1);
        return Math.Clamp(quantized, 0, byte.MaxValue);
    }

    /// <summary>
    /// Interpolates an LDR channel value and returns the full 16-bit UNORM result
    /// (before reduction to byte). Used by the HDR output path for LDR endpoints.
    /// </summary>
    private static ushort InterpolateLdrAsUnorm16(int p0, int p1, int weight)
    {
        int c0 = (p0 << 8) | p0;
        int c1 = (p1 << 8) | p1;
        int c = (c0 * (64 - weight) + c1 * weight + 32) / 64;
        return (ushort)Math.Clamp(c, 0, 0xFFFF);
    }

    /// <summary>
    /// Interpolates an HDR channel value between two endpoints using the specified weight.
    /// </summary>
    /// <remarks>
    /// HDR endpoints are already 16-bit values (FP16 bit patterns). Unlike LDR interpolation
    /// which expands 8-bit to 16-bit before interpolating, HDR interpolation operates directly
    /// on the 16-bit values
    /// </remarks>
    private static ushort InterpolateChannelHdr(int p0, int p1, int weight)
    {
        int c = (p0 * (64 - weight) + p1 * weight + 32) / 64;
        return (ushort)Math.Clamp(c, 0, 0xFFFF);
    }

    private void WriteLdrSinglePartition(
        Span<byte> buffer,
        Footprint footprint,
        int lowR,
        int lowG,
        int lowB,
        int lowA,
        int highR,
        int highG,
        int highB,
        int highA)
    {
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            SimdHelpers.WriteSinglePixelLdr(
                buffer, i * 4,
                lowR, lowG, lowB, lowA, highR, highG, highB, highA,
                _weights[i]);
        }
    }

    private void WriteAllPixelsGeneral(Footprint footprint, Span<byte> buffer)
    {
        int pixelCount = footprint.PixelCount;
        for (int i = 0; i < pixelCount; i++)
        {
            int part = _partition.Assignment[i];
            ref var endpoint = ref _endpoints[part];

            int weight = _weights[i];
            if (!endpoint.IsHdr)
            {
                if (_dualPlane is not null)
                {
                    SimdHelpers.WriteSinglePixelLdrDualPlane(
                        buffer, i * 4,
                        endpoint.LdrLow.R, endpoint.LdrLow.G, endpoint.LdrLow.B, endpoint.LdrLow.A,
                        endpoint.LdrHigh.R, endpoint.LdrHigh.G, endpoint.LdrHigh.B, endpoint.LdrHigh.A,
                        weight, _dualPlane.Channel, _dualPlane.Weights[i]);
                }
                else
                {
                    SimdHelpers.WriteSinglePixelLdr(
                        buffer, i * 4,
                        endpoint.LdrLow.R, endpoint.LdrLow.G, endpoint.LdrLow.B, endpoint.LdrLow.A,
                        endpoint.LdrHigh.R, endpoint.LdrHigh.G, endpoint.LdrHigh.B, endpoint.LdrHigh.A,
                        weight);
                }
            }
            else
            {
                int dualPlaneChannel = _dualPlane?.Channel ?? -1;
                int dualPlaneWeight = _dualPlane?.Weights[i] ?? weight;
                buffer[i * 4 + 0] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow[0], endpoint.HdrHigh[0],
                    dualPlaneChannel == 0 ? dualPlaneWeight : weight) >> 8);
                buffer[i * 4 + 1] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow[1], endpoint.HdrHigh[1],
                    dualPlaneChannel == 1 ? dualPlaneWeight : weight) >> 8);
                buffer[i * 4 + 2] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow[2], endpoint.HdrHigh[2],
                    dualPlaneChannel == 2 ? dualPlaneWeight : weight) >> 8);
                buffer[i * 4 + 3] = (byte)(InterpolateChannelHdr(
                    endpoint.HdrLow[3], endpoint.HdrHigh[3],
                    dualPlaneChannel == 3 ? dualPlaneWeight : weight) >> 8);
            }
        }
    }

    private class DualPlaneData
    {
        public int Channel;
        public int[] Weights = [];
    }
}
