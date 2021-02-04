// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Text;

namespace SixLabors.ImageSharp.Textures.Common.Extensions
{
    public static class ToStringExtension
    {
        public static string FourCcToString(this uint value)
        {
            return Encoding.UTF8.GetString(BitConverter.GetBytes(value));
        }
    }
}
