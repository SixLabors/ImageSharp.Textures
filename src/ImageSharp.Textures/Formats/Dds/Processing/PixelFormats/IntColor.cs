// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats
{
    internal class IntColor
    {
        public IntColor()
        {
        }

        public IntColor(int nr, int ng, int nb)
        {
            this.R = nr;
            this.G = ng;
            this.B = nb;
            this.Pad = 0;
        }

        public int R { get; set; }

        public int G { get; set; }

        public int B { get; set; }

        public int Pad { get; }

        public static IntColor operator +(IntColor a, IntColor c)
        {
            a.R += c.R;
            a.G += c.G;
            a.B += c.B;
            return a;
        }

        public static IntColor operator &(IntColor a, IntColor c)
        {
            a.R &= c.R;
            a.G &= c.G;
            a.B &= c.B;
            return a;
        }

        public IntColor SignExtend(LdrColorA prec)
        {
            this.R = SIGN_EXTEND(this.R, prec.R);
            this.G = SIGN_EXTEND(this.G, prec.G);
            this.B = SIGN_EXTEND(this.B, prec.B);
            return this;
        }

        private static int SIGN_EXTEND(int x, int nb)
        {
            return ((x & 1 << (nb - 1)) != 0 ? ~0 ^ (1 << nb) - 1 : 0) | x;
        }

        public void ToF16Signed(ushort[] aF16)
        {
            aF16[0] = Int2F16Signed(this.R);
            aF16[1] = Int2F16Signed(this.G);
            aF16[2] = Int2F16Signed(this.B);
        }

        public void ToF16Unsigned(ushort[] aF16)
        {
            aF16[0] = Int2F16Unsigned(this.R);
            aF16[1] = Int2F16Unsigned(this.G);
            aF16[2] = Int2F16Unsigned(this.B);
        }

        private static ushort Int2F16Unsigned(int input)
        {
            Guard.MustBeBetweenOrEqualTo(input, 0, Constants.F16MAX, nameof(input));

            ushort res = (ushort)input;

            return res;
        }

        private static ushort Int2F16Signed(int input)
        {
            int s = 0;
            if (input < 0)
            {
                s = Constants.F16S_MASK;
                input = -input;
            }

            ushort res = (ushort)(s | input);

            return res;
        }
    }
}
