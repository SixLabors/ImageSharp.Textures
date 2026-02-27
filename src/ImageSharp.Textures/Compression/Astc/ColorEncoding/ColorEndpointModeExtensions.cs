// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

internal static class ColorEndpointModeExtensions
{
    public static int GetEndpointModeClass(this ColorEndpointMode mode)
        => (int)mode / 4;

    public static int GetColorValuesCount(this ColorEndpointMode mode)
        => (mode.GetEndpointModeClass() + 1) * 2;

    /// <summary>
    /// Determines whether the specified endpoint mode uses HDR (High Dynamic Range) encoding.
    /// </summary>
    /// <returns>
    /// True if the mode is one of the 6 HDR modes (2, 3, 7, 11, 14, 15), false otherwise.
    /// </returns>
    public static bool IsHdr(this ColorEndpointMode mode)
        => mode switch
        {
            ColorEndpointMode.HdrLumaLargeRange => true,      // Mode 2
            ColorEndpointMode.HdrLumaSmallRange => true,      // Mode 3
            ColorEndpointMode.HdrRgbBaseScale => true,        // Mode 7
            ColorEndpointMode.HdrRgbDirect => true,           // Mode 11
            ColorEndpointMode.HdrRgbDirectLdrAlpha => true,   // Mode 14
            ColorEndpointMode.HdrRgbDirectHdrAlpha => true,   // Mode 15
            _ => false
        };
}
