// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats
{
    internal class LdrColorA
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public LdrColorA()
        {
        }

        public LdrColorA(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public ref byte this[int uElement]
        {
            get
            {
                switch (uElement)
                {
                    case 0: return ref this.R;
                    case 1: return ref this.G;
                    case 2: return ref this.B;
                    case 3: return ref this.A;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public static void InterpolateRgb(LdrColorA c0, LdrColorA c1, int wc, int wcprec, LdrColorA outt)
        {
            int[] aWeights = null;
            switch (wcprec)
            {
                case 2: aWeights = Constants.g_aWeights2; Debug.Assert(wc < 4, "wc is expected to be smaller then 4"); break;
                case 3: aWeights = Constants.g_aWeights3; Debug.Assert(wc < 8, "wc is expected to be smaller then 8"); break;
                case 4: aWeights = Constants.g_aWeights4; Debug.Assert(wc < 16, "wc is expected to be smaller then 16"); break;
                default: Debug.Assert(false); outt.R = outt.G = outt.B = 0; return;
            }

            outt.R = (byte)(((c0.R * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc])) + (c1.R * (uint)aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
            outt.G = (byte)(((c0.G * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc])) + (c1.G * (uint)aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
            outt.B = (byte)(((c0.B * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc])) + (c1.B * (uint)aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
        }

        public static void InterpolateA(LdrColorA c0, LdrColorA c1, int wa, int waprec, LdrColorA outt)
        {
            int[] aWeights = null;
            switch (waprec)
            {
                case 2: aWeights = Constants.g_aWeights2; Debug.Assert(wa < 4, "wc is expected to be smaller then 4"); break;
                case 3: aWeights = Constants.g_aWeights3; Debug.Assert(wa < 8, "wc is expected to be smaller then 8"); break;
                case 4: aWeights = Constants.g_aWeights4; Debug.Assert(wa < 16, "wc is expected to be smaller then 16"); break;
                default: Debug.Assert(false); outt.A = 0; return;
            }

            outt.A = (byte)(((c0.A * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wa])) + (c1.A * (uint)aWeights[wa]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
        }

        public static void Interpolate(LdrColorA c0, LdrColorA c1, int wc, int wa, int wcprec, int waprec, LdrColorA outt)
        {
            InterpolateRgb(c0, c1, wc, wcprec, outt);
            InterpolateA(c0, c1, wa, waprec, outt);
        }
    }
}
