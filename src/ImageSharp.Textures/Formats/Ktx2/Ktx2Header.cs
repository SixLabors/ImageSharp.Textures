// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Textures.Common.Exceptions;
using SixLabors.ImageSharp.Textures.Formats.Ktx.Enums;

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Describes a KTX2 file header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Ktx2Header
    {
        public Ktx2Header(
            uint vkFormat,
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

        public uint VkFormat { get; }

        public uint TypeSize { get; }

        public uint PixelWidth { get; }

        public uint PixelHeight { get; }

        public uint PixelDepth { get; }

        public uint LayerCount { get; }

        public uint FaceCount { get; }

        public uint LevelCount { get; }

        public uint SupercompressionScheme { get; }

        public uint DfdByteOffset { get; }

        public uint DfdByteLength { get; }

        public uint KvdByteOffset { get; }

        public uint KvdByteLength { get; }

        public ulong SgdByteOffset { get; }

        public ulong SgdByteLength { get; }

        public static Ktx2Header Parse(ReadOnlySpan<byte> data)
        {
            if (data.Length < Ktx2Constants.KtxHeaderSize)
            {
                throw new ArgumentException(nameof(data), $"Ktx2 header must be {Ktx2Constants.KtxHeaderSize} bytes. Was {data.Length} bytes.");
            }

            return new Ktx2Header(
                BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(0, 4)),
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
