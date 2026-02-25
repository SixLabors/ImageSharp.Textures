// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.


using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.IO;

/// <summary>
/// A simple bit stream used for reading/writing arbitrary-sized chunks.
/// </summary>
internal struct BitStream
{
    private ulong _low;
    private ulong _high;
    private uint _dataSize; // number of valid bits in the 128-bit buffer

    public uint Bits => _dataSize;

    public BitStream(ulong data = 0, uint dataSize = 0)
    {
        _low = data;
        _high = 0;
        _dataSize = dataSize;
    }

    public BitStream(UInt128 data, uint dataSize)
    {
        _low = data.Low();
        _high = data.High();
        _dataSize = dataSize;
    }

    public void PutBits<T>(T x, int size) where T : unmanaged
    {
        ulong value = x switch
        {
            uint ui => ui,
            ulong ul => ul,
            ushort us => us,
            byte b => b,
            _ => Convert.ToUInt64(x)
        };

        if (_dataSize + (uint)size > 128)
            throw new InvalidOperationException("Not enough space in BitStream");

        if (_dataSize < 64)
        {
            int lowFree = (int)(64 - _dataSize);
            if (size <= lowFree)
            {
                _low |= (value & MaskFor(size)) << (int)_dataSize;
            }
            else
            {
                _low |= (value & MaskFor(lowFree)) << (int)_dataSize;
                _high |= (value >> lowFree) & MaskFor(size - lowFree);
            }
        }
        else
        {
            int shift = (int)(_dataSize - 64);
            _high |= (value & MaskFor(size)) << shift;
        }

        _dataSize += (uint)size;
    }

    /// <summary>
    /// Attempt to retrieve the specified number of bits from the buffer.
    /// The buffer is shifted accordingly if successful.
    /// </summary>
    public bool TryGetBits<T>(int count, out T bits) where T : unmanaged
    {
        T? result = null;

        if (typeof(T) == typeof(UInt128))
        {
            result = (T?)(object?)GetBitsUInt128(count);
        }
        else if (count <= _dataSize)
        {
            ulong value = count switch
            {
                0 => 0,
                <= 64 => _low & MaskFor(count),
                _ => _low
            };

            ShiftBuffer(count);
            object boxed = Convert.ChangeType(value, typeof(T));
            result = (T)boxed;
        }

        bits = result ?? default;

        return result is not null;
    }

    public bool TryGetBits(int count, out ulong bits)
    {
        if (count > _dataSize) { bits = 0; return false; }
        bits = count switch
        {
            0 => 0,
            <= 64 => _low & MaskFor(count),
            _ => _low
        };
        ShiftBuffer(count);
        return true;
    }

    public bool TryGetBits(int count, out uint bits)
    {
        if (count > _dataSize) { bits = 0; return false; }
        bits = (uint)(count switch
        {
            0 => 0UL,
            <= 64 => _low & MaskFor(count),
            _ => _low
        });
        ShiftBuffer(count);
        return true;
    }

    private static ulong MaskFor(int bits)
        => bits == 64
            ? ~0UL
            : ((1UL << bits) - 1UL);

    private UInt128? GetBitsUInt128(int count)
    {
        if (count > _dataSize)
            return null;

        UInt128 result = count switch
        {
            0 => UInt128.Zero,
            <= 64 => (UInt128)(_low & MaskFor(count)),
            128 => new UInt128(_high, _low),
            _ => new UInt128(
                (count - 64 == 64) ? _high : (_high & MaskFor(count - 64)),
                _low)
        };

        ShiftBuffer(count);

        return result;
    }

    private void ShiftBuffer(int count)
    {
        if (count < 64)
        {
            _low = (_low >> count) | (_high << (64 - count));
            _high = _high >> count;
        }
        else if (count == 64)
        {
            _low = _high;
            _high = 0;
        }
        else
        {
            _low = _high >> (count - 64);
            _high = 0;
        }

        _dataSize -= (uint)count;
    }
}
