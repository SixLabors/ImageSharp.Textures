// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Benchmarks;

public static class BenchmarkTestDataLocator
{
    private const string SolutionFileName = "ImageSharp.Textures.sln";

    /// <summary>
    /// Locates a test data file under tests/Images/Input/Astc by walking up
    /// from the benchmark output directory to find the solution root.
    /// </summary>
    /// <param name="relativePath">Relative path from the Astc test data root (e.g. "Input/atlas_small_4x4.astc").</param>
    /// <returns>Full path to the test data file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file cannot be found.</exception>
    public static string FindAstcTestData(string relativePath)
    {
        string dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; ++i)
        {
            if (File.Exists(Path.Combine(dir, SolutionFileName)))
            {
                string candidate = Path.Combine(dir, "tests", "Images", "Input", "Astc", relativePath);
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }

            dir = Path.GetFullPath(Path.Combine(dir, ".."));
        }

        throw new FileNotFoundException($"Could not locate test data file: {relativePath}");
    }
}
