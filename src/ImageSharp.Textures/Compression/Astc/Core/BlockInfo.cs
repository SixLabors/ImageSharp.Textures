// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Core;

/// <summary>
/// Decoded block-mode metadata for a single 128-bit ASTC block. Populated by the block-mode
/// parser (produces an instance via <c>BlockModeDecoder.Decode</c>).
/// </summary>
internal readonly struct BlockInfo
{
    /// <summary>Every ASTC compressed block is exactly 128 bits (16 bytes) regardless of footprint (spec §C.2.4).</summary>
    public const int SizeInBytes = 16;

    /// <summary>
    /// Number of output channels per decoded pixel — RGBA in both the LDR (UNORM8) and HDR
    /// (float32) profiles. Used as a multiplier on <see cref="Footprint.PixelCount"/> to size
    /// scratch and image buffers.
    /// </summary>
    public const int ChannelsPerPixel = 4;

    public BlockInfo(
        bool isVoidExtent,
        bool isHdr,
        WeightGrid weights,
        int partitionCount,
        DualPlaneInfo dualPlane,
        ColorEndpoints colors,
        EndpointModeBuffer endpointModes)
    {
        this.IsValid = true;
        this.IsVoidExtent = isVoidExtent;
        this.IsHdr = isHdr;
        this.Weights = weights;
        this.PartitionCount = partitionCount;
        this.DualPlane = dualPlane;
        this.Colors = colors;
        this.EndpointModes = endpointModes;
    }

    private BlockInfo(bool isMalformedVoidExtent)
    {
        this.IsValid = false;
        this.IsVoidExtent = isMalformedVoidExtent;
    }

    /// <summary>
    /// Gets a malformed void-extent block (spec §C.2.23 — reserved bits or coordinates
    /// invalid). <see cref="IsVoidExtent"/> is true, all other properties are <c>default</c>.
    /// </summary>
    public static BlockInfo MalformedVoidExtent { get; } = new(isMalformedVoidExtent: true);

    /// <summary>
    /// Gets a value indicating whether the block is a legal ASTC encoding. False for reserved
    /// block modes and malformed void-extent blocks (ASTC spec §C.2.10, §C.2.23); both fast and
    /// general decode paths skip invalid blocks, leaving zeros in the output.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets a value indicating whether the block is a void-extent (single-colour) block, per
    /// ASTC spec §C.2.23.
    /// </summary>
    public bool IsVoidExtent { get; }

    /// <summary>
    /// Gets a value indicating whether this block encodes HDR content. For void-extent blocks
    /// this is the dynamic-range flag at bit 9 of the block mode (FP16 vs UNORM16, ASTC spec
    /// §C.2.23); for normal blocks it's true if any partition uses an HDR endpoint mode (spec
    /// §C.2.14: modes 2, 3, 7, 11, 14, 15). Used by the LDR decoder to reject HDR content
    /// before dispatch per §C.2.19.
    /// </summary>
    public bool IsHdr { get; }

    /// <summary>
    /// Gets the weight-grid metadata: dimensions, BISE range, and packed bit count
    /// (ASTC spec §C.2.10, §C.2.16).
    /// </summary>
    public WeightGrid Weights { get; }

    /// <summary>
    /// Gets the number of colour-endpoint partitions in the block (1..4, ASTC spec §C.2.10).
    /// Zero for void-extent blocks, which carry no partitions.
    /// </summary>
    public int PartitionCount { get; }

    /// <summary>
    /// Gets the dual-plane configuration: whether a second weight plane is present and which
    /// channel it drives (ASTC spec §C.2.20).
    /// </summary>
    public DualPlaneInfo DualPlane { get; }

    /// <summary>
    /// Gets the colour-endpoint bit region — start bit, bit count, BISE range, and value
    /// count (ASTC spec §C.2.22).
    /// </summary>
    public ColorEndpoints Colors { get; }

    /// <summary>
    /// Gets the per-partition colour endpoint modes (ASTC spec §C.2.11, §C.2.14). Only the
    /// first <see cref="PartitionCount"/> slots are populated; access via
    /// <see cref="GetEndpointMode"/> or <see cref="EndpointMode0"/>.
    /// </summary>
    public EndpointModeBuffer EndpointModes { get; }

    /// <summary>
    /// Gets the colour endpoint mode for partition 0 — the only partition for single-partition
    /// blocks, and a convenience accessor for the fused fast path.
    /// </summary>
    public ColorEndpointMode EndpointMode0 => this.EndpointModes[0];

    /// <summary>
    /// Gets a value indicating whether the block can take the fused fast path:
    /// single-partition, single-plane, non-void-extent (the common shape per ASTC spec
    /// §C.2.10, §C.2.20, §C.2.23). Multi-partition, dual-plane, and void-extent blocks fall
    /// through to the general logical-block pipeline.
    /// </summary>
    public bool IsFusable
        => !this.IsVoidExtent && this.PartitionCount == 1 && !this.DualPlane.Enabled;

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
    public ColorEndpointMode GetEndpointMode(int partition)
        => (uint)partition < (uint)this.PartitionCount
            ? this.EndpointModes[partition]
            : throw new ArgumentOutOfRangeException(nameof(partition), partition, $"Must be in [0, PartitionCount={this.PartitionCount}).");

    [InlineArray(4)]
    public struct EndpointModeBuffer
    {
#pragma warning disable CS0169, IDE0051, S1144 // Accessed by runtime via [InlineArray]
        private ColorEndpointMode element0;
#pragma warning restore CS0169, IDE0051, S1144
    }
}
