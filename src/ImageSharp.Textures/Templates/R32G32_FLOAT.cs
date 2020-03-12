// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

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
    public partial struct R32G32_FLOAT : IPixel<R32G32_FLOAT>
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
        /// Initializes a new instance of the <see cref="R32G32_FLOAT"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R32G32_FLOAT(float r, float g)
        {
            this.R = r;
            this.G = g;
        }

        /// <summary>
        /// Compares two <see cref="R32G32_FLOAT"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="R32G32_FLOAT"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="R32G32_FLOAT"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(R32G32_FLOAT left, R32G32_FLOAT right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="R32G32_FLOAT"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="R32G32_FLOAT"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="R32G32_FLOAT"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(R32G32_FLOAT left, R32G32_FLOAT right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<R32G32_FLOAT> CreatePixelOperations() => new PixelOperations<R32G32_FLOAT>();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector)
        {
            this.R = vector.X;
            this.G = vector.Y;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4()
        {
            return new Vector4(
                this.R,
                this.G,
                0.0f,
                1.0f);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => this.FromScaledVector4(source.ToScaledVector4());

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

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            dest.FromScaledVector4(this.ToScaledVector4());
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is R32G32_FLOAT other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(R32G32_FLOAT other) => this.R.Equals(other.R) && this.G.Equals(other.G);

        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"R32G32_FLOAT({this.R}, {this.G})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(this.R, this.G);
    }
}
