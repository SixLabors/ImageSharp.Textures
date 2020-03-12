using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Phoenix.Import.Application
{
    public static class ResourceLoader
    {

        public static bool GetEmbeddedResourceExists(string resourceFileName, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(ResourceLoader).GetTypeInfo().Assembly;
            }

            var resourceNames = assembly.GetManifestResourceNames();

            var resourcePaths = resourceNames
                .Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();

            return resourcePaths.Any() && resourcePaths.Count() <= 1;
        }

        public static Stream GetEmbeddedResourceStream(string resourceFileName, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(ResourceLoader).GetTypeInfo().Assembly;
            }

            var resourceNames = assembly.GetManifestResourceNames();

            var resourcePaths = resourceNames
                .Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();

            if (!resourcePaths.Any())
            {
                throw new Exception($"Resource ending with {resourceFileName} not found.");
            }

            if (resourcePaths.Length > 1)
            {
                throw new Exception(
                    $"Multiple resources ending with {resourceFileName} found: {System.Environment.NewLine}{string.Join(System.Environment.NewLine, resourcePaths)}");
            }

            return assembly.GetManifestResourceStream(resourcePaths.Single());
        }

        public static string GetEmbeddedResourceString(string resourceFileName, Assembly assembly = null)
        {
            var stream = GetEmbeddedResourceStream(resourceFileName, assembly);
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static byte[] GetEmbeddedResourceBytes(string resourceFileName, Assembly assembly = null)
        {
            var stream = GetEmbeddedResourceStream(resourceFileName, assembly);
            using (var streamReader = new MemoryStream())
            {
                stream.CopyTo(streamReader);
                return streamReader.ToArray();
            }
        }
    }
}
