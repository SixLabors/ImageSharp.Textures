// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Formats.Ktx2.Enums;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

public class Ktx2ProcessorTests
{
    private static LevelIndex MakeLevelIndex(ulong byteOffset, ulong byteLength, ulong uncompressedByteLength)
    {
        byte[] buffer = new byte[24];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(0, 8), byteOffset);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(8, 8), byteLength);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(16, 8), uncompressedByteLength);
        return MemoryMarshal.Cast<byte, LevelIndex>(buffer)[0];
    }

    private static Ktx2Header MakeCubemapHeader()
        => new(
            vkFormat: VkFormat.VK_FORMAT_R8G8B8A8_UNORM,
            typeSize: 1,
            pixelWidth: 4,
            pixelHeight: 4,
            pixelDepth: 0,
            layerCount: 0,
            faceCount: 6,
            levelCount: 1,
            supercompressionScheme: 0,
            dfdByteOffset: 0,
            dfdByteLength: 0,
            kvdByteOffset: 0,
            kvdByteLength: 0,
            sgdByteOffset: 0,
            sgdByteLength: 0);

    [Theory]
    [InlineData(1UL)]
    [InlineData(5UL)]
    [InlineData(7UL)]
    [InlineData(100UL)]
    public void DecodeCubeMap_LevelByteLengthNotDivisibleBy6_Throws(ulong uncompressedByteLength)
    {
        var processor = new Ktx2Processor(MakeCubemapHeader());
        LevelIndex[] levelIndices = [MakeLevelIndex(0, uncompressedByteLength, uncompressedByteLength)];

        using var stream = new MemoryStream(new byte[(int)uncompressedByteLength]);

        Assert.Throws<TextureFormatException>(() => processor.DecodeCubeMap(stream, 4, 4, levelIndices));
    }

    [Fact]
    public void DecodeCubeMap_TruncatedStream_Throws()
    {
        // 6 faces of 64 bytes each = 384 bytes expected, but stream is shorter.
        var processor = new Ktx2Processor(MakeCubemapHeader());
        LevelIndex[] levelIndices = [MakeLevelIndex(0, 384, 384)];

        using var stream = new MemoryStream(new byte[100]);

        Assert.Throws<TextureFormatException>(() => processor.DecodeCubeMap(stream, 4, 4, levelIndices));
    }

    [Fact]
    public void DecodeMipMaps_TruncatedStream_Throws()
    {
        var header = new Ktx2Header(
            vkFormat: VkFormat.VK_FORMAT_R8G8B8A8_UNORM,
            typeSize: 1,
            pixelWidth: 4,
            pixelHeight: 4,
            pixelDepth: 0,
            layerCount: 0,
            faceCount: 1,
            levelCount: 1,
            supercompressionScheme: 0,
            dfdByteOffset: 0,
            dfdByteLength: 0,
            kvdByteOffset: 0,
            kvdByteLength: 0,
            sgdByteOffset: 0,
            sgdByteLength: 0);
        var processor = new Ktx2Processor(header);
        LevelIndex[] levelIndices = [MakeLevelIndex(0, 64, 64)];

        using var stream = new MemoryStream(new byte[10]);

        Assert.Throws<TextureFormatException>(() => processor.DecodeMipMaps(stream, 4, 4, levelIndices));
    }
}
