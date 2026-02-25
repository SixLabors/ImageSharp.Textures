// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Astc.BiseEncoding;

/// <summary>
/// The encoding modes supported by BISE.
/// </summary>
/// <remarks>
/// Note that the values correspond to the number of symbols in each alphabet.
/// </remarks>
internal enum BiseEncodingMode
{
    Unknown = 0,
    BitEncoding = 1,
    TritEncoding = 3,
    QuintEncoding = 5,
}
