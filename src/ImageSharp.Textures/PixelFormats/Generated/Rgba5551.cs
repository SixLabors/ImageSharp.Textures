// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing unsigned normalized values ranging from 0 to 1.
    /// The x, y, z and w components uses 5, 5, 5 and 1 bits respectively.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct Rgba5551 : IPixel<Rgba5551>, IPackedVector<ushort>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba5551"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        /// <param name="w">The w-component</param>
        public Rgba5551(float x, float y, float z, float w)
            : this(new Vector4(x, y, z, w))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba5551"/> struct.
        /// </summary>
        /// <param name="vector">
        /// The vector containing the components for the packed vector.
        /// </param>
        public Rgba5551(Vector4 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc/>
        public ushort PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Rgba5551"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba5551"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="Rgba5551"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rgba5551 left, Rgba5551 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rgba5551"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba5551"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgba5551"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rgba5551 left, Rgba5551 right) => !left.Equals(right);

        /// <inheritdoc />
        public static PixelOperations<Rgba5551> CreatePixelOperations() => new();

        /// <inheritdoc />
        public static PixelTypeInfo GetPixelTypeInfo()
            => PixelTypeInfo.Create<Rgba5551>(
                PixelComponentInfo.Create<Rgba5551>(4, 5, 5, 5, 1),
                PixelColorType.RGB | PixelColorType.Alpha,
                PixelAlphaRepresentation.Unassociated);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToAssociatedScaledVector4() => AlphaConversion.Premultiply(this.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToUnassociatedScaledVector4() => this.ToScaledVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToAssociatedVector4() => AlphaConversion.Premultiply(this.ToVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToUnassociatedVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromAssociatedScaledVector4(Vector4 source) => FromScaledVector4(AlphaConversion.UnPremultiply(source));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromUnassociatedScaledVector4(Vector4 source) => FromScaledVector4(source);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromAssociatedVector4(Vector4 source) => FromVector4(AlphaConversion.UnPremultiply(source));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromUnassociatedVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromScaledVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromVector4(Vector4 source)
        {
            Rgba5551 result = default;
            var vector4 = new Vector4(source.X, source.Y, source.Z, source.W);
            result.PackedValue = Pack(ref vector4);
            return result;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(
                (this.PackedValue & 31) / 31F,
                ((this.PackedValue >> 5) & 31) / 31F,
                ((this.PackedValue >> 10) & 31) / 31F,
                ((this.PackedValue >> 15) & 1) / 1F);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromBgra5551(Bgra5551 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromArgb32(Argb32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromBgr24(Bgr24 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromBgra32(Bgra32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromAbgr32(Abgr32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromL8(L8 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromL16(L16 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromLa16(La16 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromLa32(La32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromRgb24(Rgb24 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromRgba32(Rgba32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rgba32 ToRgba32() => Rgba32.FromScaledVector4(this.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromRgb48(Rgb48 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba5551 FromRgba64(Rgba64 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Rgba5551 other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rgba5551 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Rgba5551({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort Pack(ref Vector4 vector)
        {
            vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);
            return (ushort)(
                ((uint)Math.Round(vector.X * 31F) & 31)
                | (((uint)Math.Round(vector.Y * 31F) & 31) << 5)
                | (((uint)Math.Round(vector.Z * 31F) & 31) << 10)
                | (((uint)Math.Round(vector.W * 1F) & 1) << 15));
        }
    }
}
