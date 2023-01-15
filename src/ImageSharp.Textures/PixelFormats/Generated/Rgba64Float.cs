// Copyright (c) Six Labors.
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
    /// The x, y, z and w components use 16 bits.
    /// <para>
    /// Ranges from [0, 0, 0, 0] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public partial struct Rgba64Float : IPixel<Rgba64Float>, IPackedVector<ulong>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba64Float"/> struct.
        /// </summary>
        /// <param name="x">The x-component</param>
        /// <param name="y">The y-component</param>
        /// <param name="z">The z-component</param>
        /// <param name="w">The w-component</param>
        public Rgba64Float(float x, float y, float z, float w)
            : this(new Vector4(x, y, z, w))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgba64Float"/> struct.
        /// </summary>
        /// <param name="vector">
        /// The vector containing the components for the packed vector.
        /// </param>
        public Rgba64Float(Vector4 vector) => this.PackedValue = Pack(ref vector);

        /// <inheritdoc/>
        public ulong PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="Rgba64Float"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba64Float"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="Rgba64Float"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rgba64Float left, Rgba64Float right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rgba64Float"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgba64Float"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgba64Float"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rgba64Float left, Rgba64Float right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<Rgba64Float> CreatePixelOperations() => new();

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
            var vector4 = new Vector4(vector.X, vector.Y, vector.Z, vector.W);
            this.PackedValue = Pack(ref vector4);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(
                FloatHelper.UnpackFloat16ToFloat((ushort)(this.PackedValue & 65535)),
                FloatHelper.UnpackFloat16ToFloat((ushort)((this.PackedValue >> 16) & 65535)),
                FloatHelper.UnpackFloat16ToFloat((ushort)((this.PackedValue >> 32) & 65535)),
                FloatHelper.UnpackFloat16ToFloat((ushort)((this.PackedValue >> 48) & 65535)));

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

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => this.FromScaledVector4(source.ToScaledVector4());

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
        public override bool Equals(object obj) => obj is Rgba64Float other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rgba64Float other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Rgba64Float({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##}, {vector.W:#0.##})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Pack(ref Vector4 vector)
        {
            vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);
            return (ulong)(
                (uint)FloatHelper.PackFloatToFloat16(vector.X)
                | ((uint)FloatHelper.PackFloatToFloat16(vector.Y) << 16)
                | ((uint)FloatHelper.PackFloatToFloat16(vector.Z) << 32)
                | ((uint)FloatHelper.PackFloatToFloat16(vector.W) << 48));
        }
    }
}
