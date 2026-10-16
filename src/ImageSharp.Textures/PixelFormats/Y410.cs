// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Pixel format based on 16-bit per channel packed YUV 4:4:4 data.
    /// </summary>
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
            readonly get => Unsafe.As<Y410, uint>(ref Unsafe.AsRef(in this));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.As<Y410, uint>(ref this) = value;
        }

        /// <summary>
        /// Compares two <see cref="Y410"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Y410"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Y410"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Y410 left, Y410 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Y410"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Y410"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Y410"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Y410 left, Y410 right) => !left.Equals(right);

        /// <inheritdoc />
        public override readonly bool Equals(object? obj) => obj is Y410 other && this.Equals(other);

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
        public static PixelOperations<Y410> CreatePixelOperations() => new();

        /// <inheritdoc />
        public static PixelTypeInfo GetPixelTypeInfo()
            => PixelTypeInfo.Create<Y410>(
                PixelComponentInfo.Create<Y410>(4, 10, 10, 10, 2),
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
        public static Y410 FromAssociatedScaledVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Y410 FromUnassociatedScaledVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Y410 FromAssociatedVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Y410 FromUnassociatedVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => this.Yuv.GetHashCode();

        /// <inheritdoc/>
        public static Y410 FromArgb32(Argb32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromBgr24(Bgr24 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromBgra32(Bgra32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromAbgr32(Abgr32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromBgra5551(Bgra5551 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromL16(L16 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromL8(L8 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromLa16(La16 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromLa32(La32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromRgb24(Rgb24 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromRgb48(Rgb48 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromRgba32(Rgba32 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromRgba64(Rgba64 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromScaledVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public static Y410 FromVector4(Vector4 source) => throw new NotImplementedException();

        /// <inheritdoc/>
        public readonly Rgba32 ToRgba32() => Rgba32.FromScaledVector4(this.ToScaledVector4());

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
            y -= 64;
            u -= 512;
            v -= 512;

            // R = 1.1678Y' + 1.6007Cr'
            // G = 1.1678Y' - 0.3929Cb' - 0.8152Cr'
            // B = 1.1678Y' + 2.0232Cb'
            return ColorSpaceConversion.YuvToRgba10Bit(y, u, v, a);
        }
    }
}
