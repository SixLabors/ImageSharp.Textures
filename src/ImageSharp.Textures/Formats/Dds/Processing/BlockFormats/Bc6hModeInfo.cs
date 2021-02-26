// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    internal struct Bc6hModeInfo
    {
        public byte Mode;
        public byte Partitions;
        public bool Transformed;
        public byte IndexPrec;
        public readonly LdrColorA[][] RgbaPrec; // [Constants.BC6H_MAX_REGIONS][2];

        public Bc6hModeInfo(byte m, byte p, bool t, byte i, LdrColorA[][] prec)
        {
            this.Mode = m;
            this.Partitions = p;
            this.Transformed = t;
            this.IndexPrec = i;
            this.RgbaPrec = prec;
        }
    }
}