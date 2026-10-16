// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Common.Helpers;

namespace SixLabors.ImageSharp.Textures.PixelFormats;

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
    public static PixelOperations<Rgb48Float> CreatePixelOperations() => new();

    /// <inheritdoc />
    public static PixelTypeInfo GetPixelTypeInfo()
        => PixelTypeInfo.Create<Rgb48Float>(
            PixelComponentInfo.Create<Rgb48Float>(3, 16, 16, 16),
            PixelColorType.RGB,
            PixelAlphaRepresentation.None);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToAssociatedScaledVector4() => this.ToScaledVector4();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToUnassociatedScaledVector4() => this.ToScaledVector4();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToAssociatedVector4() => this.ToVector4();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToUnassociatedVector4() => this.ToVector4();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromAssociatedScaledVector4(Vector4 source) => FromScaledVector4(source);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromUnassociatedScaledVector4(Vector4 source) => FromScaledVector4(source);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromAssociatedVector4(Vector4 source) => FromVector4(source);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromUnassociatedVector4(Vector4 source) => FromVector4(source);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromScaledVector4(Vector4 source) => FromVector4(source);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToScaledVector4() => this.ToVector4();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromVector4(Vector4 source)
    {
        Rgb48Float result = default;
        source = Vector4.Clamp(source, Vector4.Zero, Vector4.One);
        result.R = (ushort)FloatHelper.PackFloatToFloat16(source.X);
        result.G = (ushort)FloatHelper.PackFloatToFloat16(source.Y);
        result.B = (ushort)FloatHelper.PackFloatToFloat16(source.Z);
        return result;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector4 ToVector4() => new(
            FloatHelper.UnpackFloat16ToFloat(this.R),
            FloatHelper.UnpackFloat16ToFloat(this.G),
            FloatHelper.UnpackFloat16ToFloat(this.B),
            1.0f);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromBgra5551(Bgra5551 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromArgb32(Argb32 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromBgr24(Bgr24 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromBgra32(Bgra32 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromAbgr32(Abgr32 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromL8(L8 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromL16(L16 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromLa16(La16 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromLa32(La32 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromRgb24(Rgb24 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromRgba32(Rgba32 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Rgba32 ToRgba32() => Rgba32.FromScaledVector4(this.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromRgb48(Rgb48 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rgb48Float FromRgba64(Rgba64 source) => FromScaledVector4(source.ToScaledVector4());

    /// <inheritdoc />
    public override readonly bool Equals(object? obj) => obj is Rgb48Float other && this.Equals(other);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Rgb48Float other) =>
        this.R.Equals(other.R) &&
        this.G.Equals(other.G) &&
        this.B.Equals(other.B);

    /// <inheritdoc />
    public override readonly string ToString()
    {
        Vector4 vector = this.ToVector4();
        return FormattableString.Invariant($"Rgb48Float({vector.X:#0.##}, {vector.Y:#0.##}, {vector.Z:#0.##})");
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly int GetHashCode() => HashCode.Combine(this.R, this.G, this.B);
}
