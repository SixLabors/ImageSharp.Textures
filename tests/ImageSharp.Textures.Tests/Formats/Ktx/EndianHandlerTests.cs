// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

public class EndianHandlerTests
{
    // Force the "swapping" path regardless of host endianness: claim the file disagrees with the host.
    private static EndianHandler Swapping() => new(isFileLittleEndian: !BitConverter.IsLittleEndian);

    // Force the "native" (no-swap) path: claim the file matches the host.
    private static EndianHandler Native() => new(isFileLittleEndian: BitConverter.IsLittleEndian);

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void ConvertPixelData_Swapping_TypeSize2_UnalignedLength_Throws(int dataLength)
    {
        byte[] data = new byte[dataLength];

        Assert.Throws<TextureFormatException>(() => Swapping().ConvertPixelData(data, 2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void ConvertPixelData_Swapping_TypeSize4_UnalignedLength_Throws(int dataLength)
    {
        byte[] data = new byte[dataLength];

        Assert.Throws<TextureFormatException>(() => Swapping().ConvertPixelData(data, 4));
    }

    [Fact]
    public void ConvertPixelData_Swapping_TypeSize2_SwapsBytes()
    {
        byte[] data = [0x12, 0x34, 0xAB, 0xCD];

        Swapping().ConvertPixelData(data, 2);

        Assert.Equal([0x34, 0x12, 0xCD, 0xAB], data);
    }

    [Fact]
    public void ConvertPixelData_Swapping_TypeSize4_SwapsBytes()
    {
        byte[] data = [0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01];

        Swapping().ConvertPixelData(data, 4);

        Assert.Equal([0x78, 0x56, 0x34, 0x12, 0x01, 0xEF, 0xCD, 0xAB], data);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(3u)]
    [InlineData(8u)]
    public void ConvertPixelData_Swapping_UnsupportedTypeSize_IsNoOp(uint typeSize)
    {
        byte[] data = [0x12, 0x34, 0x56, 0x78];
        byte[] original = [.. data];

        Swapping().ConvertPixelData(data, typeSize);

        Assert.Equal(original, data);
    }

    [Theory]
    [InlineData(2u)]
    [InlineData(4u)]
    public void ConvertPixelData_Native_IsAlwaysNoOp(uint typeSize)
    {
        byte[] data = [0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01];
        byte[] original = [.. data];

        Native().ConvertPixelData(data, typeSize);

        Assert.Equal(original, data);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void ConvertPixelData_Native_UnalignedLength_IsNoOp(int dataLength)
    {
        // Native path skips the length check entirely since no swap happens.
        byte[] data = new byte[dataLength];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(0x10 + i);
        }

        byte[] original = [.. data];

        Native().ConvertPixelData(data, 2);

        Assert.Equal(original, data);
    }
}
