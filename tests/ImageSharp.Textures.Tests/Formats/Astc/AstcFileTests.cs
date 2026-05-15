// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Compression.Astc.IO;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class AstcFileTests
{
    private const int BlockSize = 16;

    private static byte[] BuildFile(
        byte blockWidth = 4,
        byte blockHeight = 4,
        int imageWidth = 16,
        int imageHeight = 16,
        int payloadBlockCount = -1)
    {
        int blocksWide = (imageWidth + blockWidth - 1) / blockWidth;
        int blocksHigh = (imageHeight + blockHeight - 1) / blockHeight;
        int actualBlocks = payloadBlockCount < 0 ? blocksWide * blocksHigh : payloadBlockCount;

        byte[] data = new byte[AstcFileHeader.SizeInBytes + (actualBlocks * BlockSize)];
        BinaryPrimitives.WriteUInt32LittleEndian(data, AstcFileHeader.Magic);
        data[4] = blockWidth;
        data[5] = blockHeight;
        data[6] = 1;
        data[7] = (byte)(imageWidth & 0xFF);
        data[8] = (byte)((imageWidth >> 8) & 0xFF);
        data[9] = (byte)((imageWidth >> 16) & 0xFF);
        data[10] = (byte)(imageHeight & 0xFF);
        data[11] = (byte)((imageHeight >> 8) & 0xFF);
        data[12] = (byte)((imageHeight >> 16) & 0xFF);
        data[13] = 1;
        return data;
    }

    [Fact]
    public void FromMemory_NullData_Throws()
        => Assert.Throws<ArgumentNullException>(() => AstcFile.FromMemory(null));

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(15)]
    public void FromMemory_ShorterThanHeader_Throws(int length)
        => Assert.Throws<ArgumentOutOfRangeException>(() => AstcFile.FromMemory(new byte[length]));

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(15)]
    [InlineData(17)]
    [InlineData(31)]
    public void FromMemory_PayloadLengthNotMultipleOf16_Throws(int extraPayloadBytes)
    {
        byte[] data = BuildFile(imageWidth: 4, imageHeight: 4, payloadBlockCount: 1);
        byte[] padded = new byte[data.Length + extraPayloadBytes];
        data.CopyTo(padded, 0);

        Assert.Throws<ArgumentException>(() => AstcFile.FromMemory(padded));
    }

    [Theory]
    [InlineData(0)]   // zero blocks when 16 expected
    [InlineData(15)]  // less than expected
    [InlineData(17)]  // more than expected
    [InlineData(100)] // way more
    public void FromMemory_BlockCountMismatch_Throws(int payloadBlockCount)
    {
        // 16x16 at 4x4 footprint => 16 blocks expected
        byte[] data = BuildFile(imageWidth: 16, imageHeight: 16, payloadBlockCount: payloadBlockCount);

        Assert.Throws<ArgumentOutOfRangeException>(() => AstcFile.FromMemory(data));
    }

    [Fact]
    public void FromMemory_ValidFile_Succeeds()
    {
        byte[] data = BuildFile(imageWidth: 16, imageHeight: 16);

        AstcFile file = AstcFile.FromMemory(data);

        Assert.Equal(16, file.Width);
        Assert.Equal(16, file.Height);
        Assert.Equal(16 * BlockSize, file.Blocks.Length);
    }
}
