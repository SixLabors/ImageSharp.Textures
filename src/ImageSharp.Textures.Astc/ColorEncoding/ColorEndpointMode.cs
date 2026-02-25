// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.ColorEncoding;

/// <summary>
/// ASTC supports 16 color endpoint encoding schemes, known as endpoint modes
/// </summary>
/// <remarks>
/// The options for endpoint modes let you vary the following:
/// <list type="bullet">
/// <item>The number of color channels. For example, luminance, luminance+alpha, rgb, or rgba</item>
/// <item>The encoding method. For example, direct, base+offset, base+scale, or quantization level</item>
/// <item>The data range. For example, low dynamic range or High Dynamic Range</item>
/// </list>
/// </remarks>
internal enum ColorEndpointMode
{
    LdrLumaDirect = 0,
    LdrLumaBaseOffset,
    HdrLumaLargeRange,
    HdrLumaSmallRange,
    LdrLumaAlphaDirect,
    LdrLumaAlphaBaseOffset,
    LdrRgbBaseScale,
    HdrRgbBaseScale,
    LdrRgbDirect,
    LdrRgbBaseOffset,
    LdrRgbBaseScaleTwoA,
    HdrRgbDirect,
    LdrRgbaDirect,
    LdrRgbaBaseOffset,
    HdrRgbDirectLdrAlpha,
    HdrRgbDirectHdrAlpha,

    // Number of endpoint modes defined by the ASTC specification.
    ColorEndpointModeCount
}