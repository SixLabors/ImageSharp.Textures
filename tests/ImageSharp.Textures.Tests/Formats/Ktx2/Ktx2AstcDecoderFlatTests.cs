// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Text.RegularExpressions;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Tests.Enums;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.Attributes;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

[Trait("Format", "Ktx2")]
[Trait("Format", "Astc")]
public partial class Ktx2AstcDecoderFlatTests
{
    private static readonly Ktx2Decoder KtxDecoder = new();

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_4x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_5x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_5x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_6x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_6x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_8x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_8x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_8x8)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_10x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_10x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_10x8)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_10x10)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_12x10)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgba32_12x12)]
    public void Ktx2AstcDecoder_CanDecode_Rgba32_Blocksizes(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(256, firstMipMap.Width);
        Assert.Equal(256, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        Image<Rgba32> firstMipMapImage = firstMipMap as Image<Rgba32>;

        // Note that the comparer is given a higher threshold to allow for the lossy compression of ASTC,
        // especially at larger block sizes, but the output is still expected to be very similar to the reference image.
        // A single reference image is used to save on the amount of test data otherwise required for each block size.
        firstMipMapImage.CompareToReferenceOutput(ImageComparer.TolerantPercentage(4.0f), provider, appendPixelTypeToFileName: false);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_4x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_5x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_5x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_6x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_6x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_8x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_8x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_8x8)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_10x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_10x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_10x8)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_10x10)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_12x10)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_12x12)]
    public void Ktx2AstcDecoder_CanDecode_Rgba32_Unorm(TestTextureProvider provider)
    {
        string blockSize = GetBlockSizeFromFileName(provider.InputFile);
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        (firstMipMap as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.05f), provider, testOutputDetails: $"{blockSize}");
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_4x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_5x4)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_5x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_6x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_6x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_8x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_8x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_8x8)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_10x5)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_10x6)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_10x8)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_10x10)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_12x10)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_sRgb_12x12)]
    public void Ktx2AstcDecoder_CanDecode_Rgba32_Srgb(TestTextureProvider provider)
    {
        string blockSize = GetBlockSizeFromFileName(provider.InputFile);
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        (firstMipMap as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.05f), provider, testOutputDetails: $"{blockSize}");
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Srgb_Large)]
    public void Ktx2AstcDecoder_CanDecode_Rgba32_Srgb_Large(TestTextureProvider provider)
    {
        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(2048, firstMipMap.Width);
        Assert.Equal(2048, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        (firstMipMap as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.Exact, provider);
    }

    [Theory]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Srgb_6x6_MipMap)]
    public void Ktx2AstcDecoder_CanDecode_MipMaps(TestTextureProvider provider)
    {
        int mimMapLevel = 0;

        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        foreach (MipMap mipMap in flatTexture.MipMaps)
        {
            using Image image = mipMap.GetImage();
            (image as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.Exact, provider, testOutputDetails: $"{mimMapLevel++}");
        }
    }

    [Theory(Skip = "Supercompression support not yet implemented")]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_4x4_Zlib1)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_4x4_Zlib9)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_4x4_Zstd1)]
    [WithFile(TestTextureFormat.Ktx2, TestTextureType.Flat, TestTextureTool.ToKtx, TestImages.Ktx2.Astc.Rgb32_Unorm_4x4_Zstd9)]
    public void Ktx2AstcDecoder_CanDecode_Rgba32_Supercompressed(TestTextureProvider provider)
    {
        string fileName = Path.GetFileNameWithoutExtension(provider.InputFile);
        string compressionDetails = fileName.Contains("ZLIB", StringComparison.Ordinal)
            ? fileName[fileName.IndexOf("ZLIB", StringComparison.Ordinal)..]
            : fileName[fileName.IndexOf("ZSTD", StringComparison.Ordinal)..];

        using Texture texture = provider.GetTexture(KtxDecoder);
        provider.SaveTextures(texture);
        FlatTexture flatTexture = texture as FlatTexture;

        Assert.NotNull(flatTexture?.MipMaps);
        Assert.Single(flatTexture.MipMaps);

        using Image firstMipMap = flatTexture.MipMaps[0].GetImage();
        Assert.Equal(16, firstMipMap.Width);
        Assert.Equal(16, firstMipMap.Height);
        Assert.Equal(32, firstMipMap.PixelType.BitsPerPixel);

        (firstMipMap as Image<Rgba32>).CompareToReferenceOutput(ImageComparer.TolerantPercentage(0.05f), provider, testOutputDetails: $"{compressionDetails}");
    }

    private static string GetBlockSizeFromFileName(string fileName)
    {
        Match match = GetBlockSizeFromFileName().Match(fileName);

        return match.Success ? match.Value : string.Empty;
    }

    [GeneratedRegex(@"(\d+x\d+)")]
    private static partial Regex GetBlockSizeFromFileName();
}
