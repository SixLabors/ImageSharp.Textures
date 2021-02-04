// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats
{
    public interface IBlock<TSelf> : IBlock
        where TSelf : struct, IBlock<TSelf>
    {
    }
}
