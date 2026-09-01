// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2;

/// <summary>
/// The UASTC payload variant resolved from a KTX2 Data Format Descriptor
/// </summary>
internal enum Ktx2UastcFormat
{
    /// <summary>
    /// The payload is not UASTC.
    /// </summary>
    None,

    /// <summary>
    /// UASTC with a linear transfer function.
    /// </summary>
    Linear,

    /// <summary>
    /// UASTC with an sRGB transfer function.
    /// </summary>
    Srgb,
}
