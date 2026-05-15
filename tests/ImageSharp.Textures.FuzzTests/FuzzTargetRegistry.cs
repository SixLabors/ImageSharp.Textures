// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.FuzzTests;

/// <summary>
/// Registry of available <see cref="IFuzzTarget"/>s
/// </summary>
internal static class FuzzTargetRegistry
{
    public static IReadOnlyList<IFuzzTarget> All { get; } =
    [
        new Targets.BlockModeTarget(),
        new Targets.DecompressBlockTarget(),
        new Targets.DecompressHdrBlockTarget(),
        new Targets.DecompressImageTarget(),
        new Targets.DecompressHdrImageTarget(),
    ];

    /// <summary>
    /// Returns the target whose <see cref="IFuzzTarget.Name"/> matches <paramref name="name"/>
    /// </summary>
    public static IFuzzTarget? TryGet(string name)
    {
        foreach (IFuzzTarget target in All)
        {
            if (target.Name == name)
            {
                return target;
            }
        }

        return null;
    }
}
