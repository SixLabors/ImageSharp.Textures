// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture format for the VK_FORMAT_E5B9G9R9_UFLOAT_PACK32 packed pixel type.
/// Each pixel is a 32-bit unsigned integer containing three 9-bit mantissas
/// (R bits 0-8, G bits 9-17, B bits 18-26) and a 5-bit shared exponent (bits 27-31).
/// </summary>
internal readonly struct Rgbe9995PackedFloat : IBlock<Rgbe9995PackedFloat>
{
    /// <inheritdoc/>
    public int BitsPerPixel => 32;

    /// <inheritdoc/>
    public byte PixelDepthBytes => 4;

    /// <inheritdoc/>
    public byte DivSize => 1;

    /// <inheritdoc/>
    public byte CompressedBytesPerBlock => 4;

    /// <inheritdoc/>
    public bool Compressed => false;

    /// <inheritdoc/>
    public Image GetImage(byte[] blockData, int width, int height)
    {
        byte[] decompressedData = this.Decompress(blockData, width, height);
        return Image.LoadPixelData<Textures.PixelFormats.Rgba128Float>(decompressedData, width, height);
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] blockData, int width, int height)
    {
        int pixelCount = width * height;
        byte[] output = new byte[pixelCount * 16];
        Span<byte> inputSpan = blockData.AsSpan();
        Span<byte> outputSpan = output.AsSpan();

        for (int i = 0; i < pixelCount; i++)
        {
            uint packed = BinaryPrimitives.ReadUInt32LittleEndian(inputSpan[(i * 4)..]);

            uint rm = packed & 0x1FFu;
            uint gm = (packed >> 9) & 0x1FFu;
            uint bm = (packed >> 18) & 0x1FFu;
            uint exponent = (packed >> 27) & 0x1Fu;

            // Construct 2^(exponent - 24) exactly via IEEE bit pattern.
            // exponent range [0,31] maps to IEEE exponent [103,134].
            float scale = BitConverter.UInt32BitsToSingle((exponent + 103u) << 23);

            float r = rm * scale;
            float g = gm * scale;
            float b = bm * scale;

            int outOffset = i * 16;
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[outOffset..], r);
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[(outOffset + 4)..], g);
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[(outOffset + 8)..], b);
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[(outOffset + 12)..], 1.0f);
        }

        return output;
    }
}
