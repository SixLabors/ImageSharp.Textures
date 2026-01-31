// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Diagnostics;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding.PixelFormats
{
    internal class LdrColorA
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdrColorA" /> class.
        /// </summary>
        public LdrColorA()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdrColorA" /> class.
        /// </summary>
        /// <param name="r">The red color value.</param>
        /// <param name="g">The green color value.</param>
        /// <param name="b">The blue color value.</param>
        /// <param name="a">The alpha value.</param>
        public LdrColorA(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// Gets or sets the alpha value.
        /// </summary>
        public byte A { get; set; }

        public byte this[int uElement]
        {
            get => uElement switch
            {
                0 => this.R,
                1 => this.G,
                2 => this.B,
                3 => this.A,
                _ => throw new ArgumentOutOfRangeException(nameof(uElement)),
            };

            set
            {
                switch (uElement)
                {
                    case 0:
                        this.R = value;
                        break;
                    case 1:
                        this.G = value;
                        break;
                    case 2:
                        this.B = value;
                        break;
                    case 3:
                        this.A = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(uElement));
                }
            }
        }

        public void SwapRedWithAlpha()
        {
            byte tmp = this.A;
            this.A = this.R;
            this.R = tmp;
        }

        public void SwapBlueWithAlpha()
        {
            byte tmp = this.A;
            this.A = this.B;
            this.B = tmp;
        }

        public void SwapGreenWithAlpha()
        {
            byte tmp = this.A;
            this.A = this.G;
            this.G = tmp;
        }

        public static void InterpolateRgb(LdrColorA c0, LdrColorA c1, int wc, int wcprec, LdrColorA outt)
        {
            DebugGuard.MustBeBetweenOrEqualTo(wcprec, 2, 4, nameof(wcprec));

            int[] aWeights;
            switch (wcprec)
            {
                case 2:
                {
                    aWeights = Constants.Weights2;
                    Debug.Assert(wc < 4, "wc is expected to be smaller then 4");
                    break;
                }

                case 3:
                {
                    aWeights = Constants.Weights3;
                    Debug.Assert(wc < 8, "wc is expected to be smaller then 8");
                    break;
                }

                case 4:
                {
                    aWeights = Constants.Weights4;
                    Debug.Assert(wc < 16, "wc is expected to be smaller then 16");
                    break;
                }

                default:
                    outt.R = outt.G = outt.B = 0;
                    return;
            }

            outt.R = (byte)(((c0.R * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc])) + (c1.R * (uint)aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
            outt.G = (byte)(((c0.G * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc])) + (c1.G * (uint)aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
            outt.B = (byte)(((c0.B * (uint)(Constants.BC67_WEIGHT_MAX - aWeights[wc])) + (c1.B * (uint)aWeights[wc]) + Constants.BC67_WEIGHT_ROUND) >> Constants.BC67_WEIGHT_SHIFT);
        }

        public static void InterpolateA(LdrColorA c0, LdrColorA c1, int wa, int waprec, LdrColorA outt)
        {
            DebugGuard.MustBeBetweenOrEqualTo(waprec, 2, 4, nameof(waprec));

            int[] aWeights;
            switch (waprec)
            {
                case 2:
                {
                    aWeights = Constants.Weights2;
                    Debug.Assert(wa < 4, "wc is expected to be smaller then 4");
                    break;
                }

                case 3:
                {
                    aWeights = Constants.Weights3;
                    Debug.Assert(wa < 8, "wc is expected to be smaller then 8");
                    break;
                }

                case 4:
                {
                    aWeights = Constants.Weights4;
                    Debug.Assert(wa < 16, "wc is expected to be smaller then 16");
                    break;
                }

                default:
                    outt.A = 0;
                    return;
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
