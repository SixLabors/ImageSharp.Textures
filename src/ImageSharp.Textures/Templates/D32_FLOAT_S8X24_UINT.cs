// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Common.Helpers;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing unsigned normalized values ranging from 0 to 1.
    /// The x and y components uses 32 and 8 bits respectively.
    /// <para>
    /// Ranges from [0, 0] to [1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct D32_FLOAT_S8X24_UINT : IPixel<D32_FLOAT_S8X24_UINT>, IPackedVector<ulong>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="D32_FLOAT_S8X24_UINT"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        public D32_FLOAT_S8X24_UINT(float x, float y)
            : this(new Vector2(x, y))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="D32_FLOAT_S8X24_UINT"/> struct.
        /// </summary>
        /// <param name="vector">
        /// The vector containing the components for the packed vector.
        /// </param>
        public D32_FLOAT_S8X24_UINT(Vector2 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc/>
        public ulong PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="D32_FLOAT_S8X24_UINT"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="D32_FLOAT_S8X24_UINT"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="D32_FLOAT_S8X24_UINT"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(D32_FLOAT_S8X24_UINT left, D32_FLOAT_S8X24_UINT right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="D32_FLOAT_S8X24_UINT"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="D32_FLOAT_S8X24_UINT"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="D32_FLOAT_S8X24_UINT"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(D32_FLOAT_S8X24_UINT left, D32_FLOAT_S8X24_UINT right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<D32_FLOAT_S8X24_UINT> CreatePixelOperations() => new PixelOperations<D32_FLOAT_S8X24_UINT>();

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
            var vector2 = new Vector2(vector.X, vector.Y);
            this.PackedValue = Pack(ref vector2);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4()
        {
            return new Vector4(
                FloatHelper.UnpackFloat32ToFloat((uint)(this.PackedValue & 4294967295)),
                ((this.PackedValue >> 32) & 255) / 255F,
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
        public override bool Equals(object obj) => obj is D32_FLOAT_S8X24_UINT other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(D32_FLOAT_S8X24_UINT other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"D32_FLOAT_S8X24_UINT({vector.X:#0.##}, {vector.Y:#0.##})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Pack(ref Vector2 vector)
        {
            vector = Vector2.Clamp(vector, Vector2.Zero, Vector2.One);
            return (ulong)(
                (uint)FloatHelper.PackFloatToFloat32(vector.X)
                | (((uint)Math.Round(vector.Y * 255F) & 255) << 32));
        }
    }
}
