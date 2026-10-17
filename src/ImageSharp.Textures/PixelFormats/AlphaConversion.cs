// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace SixLabors.ImageSharp.Textures.PixelFormats;

internal static class AlphaConversion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Premultiply(Vector4 vector)
    {
        float alpha = vector.W;
        vector *= alpha;
        vector.W = alpha;

        return vector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 UnPremultiply(Vector4 vector)
    {
        float alpha = vector.W;
        if (alpha == 0F)
        {
            return Vector4.Zero;
        }

        vector /= alpha;
        vector.W = alpha;

        return vector;
    }
}
