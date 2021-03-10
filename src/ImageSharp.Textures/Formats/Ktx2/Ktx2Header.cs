// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Formats.Ktx2.Enums;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Describes a KTX2 file header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Ktx2Header
    {
        public Ktx2Header(
            VkFormat vkFormat,
            uint typeSize,
            uint pixelWidth,
            uint pixelHeight,
            uint pixelDepth,
            uint layerCount,
            uint faceCount,
            uint levelCount,
            uint supercompressionScheme,
            uint dfdByteOffset,
            uint dfdByteLength,
            uint kvdByteOffset,
            uint kvdByteLength,
            ulong sgdByteOffset,
            ulong sgdByteLength)
        {
            this.VkFormat = vkFormat;
            this.TypeSize = typeSize;
            this.PixelWidth = pixelWidth;
            this.PixelHeight = pixelHeight;
            this.PixelDepth = pixelDepth;
            this.LayerCount = layerCount;
            this.FaceCount = faceCount;
            this.LevelCount = levelCount;
            this.SupercompressionScheme = supercompressionScheme;
            this.DfdByteOffset = dfdByteOffset;
            this.DfdByteLength = dfdByteLength;
            this.KvdByteOffset = kvdByteOffset;
            this.KvdByteLength = kvdByteLength;
            this.SgdByteOffset = sgdByteOffset;
            this.SgdByteLength = sgdByteLength;
        }

        /// <summary>
        /// Gets the vkFormat.
        /// vkFormat specifies the image format using Vulkan VkFormat enum values. It can be any value defined in core Vulkan 1.2.
        /// </summary>
        public VkFormat VkFormat { get; }

        /// <summary>
        /// Gets the type size.
        /// typeSize specifies the size of the data type in bytes used to upload the data to a graphics API.
        /// When typeSize is greater than 1, software on big-endian systems must endian convert all image data since it is little-endian.
        /// </summary>
        public uint TypeSize { get; }

        /// <summary>
        /// Gets the width of the texture image for level 0, in pixels.
        /// pixelWidth cannot be 0.
        /// </summary>
        public uint PixelWidth { get; }

        /// <summary>
        /// Gets the height of the texture image for level 0, in pixels.
        /// For 1D textures, pixelHeight and pixelDepth must be 0.
        /// </summary>
        public uint PixelHeight { get; }

        /// <summary>
        /// Gets the pixel depth.
        /// For 1D textures pixelDepth must be 0.
        /// For 2D and cubemap textures, pixelDepth must be 0.
        /// pixelDepth must be 0 for depth or stencil formats.
        /// </summary>
        public uint PixelDepth { get; }

        /// <summary>
        /// Gets the layer count.
        /// layerCount specifies the number of array elements. If the texture is not an array texture, layerCount must equal 0.
        /// </summary>
        public uint LayerCount { get; }

        /// <summary>
        /// Gets the face count.
        /// faceCount specifies the number of cubemap faces. For cubemaps and cubemap arrays this must be 6. For non cubemaps this must be 1.
        /// Cubemap faces are stored in the order: +X, -X, +Y, -Y, +Z, -Z.
        /// </summary>
        public uint FaceCount { get; }

        /// <summary>
        /// Gets the level count.
        /// levelCount specifies the number of levels in the Mip Level Array and, by extension, the number of indices in the Level Index array.
        /// levelCount=0  is allowed, except for block-compressed formats, and means that a file contains only the base level and consumers,
        /// particularly loaders, should generate other levels if needed.
        /// </summary>
        public uint LevelCount { get; }

        /// <summary>
        /// Gets the supercompression scheme.
        /// supercompressionScheme indicates if a supercompression scheme has been applied to the data in levelImages.
        /// It must be one of the values from Table 2, “Supercompression Schemes”. A value of 0 indicates no supercompression.
        /// </summary>
        public uint SupercompressionScheme { get; }

        /// <summary>
        /// Gets the DFD byte offset.
        /// The offset from the start of the file of the dfdTotalSize field of the Data Format Descriptor.
        /// </summary>
        public uint DfdByteOffset { get; }

        /// <summary>
        /// Gets the total number of bytes in the Data Format Descriptor including the dfdTotalSize field. dfdByteLength must equal dfdTotalSize.
        /// </summary>
        public uint DfdByteLength { get; }

        /// <summary>
        /// Gets the key value pair offsets.
        /// An arbitrary number of key/value pairs may follow the Index. These can be used to encode any arbitrary data.
        /// The kvdByteOffset field gives the offset of this data, i.e. that of first key/value pair, from the start of the file. The value must be 0 when kvdByteLength = 0.
        /// </summary>
        public uint KvdByteOffset { get; }

        /// <summary>
        /// Gets the total number of bytes of key/value data including all keyAndValueByteLength fields, all keyAndValue fields and all valuePadding fields.
        /// </summary>
        public uint KvdByteLength { get; }

        /// <summary>
        /// Gets the offset from the start of the file of supercompressionGlobalData. The value must be 0 when sgdByteLength = 0.
        /// </summary>
        public ulong SgdByteOffset { get; }

        /// <summary>
        /// Gets the number of bytes of supercompressionGlobalData.
        /// For supercompression schemes for which no reference is provided in the Global Data Format column of Table 2, “Supercompression Schemes”. the value must be 0.
        /// </summary>
        public ulong SgdByteLength { get; }

        public static Ktx2Header Parse(ReadOnlySpan<byte> data)
        {
            if (data.Length < Ktx2Constants.KtxHeaderSize)
            {
                throw new ArgumentException(nameof(data), $"Ktx2 header must be {Ktx2Constants.KtxHeaderSize} bytes. Was {data.Length} bytes.");
            }

            return new Ktx2Header(
                (VkFormat)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(0, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(8, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(12, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(16, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(20, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(24, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(28, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(32, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(36, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(40, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(44, 4)),
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(48, 4)),
                BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(52, 8)),
                BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(60, 8)));
        }
    }
}
