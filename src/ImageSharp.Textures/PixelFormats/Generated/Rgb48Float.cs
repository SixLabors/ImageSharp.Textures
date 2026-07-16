// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Common.Helpers;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Pixel type containing three 16-bit float values (half-precision floating-point).
    /// The color components are stored in red, green, blue order.
    /// <para>
    /// Ranges from [0, 0, 0] to [1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public partial struct Rgb48Float : IPixel<Rgb48Float>
    {
        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        [FieldOffset(0)]
        public ushort R;

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        [FieldOffset(2)]
        public ushort G;

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        [FieldOffset(4)]
        public ushort B;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgb48Float"/> struct.
        /// </summary>
        /// <param name="r">The red component as a half-precision float.</param>
        /// <param name="g">The green component as a half-precision float.</param>
        /// <param name="b">The blue component as a half-precision float.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rgb48Float(ushort r, ushort g, ushort b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rgb48Float"/> struct.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rgb48Float(float r, float g, float b)
        {
            this.R = (ushort)FloatHelper.PackFloatToFloat16(r);
            this.G = (ushort)FloatHelper.PackFloatToFloat16(g);
            this.B = (ushort)FloatHelper.PackFloatToFloat16(b);
        }

        /// <summary>
        /// Compares two <see cref="Rgb48Float"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgb48Float"/> on the left side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="right">The <see cref="Rgb48Float"/> on the right side of the operand.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rgb48Float left, Rgb48Float right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Rgb48Float"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="Rgb48Float"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="Rgb48Float"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rgb48Float left, Rgb48Float right) => !left.Equals(right);

        /// <inheritdoc />
        public PixelOperations<Rgb48Float> CreatePixelOperations() => new();

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
            vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);
            this.R = (ushort)FloatHelper.PackFloatToFloat16(vector.X);
            this.G = (ushort)FloatHelper.PackFloatToFloat16(vector.Y);
            this.B = (ushort)FloatHelper.PackFloatToFloat16(vector.Z);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ToVector4() => new(
                FloatHelper.UnpackFloat16ToFloat(this.R),
                FloatHelper.UnpackFloat16ToFloat(this.G),
                FloatHelper.UnpackFloat16ToFloat(this.B),
                1.0f);

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
        public override bool Equals(object? obj) => obj is Rgb48Float other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rgb48Float other) =>
            this.R.Equals(other.R) &&
            this.G.Equals(other.G) &&
            this.B.Equals(other.B);

        /// <inheritdoc />
        public override string ToString()
        {
            var vector = this.ToVector4();
            return FormattableString.Invariant($"Rgb48Float({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##})");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(this.R, this.G, this.B);
    }
}
