// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    using System;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Textures.Formats.Dds;

    internal class DdsDxt5 : DdsCompressed
    {
        private const byte PIXEL_DEPTH = 4;
        private const byte DIV_SIZE = 4;

        private readonly byte[] alpha = new byte[8];
        private readonly Rgb24[] colors = new Rgb24[4];

        public override int BitsPerPixel => 8 * PIXEL_DEPTH;
        public override ImageFormat Format => ImageFormat.Rgba32;
        protected override byte DivSize => DIV_SIZE;
        protected override byte CompressedBytesPerBlock => 16;

        public DdsDxt5(DdsHeader ddsHeader, DdsHeaderDxt10 ddsHeaderDxt10)
            : base(ddsHeader, ddsHeaderDxt10)
        {
        }

        protected override byte PixelDepthBytes => PIXEL_DEPTH;

        protected override int Decode(Span<byte> stream, Span<byte> data, int streamIndex, int dataIndex, int stride)
        {
            streamIndex = DdsBc5.ExtractGradient(alpha, stream, streamIndex);

            ulong alphaCodes = stream[streamIndex++];
            alphaCodes |= ((ulong)stream[streamIndex++] << 8);
            alphaCodes |= ((ulong)stream[streamIndex++] << 16);
            alphaCodes |= ((ulong)stream[streamIndex++] << 24);
            alphaCodes |= ((ulong)stream[streamIndex++] << 32);
            alphaCodes |= ((ulong)stream[streamIndex++] << 40);

            // Colors are stored in a pair of 16 bits
            ushort color0 = stream[streamIndex++];
            color0 |= (ushort)(stream[streamIndex++] << 8);

            ushort color1 = (stream[streamIndex++]);
            color1 |= (ushort)(stream[streamIndex++] << 8);

            // Extract R5G6B5 (in that order)
            colors[0].R = (byte)((color0 & 0x1f));
            colors[0].G = (byte)((color0 & 0x7E0) >> 5);
            colors[0].B = (byte)((color0 & 0xF800) >> 11);
            colors[0].R = (byte)(colors[0].R << 3 | colors[0].R >> 2);
            colors[0].G = (byte)(colors[0].G << 2 | colors[0].G >> 3);
            colors[0].B = (byte)(colors[0].B << 3 | colors[0].B >> 2);

            colors[1].R = (byte)((color1 & 0x1f));
            colors[1].G = (byte)((color1 & 0x7E0) >> 5);
            colors[1].B = (byte)((color1 & 0xF800) >> 11);
            colors[1].R = (byte)(colors[1].R << 3 | colors[1].R >> 2);
            colors[1].G = (byte)(colors[1].G << 2 | colors[1].G >> 3);
            colors[1].B = (byte)(colors[1].B << 3 | colors[1].B >> 2);

            colors[2].R = (byte)((2 * colors[0].R + colors[1].R) / 3);
            colors[2].G = (byte)((2 * colors[0].G + colors[1].G) / 3);
            colors[2].B = (byte)((2 * colors[0].B + colors[1].B) / 3);

            colors[3].R = (byte)((colors[0].R + 2 * colors[1].R) / 3);
            colors[3].G = (byte)((colors[0].G + 2 * colors[1].G) / 3);
            colors[3].B = (byte)((colors[0].B + 2 * colors[1].B) / 3);

            for (int alphaShift = 0; alphaShift < 48; alphaShift += 12)
            {
                byte rowVal = stream[streamIndex++];
                for (int j = 0; j < 4; j++)
                {
                    // 3 bits determine alpha index to use
                    byte alphaIndex = (byte)((alphaCodes >> (alphaShift + 3 * j)) & 0x07);
                    var col = colors[((rowVal >> (j * 2)) & 0x03)];
                    data[dataIndex++] = col.R;
                    data[dataIndex++] = col.G;
                    data[dataIndex++] = col.B;
                    data[dataIndex++] = alpha[alphaIndex];
                }
                dataIndex += PIXEL_DEPTH * (stride - DIV_SIZE);
            }
            return streamIndex;
        }
    }
}
