// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    internal struct Bc6HModeDescriptor
    {
        public readonly Bc6hEField MBc6HEField;
        public readonly byte Bit;

        public Bc6HModeDescriptor(Bc6hEField bc6Hef, byte uB)
        {
            this.MBc6HEField = bc6Hef;
            this.Bit = uB;
        }
    }
}
