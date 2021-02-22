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
    public struct Y416 : IPixel<Y416>, IPackedVector<ulong>
    {
        private static readonly Vector4 Multiplier = new Vector4(65535F, 65535F, 65535F, 65535F);

        /// <inheritdoc/>
        public ulong PackedValue { get; set; }

        /// <summary>
        /// Gets or sets the packed representation of the Y410 struct.
        /// </summary>
        public ulong Yuv
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Unsafe.As<Y416, ulong>(ref Unsafe.AsRef(this));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.As<Y416, ulong>(ref this) = value;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Y416 other) => this.Yuv.Equals(other.Yuv);

        /// <inheritdoc />
        public override readonly string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Y416({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelOperations<Y416> CreatePixelOperations() => new PixelOperations<Y416>();

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
            uint u = (uint)(this.PackedValue & 0xFFFF);
            uint y = (uint)((this.PackedValue >> 16) & 0xFFFF);
            uint v = (uint)((this.PackedValue >> 32) & 0xFFFF);
            uint a = (uint)((this.PackedValue >> 48) & 0xFFFF);

            // http://msdn.microsoft.com/en-us/library/windows/desktop/bb970578.aspx

            // Y'  = Y - 4096
            // Cb' = Cb - 32768
            // Cr' = Cr - 32768
            y = y - 4096;
            u = u - 32768;
            v = v - 32768;

            uint r = ((76607 * y) + (105006 * v) + 32768) >> 16;
            uint g = ((76607 * y) - (25772 * u) - (53477 * v) + 32768) >> 16;
            uint b = ((76607 * y) + (132718 * u) + 32768) >> 16;

            return new Vector4(Math.Min(Math.Max(r, 0), 65535), Math.Min(Math.Max(g, 0), 65535), Math.Min(Math.Max(b, 0), 65535), Math.Min(Math.Max(a, 0), 65535)) / Multiplier;
        }
    }
}
