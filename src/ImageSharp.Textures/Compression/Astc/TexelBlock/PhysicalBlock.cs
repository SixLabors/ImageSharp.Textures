// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.TexelBlock;

/// <summary>
/// A physical ASTC texel block (128 bits). Decoding is delegated to <see cref="BlockInfo"/>;
/// this type only exposes the block size constant and a thin wrapper over the raw bits for
/// callers that want structured access instead of carrying raw <see cref="UInt128"/> values.
/// </summary>
internal readonly struct PhysicalBlock
{
    public const int SizeInBytes = 16;
    private readonly BlockInfo info;

    private PhysicalBlock(UInt128 bits, BlockInfo info)
    {
        this.BlockBits = bits;
        this.info = info;
    }

    public UInt128 BlockBits { get; }

    public bool IsVoidExtent => this.info.IsVoidExtent;

    public bool IsIllegalEncoding => !this.info.IsValid;

    public static PhysicalBlock Create(UInt128 bits)
        => new(bits, BlockInfo.Decode(bits));

    public static PhysicalBlock Create(ulong low) => Create((UInt128)low);

    public static PhysicalBlock Create(ulong low, ulong high) => Create(new UInt128(high, low));
}
