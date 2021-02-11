// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Text;

namespace SixLabors.ImageSharp.Textures.Common.Extensions
{
    /// <summary>
    /// To string extension methods.
    /// </summary>
    public static class ToStringExtension
    {
        /// <summary>
        /// Converts a FourCC value to a string.
        /// </summary>
        /// <param name="value">The FourCC.</param>
        /// <returns>A string for the FourCC.</returns>
        public static string FourCcToString(this uint value)
        {
            return Encoding.UTF8.GetString(BitConverter.GetBytes(value));
        }
    }
}
