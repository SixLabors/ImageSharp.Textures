// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Pixel where each pixel is represented by a 32 bit float value.
    /// </summary>
    public struct Fp32 : IPixel<Fp32>, IPackedVector<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Fp32"/> struct.
        /// </summary>
        /// <param name="x">The x-component.</param>
        public Fp32(float x)
            : this(new Vector<float>(x))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fp32"/> struct.
        /// </summary>
        /// <param name="vector">The vector containing the component values.</param>
        public Fp32(Vector<float> vector) => this.PackedValue = Pack(vector);

        /// <inheritdoc/>
        public float PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Fp32"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Fp32"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Fp32"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fp32 left, Fp32 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Fp32"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Fp32"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Fp32"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fp32 left, Fp32 right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<Fp32> CreatePixelOperations() => new();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => this.PackedValue = (ushort)Pack(new Vector<float>(vector.X));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(this.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        public void ToRgba32(ref Rgba32 dest) => throw new NotImplementedException();

        /// <summary>
        /// Expands the packed representation into a <see cref="Vector"/>.
        /// The vector components are typically expanded in least to greatest significance order.
        /// </summary>
        /// <returns>The <see cref="Vector"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<float> ToVector() => new Vector<float>(this.PackedValue);

        /// <inheritdoc />
        public override readonly bool Equals(object obj) => obj is Fp32 other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Fp32 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override string ToString()
        {
            var vector = this.ToVector();
            return FormattableString.Invariant($"Fp32({vector:#0.##}");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Pack(Vector<float> vector) => vector[0];
    }
}
