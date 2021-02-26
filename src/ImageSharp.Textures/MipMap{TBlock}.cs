// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;

namespace SixLabors.ImageSharp.Textures
{
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

        /// <summary>
        /// Gets or sets the byte data for the mipmap.
        /// </summary>
        public byte[] BlockData { get; set; }

        /// <summary>
        /// Gets the width of the mipmap.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the mipmap.
        /// </summary>
        public int Height { get; }

        /// <inheritdoc/>
        public override Image GetImage()
        {
            return this.BlockFormat.GetImage(this.BlockData, this.Width, this.Height);
        }
    }
}
