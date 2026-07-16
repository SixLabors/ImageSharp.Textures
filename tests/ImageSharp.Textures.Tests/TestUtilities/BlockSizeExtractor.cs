// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Text.RegularExpressions;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities;

/// <summary>
/// Helpers for parameterized block-footprint tests that extract the block dimensions
/// (e.g. "4x4", "10x5") from an input file name.
/// </summary>
public static partial class BlockSizeExtractor
{
    /// <summary>
    /// Returns the first "NxM" substring in <paramref name="fileName"/>, or an empty
    /// string if none is present.
    /// </summary>
    public static string FromFileName(string fileName)
    {
        Match match = BlockSizeRegex().Match(fileName);
        return match.Success ? match.Value : string.Empty;
    }

    [GeneratedRegex(@"(\d+x\d+)")]
    private static partial Regex BlockSizeRegex();
}
