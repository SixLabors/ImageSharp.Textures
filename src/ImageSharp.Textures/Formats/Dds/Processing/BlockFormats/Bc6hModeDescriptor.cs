// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    internal struct Bc6hModeDescriptor
    {
        public Bc6hEField MBc6HEField;
        public byte m_uBit;

        public Bc6hModeDescriptor(Bc6hEField bc6HEF, byte uB)
        {
            this.MBc6HEField = bc6HEF;
            this.m_uBit = uB;
        }
    }
}