using System;
using System.Collections.Generic;
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
