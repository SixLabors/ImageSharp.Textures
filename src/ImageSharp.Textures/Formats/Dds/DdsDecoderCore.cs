// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Emums;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Extensions;
    using SixLabors.ImageSharp.Textures.Formats.Dds.Processing;
    using SixLabors.ImageSharp.Textures.TextureFormats;
    using SixLabors.Memory;

    internal sealed class DdsDecoderCore
    {
        /// <summary>
        /// The file header containing general information about the image.
        /// </summary>
        private DdsHeader ddsHeader;

        /// <summary>
        /// The dxt10 header if available
        /// </summary>
        private DdsHeaderDxt10 ddsDxt10header;

        /// <summary>
        /// The global configuration.
        /// </summary>
        private readonly Configuration configuration;

        /// <summary>
        /// Used for allocating memory during processing operations.
        /// </summary>
        private readonly MemoryAllocator memoryAllocator;

        /// <summary>
        /// The stream to decode from.
        /// </summary>
        private Stream currentStream;

        /// <summary>
        /// The bitmap decoder options.
        /// </summary>
        private readonly IDdsDecoderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DdsDecoderCore"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        public DdsDecoderCore(Configuration configuration, IDdsDecoderOptions options)
        {
            this.configuration = configuration;
            this.memoryAllocator = configuration.MemoryAllocator;
            this.options = options;
        }

        /// <summary>
        /// Decodes the image from the specified stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="stream">The stream, where the image should be decoded from. Cannot be null.</param>
        /// <exception cref="System.ArgumentNullException">
        ///    <para><paramref name="stream"/> is null.</para>
        /// </exception>
        /// <returns>The decoded image.</returns>
        public Texture DecodeTexture(Stream stream)
        {
            try
            {
                this.ReadFileHeader(stream);

                if (this.ddsHeader.Width == 0 || this.ddsHeader.Height == 0)
                {
                    throw new UnknownTextureFormatException("Width or height cannot be 0");
                }

                if (this.ddsHeader.IsVolumeTexture())
                {
                    var depths = ddsHeader.ComputeDepth();

                    var texture = new Volume();
                    for (int depth = 0; depth < depths; depth++)
                    {
                        var mipMaps = Processing.DdsProcessor.DecodeDds(stream, this.ddsHeader, this.ddsDxt10header);
                        var surface = new Surface();
                        surface.MipMaps.AddRange(mipMaps);
                        texture.Slices.Add(surface);
                    }

                    return texture;
                }
                else if (this.ddsHeader.IsCubemap())
                {
                    DdsSurfaceType[] faces = this.ddsHeader.GetExistingCubemapFaces();
               
                    var texture = new Cubemap();
                    for (int face = 0; face < faces.Length; face++)
                    {
                        var mipMaps = Processing.DdsProcessor.DecodeDds(stream, this.ddsHeader, this.ddsDxt10header);
                        if (faces[face] == DdsSurfaceType.CubemapPositiveX)
                        {
                            texture.PositiveX.MipMaps.AddRange(mipMaps);
                        }
                        if (faces[face] == DdsSurfaceType.CubemapNegativeX)
                        {
                            texture.NegativeX.MipMaps.AddRange(mipMaps);
                        }
                        if (faces[face] == DdsSurfaceType.CubemapPositiveY)
                        {
                            texture.PositiveY.MipMaps.AddRange(mipMaps);
                        }
                        if (faces[face] == DdsSurfaceType.CubemapNegativeY)
                        {
                            texture.NegativeY.MipMaps.AddRange(mipMaps);
                        }
                        if (faces[face] == DdsSurfaceType.CubemapPositiveZ)
                        {
                            texture.PositiveZ.MipMaps.AddRange(mipMaps);
                        }
                        if (faces[face] == DdsSurfaceType.CubemapNegativeZ)
                        {
                            texture.NegativeZ.MipMaps.AddRange(mipMaps);
                        }
                    }

                    return texture;
                }
                else
                {
                    var texture = new Surface();
                    var mipMaps = Processing.DdsProcessor.DecodeDds(stream, this.ddsHeader, this.ddsDxt10header);
                    texture.MipMaps.AddRange(mipMaps);
                    return texture;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new TextureFormatException("Dds image does not have a valid format.", e);
            }
        }

        /// <summary>
        /// Reads the raw image information from the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        public ITextueInfo Identify(Stream stream)
        {
            this.ReadFileHeader(stream);

            D3dFormat d3dFormat = this.ddsHeader.PixelFormat.GetD3DFormat();
            DxgiFormat dxgiFormat = this.ddsHeader.ShouldHaveDxt10Header() ? this.ddsDxt10header.DxgiFormat : DxgiFormat.Unknown;
            int bitsPerPixel = DdsTools.GetBitsPerPixel(d3dFormat, dxgiFormat);

            return new TextureInfo(
                new TextureTypeInfo(bitsPerPixel),
                (int)this.ddsHeader.Width,
                (int)this.ddsHeader.Height);
        }

        /// <summary>
        /// Reads the dds file header from the stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        private void ReadFileHeader(Stream stream)
        {
            this.currentStream = stream;

#if NETCOREAPP2_1 || NETCOREAPP3_1
            Span<byte> magicBuffer = stackalloc byte[4];
#else
            var magicBuffer = new byte[4];
#endif
            this.currentStream.Read(magicBuffer, 0, 4);
            uint magicValue = BinaryPrimitives.ReadUInt32LittleEndian(magicBuffer);
            if (magicValue != DdsFourCC.DdsMagicWord)
            {
                throw new NotSupportedException($"Invalid DDS magic value.");
            }

#if NETCOREAPP2_1 || NETCOREAPP3_1
            Span<byte> ddsHeaderBuffer = stackalloc byte[DdsConstants.DdsHeaderSize];
#else
            var ddsHeaderBuffer = new byte[DdsConstants.DdsHeaderSize];
#endif

            this.currentStream.Read(ddsHeaderBuffer, 0, DdsConstants.DdsHeaderSize);
            this.ddsHeader = DdsHeader.Parse(ddsHeaderBuffer);
            this.ddsHeader.Validate();

            if (this.ddsHeader.ShouldHaveDxt10Header())
            {
#if NETCOREAPP2_1 || NETCOREAPP3_1
                Span<byte> ddsDxt10headerBuffer = stackalloc byte[DdsConstants.DdsDxt10HeaderSize];
#else
                var ddsDxt10headerBuffer = new byte[DdsConstants.DdsDxt10HeaderSize];
#endif
                this.currentStream.Read(ddsDxt10headerBuffer, 0, DdsConstants.DdsDxt10HeaderSize);
                this.ddsDxt10header = DdsHeaderDxt10.Parse(ddsDxt10headerBuffer);
            }
        }
    }
}
