// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Pixel type containing four 32-bit unsigned normalized values ranging from 0 to 4294967295.
    /// The color components are stored in red, green, blue, alpha
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public partial struct Rgba128 : IPixel<Rgba128>
    {
        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        [FieldOffset(0)]
        public uint R;

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        [FieldOffset(4)]
        public uint G;

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        [FieldOffset(8)]
        public uint B;

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        [FieldOffset(12)]
        public uint A;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba128"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rgba128(uint r, uint g, uint b, uint a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        /// <summary>
        /// Compares two <see cref="Rgba128"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba128"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="Rgba128"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rgba128 left, Rgba128 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rgba128"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba128"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgba128"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rgba128 left, Rgba128 right) => !left.Equals(right);

        /// <inheritdoc />
        public static PixelOperations<Rgba128> CreatePixelOperations() => new();

        /// <inheritdoc />
        public static PixelTypeInfo GetPixelTypeInfo()
            => PixelTypeInfo.Create<Rgba128>(
                PixelComponentInfo.Create<Rgba128>(4, 32, 32, 32, 32),
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
        public static Rgba128 FromAssociatedScaledVector4(Vector4 source) => FromScaledVector4(AlphaConversion.UnPremultiply(source));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromUnassociatedScaledVector4(Vector4 source) => FromScaledVector4(source);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromAssociatedVector4(Vector4 source) => FromVector4(AlphaConversion.UnPremultiply(source));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromUnassociatedVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromScaledVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromVector4(Vector4 source)
        {
            Rgba128 result = default;
            result.R = (uint)(source.X * 4294967295);
            result.G = (uint)(source.Y * 4294967295);
            result.B = (uint)(source.Z * 4294967295);
            result.A = (uint)(source.W * 4294967295);
            return result;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(
                this.R / 4294967295F,
                this.G / 4294967295F,
                this.B / 4294967295F,
                this.A / 4294967295F);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromBgra5551(Bgra5551 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromArgb32(Argb32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromBgr24(Bgr24 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromBgra32(Bgra32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromAbgr32(Abgr32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromL8(L8 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromL16(L16 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromLa16(La16 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromLa32(La32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromRgb24(Rgb24 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromRgba32(Rgba32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rgba32 ToRgba32() => Rgba32.FromScaledVector4(this.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromRgb48(Rgb48 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rgba128 FromRgba64(Rgba64 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Rgba128 other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rgba128 other) => this.R.Equals(other.R) && this.G.Equals(other.G) && this.B.Equals(other.B) && this.A.Equals(other.A);

        /// <inheritdoc />
        public override string ToString() => FormattableString.Invariant($"Rgba128({this.R}, {this.G}, {this.B}, {this.A})");

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(this.R, this.G, this.B, this.A);
    }
}
