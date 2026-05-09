// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class AstcFileHeaderTests
{
    private static byte[] BuildHeader(
        byte blockWidth = 4,
        byte blockHeight = 4,
        byte blockDepth = 1,
        int imageWidth = 16,
        int imageHeight = 16,
        int imageDepth = 1)
    {
        byte[] data = new byte[AstcFileHeader.SizeInBytes];
        BinaryPrimitives.WriteUInt32LittleEndian(data, AstcFileHeader.Magic);
        data[4] = blockWidth;
        data[5] = blockHeight;
        data[6] = blockDepth;
        data[7] = (byte)(imageWidth & 0xFF);
        data[8] = (byte)((imageWidth >> 8) & 0xFF);
        data[9] = (byte)((imageWidth >> 16) & 0xFF);
        data[10] = (byte)(imageHeight & 0xFF);
        data[11] = (byte)((imageHeight >> 8) & 0xFF);
        data[12] = (byte)((imageHeight >> 16) & 0xFF);
        data[13] = (byte)(imageDepth & 0xFF);
        data[14] = (byte)((imageDepth >> 8) & 0xFF);
        data[15] = (byte)((imageDepth >> 16) & 0xFF);
        return data;
    }

    [Fact]
    public void FromMemory_WrongMagic_Throws()
    {
        byte[] data = BuildHeader();
        BinaryPrimitives.WriteUInt32LittleEndian(data, 0xDEADBEEF);

        Assert.Throws<ArgumentOutOfRangeException>(() => AstcFileHeader.FromMemory(data));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(15)]
    public void FromMemory_ShortBuffer_Throws(int length)
    {
        byte[] data = new byte[length];

        Assert.Throws<ArgumentOutOfRangeException>(() => AstcFileHeader.FromMemory(data));
    }

    [Theory]
    [InlineData(3, 3)]    // too small
    [InlineData(4, 3)]    // invalid combo
    [InlineData(7, 7)]    // not in the spec
    [InlineData(13, 13)]  // too big
    [InlineData(0, 4)]    // zero
    [InlineData(4, 0)]    // zero
    [InlineData(255, 255)] // garbage
    public void FromMemory_InvalidBlockDimensions_Throws(byte blockWidth, byte blockHeight)
    {
        byte[] data = BuildHeader(blockWidth: blockWidth, blockHeight: blockHeight);

        Assert.Throws<NotSupportedException>(() => AstcFileHeader.FromMemory(data));
    }

    [Theory]
    [InlineData(2)]  // 3D not supported
    [InlineData(4)]
    [InlineData(0)]  // depth must be at least 1
    public void FromMemory_BlockDepthOtherThan1_Throws(byte blockDepth)
    {
        byte[] data = BuildHeader(blockDepth: blockDepth);

        Assert.Throws<NotSupportedException>(() => AstcFileHeader.FromMemory(data));
    }

    [Theory]
    [InlineData(4, 4)]
    [InlineData(5, 4)]
    [InlineData(5, 5)]
    [InlineData(6, 5)]
    [InlineData(6, 6)]
    [InlineData(8, 5)]
    [InlineData(8, 6)]
    [InlineData(8, 8)]
    [InlineData(10, 5)]
    [InlineData(10, 6)]
    [InlineData(10, 8)]
    [InlineData(10, 10)]
    [InlineData(12, 10)]
    [InlineData(12, 12)]
    public void FromMemory_Valid2DFootprints_Succeed(byte blockWidth, byte blockHeight)
    {
        byte[] data = BuildHeader(blockWidth: blockWidth, blockHeight: blockHeight);

        AstcFileHeader header = AstcFileHeader.FromMemory(data);

        Assert.Equal(blockWidth, header.BlockWidth);
        Assert.Equal(blockHeight, header.BlockHeight);
    }

    [Fact]
    public void FromMemory_ImageDimensionsOverflow_Throws()
    {
        // 65536 * 65536 * 4 bytes per pixel > int.MaxValue
        byte[] data = BuildHeader(imageWidth: 65536, imageHeight: 65536);

        Assert.Throws<ArgumentOutOfRangeException>(() => AstcFileHeader.FromMemory(data));
    }

    [Theory]
    [InlineData(0, 16, 1)]
    [InlineData(16, 0, 1)]
    [InlineData(16, 16, 0)]
    public void FromMemory_ZeroDimensions_Throws(int width, int height, int depth)
    {
        byte[] data = BuildHeader(imageWidth: width, imageHeight: height, imageDepth: depth);

        Assert.Throws<ArgumentOutOfRangeException>(() => AstcFileHeader.FromMemory(data));
    }
}
