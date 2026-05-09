// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

public class SwappingEndianHandlerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void ConvertPixelData_TypeSize2_UnalignedLength_Throws(int dataLength)
    {
        var handler = new SwappingEndianHandler(isLittleEndian: true);
        byte[] data = new byte[dataLength];

        Assert.Throws<TextureFormatException>(() => handler.ConvertPixelData(data, 2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void ConvertPixelData_TypeSize4_UnalignedLength_Throws(int dataLength)
    {
        var handler = new SwappingEndianHandler(isLittleEndian: true);
        byte[] data = new byte[dataLength];

        Assert.Throws<TextureFormatException>(() => handler.ConvertPixelData(data, 4));
    }

    [Fact]
    public void ConvertPixelData_TypeSize2_SwapsBytes()
    {
        var handler = new SwappingEndianHandler(isLittleEndian: true);
        byte[] data = [0x12, 0x34, 0xAB, 0xCD];

        handler.ConvertPixelData(data, 2);

        Assert.Equal([0x34, 0x12, 0xCD, 0xAB], data);
    }

    [Fact]
    public void ConvertPixelData_TypeSize4_SwapsBytes()
    {
        var handler = new SwappingEndianHandler(isLittleEndian: true);
        byte[] data = [0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01];

        handler.ConvertPixelData(data, 4);

        Assert.Equal([0x78, 0x56, 0x34, 0x12, 0x01, 0xEF, 0xCD, 0xAB], data);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(3u)]
    [InlineData(8u)]
    public void ConvertPixelData_UnsupportedTypeSize_IsNoOp(uint typeSize)
    {
        var handler = new SwappingEndianHandler(isLittleEndian: true);
        byte[] data = [0x12, 0x34, 0x56, 0x78];
        byte[] original = [.. data];

        handler.ConvertPixelData(data, typeSize);

        Assert.Equal(original, data);
    }
}
