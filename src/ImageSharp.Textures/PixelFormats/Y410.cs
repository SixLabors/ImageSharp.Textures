// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Y410 : IPixel<Y410>, IPackedVector<uint>
    {
        /// <inheritdoc/>
        public uint PackedValue { get; set; }

        /// <summary>
        /// Gets or sets the packed representation of the Y410 struct.
        /// </summary>
        public uint Yuv
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Unsafe.As<Y410, uint>(ref Unsafe.AsRef(this));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.As<Y410, uint>(ref this) = value;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Y410 other) => this.Yuv.Equals(other.Yuv);

        /// <inheritdoc />
        public override readonly string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Y416({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelOperations<Y410> CreatePixelOperations() => new PixelOperations<Y410>();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => this.Yuv.GetHashCode();

        /// <inheritdoc/>
        public void FromArgb32(Argb32 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromBgr24(Bgr24 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromBgra32(Bgra32 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromBgra5551(Bgra5551 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromL16(L16 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromL8(L8 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromLa16(La16 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromLa32(La32 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromRgb24(Rgb24 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromRgb48(Rgb48 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromRgba32(Rgba32 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromRgba64(Rgba64 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void FromVector4(Vector4 vector) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public void ToRgba32(ref Rgba32 dest) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToVector4()
        {
            uint u = (this.PackedValue >> 0) & 0x03FF;
            uint y = (this.PackedValue >> 10) & 0x03FF;
            uint v = (this.PackedValue >> 20) & 0x03FF;
            uint a = (this.PackedValue >> 30) & 0x03;

            // http://msdn.microsoft.com/en-us/library/windows/desktop/bb970578.aspx
            // Y'  = Y - 64
            // Cb' = Cb - 512
            // Cr' = Cr - 512
            y = y - 64;
            u = u - 512;
            v = v - 512;

            // R = 1.1678Y' + 1.6007Cr'
            // G = 1.1678Y' - 0.3929Cb' - 0.8152Cr'
            // B = 1.1678Y' + 2.0232Cb'
            return ColorSpaceConversion.YuvToRgba10Bit(y, u , v, a);
        }
    }
}
