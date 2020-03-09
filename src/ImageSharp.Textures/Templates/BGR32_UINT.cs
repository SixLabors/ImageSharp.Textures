// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing unsigned normalized values ranging from 0 to 1.
    /// The x, y and z components use 8 bits.
    /// <para>
    /// Ranges from [0, 0, 0] to [1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct BGR32_UINT : IPixel<BGR32_UINT>, IPackedVector<uint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BGR32_UINT"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        public BGR32_UINT(float x, float y, float z)
            : this(new Vector3(x, y, z))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BGR32_UINT"/> struct.
        /// </summary>
        /// <param name="vector">
        /// The vector containing the components for the packed vector.
        /// </param>
        public BGR32_UINT(Vector3 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc/>
        public uint PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="BGR32_UINT"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="BGR32_UINT"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="BGR32_UINT"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BGR32_UINT left, BGR32_UINT right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="BGR32_UINT"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="BGR32_UINT"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="BGR32_UINT"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BGR32_UINT left, BGR32_UINT right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<BGR32_UINT> CreatePixelOperations() => new PixelOperations<BGR32_UINT>();

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
            var vector3 = new Vector3(vector.X, vector.Y, vector.Z);
            this.PackedValue = Pack(ref vector3);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4()
        {
            return new Vector4(
                ((this.PackedValue >> 16) & 0xFF) / 255F,
                ((this.PackedValue >> 8) & 0xFF) / 255F,
                (this.PackedValue & 0xFF) / 255F,
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
        public override bool Equals(object obj) => obj is BGR32_UINT other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BGR32_UINT other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"BGR32_UINT({vector.Z:#0.##}, {vector.Y:#0.##}, {vector.X:#0.##})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Pack(ref Vector3 vector)
        {
            vector = Vector3.Clamp(vector, Vector3.Zero, Vector3.One);
            return (uint)(
                (((uint)Math.Round(vector.X * 255F) & 0xFF) << 16)
                | (((uint)Math.Round(vector.Y * 255F) & 0xFF) << 8)
                | ((uint)Math.Round(vector.Z * 255F) & 0xFF));
        }
    }
}
