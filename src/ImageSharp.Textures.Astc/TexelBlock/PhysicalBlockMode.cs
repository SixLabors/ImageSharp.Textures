// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.TexelBlock;

/// <summary>
/// The overall block modes defined in table C.2.8.  There are 10
/// weight grid encoding schemes + void extent.
/// </summary>
internal enum PhysicalBlockMode
{
    WidthB4HeightA2,
    WidthB8HeightA2,
    WidthA2HeightB8,
    WidthA2HeightB6,
    WidthB2HeightA2,
    Width12HeightA2,
    WidthA2Height12,
    Width6Height10,
    Width10Height6,
    WidthA6HeightB6,
    VoidExtent,
}
