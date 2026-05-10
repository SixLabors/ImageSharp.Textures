// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

// Test-only surface for constructing and mutating LogicalBlock state. Production decode
// paths go through LogicalBlock.UnpackLogicalBlock and produce an immutable result.
// Keeping these in a separate partial makes it obvious which members are load-bearing
// for decode versus which exist to support tests that build blocks by hand.
internal sealed partial class LogicalBlock
{
    public LogicalBlock(Footprint footprint)
    {
        this.endpoints = [ColorEndpointPair.Ldr(default, default)];
        this.endpointCount = 1;
        this.weights = new int[footprint.PixelCount];
        this.partition = new Partition(footprint, 1, 0)
        {
            Assignment = new int[footprint.PixelCount]
        };
    }

    public void SetWeightAt(int x, int y, int weight)
    {
        if (weight is < 0 or > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(weight));
        }

        this.weights[(y * this.GetFootprint().Width) + x] = weight;
    }

    public void SetDualPlaneWeightAt(int channel, int x, int y, int weight)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channel);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weight, 64);

        if (!this.IsDualPlane())
        {
            throw new InvalidOperationException("Not a dual plane block");
        }

        if (this.dualPlane is not null && this.dualPlane.Channel == channel)
        {
            this.dualPlane.Weights[(y * this.GetFootprint().Width) + x] = weight;
        }
        else
        {
            this.SetWeightAt(x, y, weight);
        }
    }

    public void SetPartition(Partition p)
    {
        if (!p.Footprint.Equals(this.partition.Footprint))
        {
            throw new InvalidOperationException("New partitions may not be for a different footprint");
        }

        this.partition = p;
        if (this.endpointCount < p.PartitionCount)
        {
            ColorEndpointPair[] newEndpoints = new ColorEndpointPair[p.PartitionCount];
            Array.Copy(this.endpoints, newEndpoints, this.endpointCount);
            for (int i = this.endpointCount; i < p.PartitionCount; i++)
            {
                newEndpoints[i] = ColorEndpointPair.Ldr(default, default);
            }

            this.endpoints = newEndpoints;
        }

        this.endpointCount = p.PartitionCount;
    }

    public void SetEndpoints(Rgba32 firstEndpoint, Rgba32 secondEndpoint, int subset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subset);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subset, this.partition.PartitionCount);

        this.endpoints[subset] = ColorEndpointPair.Ldr(firstEndpoint, secondEndpoint);
    }

    public void SetDualPlaneChannel(int channel)
    {
        if (channel < 0)
        {
            this.dualPlane = null;
        }
        else if (this.dualPlane != null)
        {
            this.dualPlane.Channel = channel;
        }
        else
        {
            this.dualPlane = new DualPlaneData { Channel = channel, Weights = (int[])this.weights.Clone() };
        }
    }
}
