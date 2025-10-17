// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx.Enums;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Describes a KTX file header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct KtxHeader
    {
        public KtxHeader(
            KtxEndianness endianness,
            GlType glType,
            uint glTypeSize,
            GlPixelFormat glFormat,
            GlInternalPixelFormat glInternalFormat,
            GlBaseInternalPixelFormat glBaseInternalFormat,
            uint width,
            uint height,
            uint pixelDepth,
            uint numberOfArrayElements,
            uint numberOfFaces,
            uint numberOfMipmapLevels,
            uint bytesOfKeyValueData)
        {
            this.Endianness = endianness;
            this.GlTypeParameter = glType;
            this.GlTypeSize = glTypeSize;
            this.GlFormat = glFormat;
            this.GlInternalFormat = glInternalFormat;
            this.GlBaseInternalFormat = glBaseInternalFormat;
            this.Width = width;
            this.Height = height;
            this.PixelDepth = pixelDepth;
            this.NumberOfArrayElements = numberOfArrayElements;
            this.NumberOfFaces = numberOfFaces;
            this.NumberOfMipmapLevels = numberOfMipmapLevels;
            this.BytesOfKeyValueData = bytesOfKeyValueData;
        }

        /// <summary>
        /// Gets the endianness.
        /// endianness contains the number 0x04030201 written as a 32 bit integer. If the file is little endian then this is represented as the bytes 0x01 0x02 0x03 0x04.
        /// If the file is big endian then this is represented as the bytes 0x04 0x03 0x02 0x01.
        /// </summary>
        public KtxEndianness Endianness { get; }

        /// <summary>
        /// Gets the glType of the texture.
        /// For compressed textures, glType must equal 0. For uncompressed textures, glType specifies the type parameter passed to glTex{,Sub}Image*D,
        /// usually one of the values from table 8.2 of the OpenGL 4.4 specification
        /// </summary>
        public GlType GlTypeParameter { get; }

        /// <summary>
        /// Gets the glTypeSize.
        /// glTypeSize specifies the data type size that should be used when endianness conversion is required for the texture data stored in the file.
        /// If glType is not 0, this should be the size in bytes corresponding to glType. For texture data which does not depend on platform endianness,
        /// including compressed texture data, glTypeSize must equal 1.
        /// </summary>
        public uint GlTypeSize { get; }

        /// <summary>
        /// Gets the glFormat.
        /// For compressed textures, glFormat must equal 0. For uncompressed textures, glFormat specifies the format parameter passed to glTex{,Sub}Image*D,
        /// usually one of the values from table 8.3 of the OpenGL 4.4 specification. (RGB, RGBA, BGRA, etc.)
        /// </summary>
        public GlPixelFormat GlFormat { get; }

        /// <summary>
        /// Gets the internal format.
        /// For compressed textures, glInternalFormat must equal the compressed internal format, usually one of the values from table 8.14 of the OpenGL 4.4 specification.
        /// For uncompressed textures, glInternalFormat specifies the internalformat parameter passed to glTexStorage*D or glTexImage*D,
        /// usually one of the sized internal formats from tables 8.12 and 8.13 of the OpenGL 4.4 specification.
        /// </summary>
        public GlInternalPixelFormat GlInternalFormat { get; }

        /// <summary>
        /// Gets the base internal format.
        /// For both compressed and uncompressed textures, glBaseInternalFormat specifies the base internal format of the texture,
        /// usually one of the values from table 8.11 of the OpenGL 4.4 specification (RGB, RGBA, ALPHA, etc.).
        /// For uncompressed textures, this value will be the same as glFormat and is used as the internalformat parameter when loading into a context that does not support sized formats,
        /// such as an unextended OpenGL ES 2.0 context.
        /// </summary>
        public GlBaseInternalPixelFormat GlBaseInternalFormat { get; }

        /// <summary>
        /// Gets the width in pixels of the texture at level 0.
        /// </summary>
        public uint Width { get; }

        /// <summary>
        /// Gets the height in pixels of the texture at level 0.
        /// For 1D textures pixelHeight must be 0.
        /// </summary>
        public uint Height { get; }

        /// <summary>
        /// Gets the pixel depth.
        /// For 1D textures pixelDepth must be 0. For 2D and cube textures pixelDepth must be 0.
        /// </summary>
        public uint PixelDepth { get; }

        /// <summary>
        /// Gets the number of array elements.
        /// If the texture is not an array texture, numberOfArrayElements must equal 0.
        /// </summary>
        public uint NumberOfArrayElements { get; }

        /// <summary>
        /// Gets the number of faces.
        /// numberOfFaces specifies the number of cubemap faces. For cubemaps and cubemap arrays this should be 6. For non cubemaps this should be 1.
        /// Cube map faces are stored in the order: +X, -X, +Y, -Y, +Z, -Z.
        /// </summary>
        public uint NumberOfFaces { get; }

        /// <summary>
        /// Gets the number of mipmap levels.
        /// numberOfMipmapLevels must equal 1 for non-mipmapped textures.
        /// </summary>
        public uint NumberOfMipmapLevels { get; }

        /// <summary>
        /// Gets the BytesOfKeyValueData.
        /// An arbitrary number of key/value pairs may follow the header. This can be used to encode any arbitrary data.
        /// </summary>
        public uint BytesOfKeyValueData { get; }

        public static KtxHeader Parse(ReadOnlySpan<byte> data)
        {
            if (data.Length < KtxConstants.KtxHeaderSize)
            {
                throw new ArgumentException(nameof(data), $"Ktx header must be {KtxConstants.KtxHeaderSize} bytes. Was {data.Length} bytes.");
            }

            var endianness = (KtxEndianness)BinaryPrimitives.ReadUInt32LittleEndian(data);
            if (endianness != KtxEndianness.BigEndian && endianness != KtxEndianness.LittleEndian)
            {
                throw new TextureFormatException("ktx file header has an invalid value for endianness");
            }

            if (endianness == KtxEndianness.LittleEndian)
            {
                return new KtxHeader(
                    (KtxEndianness)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(0, 4)),
                    (GlType)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(8, 4)),
                    (GlPixelFormat)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(12, 4)),
                    (GlInternalPixelFormat)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(16, 4)),
                    (GlBaseInternalPixelFormat)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(20, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(24, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(28, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(32, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(36, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(40, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(44, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(48, 4)));
            }

            return new KtxHeader(
                (KtxEndianness)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0, 4)),
                (GlType)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(8, 4)),
                (GlPixelFormat)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(12, 4)),
                (GlInternalPixelFormat)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(16, 4)),
                (GlBaseInternalPixelFormat)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(20, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(24, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(28, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(32, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(36, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(40, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(44, 4)),
                BinaryPrimitives.ReadUInt32BigEndian(data.Slice(48, 4)));
        }
    }
}
