// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats
{
    using System;
    using System.Diagnostics;

    internal static class Helpers
    {
        public static bool IsFixUpOffset(byte uPartitions, byte uShape, int uOffset)
        {
            Debug.Assert(uPartitions < 3 && uShape < 64 && uOffset < 16 && uOffset >= 0);
            for (byte p = 0; p <= uPartitions; p++)
            {
                if (uOffset == Constants.g_aFixUp[uPartitions][uShape][p])
                {
                    return true;
                }
            }

            return false;
        }

        public static void TransformInverseSigned(IntEndPntPair[] aEndPts, LdrColorA Prec)
        {
            var WrapMask = new IntColor((1 << Prec.r) - 1, (1 << Prec.g) - 1, (1 << Prec.b) - 1);
            aEndPts[0].B += aEndPts[0].A;
            aEndPts[0].B &= WrapMask;
            aEndPts[1].A += aEndPts[0].A;
            aEndPts[1].A &= WrapMask;
            aEndPts[1].B += aEndPts[0].A;
            aEndPts[1].B &= WrapMask;
            aEndPts[0].B.SignExtend(Prec);
            aEndPts[1].A.SignExtend(Prec);
            aEndPts[1].B.SignExtend(Prec);
        }

        public static void TransformInverseUnsigned(IntEndPntPair[] aEndPts, LdrColorA Prec)
        {
            var WrapMask = new IntColor((1 << Prec.r) - 1, (1 << Prec.g) - 1, (1 << Prec.b) - 1);
            aEndPts[0].B += aEndPts[0].A;
            aEndPts[0].B &= WrapMask;
            aEndPts[1].A += aEndPts[0].A;
            aEndPts[1].A &= WrapMask;
            aEndPts[1].B += aEndPts[0].A;
            aEndPts[1].B &= WrapMask;
        }

        public static int DivRem(int a, int b, out int result)
        {
            int div = a / b;
            result = a - (div * b);
            return div;
        }

        // Fill colors where each pixel is 4 bytes (rgba)
        public static void FillWithErrorColors(Span<byte> pOut, ref int index, int numPixels, byte divSize, int stride)
        {
            int rem;
            for (int i = 0; i < numPixels; ++i)
            {
                pOut[(int)index++] = 0;
                pOut[(int)index++] = 0;
                pOut[(int)index++] = 0;
                pOut[(int)index++] = 255;
                DivRem(i + 1, divSize, out rem);
                if (rem == 0)
                {
                    index += 4 * (stride - divSize);
                }
            }
        }
    }
}
