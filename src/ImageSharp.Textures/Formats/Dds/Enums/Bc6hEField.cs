// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing
{
    internal enum Bc6hEField : byte
    {
        NA, // N/A
        M,  // Mode
        D,  // Shape
        RW,
        RX,
        RY,
        RZ,
        GW,
        GX,
        GY,
        GZ,
        BW,
        BX,
        BY,
        BZ,
    }
}