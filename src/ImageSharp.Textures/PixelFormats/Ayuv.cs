// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Diagnostics.CodeAnalysis;
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
            readonly get => Unsafe.As<Ayuv, uint>(ref Unsafe.AsRef(in this));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.As<Ayuv, uint>(ref this) = value;
        }

        /// <summary>
        /// Compares two <see cref="Ayuv"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Ayuv"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Ayuv"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Ayuv left, Ayuv right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Ayuv"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Ayuv"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Ayuv"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Ayuv left, Ayuv right) => !left.Equals(right);

        /// <inheritdoc />
        public override readonly string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Ayuv({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PixelOperations<Ayuv> CreatePixelOperations() => new();

        /// <inheritdoc />
        public static PixelTypeInfo GetPixelTypeInfo()
            => PixelTypeInfo.Create<Ayuv>(
                PixelComponentInfo.Create<Ayuv>(4, 8, 8, 8, 8),
                PixelColorType.YCbCr | PixelColorType.Alpha,
                PixelAlphaRepresentation.Unassociated);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToAssociatedScaledVector4() => AlphaConversion.Premultiply(this.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToUnassociatedScaledVector4() => this.ToScaledVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToAssociatedVector4() => AlphaConversion.Premultiply(this.ToVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToUnassociatedVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ayuv FromAssociatedScaledVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ayuv FromUnassociatedScaledVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ayuv FromAssociatedVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ayuv FromUnassociatedVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => this.Yuv.GetHashCode();

        /// <inheritdoc/>
        public static Ayuv FromArgb32(Argb32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromBgr24(Bgr24 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromBgra32(Bgra32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromAbgr32(Abgr32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromBgra5551(Bgra5551 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromL16(L16 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromL8(L8 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromLa16(La16 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromLa32(La32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromRgb24(Rgb24 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromRgb48(Rgb48 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromRgba32(Rgba32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromRgba64(Rgba64 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromScaledVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Ayuv FromVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public readonly Rgba32 ToRgba32() => Rgba32.FromScaledVector4(this.ToScaledVector4());

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

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is Ayuv ayuv && this.Equals(ayuv);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Ayuv other) => this.Yuv.Equals(other.Yuv);
    }
}
