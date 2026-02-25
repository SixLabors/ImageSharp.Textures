// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Astc.Core;
using SixLabors.ImageSharp.Textures.Astc.TexelBlock;
using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;

internal class ImageBuffer
{
    public const int Align = 4;

    public byte[] Data { get; }
    public int Stride { get; }
    public int BytesPerPixel { get; }
    public int Width { get; }
    public int Height { get; }
    public int DataSize => Data.Length;

    public ImageBuffer(byte[] data, int width, int height, int bytesPerPixel)
    {
        Data = data;
        BytesPerPixel = bytesPerPixel;
        Width = width;
        Height = height;
        int rowBytes = width * bytesPerPixel;
        Stride = (rowBytes + (Align - 1)) / Align * Align;
    }

    public static ImageBuffer Allocate(int width, int height, int bytesPerPixel)
    {
        int rowBytes = width * bytesPerPixel;
        var stride = (rowBytes + (Align - 1)) / Align * Align;
        var data = new byte[stride * height];

        return new ImageBuffer(data, width, height, bytesPerPixel);
    }

    public static ImageBuffer FromAstcBuffer(Footprint footprint, byte[] astcData, int width, int height, bool hasAlpha)
    {
        var decodedImage = Allocate(width, height, hasAlpha ? RgbaColor.BytesPerPixel : RgbColor.BytesPerPixel);

        int blockWidth = footprint.Width;
        int blockHeight = footprint.Height;

        for (int i = 0; i < astcData.Length; i += PhysicalBlock.SizeInBytes)
        {
            int blockIndex = i / PhysicalBlock.SizeInBytes;
            int blocksWide = (width + blockWidth - 1) / blockWidth;
            int blockX = blockIndex % blocksWide;
            int blockY = blockIndex / blocksWide;

            var blockSpan = astcData.AsSpan(i, PhysicalBlock.SizeInBytes).ToArray();
            var bits = new UInt128(
                BitConverter.ToUInt64(blockSpan, 8),
                BitConverter.ToUInt64(blockSpan, 0));
            var info = BlockInfo.Decode(bits);

            var logicalBlock = LogicalBlock.UnpackLogicalBlock(footprint, bits, in info);
            logicalBlock.Should().NotBeNull();

            for (int y = 0; y < blockHeight; ++y)
            {
                for (int x = 0; x < blockWidth; ++x)
                {
                    int px = blockWidth * blockX + x;
                    int py = blockHeight * blockY + y;
                    if (px >= width || py >= height) continue;

                    var decoded = logicalBlock!.ColorAt(x, y);
                    int row = py * decodedImage.Stride;
                    int off = row + px * decodedImage.BytesPerPixel;
                    decodedImage.Data[off + 0] = decoded.R;
                    decodedImage.Data[off + 1] = decoded.G;
                    decodedImage.Data[off + 2] = decoded.B;
                    if (hasAlpha) decodedImage.Data[off + 3] = decoded.A;
                }
            }
        }

        return decodedImage;
    }
}
