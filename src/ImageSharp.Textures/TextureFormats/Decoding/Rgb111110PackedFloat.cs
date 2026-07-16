// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers.Binary;
using SixLabors.ImageSharp.Textures.Common.Helpers;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

/// <summary>
/// Texture format for the VK_FORMAT_B10G11R11_UFLOAT_PACK32 packed pixel type.
/// Each pixel is a 32-bit unsigned integer containing three unsigned float channels:
/// R (11-bit, bits 0-10), G (11-bit, bits 11-21), B (10-bit, bits 22-31).
/// </summary>
internal readonly struct Rgb111110PackedFloat : IBlock<Rgb111110PackedFloat>
{
    private const uint VulkanExponentBias = 15;

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

            float r = FloatHelper.UnpackFloat11ToFloat(packed & 0x7FFu, VulkanExponentBias);
            float g = FloatHelper.UnpackFloat11ToFloat((packed >> 11) & 0x7FFu, VulkanExponentBias);
            float b = FloatHelper.UnpackFloat10ToFloat((packed >> 22) & 0x3FFu, VulkanExponentBias);

            int outOffset = i * 16;
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[outOffset..], r);
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[(outOffset + 4)..], g);
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[(outOffset + 8)..], b);
            BinaryPrimitives.WriteSingleLittleEndian(outputSpan[(outOffset + 12)..], 1.0f);
        }

        return output;
    }
}
