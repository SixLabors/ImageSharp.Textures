using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Textures.PixelFormats;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using Xunit;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.PixelFormat
{
    public class PixelFormatTests
    {
        [Fact]
        public void Test_Rg16()
        {
            string[] hexValues = new[] { "FF", "FF00" };
            for (int i = 0; i < 2; i++)
            {
                int x = i == 0 ? 1 : 0;
                int y = i == 1 ? 1 : 0;

                var testPixel = new Rg16(x, y);

                Assert.Equal($"Rg16({x}, {y})", testPixel.ToString());

                var destPixel = new ImageSharp.PixelFormats.Rgba32(0);
                testPixel.ToRgba32(ref destPixel);

                Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X"));

                Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
                Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
                Assert.Equal(0, destPixel.B);
                Assert.Equal(255, destPixel.A);

                var vector4 = testPixel.ToVector4();
                testPixel.FromVector4(vector4);
                Assert.Equal(testPixel.ToVector4(), vector4);
            }
        }

        [Fact]
        public void Test_Bgr555()
        {
            string[] hexValues = new[] { "7C00", "3E0", "1F" };
            for (int i = 0; i < 3; i++)
            {
                int x = i == 0 ? 1 : 0;
                int y = i == 1 ? 1 : 0;
                int z = i == 2 ? 1 : 0;

                var testPixel = new Bgr555(x, y, z);

                Assert.Equal($"Bgr555({z}, {y}, {x})", testPixel.ToString());

                var destPixel = new ImageSharp.PixelFormats.Rgba32(0);
                testPixel.ToRgba32(ref destPixel);

                Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X"));

                Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
                Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
                Assert.Equal(i == 2 ? 255 : 0, destPixel.B);
                Assert.Equal(255, destPixel.A);

                var vector4 = testPixel.ToVector4();
                testPixel.FromVector4(vector4);
                Assert.Equal(testPixel.ToVector4(), vector4);
            }
        }

        [Fact]
        public void Test_Bgr32()
        {
            string[] hexValues = new[] { "FF0000", "FF00", "FF" };
            for (int i = 0; i < 3; i++)
            {
                int x = i == 0 ? 1 : 0;
                int y = i == 1 ? 1 : 0;
                int z = i == 2 ? 1 : 0;

                var testPixel = new Bgr32(x, y, z);

                Assert.Equal($"Bgr32({z}, {y}, {x})", testPixel.ToString());

                var destPixel = new ImageSharp.PixelFormats.Rgba32(0);
                testPixel.ToRgba32(ref destPixel);

                Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X"));

                Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
                Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
                Assert.Equal(i == 2 ? 255 : 0, destPixel.B);
                Assert.Equal(255, destPixel.A);

                var vector4 = testPixel.ToVector4();
                testPixel.FromVector4(vector4);
                Assert.Equal(testPixel.ToVector4(), vector4);
            }
        }

        [Fact]
        public void Test_Rgb32()
        {
            string[] hexValues = new[] { "FF", "FF00", "FF0000" };
            for (int i = 0; i < 3; i++)
            {
                int x = i == 0 ? 1 : 0;
                int y = i == 1 ? 1 : 0;
                int z = i == 2 ? 1 : 0;

                var testPixel = new Rgb32(x, y, z);

                Assert.Equal($"Rgb32({x}, {y}, {z})", testPixel.ToString());

                var destPixel = new ImageSharp.PixelFormats.Rgba32(0);
                testPixel.ToRgba32(ref destPixel);

                Assert.Equal(hexValues[i], testPixel.PackedValue.ToString("X"));

                Assert.Equal(i == 0 ? 255 : 0, destPixel.R);
                Assert.Equal(i == 1 ? 255 : 0, destPixel.G);
                Assert.Equal(i == 2 ? 255 : 0, destPixel.B);
                Assert.Equal(255, destPixel.A);

                var vector4 = testPixel.ToVector4();
                testPixel.FromVector4(vector4);
                Assert.Equal(testPixel.ToVector4(), vector4);
            }
        }
    }
}
