// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.Uastc;

/// <summary>
/// LSB-first bit reader over a 16-byte UASTC block. Bits are consumed least-significant-bit
/// first within each byte, bytes in ascending order.
/// </summary>
internal ref struct UastcBitReader
{
    private readonly ReadOnlySpan<byte> bytes;
    private int bitOffset;

    public UastcBitReader(ReadOnlySpan<byte> block)
    {
        this.bytes = block;
        this.bitOffset = 0;
    }

    /// <summary>
    /// Gets the current bit position.
    /// </summary>
    public readonly int BitOffset => this.bitOffset;

    /// <summary>
    /// Reads a single bit.
    /// </summary>
    public int ReadBit()
    {
        int b = (this.bytes[this.bitOffset >> 3] >> (this.bitOffset & 7)) & 1;
        this.bitOffset++;
        return b;
    }

    /// <summary>
    /// Reads up to 32 bits LSB-first. Spans byte boundaries.
    /// </summary>
    public uint ReadBits(int count)
    {
        uint result = 0;
        int read = 0;
        while (read < count)
        {
            int byteBitOffset = this.bitOffset & 7;
            int bitsToRead = Math.Min(count - read, 8 - byteBitOffset);
            uint byteBits = (uint)(this.bytes[this.bitOffset >> 3] >> byteBitOffset) & ((1u << bitsToRead) - 1u);
            result |= byteBits << read;
            read += bitsToRead;
            this.bitOffset += bitsToRead;
        }

        return result;
    }

    /// <summary>
    /// Advances the read position by <paramref name="count"/> bits without returning them.
    /// </summary>
    public void Skip(int count) => this.bitOffset += count;
}
