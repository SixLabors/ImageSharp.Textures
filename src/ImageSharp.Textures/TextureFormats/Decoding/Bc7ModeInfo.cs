// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.TextureFormats.Decoding.PixelFormats;

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    internal struct Bc7ModeInfo
    {
        public readonly byte UPartitions;
        public readonly byte UPartitionBits;
        public readonly byte UPBits;
        public readonly byte URotationBits;
        public readonly byte UIndexModeBits;
        public readonly byte UIndexPrec;
        public readonly byte UIndexPrec2;
        public readonly LdrColorA RgbaPrec;
        public readonly LdrColorA RgbaPrecWithP;

        public Bc7ModeInfo(byte uParts, byte uPartBits, byte upBits, byte uRotBits, byte uIndModeBits, byte uIndPrec, byte uIndPrec2, LdrColorA rgbaPrec, LdrColorA rgbaPrecWithP)
        {
            this.UPartitions = uParts;
            this.UPartitionBits = uPartBits;
            this.UPBits = upBits;
            this.URotationBits = uRotBits;
            this.UIndexModeBits = uIndModeBits;
            this.UIndexPrec = uIndPrec;
            this.UIndexPrec2 = uIndPrec2;
            this.RgbaPrec = rgbaPrec;
            this.RgbaPrecWithP = rgbaPrecWithP;
        }
    }
}
