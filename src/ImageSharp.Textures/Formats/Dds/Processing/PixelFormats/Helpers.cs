// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats
{
    internal static class Helpers
    {
        public static bool IsFixUpOffset(byte uPartitions, byte uShape, int uOffset)
        {
            Guard.MustBeLessThan(uPartitions, (byte)3, nameof(uPartitions));
            Guard.MustBeLessThan(uShape, (byte)64, nameof(uShape));
            Guard.MustBeBetweenOrEqualTo(uOffset, 0, 15, nameof(uOffset));

            for (byte p = 0; p <= uPartitions; p++)
            {
                if (uOffset == Constants.FixUp[uPartitions][uShape][p])
                {
                    return true;
                }
            }

            return false;
        }

        public static void TransformInverseSigned(IntEndPntPair[] aEndPts, LdrColorA prec)
        {
            var wrapMask = new IntColor((1 << prec.R) - 1, (1 << prec.G) - 1, (1 << prec.B) - 1);
            aEndPts[0].B += aEndPts[0].A;
            aEndPts[0].B &= wrapMask;
            aEndPts[1].A += aEndPts[0].A;
            aEndPts[1].A &= wrapMask;
            aEndPts[1].B += aEndPts[0].A;
            aEndPts[1].B &= wrapMask;
            aEndPts[0].B.SignExtend(prec);
            aEndPts[1].A.SignExtend(prec);
            aEndPts[1].B.SignExtend(prec);
        }

        public static void TransformInverseUnsigned(IntEndPntPair[] aEndPts, LdrColorA prec)
        {
            var wrapMask = new IntColor((1 << prec.R) - 1, (1 << prec.G) - 1, (1 << prec.B) - 1);
            aEndPts[0].B += aEndPts[0].A;
            aEndPts[0].B &= wrapMask;
            aEndPts[1].A += aEndPts[0].A;
            aEndPts[1].A &= wrapMask;
            aEndPts[1].B += aEndPts[0].A;
            aEndPts[1].B &= wrapMask;
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
            for (int i = 0; i < numPixels; ++i)
            {
                pOut[index++] = 0;
                pOut[index++] = 0;
                pOut[index++] = 0;
                pOut[index++] = 255;
                DivRem(i + 1, divSize, out int rem);
                if (rem == 0)
                {
                    index += 4 * (stride - divSize);
                }
            }
        }
    }
}
