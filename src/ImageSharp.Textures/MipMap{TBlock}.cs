// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

namespace SixLabors.ImageSharp.Textures
{
    /// <summary>
    /// Represents a mipmap for a specific texture format.
    /// </summary>
    /// <typeparam name="TBlock">The type of the texture block.</typeparam>
    /// <seealso cref="SixLabors.ImageSharp.Textures.MipMap" />
    public sealed class MipMap<TBlock> : MipMap
        where TBlock : struct, IBlock<TBlock>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MipMap{TBlock}"/> class.
        /// </summary>
        /// <param name="blockFormat">The block format.</param>
        /// <param name="blockData">The block data.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        public MipMap(TBlock blockFormat, byte[] blockData, int width, int height)
        {
            this.BlockFormat = blockFormat;
            this.BlockData = blockData;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Gets the block format.
        /// </summary>
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
        public override Image GetImage() => this.BlockFormat.GetImage(this.BlockData, this.Width, this.Height);
    }
}
