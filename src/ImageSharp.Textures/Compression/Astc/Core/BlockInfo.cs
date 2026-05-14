// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Decoded block-mode metadata for a single 128-bit ASTC block. Populated by the block-mode
/// parser (produces an instance via <c>BlockModeDecoder.Decode</c>); every field maps to a
/// spec concept:
/// <list type="bullet">
/// <item><description><see cref="GridWidth"/>, <see cref="GridHeight"/>, <see cref="WeightRange"/>, <see cref="WeightBitCount"/> — spec §C.2.7, §C.2.8.</description></item>
/// <item><description><see cref="PartitionCount"/> — spec §C.2.10.</description></item>
/// <item><description><see cref="IsDualPlane"/>, <see cref="DualPlaneChannel"/> — spec §C.2.20.</description></item>
/// <item><description><see cref="ColorStartBit"/>, <see cref="ColorBitCount"/>, <see cref="ColorValuesRange"/>, <see cref="ColorValuesCount"/> — spec §C.2.16.</description></item>
/// <item><description><see cref="EndpointModes"/> — spec §C.2.11, §C.2.14.</description></item>
/// <item><description><see cref="IsVoidExtent"/> — spec §C.2.23.</description></item>
/// </list>
/// </summary>
internal struct BlockInfo
{
    /// <summary>Every ASTC compressed block is exactly 128 bits (16 bytes) regardless of footprint (spec §C.2.4).</summary>
    public const int SizeInBytes = 16;

    /// <summary>
    /// Number of output channels per decoded pixel — RGBA in both the LDR (UNORM8) and HDR
    /// (float32) profiles. Used as a multiplier on <see cref="Footprint.PixelCount"/> to size
    /// scratch and image buffers.
    /// </summary>
    public const int ChannelsPerPixel = 4;

    public bool IsValid;
    public bool IsVoidExtent;

    // Weight grid
    public int GridWidth;
    public int GridHeight;
    public int WeightRange;
    public int WeightBitCount;

    // Partitions
    public int PartitionCount;

    // Dual plane
    public bool IsDualPlane;
    public int DualPlaneChannel; // only valid if IsDualPlane

    // Color endpoints
    public int ColorStartBit;
    public int ColorBitCount;
    public int ColorValuesRange;
    public int ColorValuesCount;

    // Endpoint modes (up to 4 partitions). Indexed via GetEndpointMode.
    public EndpointModeBuffer EndpointModes;

    public ColorEndpointMode EndpointMode0
    {
        readonly get => this.EndpointModes[0];
        set => this.EndpointModes[0] = value;
    }

    /// <summary>
    /// Gets a value indicating whether the block can take the fused fast path:
    /// single-partition, single-plane, non-void-extent (the common shape per ASTC spec
    /// §C.2.10, §C.2.20, §C.2.23). Multi-partition, dual-plane, and void-extent blocks fall
    /// through to the general logical-block pipeline.
    /// </summary>
    public readonly bool IsFusable
        => !this.IsVoidExtent && this.PartitionCount == 1 && !this.IsDualPlane;

    /// <summary>
    /// Gets the colour endpoint mode for the given partition index. Only the first
    /// <see cref="PartitionCount"/> slots in <see cref="EndpointModes"/> are populated by
    /// <see cref="BlockDecoding.BlockModeDecoder"/>; the trailing slots retain their
    /// <c>default(ColorEndpointMode)</c> value and reading them would silently return
    /// <see cref="ColorEndpointMode.LdrLumaDirect"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="partition"/> is outside
    /// <c>[0, <see cref="PartitionCount"/>)</c>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ColorEndpointMode GetEndpointMode(int partition)
        => (uint)partition < (uint)this.PartitionCount
            ? this.EndpointModes[partition]
            : throw new ArgumentOutOfRangeException(nameof(partition), partition, $"Must be in [0, PartitionCount={this.PartitionCount}).");

    /// <summary>
    /// Returns true if any of this block's active partitions uses an HDR endpoint mode (spec
    /// §C.2.14: modes 2, 3, 7, 11, 14, 15). Does not detect HDR void-extent blocks (those
    /// carry their own HDR flag and have <see cref="PartitionCount"/> == 0); callers that need
    /// to reject both cases should also check <see cref="IsVoidExtent"/> against the HDR flag
    /// in the raw bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasHdrEndpoints()
    {
        for (int i = 0; i < this.PartitionCount; i++)
        {
            if (this.GetEndpointMode(i).IsHdr())
            {
                return true;
            }
        }

        return false;
    }

    [InlineArray(4)]
    public struct EndpointModeBuffer
    {
#pragma warning disable CS0169, IDE0051, S1144 // Accessed by runtime via [InlineArray]
        private ColorEndpointMode element0;
#pragma warning restore CS0169, IDE0051, S1144
    }
}
