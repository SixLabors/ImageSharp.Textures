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

        public static Image<TPixel> LoadPixelData<TPixel>(byte[] data, int width, int height, int rowSkip = 0)
                    where TPixel : struct, IPixel<TPixel>
        {
            ReadOnlySpan<TPixel> pixelData = MemoryMarshal.Cast<byte, TPixel>(new ReadOnlySpan<byte>(data));

            int rowCount = width + rowSkip;
            int count = rowCount * height;
            Guard.MustBeGreaterThanOrEqualTo(pixelData.Length, count, nameof(data));

            var image = new Image<TPixel>(width, height);

            if (rowSkip == 0)
            {
                pixelData.Slice(0, count).CopyTo(image.Frames.RootFrame.GetPixelSpan());
            }
            else
            {
                int offset = 0;
                for (int y = 0; y < height; y++)
                {
                    pixelData.Slice(offset, width).CopyTo(image.Frames.RootFrame.GetPixelRowSpan(y));
                    offset += rowCount;
                }
            }

            return image;
        }

        //private byte[] InMemoryDecode(byte[] memBuffer, int bIndex)
        //{

        //    //Decode(memBuffer, data, bIndex, (uint)dataIndex, (uint)stridePixels);

        //}

        public override Image GetImage()
        {
            int widthBlocks = this.Width;

            byte[] decompressedData = this.BlockFormat.Decompress(this.BlockData, this.Width, this.Height);

            if (this.BlockFormat.Format == ImageFormat.R5g5b5)
            {
                // Turn the alpha channel on
                for (int i = 1; i < decompressedData.Length; i += 2)
                {
                    decompressedData[i] |= 128;
                }

                return LoadPixelData<Bgra5551>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else if (this.BlockFormat.Format == ImageFormat.R5g5b5a1)
            {
                return LoadPixelData<Bgra5551>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else if (this.BlockFormat.Format == ImageFormat.R5g6b5)
            {
                return LoadPixelData<Bgr565>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else if (this.BlockFormat.Format == ImageFormat.Rgb24)
            {
                //Rgb24
                return LoadPixelData<PixelFormats.Bgr24>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else if (this.BlockFormat.Format == ImageFormat.Rgb8)
            {
                return LoadPixelData<L8>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else if (this.BlockFormat.Format == ImageFormat.Rgba16)
            {
                return LoadPixelData<Bgra4444>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else if (this.BlockFormat.Format == ImageFormat.Rgba32)
            {
                //Rgba32
                return LoadPixelData<PixelFormats.Bgra32>(decompressedData, this.Width, this.Height, widthBlocks - this.Width);
            }
            else
            {
                throw new NotImplementedException($"Unrecognized format: ${this.BlockFormat.Format}");
            }
        }
    }
}
