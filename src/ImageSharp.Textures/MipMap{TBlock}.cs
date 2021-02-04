// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures
{
    public abstract class MipMap
    {
        public abstract Image GetImage();
    }

    public sealed class MipMap<TBlock> : MipMap
        where TBlock : struct, IBlock<TBlock>
    {
        public MipMap(TBlock blockFormat, byte[] blockData, int width, int height)
        {
            this.BlockFormat = blockFormat;
            this.BlockData = blockData;
            this.Width = width;
            this.Height = height;
        }

        public TBlock BlockFormat { get; }

        public byte[] BlockData { get; set; }

        public int Width { get; }

        public int Height { get; }

        /// <inheritdoc/>
        public override Image GetImage()
        {
            return this.BlockFormat.GetImage(this.BlockData, this.Width, this.Height);
        }
    }
}
