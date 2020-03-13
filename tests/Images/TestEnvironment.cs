// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SixLabors.ImageSharp.Textures
{
    public static class TestEnvironment
    {
        private const string ImageSharpTexturesSolutionFileName = "ImageSharp.Textures.sln";

        private const string ToolsRelativePath = @"tests\Tools";

        private const string InputImagesRelativePath = @"tests\Images\Input";

        private const string BaselineDirectoryRelativePath = @"tests\Images\Baseline";

        private const string ActualOutputDirectoryRelativePath = @"tests\Images\ActualOutput";

        private static readonly Lazy<string> SolutionDirectoryFullPathLazy = new Lazy<string>(GetSolutionDirectoryFullPathImpl);

        private static readonly Lazy<bool> RunsOnCiLazy = new Lazy<bool>(
            () => bool.TryParse(Environment.GetEnvironmentVariable("CI"), out bool isCi) && isCi);

        internal static bool RunsOnCI => RunsOnCiLazy.Value;

        internal static string SolutionDirectoryFullPath => SolutionDirectoryFullPathLazy.Value;

        private static string GetSolutionDirectoryFullPathImpl()
        {
            string assemblyLocation = typeof(TestEnvironment).GetTypeInfo().Assembly.Location;

            var assemblyFile = new FileInfo(assemblyLocation);

            DirectoryInfo directory = assemblyFile.Directory;

            while (!directory.EnumerateFiles(ImageSharpTexturesSolutionFileName).Any())
            {
                try
                {
                    directory = directory.Parent;
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Unable to find ImageSharp solution directory from {assemblyLocation} because of {ex.GetType().Name}!",
                        ex);
                }

                if (directory == null)
                {
                    throw new Exception($"Unable to find ImageSharp solution directory from {assemblyLocation}!");
                }
            }

            return directory.FullName;
        }

        private static string GetFullPath(string relativePath) =>
            Path.Combine(SolutionDirectoryFullPath, relativePath)
            .Replace('\\', Path.DirectorySeparatorChar);

        /// <summary>
        /// Gets the correct full path to the Tools directory.
        /// </summary>
        internal static string ToolsDirectoryFullPath => GetFullPath(ToolsRelativePath);

        /// <summary>
        /// Gets the correct full path to the Input Images directory.
        /// </summary>
        internal static string InputImagesDirectoryFullPath => GetFullPath(InputImagesRelativePath);

        /// <summary>
        /// Gets the correct full path to the Baseline directory.
        /// </summary>
        internal static string BaselineDirectoryFullPath => GetFullPath(BaselineDirectoryRelativePath);

        /// <summary>
        /// Gets the correct full path to the Actual Output directory. (To be written to by the test cases.)
        /// </summary>
        internal static string ActualOutputDirectoryFullPath => GetFullPath(ActualOutputDirectoryRelativePath);
    }
}
