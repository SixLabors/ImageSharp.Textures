// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.Uastc;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Uastc;

[Trait("Format", "Astc")]
public class UastcBitReaderTests
{
    [Fact]
    public void ReadBits_IsLsbFirstWithinAndAcrossBytes()
    {
        // Set the first two bytes, the rest is just zero padding
        byte[] data = [0xA5, 0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        UastcBitReader reader = new(data);

        // Reads start at the LSB, so this takes byte0's low nibble (bits 0-3 = 0101), not the high one.
        Assert.Equal(5u, reader.ReadBits(4));

        // Then byte0's remaining high nibble (bits 4-7 = 1010); byte0 is now exhausted.
        Assert.Equal(10u, reader.ReadBits(4));

        // Rolls into byte1 at its LSB (low bits 11), showing a read spans the byte boundary.
        Assert.Equal(3u, reader.ReadBits(2));

        Assert.Equal(10, reader.BitOffset);
    }

    [Fact]
    public void ReadBit_ReturnsBitsInLsbOrder()
    {
        byte[] data = [0b0000_0110, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        UastcBitReader reader = new(data);

        Assert.Equal(0, reader.ReadBit()); // bit 0
        Assert.Equal(1, reader.ReadBit()); // bit 1
        Assert.Equal(1, reader.ReadBit()); // bit 2
        Assert.Equal(0, reader.ReadBit()); // bit 3
    }

    [Fact]
    public void ReadBits_SpanningThreeBytes()
    {
        byte[] data = [0xFF, 0x00, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        UastcBitReader reader = new(data);

        // One 24-bit read spans all three bytes, earliest byte in the low bits
        Assert.Equal(0xFF00FFu, reader.ReadBits(24));
    }

    [Fact]
    public void Skip_AdvancesOffset()
    {
        byte[] data = new byte[16];
        data[1] = 0xFF; // bits 8..15
        UastcBitReader reader = new(data);

        reader.Skip(8);
        Assert.Equal(8, reader.BitOffset);
        Assert.Equal(0xFFu, reader.ReadBits(8));
    }
}
