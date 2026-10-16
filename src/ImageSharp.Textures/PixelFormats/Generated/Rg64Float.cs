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
    /// Pixel type containing two 32-bit unsigned normalized values ranging from 0 to 4294967295.
    /// The color components are stored in red, green
    /// <para>
    /// Ranges from [0, 0] to [1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public partial struct Rg64Float : IPixel<Rg64Float>
    {
        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        [FieldOffset(0)]
        public float R;

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        [FieldOffset(4)]
        public float G;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rg64Float"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rg64Float(float r, float g)
        {
            this.R = r;
            this.G = g;
        }

        /// <summary>
        /// Compares two <see cref="Rg64Float"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rg64Float"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="Rg64Float"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rg64Float left, Rg64Float right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rg64Float"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rg64Float"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rg64Float"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rg64Float left, Rg64Float right) => !left.Equals(right);

        /// <inheritdoc />
        public static PixelOperations<Rg64Float> CreatePixelOperations() => new();

        /// <inheritdoc />
        public static PixelTypeInfo GetPixelTypeInfo()
            => PixelTypeInfo.Create<Rg64Float>(
                PixelComponentInfo.Create<Rg64Float>(2, 32, 32),
                PixelColorType.Red | PixelColorType.Green,
                PixelAlphaRepresentation.None);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToAssociatedScaledVector4() => this.ToScaledVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToUnassociatedScaledVector4() => this.ToScaledVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToAssociatedVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToUnassociatedVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromAssociatedScaledVector4(Vector4 source) => FromScaledVector4(source);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromUnassociatedScaledVector4(Vector4 source) => FromScaledVector4(source);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromAssociatedVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromUnassociatedVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromScaledVector4(Vector4 source) => FromVector4(source);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromVector4(Vector4 source)
        {
            Rg64Float result = default;
            result.R = source.X;
            result.G = source.Y;
            return result;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(
                this.R,
                this.G,
                0.0f,
                1.0f);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromBgra5551(Bgra5551 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromArgb32(Argb32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromBgr24(Bgr24 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromBgra32(Bgra32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromAbgr32(Abgr32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromL8(L8 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromL16(L16 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromLa16(La16 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromLa32(La32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromRgb24(Rgb24 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromRgba32(Rgba32 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rgba32 ToRgba32() => Rgba32.FromScaledVector4(this.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromRgb48(Rgb48 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rg64Float FromRgba64(Rgba64 source) => FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Rg64Float other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rg64Float other) => this.R.Equals(other.R) && this.G.Equals(other.G);

        /// <inheritdoc />
        public override string ToString() => FormattableString.Invariant($"Rg64Float({this.R}, {this.G})");

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(this.R, this.G);
    }
}
