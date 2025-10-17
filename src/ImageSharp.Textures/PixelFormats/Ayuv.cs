// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Pixel format for YUV 4:4:4 data.
    /// </summary>
    public struct Ayuv : IPixel<Ayuv>, IPackedVector<uint>
    {
        /// <inheritdoc/>
        public uint PackedValue { get; set; }

        /// <summary>
        /// Gets or sets the packed representation of the Ayuv struct.
        /// </summary>
        public uint Yuv
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => Unsafe.As<Ayuv, uint>(ref Unsafe.AsRef(this));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.As<Ayuv, uint>(ref this) = value;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Ayuv other) => this.Yuv.Equals(other.Yuv);

        /// <inheritdoc />
        public override readonly string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Ayuv({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelOperations<Ayuv> CreatePixelOperations() => new();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => this.Yuv.GetHashCode();

        /// <inheritdoc/>
        public void FromArgb32(Argb32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromBgr24(Bgr24 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromBgra32(Bgra32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromAbgr32(Abgr32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromBgra5551(Bgra5551 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromL16(L16 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromL8(L8 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromLa16(La16 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromLa32(La32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromRgb24(Rgb24 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromRgb48(Rgb48 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromRgba32(Rgba32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromRgba64(Rgba64 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void FromVector4(Vector4 vector) => throw new NotImplementedException();

        /// <inheritdoc/>
        public void ToRgba32(ref Rgba32 dest) => throw new NotImplementedException();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToVector4()
        {
            int v = (int)(this.PackedValue & 0xFF);
            int u = (int)((this.PackedValue >> 8) & 0xFF);
            int y = (int)((this.PackedValue >> 16) & 0xFF);
            int a = (int)((this.PackedValue >> 24) & 0xFF);

            // http://msdn.microsoft.com/en-us/library/windows/desktop/dd206750.aspx

            // Y'  = Y - 16
            // Cb' = Cb - 128
            // Cr' = Cr - 128
            y -= 16;
            u -= 128;
            v -= 128;

            // R = 1.1644Y' + 1.5960Cr'
            // G = 1.1644Y' - 0.3917Cb' - 0.8128Cr'
            // B = 1.1644Y' + 2.0172Cb'
            return ColorSpaceConversion.YuvToRgba8Bit(y, u, v, a);
        }
    }
}
