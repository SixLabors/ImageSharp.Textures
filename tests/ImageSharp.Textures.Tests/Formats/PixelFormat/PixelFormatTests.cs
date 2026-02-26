// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.PixelFormats;
using Rg16 = SixLabors.ImageSharp.Textures.PixelFormats.Rg16;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.PixelFormat;

[Trait("Category", "PixelFormats")]
public class PixelFormatTests
{
    [Fact]
    public void Test_Rg16()
    {
        string[] hexValues = ["FF", "FF00"];
        for (int i = 0; i < 2; i++)
        {
            int x = i == 0 ? 1 : 0;
            int y = i == 1 ? 1 : 0;

            Rg16 testPixel = new(x, y);

            Assert.Equal(string.Format(CultureInfo.InvariantCulture, "Rg16({0}, {1})", x, y), testPixel.ToString());

            Rgba32 destPixel = new(0);
            testPixel.ToRgba32(ref destPixel);

            Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X", CultureInfo.InvariantCulture));

            Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
            Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
            Assert.Equal(0, destPixel.B);
            Assert.Equal(255, destPixel.A);

            Vector4 vector4 = testPixel.ToVector4();
            testPixel.FromVector4(vector4);
            Assert.Equal(testPixel.ToVector4(), vector4);
        }
    }

    [Fact]
    public void Test_Bgr555()
    {
        string[] hexValues = ["7C00", "3E0", "1F"];
        for (int i = 0; i < 3; i++)
        {
            int x = i == 0 ? 1 : 0;
            int y = i == 1 ? 1 : 0;
            int z = i == 2 ? 1 : 0;

            Bgr555 testPixel = new(x, y, z);

            Rgba32 destPixel = new(0);
            testPixel.ToRgba32(ref destPixel);

            Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X", CultureInfo.InvariantCulture));

            Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
            Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
            Assert.Equal(i == 2 ? 255 : 0, destPixel.B);
            Assert.Equal(255, destPixel.A);

            Vector4 vector4 = testPixel.ToVector4();
            testPixel.FromVector4(vector4);
            Assert.Equal(testPixel.ToVector4(), vector4);
        }
    }

    [Fact]
    public void Test_Bgr32()
    {
        string[] hexValues = ["FF0000", "FF00", "FF"];
        for (int i = 0; i < 3; i++)
        {
            int x = i == 0 ? 1 : 0;
            int y = i == 1 ? 1 : 0;
            int z = i == 2 ? 1 : 0;

            Bgr32 testPixel = new(x, y, z);

            Rgba32 destPixel = new(0);
            testPixel.ToRgba32(ref destPixel);

            Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X", CultureInfo.InvariantCulture));

            Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
            Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
            Assert.Equal(i == 2 ? 255 : 0, destPixel.B);
            Assert.Equal(255, destPixel.A);

            Vector4 vector4 = testPixel.ToVector4();
            testPixel.FromVector4(vector4);
            Assert.Equal(testPixel.ToVector4(), vector4);
        }
    }

    [Fact]
    public void Test_Rgb32()
    {
        string[] hexValues = ["FF", "FF00", "FF0000"];
        for (int i = 0; i < 3; i++)
        {
            int x = i == 0 ? 1 : 0;
            int y = i == 1 ? 1 : 0;
            int z = i == 2 ? 1 : 0;

            Rgb32 testPixel = new(x, y, z);

            Rgba32 destPixel = new(0);
            testPixel.ToRgba32(ref destPixel);

            Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X", CultureInfo.InvariantCulture));

            Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
            Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
            Assert.Equal(i == 2 ? 255 : 0, destPixel.B);
            Assert.Equal(255, destPixel.A);

            Vector4 vector4 = testPixel.ToVector4();
            testPixel.FromVector4(vector4);
            Assert.Equal(testPixel.ToVector4(), vector4);
        }
    }
}
