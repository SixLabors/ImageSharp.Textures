// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures
{
    using System;
    using System.Runtime.InteropServices;
    using SixLabors.ImageSharp.Advanced;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Textures.Formats.Dds;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Processing;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Processing.BlockFormats;


    public abstract class MipMap
    {
        public abstract Image GetImage();
    }


    public sealed class MipMap<TBlock> : MipMap
    where TBlock : struct, IBlock<TBlock>
    {
        public TBlock BlockFormat { get; }

        public byte[] BlockData { get; set; }

        public int Width { get; }

        public int Height { get; }


        public MipMap(TBlock blockFormat, byte[] blockData, int width, int height)
        {
            this.BlockFormat = blockFormat;
            this.BlockData = blockData;
            this.Width = width;
            this.Height = height;
        }

        public override Image GetImage()
        {
            return this.BlockFormat.GetImage(this.BlockData, this.Width, this.Height);
        }
    }
}
