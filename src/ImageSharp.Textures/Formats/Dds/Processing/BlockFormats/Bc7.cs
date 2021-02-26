// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;
using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    public struct Bc7 : IBlock<Bc7>
    {
        // Code based on commit 138efff1b9c53fd9a5dd34b8c865e8f5ae798030 2019/10/24 in DirectXTex C++ library
        private static readonly Bc7ModeInfo[] msAInfo =
        {
            // Mode 0: Color only, 3 Subsets, RGBP 4441 (unique P-bit), 3-bit indecies, 16 partitions
            new Bc7ModeInfo(2, 4, 6, 0, 0, 3, 0, new LdrColorA(4, 4, 4, 0), new LdrColorA(5, 5, 5, 0)),

            // Mode 1: Color only, 2 Subsets, RGBP 6661 (shared P-bit), 3-bit indecies, 64 partitions
            new Bc7ModeInfo(1, 6, 2, 0, 0, 3, 0, new LdrColorA(6, 6, 6, 0), new LdrColorA(7, 7, 7, 0)),

            // Mode 2: Color only, 3 Subsets, RGB 555, 2-bit indecies, 64 partitions
            new Bc7ModeInfo(2, 6, 0, 0, 0, 2, 0, new LdrColorA(5, 5, 5, 0), new LdrColorA(5, 5, 5, 0)),

            // Mode 3: Color only, 2 Subsets, RGBP 7771 (unique P-bit), 2-bits indecies, 64 partitions
            new Bc7ModeInfo(1, 6, 4, 0, 0, 2, 0, new LdrColorA(7, 7, 7, 0), new LdrColorA(8, 8, 8, 0)),

            // Mode 4: Color w/ Separate Alpha, 1 Subset, RGB 555, A6, 16x2/16x3-bit indices, 2-bit rotation, 1-bit index selector
            new Bc7ModeInfo(0, 0, 0, 2, 1, 2, 3, new LdrColorA(5, 5, 5, 6), new LdrColorA(5, 5, 5, 6)),

            // Mode 5: Color w/ Separate Alpha, 1 Subset, RGB 777, A8, 16x2/16x2-bit indices, 2-bit rotation
            new Bc7ModeInfo(0, 0, 0, 2, 0, 2, 2, new LdrColorA(7, 7, 7, 8), new LdrColorA(7, 7, 7, 8)),

            // Mode 6: Color+Alpha, 1 Subset, RGBAP 77771 (unique P-bit), 16x4-bit indecies
            new Bc7ModeInfo(0, 0, 2, 0, 0, 4, 0, new LdrColorA(7, 7, 7, 7), new LdrColorA(8, 8, 8, 8)),

            // Mode 7: Color+Alpha, 2 Subsets, RGBAP 55551 (unique P-bit), 2-bit indices, 64 partitions
            new Bc7ModeInfo(1, 6, 4, 0, 0, 2, 0, new LdrColorA(5, 5, 5, 5), new LdrColorA(6, 6, 6, 6))
        };

        /// <inheritdoc/>
        public int BitsPerPixel => 32;

        /// <inheritdoc/>
        public byte PixelDepthBytes => 4;

        /// <inheritdoc/>
        public byte DivSize => 4;

        /// <inheritdoc/>
        public byte CompressedBytesPerBlock => 16;

        /// <inheritdoc/>
        public bool Compressed => true;

        /// <inheritdoc/>
        public Image GetImage(byte[] blockData, int width, int height)
        {
            byte[] decompressedData = this.Decompress(blockData, width, height);
            return Image.LoadPixelData<ImageSharp.PixelFormats.Rgba32>(decompressedData, width, height);
        }

        /// <inheritdoc/>
        public byte[] Decompress(byte[] blockData, int width, int height)
        {
            byte[] currentBlock = new byte[this.CompressedBytesPerBlock];
            IBlock self = this;

            return Helper.InMemoryDecode<Bc7>(blockData, width, height, (byte[] stream, byte[] data, int streamIndex, int dataIndex, int stride) =>
            {
                // I would prefer to use Span, but not sure if I should reference System.Memory in this project copy data instead.
                Buffer.BlockCopy(blockData, streamIndex, currentBlock, 0, currentBlock.Length);
                streamIndex += currentBlock.Length;

                uint uFirst = 0;
                while (uFirst < 128 && GetBit(currentBlock, ref uFirst) == 0)
                {
                }

                byte uMode = (byte)(uFirst - 1);

                if (uMode < 8)
                {
                    byte uPartitions = msAInfo[uMode].UPartitions;
                    Debug.Assert(uPartitions < Constants.BC7_MAX_REGIONS);

                    byte uNumEndPts = (byte)((uPartitions + 1u) << 1);
                    byte uIndexPrec = msAInfo[uMode].UIndexPrec;
                    byte uIndexPrec2 = msAInfo[uMode].UIndexPrec2;
                    int i;
                    uint uStartBit = uMode + 1u;
                    int[] p = new int[6];
                    byte uShape = GetBits(currentBlock, ref uStartBit, msAInfo[uMode].UPartitionBits);
                    Debug.Assert(uShape < Constants.BC7_MAX_SHAPES);

                    byte uRotation = GetBits(currentBlock, ref uStartBit, msAInfo[uMode].URotationBits);
                    Debug.Assert(uRotation < 4);

                    byte uIndexMode = GetBits(currentBlock, ref uStartBit, msAInfo[uMode].UIndexModeBits);
                    Debug.Assert(uIndexMode < 2);

                    var c = new LdrColorA[Constants.BC7_MAX_REGIONS << 1];
                    for (i = 0; i < c.Length; ++i)
                    {
                        c[i] = new LdrColorA();
                    }

                    LdrColorA rgbaPrec = msAInfo[uMode].RgbaPrec;
                    LdrColorA rgbaPrecWithP = msAInfo[uMode].RgbaPrecWithP;

                    Debug.Assert(uNumEndPts <= (Constants.BC7_MAX_REGIONS << 1));

                    // Red channel
                    for (i = 0; i < uNumEndPts; i++)
                    {
                        if (uStartBit + rgbaPrec.R > 128)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                            return dataIndex;
                        }

                        c[i].R = GetBits(currentBlock, ref uStartBit, rgbaPrec.R);
                    }

                    // Green channel
                    for (i = 0; i < uNumEndPts; i++)
                    {
                        if (uStartBit + rgbaPrec.G > 128)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                            return dataIndex;
                        }

                        c[i].G = GetBits(currentBlock, ref uStartBit, rgbaPrec.G);
                    }

                    // Blue channel
                    for (i = 0; i < uNumEndPts; i++)
                    {
                        if (uStartBit + rgbaPrec.B > 128)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                            return dataIndex;
                        }

                        c[i].B = GetBits(currentBlock, ref uStartBit, rgbaPrec.B);
                    }

                    // Alpha channel
                    for (i = 0; i < uNumEndPts; i++)
                    {
                        if (uStartBit + rgbaPrec.A > 128)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                            return dataIndex;
                        }

                        c[i].A = (byte)(rgbaPrec.A != 0 ? GetBits(currentBlock, ref uStartBit, rgbaPrec.A) : 255u);
                    }

                    // P-bits
                    Debug.Assert(msAInfo[uMode].UPBits <= 6);
                    for (i = 0; i < msAInfo[uMode].UPBits; i++)
                    {
                        if (uStartBit > 127)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                            return dataIndex;
                        }

                        p[i] = GetBit(currentBlock, ref uStartBit);
                    }

                    if (msAInfo[uMode].UPBits != 0)
                    {
                        for (i = 0; i < uNumEndPts; i++)
                        {
                            int pi = i * msAInfo[uMode].UPBits / uNumEndPts;
                            for (byte ch = 0; ch < Constants.BC7_NUM_CHANNELS; ch++)
                            {
                                if (rgbaPrec[ch] != rgbaPrecWithP[ch])
                                {
                                    c[i][ch] = (byte)((c[i][ch] << 1) | p[pi]);
                                }
                            }
                        }
                    }

                    for (i = 0; i < uNumEndPts; i++)
                    {
                        c[i] = Unquantize(c[i], rgbaPrecWithP);
                    }

                    byte[] w1 = new byte[Constants.NumPixelsPerBlock], w2 = new byte[Constants.NumPixelsPerBlock];

                    // read color indices
                    for (i = 0; i < Constants.NumPixelsPerBlock; i++)
                    {
                        uint uNumBits = Helpers.IsFixUpOffset(msAInfo[uMode].UPartitions, uShape, i) ? uIndexPrec - 1u : uIndexPrec;
                        if (uStartBit + uNumBits > 128)
                        {
                            Debug.WriteLine("BC7: Invalid block encountered during decoding");
                            Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                            return dataIndex;
                        }

                        w1[i] = GetBits(currentBlock, ref uStartBit, uNumBits);
                    }

                    // read alpha indices
                    if (uIndexPrec2 != 0)
                    {
                        for (i = 0; i < Constants.NumPixelsPerBlock; i++)
                        {
                            uint uNumBits = i != 0 ? uIndexPrec2 : uIndexPrec2 - 1u;
                            if (uStartBit + uNumBits > 128)
                            {
                                Debug.WriteLine("BC7: Invalid block encountered during decoding");
                                Helpers.FillWithErrorColors(data, ref dataIndex, Constants.NumPixelsPerBlock, self.DivSize, stride);
                                return dataIndex;
                            }

                            w2[i] = GetBits(currentBlock, ref uStartBit, uNumBits);
                        }
                    }

                    for (i = 0; i < Constants.NumPixelsPerBlock; ++i)
                    {
                        byte uRegion = Constants.g_aPartitionTable[uPartitions][uShape][i];
                        var outPixel = new LdrColorA();
                        if (uIndexPrec2 == 0)
                        {
                            LdrColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w1[i], uIndexPrec, uIndexPrec, outPixel);
                        }
                        else
                        {
                            if (uIndexMode == 0)
                            {
                                LdrColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w2[i], uIndexPrec, uIndexPrec2, outPixel);
                            }
                            else
                            {
                                LdrColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w2[i], w1[i], uIndexPrec2, uIndexPrec, outPixel);
                            }
                        }

                        switch (uRotation)
                        {
                            case 1: ByteSwap(ref outPixel.R, ref outPixel.A); break;
                            case 2: ByteSwap(ref outPixel.G, ref outPixel.A); break;
                            case 3: ByteSwap(ref outPixel.B, ref outPixel.A); break;
                        }

                        // Note: whether it's sRGB is not taken into consideration
                        // we're returning data that could be either/or depending
                        // on the input BC7 format
                        data[dataIndex++] = outPixel.B;
                        data[dataIndex++] = outPixel.G;
                        data[dataIndex++] = outPixel.R;
                        data[dataIndex++] = outPixel.A;

                        // Is mult 4?
                        if (((i + 1) & 0x3) == 0)
                        {
                            dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("BC7: Reserved mode 8 encountered during decoding");

                    // Per the BC7 format spec, we must return transparent black
                    for (int i = 0; i < Constants.NumPixelsPerBlock; ++i)
                    {
                        data[dataIndex++] = 0;
                        data[dataIndex++] = 0;
                        data[dataIndex++] = 0;
                        data[dataIndex++] = 0;

                        // Is mult 4?
                        if (((i + 1) & 0x3) == 0)
                        {
                            dataIndex += self.PixelDepthBytes * (stride - self.DivSize);
                        }
                    }
                }

                return streamIndex;
            });
        }

        public static byte GetBit(byte[] currentBlock, ref uint uStartBit)
        {
            Guard.MustBeLessThan<uint>(uStartBit, 128, nameof(uStartBit));
            uint uIndex = uStartBit >> 3;
            byte ret = (byte)((currentBlock[uIndex] >> (int)(uStartBit - (uIndex << 3))) & 0x01);
            uStartBit++;
            return ret;
        }

        public static byte GetBits(byte[] currentBlock, ref uint uStartBit, uint uNumBits)
        {
            if (uNumBits == 0)
            {
                return 0;
            }

            Debug.Assert(uStartBit + uNumBits <= 128 && uNumBits <= 8);
            byte ret;
            uint uIndex = uStartBit >> 3;
            uint uBase = uStartBit - (uIndex << 3);
            if (uBase + uNumBits > 8)
            {
                uint uFirstIndexBits = 8 - uBase;
                uint uNextIndexBits = uNumBits - uFirstIndexBits;
                ret = (byte)((uint)(currentBlock[uIndex] >> (int)uBase) | ((currentBlock[uIndex + 1] & ((1u << (int)uNextIndexBits) - 1)) << (int)uFirstIndexBits));
            }
            else
            {
                ret = (byte)((currentBlock[uIndex] >> (int)uBase) & ((1 << (int)uNumBits) - 1));
            }

            Debug.Assert(ret < (1 << (int)uNumBits));
            uStartBit += uNumBits;
            return ret;
        }

        private static byte Unquantize(byte comp, uint uPrec)
        {
            Guard.MustBeBetweenOrEqualTo<uint>(uPrec, 1, 8, nameof(uPrec));
            comp = (byte)(comp << (int)(8u - uPrec));
            return (byte)(comp | (comp >> (int)uPrec));
        }

        private static LdrColorA Unquantize(LdrColorA c, LdrColorA rgbaPrec)
        {
            var q = new LdrColorA
            {
                R = Unquantize(c.R, rgbaPrec.R),
                G = Unquantize(c.G, rgbaPrec.G),
                B = Unquantize(c.B, rgbaPrec.B),
                A = rgbaPrec.A > 0 ? Unquantize(c.A, rgbaPrec.A) : (byte)255u
            };

            return q;
        }

        private static void ByteSwap(ref byte a, ref byte b)
        {
            byte tmp = a;
            a = b;
            b = tmp;
        }
    }
}
