// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    internal struct Bc6hsModeDescriptor
    {
        public readonly Bc6hEField MBc6HEField;
        public readonly byte Bit;

        public Bc6hsModeDescriptor(Bc6hEField bc6Hef, byte b)
        {
            this.MBc6HEField = bc6Hef;
            this.Bit = b;
        }
    }
}
