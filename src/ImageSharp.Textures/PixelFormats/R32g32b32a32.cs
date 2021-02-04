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
    /// Pixel type containing four 16-bit unsigned normalized values ranging from 0 to 65535.
    /// The color components are stored in red, green, blue and alpha order (least significant to most significant byte).
    /// <para>
    /// Ranges from [0, 0, 0, 1] to [1, 1, 1, 1] in vector form.
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct R32g32b32a32 : IPixel<R32g32b32a32>
    {
        /// <summary>
        /// Gets or sets the red component.
        /// </summary>
        public ushort R;

        /// <summary>
        /// Gets or sets the green component.
        /// </summary>
        public ushort G;

        /// <summary>
        /// Gets or sets the blue component.
        /// </summary>
        public ushort B;

        /// <summary>
        /// Gets or sets the alpha component.
        /// </summary>
        public ushort A;

        /// <summary>
        /// Compares two <see cref="R32g32b32a32"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="R32g32b32a32"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="R32g32b32a32"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(R32g32b32a32 left, R32g32b32a32 right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="R32g32b32a32"/> objects for equality.
        /// </summary>
        /// <param name="left">The <see cref="R32g32b32a32"/> on the left side of the operand.</param>
        /// <param name="right">The <see cref="R32g32b32a32"/> on the right side of the operand.</param>
        /// <returns>
        /// True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(R32g32b32a32 left, R32g32b32a32 right) => !left.Equals(right);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is R32g32b32a32 r32g32b32a32 && this.Equals(r32g32b32a32);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(R32g32b32a32 other) => this.R.Equals(other.R) && this.G.Equals(other.G) && this.B.Equals(other.B) && this.A.Equals(other.A);

        /// <inheritdoc/>
        public override string ToString() => $"R32g32b32a32({this.R}, {this.G}, {this.B}, {this.A})";

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => HashCode.Combine(this.R, this.G, this.B, this.A);

        /// <inheritdoc />
        public PixelOperations<R32g32b32a32> CreatePixelOperations() => new PixelOperations<R32g32b32a32>();

        /// <inheritdoc />
        public void FromArgb32(Argb32 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromBgr24(Bgr24 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromBgra32(Bgra32 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromBgra5551(Bgra5551 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromL16(L16 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromL8(L8 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromLa16(La16 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromLa32(La32 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromRgb24(Rgb24 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromRgb48(Rgb48 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromRgba32(Rgba32 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromRgba64(Rgba64 source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromScaledVector4(Vector4 vector)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void FromVector4(Vector4 vector)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ToRgba32(ref Rgba32 dest)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Vector4 ToScaledVector4()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Vector4 ToVector4()
        {
            throw new NotImplementedException();
        }
    }
}
