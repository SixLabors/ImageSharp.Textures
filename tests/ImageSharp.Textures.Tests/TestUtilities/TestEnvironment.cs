// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities
{
    public static class TestEnvironment
    {
        private static readonly Lazy<ImageSharp.Configuration> ConfigurationLazy = new Lazy<ImageSharp.Configuration>(CreateDefaultConfiguration);

        internal static ImageSharp.Configuration Configuration => ConfigurationLazy.Value;

        private const string ImageSharpTexturesSolutionFileName = "ImageSharp.Textures.sln";

        private const string InputImagesRelativePath = @"tests\Images\Input";

        private const string ActualOutputDirectoryRelativePath = @"tests\Images\ActualOutput";

        private const string ReferenceOutputDirectoryRelativePath = @"tests\Images\ReferenceOutput";

        private const string ToolsDirectoryRelativePath = @"tests\Images\External\tools";

        private static readonly Lazy<string> SolutionDirectoryFullPathLazy = new Lazy<string>(GetSolutionDirectoryFullPathImpl);

        private static readonly Lazy<string> NetCoreVersionLazy = new Lazy<string>(GetNetCoreVersion);

        static TestEnvironment() => PrepareRemoteExecutor();

        /// <summary>
        /// Gets the .NET Core version, if running on .NET Core, otherwise returns an empty string.
        /// </summary>
        internal static string NetCoreVersion => NetCoreVersionLazy.Value;

        // ReSharper disable once InconsistentNaming

        /// <summary>
        /// Gets a value indicating whether test execution runs on CI.
        /// </summary>
#if ENV_CI
        internal static bool RunsOnCI => true;
#else
        internal static bool RunsOnCI => false;
#endif

        /// <summary>
        /// Gets a value indicating whether test execution is running with code coverage testing enabled.
        /// </summary>
#if ENV_CODECOV
        internal static bool RunsWithCodeCoverage => true;
#else
        internal static bool RunsWithCodeCoverage => false;
#endif

        internal static string SolutionDirectoryFullPath => SolutionDirectoryFullPathLazy.Value;

        private static readonly FileInfo TestAssemblyFile =
            new FileInfo(typeof(TestEnvironment).GetTypeInfo().Assembly.Location);

        private static string GetSolutionDirectoryFullPathImpl()
        {
            DirectoryInfo directory = TestAssemblyFile.Directory;

            while (!directory.EnumerateFiles(ImageSharpTexturesSolutionFileName).Any())
            {
                try
                {
                    directory = directory.Parent;
                }
                catch (Exception ex)
                {
                    throw new DirectoryNotFoundException(
                        $"Unable to find ImageSharp solution directory from {TestAssemblyFile} because of {ex.GetType().Name}!",
                        ex);
                }

                if (directory == null)
                {
                    throw new DirectoryNotFoundException($"Unable to find ImageSharp solution directory from {TestAssemblyFile}!");
                }
            }

            return directory.FullName;
        }

        private static string GetFullPath(string relativePath) =>
            Path.Combine(SolutionDirectoryFullPath, relativePath)
            .Replace('\\', Path.DirectorySeparatorChar);

        /// <summary>
        /// Gets the correct full path to the Input Images directory.
        /// </summary>
        internal static string InputImagesDirectoryFullPath => GetFullPath(InputImagesRelativePath);

        /// <summary>
        /// Gets the correct full path to the Actual Output directory. (To be written to by the test cases.)
        /// </summary>
        internal static string ActualOutputDirectoryFullPath => GetFullPath(ActualOutputDirectoryRelativePath);

        /// <summary>
        /// Gets the correct full path to the Expected Output directory. (To compare the test results to.)
        /// </summary>
        internal static string ReferenceOutputDirectoryFullPath => GetFullPath(ReferenceOutputDirectoryRelativePath);

        internal static string ToolsDirectoryFullPath => GetFullPath(ToolsDirectoryRelativePath);

        internal static string GetReferenceOutputFileName(string actualOutputFileName) =>
            actualOutputFileName.Replace("ActualOutput", @"ReferenceOutput").Replace('\\', Path.DirectorySeparatorChar);

        internal static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        internal static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        internal static bool IsMono => Type.GetType("Mono.Runtime") != null; // https://stackoverflow.com/a/721194

        internal static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        internal static bool Is64BitProcess => IntPtr.Size == 8;

        internal static bool IsFramework => string.IsNullOrEmpty(NetCoreVersion);

        /// <summary>
        /// A dummy operation to enforce the execution of the static constructor.
        /// </summary>
        internal static void EnsureSharedInitializersDone()
        {
        }

        /// <summary>
        /// Creates the image output directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathParts">The path parts.</param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal static string CreateOutputDirectory(string path, params string[] pathParts)
        {
            path = Path.Combine(ActualOutputDirectoryFullPath, path);

            if (pathParts != null && pathParts.Length > 0)
            {
                path = Path.Combine(path, Path.Combine(pathParts));
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Creates Microsoft.DotNet.RemoteExecutor.exe.config for .NET framework,
        /// When running in 32 bits, enforces 32 bit execution of Microsoft.DotNet.RemoteExecutor.exe
        /// with the help of CorFlags.exe found in Windows SDK.
        /// </summary>
        private static void PrepareRemoteExecutor()
        {
            if (!IsFramework)
            {
                return;
            }

            string remoteExecutorConfigPath =
                Path.Combine(TestAssemblyFile.DirectoryName, "Microsoft.DotNet.RemoteExecutor.exe.config");

            if (File.Exists(remoteExecutorConfigPath))
            {
                // Already initialized
                return;
            }

            string testProjectConfigPath = TestAssemblyFile.FullName + ".config";
            if (File.Exists(testProjectConfigPath))
            {
                File.Copy(testProjectConfigPath, remoteExecutorConfigPath);
            }

            if (Is64BitProcess)
            {
                return;
            }

            EnsureRemoteExecutorIs32Bit();
        }

        /// <summary>
        /// Locate and run CorFlags.exe /32Bit+
        /// https://docs.microsoft.com/en-us/dotnet/framework/tools/corflags-exe-corflags-conversion-tool
        /// </summary>
        private static void EnsureRemoteExecutorIs32Bit()
        {
            string windowsSdksDir = Path.Combine(
                Environment.GetEnvironmentVariable("PROGRAMFILES(x86)"),
                "Microsoft SDKs",
                "Windows");

            FileInfo corFlagsFile = Find(new DirectoryInfo(windowsSdksDir), "CorFlags.exe");

            string remoteExecutorPath = Path.Combine(TestAssemblyFile.DirectoryName, "Microsoft.DotNet.RemoteExecutor.exe");

            string remoteExecutorTmpPath = $"{remoteExecutorPath}._tmp";

            if (File.Exists(remoteExecutorTmpPath))
            {
                // Already initialized
                return;
            }

            File.Copy(remoteExecutorPath, remoteExecutorTmpPath);

            string args = $"{remoteExecutorTmpPath} /32Bit+ /Force";

            var si = new ProcessStartInfo()
            {
                FileName = corFlagsFile.FullName,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(si);
            proc.WaitForExit();
            string standardOutput = proc.StandardOutput.ReadToEnd();
            string standardError = proc.StandardError.ReadToEnd();

            if (proc.ExitCode != 0)
            {
                throw new Exception(
                    $@"Failed to run {si.FileName} {si.Arguments}:\n STDOUT: {standardOutput}\n STDERR: {standardError}");
            }

            File.Delete(remoteExecutorPath);
            File.Copy(remoteExecutorTmpPath, remoteExecutorPath);

            static FileInfo Find(DirectoryInfo root, string name)
            {
                FileInfo fi = root.EnumerateFiles(name).FirstOrDefault();
                if (fi != null)
                {
                    return fi;
                }

                foreach (DirectoryInfo dir in root.EnumerateDirectories())
                {
                    fi = Find(dir, name);
                    if (fi != null)
                    {
                        return fi;
                    }
                }

                return null;
            }
        }

        internal static IImageDecoder GetReferenceDecoder(string filePath)
        {
            IImageFormat format = GetImageFormat(filePath);
            return Configuration.ImageFormatsManager.FindDecoder(format);
        }

        internal static IImageEncoder GetReferenceEncoder(string filePath)
        {
            IImageFormat format = GetImageFormat(filePath);
            return Configuration.ImageFormatsManager.FindEncoder(format);
        }

        internal static IImageFormat GetImageFormat(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            return Configuration.ImageFormatsManager.FindFormatByFileExtension(extension);
        }

        private static void ConfigureCodecs(
            this ImageSharp.Configuration cfg,
            IImageFormat imageFormat,
            IImageDecoder decoder,
            IImageEncoder encoder,
            IImageFormatDetector detector)
        {
            cfg.ImageFormatsManager.SetDecoder(imageFormat, decoder);
            cfg.ImageFormatsManager.SetEncoder(imageFormat, encoder);
            cfg.ImageFormatsManager.AddImageFormatDetector(detector);
        }

        private static ImageSharp.Configuration CreateDefaultConfiguration()
        {
            var cfg = new ImageSharp.Configuration();

            cfg.ConfigureCodecs(
                PngFormat.Instance,
                new PngDecoder(),
                new PngEncoder(),
                new PngImageFormatDetector());

            return cfg;
        }

        /// <summary>
        /// Solution borrowed from:
        /// https://github.com/dotnet/BenchmarkDotNet/issues/448#issuecomment-308424100
        /// </summary>
        private static string GetNetCoreVersion()
        {
            Assembly assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            string[] assemblyPath = assembly.Location.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
            {
                return assemblyPath[netCoreAppIndex + 1];
            }

            return string.Empty;
        }
    }
}
