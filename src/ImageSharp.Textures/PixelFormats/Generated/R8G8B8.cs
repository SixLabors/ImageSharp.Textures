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
    /// Pixel type containing three 8-bit unsigned normalized values ranging from 0 to 255.
    /// The color components are stored in red, green, blue
    /// <para>
    /// Ranges from [0, 0, 0] to [1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public partial struct R8G8B8 : IPixel<R8G8B8>
    {
        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        [FieldOffset(0)]
        public byte R;

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        [FieldOffset(2)]
        public byte B;

        /// <summary>
        /// Initializes a new instance of the <see cref="R8G8B8"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public R8G8B8(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        /// <summary>
        /// Compares two <see cref="R8G8B8"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="R8G8B8"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="R8G8B8"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(R8G8B8 left, R8G8B8 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="R8G8B8"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="R8G8B8"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="R8G8B8"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(R8G8B8 left, R8G8B8 right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<R8G8B8> CreatePixelOperations() => new PixelOperations<R8G8B8>();

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
            this.R = (byte)(vector.X * 255);
            this.G = (byte)(vector.Y * 255);
            this.B = (byte)(vector.Z * 255);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4()
        {
            return new Vector4(
                this.R / 255F,
                this.G / 255F,
                this.B / 255F,
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
        public override bool Equals(object obj) => obj is R8G8B8 other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(R8G8B8 other) => this.R.Equals(other.R) && this.G.Equals(other.G) && this.B.Equals(other.B);

        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"R8G8B8({this.R}, {this.G}, {this.B})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(this.R, this.G, this.B);
    }
}
