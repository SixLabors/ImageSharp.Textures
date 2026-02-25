// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using AwesomeAssertions;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc.Utils;

internal static class FileBasedHelpers
{
    private static readonly string AstcTestDataRoot = Path.Combine(TestEnvironment.InputImagesDirectoryFullPath, "Astc");

    public static string GetInputPath(string relativePath) => Path.Combine(AstcTestDataRoot, "Input", relativePath);

    public static string GetExpectedPath(string relativePath) => Path.Combine(AstcTestDataRoot, "Expected", relativePath);

    public static string GetHdrPath(string relativePath) => Path.Combine(AstcTestDataRoot, "HDR", relativePath);

    public static byte[] LoadASTCFile(string basename)
    {
        var filename = GetInputPath(basename + ".astc");
        File.Exists(filename).Should().BeTrue(because: $"Testdata missing: {filename}");
        var data = File.ReadAllBytes(filename);
        data.Length.Should().BeGreaterThanOrEqualTo(16, because: "ASTC file too small");
        return data.Skip(16).ToArray();
    }
}
