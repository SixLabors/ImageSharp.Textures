// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;

internal static class FileBasedHelpers
{
    private static readonly string AstcTestDataRoot = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc");

    public static string GetInputPath(string relativePath) => Path.Combine(AstcTestDataRoot, "Input", relativePath);

    public static string GetExpectedPath(string relativePath) => Path.Combine(AstcTestDataRoot, "Expected", relativePath);

    public static string GetHdrPath(string relativePath) => Path.Combine(AstcTestDataRoot, "HDR", relativePath);

    public static byte[] LoadASTCFile(string basename)
    {
        var filename = GetInputPath(basename + ".astc");
        File.Exists(filename).Should().BeTrue(because: $"Testdata missing: {filename}");
        var data = File.ReadAllBytes(filename);
        data.Length.Should().BeGreaterThanOrEqualTo(16, because: "ASTC file too small");
        return data.Skip(16).ToArray();
    }

    public static ImageBuffer LoadExpectedImage(string path)
    {
        const int BmpHeaderSize = 54;
        var data = File.ReadAllBytes(path);
        data.Length.Should().BeGreaterThanOrEqualTo(BmpHeaderSize);
        data[0].Should().Be((byte)'B');
        data[1].Should().Be((byte)'M');

        uint dataPos = BitConverter.ToUInt32(data, 0x0A);
        uint imageSize = BitConverter.ToUInt32(data, 0x22);
        ushort bitsPerPixel = BitConverter.ToUInt16(data, 0x1C);
        int width = BitConverter.ToInt32(data, 0x12);
        int height = BitConverter.ToInt32(data, 0x16);

        if (height < 0) height = -height;
        if (imageSize == 0) imageSize = (uint)(width * height * (bitsPerPixel / 8));
        if (dataPos < BmpHeaderSize) dataPos = BmpHeaderSize;

        (bitsPerPixel == 24 || bitsPerPixel == 32).Should().BeTrue(because: "BMP bits per pixel mismatch, expected 24 or 32");

        var result = ImageBuffer.Allocate(width, height, bitsPerPixel == 24 ? 3 : 4);
        imageSize.Should().BeLessThanOrEqualTo((uint)result.DataSize);

        var stride = result.Stride;

        for (int row = 0; row < height; ++row)
        {
            Array.Copy(data, (int)dataPos + row * stride, result.Data, row * stride, width * (bitsPerPixel / 8));
        }

        if (bitsPerPixel == 32)
        {
            for (int row = 0; row < height; ++row)
            {
                int rowOffset = row * stride;
                for (int i = 3; i < stride; i += 4)
                {
                    (result.Data[rowOffset + i - 1], result.Data[rowOffset + i - 3]) = (result.Data[rowOffset + i - 3], result.Data[rowOffset + i - 1]);
                }
            }
        }
        else
        {
            for (int row = 0; row < height; ++row)
            {
                int rowOffset = row * stride;
                for (int i = 2; i < stride; i += 3)
                {
                    (result.Data[rowOffset + i], result.Data[rowOffset + i - 2]) = (result.Data[rowOffset + i - 2], result.Data[rowOffset + i]);
                }
            }
        }

        return result;
    }
}
