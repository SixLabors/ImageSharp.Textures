// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats
{
    using System.Diagnostics;

    internal class IntColor
    {
        public int r;
        public int g;
        public int b;
        public int pad;

        public IntColor()
        {

        }

        public IntColor(int nr, int ng, int nb)
        {
            r = nr;
            g = ng;
            b = nb;
            pad = 0;
        }

        public static IntColor operator +(IntColor a, IntColor c)
        {
            a.r += c.r;
            a.g += c.g;
            a.b += c.b;
            return a;
        }

        public static IntColor operator &(IntColor a, IntColor c)
        {
            a.r &= c.r;
            a.g &= c.g;
            a.b &= c.b;
            return a;
        }

        public IntColor SignExtend(LdrColorA Prec)
        {
            r = SIGN_EXTEND(r, Prec.r);
            g = SIGN_EXTEND(g, Prec.g);
            b = SIGN_EXTEND(b, Prec.b);
            return this;
        }

        private static int SIGN_EXTEND(int x, int nb)
        {
            return ((x & 1 << nb - 1) != 0 ? ~0 ^ (1 << nb) - 1 : 0) | x;
        }

        public void ToF16(ushort[] aF16, bool bSigned)
        {
            aF16[0] = INT2F16(r, bSigned);
            aF16[1] = INT2F16(g, bSigned);
            aF16[2] = INT2F16(b, bSigned);
        }

        private static ushort INT2F16(int input, bool bSigned)
        {
            ushort res;
            if (bSigned)
            {
                int s = 0;
                if (input < 0)
                {
                    s = Constants.F16S_MASK;
                    input = -input;
                }

                res = (ushort)(s | input);
            }
            else
            {
                Debug.Assert(input >= 0 && input <= Constants.F16MAX);
                res = (ushort)input;
            }

            return res;
        }
    }
}
