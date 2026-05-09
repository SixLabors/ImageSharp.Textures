// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.IO;

/// <summary>
/// A simple bit stream used for reading/writing arbitrary-sized chunks.
/// </summary>
internal struct BitStream
{
    private ulong low;
    private ulong high;
    private uint dataSize; // number of valid bits in the 128-bit buffer

    public BitStream(ulong data = 0, uint dataSize = 0)
    {
        this.low = data;
        this.high = 0;
        this.dataSize = dataSize;
    }

    public BitStream(UInt128 data, uint dataSize)
    {
        this.low = data.Low();
        this.high = data.High();
        this.dataSize = dataSize;
    }

    public readonly uint Bits => this.dataSize;

    public void PutBits(ulong value, int size)
    {
        if (this.dataSize + (uint)size > 128)
        {
            throw new InvalidOperationException("Not enough space in BitStream");
        }

        if (this.dataSize < 64)
        {
            int lowFree = (int)(64 - this.dataSize);
            if (size <= lowFree)
            {
                this.low |= (value & MaskFor(size)) << (int)this.dataSize;
            }
            else
            {
                this.low |= (value & MaskFor(lowFree)) << (int)this.dataSize;
                this.high |= (value >> lowFree) & MaskFor(size - lowFree);
            }
        }
        else
        {
            int shift = (int)(this.dataSize - 64);
            this.high |= (value & MaskFor(size)) << shift;
        }

        this.dataSize += (uint)size;
    }

    /// <summary>
    /// Attempt to retrieve the specified number of bits from the buffer as a <see cref="UInt128"/>.
    /// The buffer is shifted accordingly if successful.
    /// </summary>
    public bool TryGetBits(int count, out UInt128 bits)
    {
        UInt128? result = this.GetBitsUInt128(count);
        bits = result ?? default;
        return result is not null;
    }

    public bool TryGetBits(int count, out ulong bits)
    {
        if (count > this.dataSize)
        {
            bits = 0;
            return false;
        }

        bits = count switch
        {
            0 => 0,
            <= 64 => this.low & MaskFor(count),
            _ => this.low
        };
        this.ShiftBuffer(count);
        return true;
    }

    public bool TryGetBits(int count, out uint bits)
    {
        if (count > this.dataSize)
        {
            bits = 0;
            return false;
        }

        bits = (uint)(count switch
        {
            0 => 0UL,
            <= 64 => this.low & MaskFor(count),
            _ => this.low
        });
        this.ShiftBuffer(count);
        return true;
    }

    private static ulong MaskFor(int bits)
        => bits == 64
            ? ~0UL
            : ((1UL << bits) - 1UL);

    private UInt128? GetBitsUInt128(int count)
    {
        if (count > this.dataSize)
        {
            return null;
        }

        UInt128 result = count switch
        {
            0 => UInt128.Zero,
            <= 64 => (UInt128)(this.low & MaskFor(count)),
            128 => new UInt128(this.high, this.low),
            _ => new UInt128(
                (count - 64 == 64) ? this.high : (this.high & MaskFor(count - 64)),
                this.low)
        };

        this.ShiftBuffer(count);

        return result;
    }

    private void ShiftBuffer(int count)
    {
        // C# masks shift amounts to the width of the operand, so `ulong << 64` and `ulong >> 64`
        // are identity, not zero. Special-case count == 0 and count >= 128 to avoid polluting
        // the low/high halves on boundary shifts.
        if (count == 0)
        {
            // Reading zero bits is a no-op.
        }
        else if (count < 64)
        {
            this.low = (this.low >> count) | (this.high << (64 - count));
            this.high >>= count;
        }
        else if (count == 64)
        {
            this.low = this.high;
            this.high = 0;
        }
        else if (count < 128)
        {
            this.low = this.high >> (count - 64);
            this.high = 0;
        }
        else
        {
            this.low = 0;
            this.high = 0;
        }

        this.dataSize -= (uint)count;
    }
}
