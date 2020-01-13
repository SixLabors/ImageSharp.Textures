// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.PixelFormats
{
    internal struct IntEndPntPair
    {
        public IntColor A;
        public IntColor B;

        public IntEndPntPair(IntColor a, IntColor b)
        {
            A = a;
            B = b;
        }
    };
}
