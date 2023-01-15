// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Textures.PixelFormats
{
    /// <summary>
    /// Packed pixel type containing a single 32-bit normalized luminance value.
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    public struct L32 : IPixel<L32>, IPackedVector<uint>
    {
        private const float Max = uint.MaxValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="L32"/> struct.
        /// </summary>
        /// <param name="luminance">The luminance component</param>
        public L32(uint luminance) => this.PackedValue = luminance;

        /// <inheritdoc />
        public uint PackedValue { get; set; }

        /// <summary>
        /// Compares two <see cref="L16"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="L16"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="L16"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(L32 left, L32 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="L32"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="L32"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="L32"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(L32 left, L32 right) => !left.Equals(right);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromScaledVector4(Vector4 vector) => this.ConvertFromRgbaScaledVector4(vector);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToScaledVector4() => this.ToVector4();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromVector4(Vector4 vector) => this.ConvertFromRgbaScaledVector4(vector);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector4 ToVector4()
        {
            float scaled = this.PackedValue / Max;
            return new Vector4(scaled, scaled, scaled, 1F);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromArgb32(Argb32 source) => this.PackedValue = Get16BitBT709Luminance(
                UpscaleFrom8BitTo16Bit(source.R),
                UpscaleFrom8BitTo16Bit(source.G),
                UpscaleFrom8BitTo16Bit(source.B));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgr24(Bgr24 source) => this.PackedValue = Get16BitBT709Luminance(
                UpscaleFrom8BitTo16Bit(source.R),
                UpscaleFrom8BitTo16Bit(source.G),
                UpscaleFrom8BitTo16Bit(source.B));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra32(Bgra32 source) => this.PackedValue = Get16BitBT709Luminance(
                UpscaleFrom8BitTo16Bit(source.R),
                UpscaleFrom8BitTo16Bit(source.G),
                UpscaleFrom8BitTo16Bit(source.B));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromAbgr32(Abgr32 source) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromBgra5551(Bgra5551 source) => this.FromScaledVector4(source.ToScaledVector4());

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL8(L8 source) => this.PackedValue = UpscaleFrom8BitTo16Bit(source.PackedValue);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromL16(L16 source) => this = new L32(source.PackedValue);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa16(La16 source) => this.PackedValue = UpscaleFrom8BitTo16Bit(source.L);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromLa32(La32 source) => this.PackedValue = source.L;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb24(Rgb24 source) => this.PackedValue = Get16BitBT709Luminance(
                UpscaleFrom8BitTo16Bit(source.R),
                UpscaleFrom8BitTo16Bit(source.G),
                UpscaleFrom8BitTo16Bit(source.B));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba32(Rgba32 source) => this.PackedValue = Get16BitBT709Luminance(
                UpscaleFrom8BitTo16Bit(source.R),
                UpscaleFrom8BitTo16Bit(source.G),
                UpscaleFrom8BitTo16Bit(source.B));

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToRgba32(ref Rgba32 dest)
        {
            byte rgb = DownScaleFrom32BitTo8Bit(this.PackedValue);
            dest.R = rgb;
            dest.G = rgb;
            dest.B = rgb;
            dest.A = byte.MaxValue;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgb48(Rgb48 source) => this.PackedValue = Get16BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromRgba64(Rgba64 source) => this.PackedValue = Get16BitBT709Luminance(source.R, source.G, source.B);

        /// <inheritdoc />
        public PixelOperations<L32> CreatePixelOperations() => new();

        /// <inheritdoc />
        public override readonly bool Equals(object obj) => obj is L16 other && this.Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(L32 other) => this.PackedValue.Equals(other.PackedValue);

        /// <inheritdoc />
        public override readonly string ToString() => $"L32({this.PackedValue})";

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => this.PackedValue.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ConvertFromRgbaScaledVector4(Vector4 vector)
        {
            vector = Clamp(vector, Vector4.Zero, Vector4.One) * Max;
            this.PackedValue = Get16BitBT709Luminance(
                vector.X,
                vector.Y,
                vector.Z);
        }

        /// <summary>
        /// Gets the luminance from the rgb components using the formula as
        /// specified by ITU-R Recommendation BT.709.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <returns>The <see cref="ushort"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort Get16BitBT709Luminance(ushort r, ushort g, ushort b)
            => (ushort)((r * .2126F) + (g * .7152F) + (b * .0722F) + 0.5F);

        /// <summary>
        /// Gets the luminance from the rgb components using the formula as specified
        /// by ITU-R Recommendation BT.709.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <returns>The <see cref="ushort"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort Get16BitBT709Luminance(float r, float g, float b)
            => (ushort)((r * .2126F) + (g * .7152F) + (b * .0722F) + 0.5F);

        /// <summary>
        /// Scales a value from an 8 bit <see cref="byte"/> to
        /// an 16 bit <see cref="ushort"/> equivalent.
        /// </summary>
        /// <param name="component">The 8 bit component value.</param>
        /// <returns>The <see cref="ushort"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort UpscaleFrom8BitTo16Bit(byte component)
            => (ushort)(component * 257);

        /// <summary>
        /// Returns the value clamped to the inclusive range of min and max.
        /// 5x Faster than <see cref="Vector4.Clamp(Vector4, Vector4, Vector4)"/>
        /// on platforms &lt; NET 5.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum inclusive value.</param>
        /// <param name="max">The maximum inclusive value.</param>
        /// <returns>The clamped <see cref="Vector4"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 Clamp(Vector4 value, Vector4 min, Vector4 max)
            => Vector4.Min(Vector4.Max(value, min), max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte DownScaleFrom32BitTo8Bit(uint component)
        {
            ushort componentAsShort = (ushort)(component >> 16);
            return DownScaleFrom16BitTo8Bit(componentAsShort);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte DownScaleFrom16BitTo8Bit(ushort component)
        {
            // To scale to 8 bits From a 16-bit value V the required value (from the PNG specification) is:
            //
            //    (V * 255) / 65535
            //
            // This reduces to round(V / 257), or floor((V + 128.5)/257)
            //
            // Represent V as the two byte value vhi.vlo.  Make a guess that the
            // result is the top byte of V, vhi, then the correction to this value
            // is:
            //
            //    error = floor(((V-vhi.vhi) + 128.5) / 257)
            //          = floor(((vlo-vhi) + 128.5) / 257)
            //
            // This can be approximated using integer arithmetic (and a signed
            // shift):
            //
            //    error = (vlo-vhi+128) >> 8;
            //
            // The approximate differs from the exact answer only when (vlo-vhi) is
            // 128; it then gives a correction of +1 when the exact correction is
            // 0.  This gives 128 errors.  The exact answer (correct for all 16-bit
            // input values) is:
            //
            //    error = (vlo-vhi+128)*65535 >> 24;
            //
            // An alternative arithmetic calculation which also gives no errors is:
            //
            //    (V * 255 + 32895) >> 16
            return (byte)(((component * 255) + 32895) >> 16);
        }
    }
}
