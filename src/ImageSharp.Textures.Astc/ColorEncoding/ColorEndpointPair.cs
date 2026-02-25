// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Astc.ColorEncoding;

/// <summary>
/// A value-type discriminated union representing either an LDR or HDR color endpoint pair.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal struct ColorEndpointPair
{
    public bool IsHdr;

    // LDR fields (used when IsHdr == false)
    public RgbaColor LdrLow;
    public RgbaColor LdrHigh;

    // HDR fields (used when IsHdr == true)
    public RgbaHdrColor HdrLow;
    public RgbaHdrColor HdrHigh;
    public bool AlphaIsLdr;
    public bool ValuesAreLns;

    public static ColorEndpointPair Ldr(RgbaColor low, RgbaColor high)
        => new() { IsHdr = false, LdrLow = low, LdrHigh = high };

    public static ColorEndpointPair Hdr(RgbaHdrColor low, RgbaHdrColor high, bool alphaIsLdr = false, bool valuesAreLns = true)
        => new() { IsHdr = true, HdrLow = low, HdrHigh = high, AlphaIsLdr = alphaIsLdr, ValuesAreLns = valuesAreLns };
}
