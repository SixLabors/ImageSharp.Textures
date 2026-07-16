// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

/// <summary>
/// One-shot generator for KTX1 ASTC test fixtures, derived from the KTX2 valid-ASTC samples.
/// The ASTC block payload is byte-identical between KTX1 and KTX2; only the container header
/// and the per-mip <c>imageSize</c> prefix differ. This generator parses each KTX2 file,
/// extracts the level-0 block stream, and emits a KTX1 wrapper with the matching
/// <c>glInternalFormat</c>.
///
/// Skipped by default — flip <see cref="Enabled"/> to <c>true</c> and run
/// <c>dotnet test --filter Ktx1AstcTestImageGenerator</c> to (re)generate the fixtures.
/// </summary>
public class Ktx1AstcTestImageGenerator
{
    private const bool Enabled = false;

    // KTX1 magic identifier: «KTX 11»\r\n\x1A\n
    private static readonly byte[] Ktx1Identifier =
    [
        0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB,
        0x0D, 0x0A, 0x1A, 0x0A,
    ];

    // GL_RGBA — the base internal format for all ASTC RGBA variants.
    private const uint GlRgba = 0x1908;

    // GL_COMPRESSED_RGBA_ASTC_<size>_KHR — base value at 4x4 = 0x93B0; subsequent footprints
    // increment by 1 in the order: 4x4, 5x4, 5x5, 6x5, 6x6, 8x5, 8x6, 8x8, 10x5, 10x6, 10x8,
    // 10x10, 12x10, 12x12.
    private const uint GlAstc4x4Rgba = 0x93B0;

    // GL_COMPRESSED_SRGB8_ALPHA8_ASTC_<size>_KHR — same ordering, base at 4x4 = 0x93D0.
    private const uint GlAstc4x4Srgb = 0x93D0;

    private static readonly (string Name, int W, int H)[] Footprints =
    [
        ("4x4", 4, 4), ("5x4", 5, 4), ("5x5", 5, 5), ("6x5", 6, 5), ("6x6", 6, 6),
        ("8x5", 8, 5), ("8x6", 8, 6), ("8x8", 8, 8),
        ("10x5", 10, 5), ("10x6", 10, 6), ("10x8", 10, 8), ("10x10", 10, 10),
        ("12x10", 12, 10), ("12x12", 12, 12),
    ];

    [Fact]
    public void Generate_All_Ktx1_Astc_Fixtures()
    {
        if (!Enabled)
        {
            return;
        }

        string ktx2Dir = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Ktx2", "Flat", "Astc");
        string ktx1Dir = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Ktx", "Flat", "Astc");
        Directory.CreateDirectory(ktx1Dir);

        for (int i = 0; i < Footprints.Length; i++)
        {
            (string name, _, _) = Footprints[i];
            uint srgbFormat = GlAstc4x4Srgb + (uint)i;
            uint unormFormat = GlAstc4x4Rgba + (uint)i;

            ConvertKtx2ToKtx1(
                Path.Combine(ktx2Dir, $"rgba32-srgb-{name}-valid.ktx2"),
                Path.Combine(ktx1Dir, $"rgba32-srgb-{name}-valid.ktx"),
                srgbFormat);

            ConvertKtx2ToKtx1(
                Path.Combine(ktx2Dir, $"rgba32-unorm-{name}-valid.ktx2"),
                Path.Combine(ktx1Dir, $"rgba32-unorm-{name}-valid.ktx"),
                unormFormat);
        }
    }

    private static void ConvertKtx2ToKtx1(string ktx2Path, string ktx1Path, uint glInternalFormat)
    {
        // KTX2 header layout used here:
        //   [12]    identifier
        //   [12-15] vkFormat (unused for the conversion, but caller chose glInternalFormat)
        //   [20-23] pixelWidth
        //   [24-27] pixelHeight
        //   [40-43] levelCount
        //   [80-...] level index table: per level (uint64 byteOffset, uint64 byteLength, uint64 uncompressedByteLength)
        byte[] ktx2 = File.ReadAllBytes(ktx2Path);
        int pixelWidth = (int)BinaryPrimitives.ReadUInt32LittleEndian(ktx2.AsSpan(20, 4));
        int pixelHeight = (int)BinaryPrimitives.ReadUInt32LittleEndian(ktx2.AsSpan(24, 4));
        uint levelCount = BinaryPrimitives.ReadUInt32LittleEndian(ktx2.AsSpan(40, 4));
        if (levelCount == 0)
        {
            levelCount = 1;
        }

        // Level 0 starts at byte 80; each entry is 24 bytes.
        ulong byteOffset = BinaryPrimitives.ReadUInt64LittleEndian(ktx2.AsSpan(80, 8));
        ulong byteLength = BinaryPrimitives.ReadUInt64LittleEndian(ktx2.AsSpan(88, 8));
        byte[] blockStream = ktx2.AsSpan((int)byteOffset, (int)byteLength).ToArray();

        // KTX1 header (64 bytes) + per-mip imageSize prefix (4 bytes) + blocks.
        using FileStream output = File.Create(ktx1Path);
        output.Write(Ktx1Identifier);
        WriteUInt32(output, 0x04030201);    // endianness (little-endian)
        WriteUInt32(output, 0);              // glType (0 for compressed)
        WriteUInt32(output, 1);              // glTypeSize (1 for compressed)
        WriteUInt32(output, 0);              // glFormat (0 for compressed)
        WriteUInt32(output, glInternalFormat);
        WriteUInt32(output, GlRgba);         // glBaseInternalFormat
        WriteUInt32(output, (uint)pixelWidth);
        WriteUInt32(output, (uint)pixelHeight);
        WriteUInt32(output, 0);              // pixelDepth
        WriteUInt32(output, 0);              // numberOfArrayElements
        WriteUInt32(output, 1);              // numberOfFaces
        WriteUInt32(output, 1);              // numberOfMipmapLevels (only emitting level 0)
        WriteUInt32(output, 0);              // bytesOfKeyValueData

        // Per-mip imageSize prefix, then the block data.
        WriteUInt32(output, (uint)blockStream.Length);
        output.Write(blockStream);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        stream.Write(buffer);
    }
}
