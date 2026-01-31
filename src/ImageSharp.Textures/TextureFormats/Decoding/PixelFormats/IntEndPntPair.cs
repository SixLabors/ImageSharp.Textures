// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding.PixelFormats
{
    internal struct IntEndPntPair
    {
        public IntColor A;
        public IntColor B;

        public IntEndPntPair(IntColor a, IntColor b)
        {
            this.A = a;
            this.B = b;
        }
    }
}
