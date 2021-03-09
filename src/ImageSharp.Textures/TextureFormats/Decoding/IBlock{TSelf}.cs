// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.TextureFormats.Decoding
{
    /// <summary>
    /// Interface for a block texture.
    /// </summary>
    /// <typeparam name="TSelf">The type of the texture.</typeparam>
    public interface IBlock<TSelf> : IBlock
        where TSelf : struct, IBlock<TSelf>
    {
    }
}
