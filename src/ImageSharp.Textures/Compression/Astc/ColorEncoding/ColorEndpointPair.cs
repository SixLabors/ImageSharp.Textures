// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;

/// <summary>
/// A value-type discriminated union representing either an LDR or HDR color endpoint pair.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal struct ColorEndpointPair
{
    public bool IsHdr;

    // LDR fields (used when IsHdr == false)
    public Rgba32 LdrLow;
    public Rgba32 LdrHigh;

    // HDR fields (used when IsHdr == true)
    public Rgba64 HdrLow;
    public Rgba64 HdrHigh;
    public bool AlphaIsLdr;
    public bool ValuesAreLns;

    public static ColorEndpointPair Ldr(Rgba32 low, Rgba32 high)
        => new() { IsHdr = false, LdrLow = low, LdrHigh = high };

    public static ColorEndpointPair Hdr(Rgba64 low, Rgba64 high, bool alphaIsLdr = false, bool valuesAreLns = true)
        => new() { IsHdr = true, HdrLow = low, HdrHigh = high, AlphaIsLdr = alphaIsLdr, ValuesAreLns = valuesAreLns };
}
