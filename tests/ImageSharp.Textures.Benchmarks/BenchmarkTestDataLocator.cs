// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Benchmarks
{
    public static class BenchmarkTestDataLocator
    {
        /// <summary>
        /// Locates a test data file by searching up from the benchmark directory and into the known test data location.
        /// </summary>
        /// <param name="relativePath">Relative path from the test data root (e.g. "Input/atlas_small_4x4.astc").</param>
        /// <returns>Full path to the test data file, or throws if not found.</returns>
        public static string FindTestData(string relativePath)
        {
            // Walk up from the current directory, searching for ImageSharp.Textures.Astc.Tests/TestData
            string dir = AppContext.BaseDirectory;
            for (int i = 0; i < 10; ++i)
            {
                string testDataDir = Path.Combine(dir, "ImageSharp.Textures.Astc.Tests", "TestData");
                string candidate = Path.Combine(testDataDir, relativePath);
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }

            throw new FileNotFoundException($"Could not locate test data file: {relativePath}");
        }
    }
}
