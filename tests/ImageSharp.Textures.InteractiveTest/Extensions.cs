using System;
using System.Collections.Generic;

namespace Phoenix.Import.Application
{
    public static class Extensions
    {
        public static void AddOrReplace(this Dictionary<string, object> dictionary, string key, object value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
            }
            dictionary.Add(key, value);
        }
    }
}
