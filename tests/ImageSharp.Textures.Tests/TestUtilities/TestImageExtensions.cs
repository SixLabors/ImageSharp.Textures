// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities.TextureProviders;
using SixLabors.ImageSharp.Textures.TextureFormats;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities;

public static class TestImageExtensions
{
    public static void DebugSave(
        this Image image,
        ITestTextureProvider provider,
        FormattableString testOutputDetails,
        string extension = "png",
        bool appendPixelTypeToFileName = false,
        bool appendSourceFileOrDescription = false,
        IImageEncoder encoder = null) => image.DebugSave(
            provider,
            (object)testOutputDetails,
            extension,
            appendPixelTypeToFileName,
            appendSourceFileOrDescription,
            encoder);

    /// <summary>
    /// Saves the image only when not running in the CI server.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="provider">The image provider.</param>
    /// <param name="testOutputDetails">Details to be concatenated to the test output file, describing the parameters of the test.</param>
    /// <param name="extension">The extension.</param>
    /// <param name="appendPixelTypeToFileName">A boolean indicating whether to append the pixel type to the  output file name.</param>
    /// <param name="appendSourceFileOrDescription">A boolean indicating whether to append SourceFileOrDescription to the test output file name.</param>
    /// <param name="encoder">Custom encoder to use.</param>
    /// <returns>The input image.</returns>
    public static Image DebugSave(
        this Image image,
        ITestTextureProvider provider,
        object testOutputDetails = null,
        string extension = "png",
        bool appendPixelTypeToFileName = false,
        bool appendSourceFileOrDescription = false,
        IImageEncoder encoder = null)
    {
        if (TestEnvironment.RunsOnCI)
        {
            return image;
        }

        // We are running locally then we want to save it out
        provider.Utility.SaveTestOutputFile(
            image,
            extension,
            testOutputDetails: testOutputDetails,
            appendPixelTypeToFileName: appendPixelTypeToFileName,
            appendSourceFileOrDescription: appendSourceFileOrDescription,
            encoder: encoder);
        return image;
    }

    public static void DebugSave(
        this Image image,
        ITestTextureProvider provider,
        IImageEncoder encoder,
        FormattableString testOutputDetails,
        bool appendPixelTypeToFileName = false) => image.DebugSave(provider, encoder, (object)testOutputDetails, appendPixelTypeToFileName);

    /// <summary>
    /// Saves the image only when not running in the CI server.
    /// </summary>
    /// <param name="image">The image</param>
    /// <param name="provider">The image provider</param>
    /// <param name="encoder">The image encoder</param>
    /// <param name="testOutputDetails">Details to be concatenated to the test output file, describing the parameters of the test.</param>
    /// <param name="appendPixelTypeToFileName">A boolean indicating whether to append the pixel type to the output file name.</param>
    public static void DebugSave(
        this Image image,
        ITestTextureProvider provider,
        IImageEncoder encoder,
        object testOutputDetails = null,
        bool appendPixelTypeToFileName = false)
    {
        if (TestEnvironment.RunsOnCI)
        {
            return;
        }

        // We are running locally then we want to save it out
        provider.Utility.SaveTestOutputFile(
            image,
            encoder: encoder,
            testOutputDetails: testOutputDetails,
            appendPixelTypeToFileName: appendPixelTypeToFileName);
    }

    public static Image<TPixel> CompareToReferenceOutput<TPixel>(
        this Image<TPixel> image,
        ITestTextureProvider provider,
        FormattableString testOutputDetails,
        string extension = "png",
        bool appendPixelTypeToFileName = false,
        bool appendSourceFileOrDescription = false)
        where TPixel : unmanaged, IPixel<TPixel> => image.CompareToReferenceOutput(
            provider,
            (object)testOutputDetails,
            extension,
            appendPixelTypeToFileName,
            appendSourceFileOrDescription);

    /// <summary>
    /// Compares the image against the expected Reference output, throws an exception if the images are not similar enough.
    /// The output file should be named identically to the output produced by <see cref="DebugSave{TPixel}(Image{TPixel}, ITestImageProvider, object, string, bool)"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    /// <param name="image">The image which should be compared to the reference image.</param>
    /// <param name="provider">The image provider.</param>
    /// <param name="testOutputDetails">Details to be concatenated to the test output file, describing the parameters of the test.</param>
    /// <param name="extension">The extension</param>
    /// <param name="appendPixelTypeToFileName">A boolean indicating whether to append the pixel type to the  output file name.</param>
    /// <param name="appendSourceFileOrDescription">A boolean indicating whether to append <see cref="ITestTextureProvider.SourceFileOrDescription"/> to the test output file name.</param>
    /// <returns>The image.</returns>
    public static Image<TPixel> CompareToReferenceOutput<TPixel>(
        this Image<TPixel> image,
        ITestTextureProvider provider,
        object testOutputDetails = null,
        string extension = "png",
        bool appendPixelTypeToFileName = false,
        bool appendSourceFileOrDescription = false)
        where TPixel : unmanaged, IPixel<TPixel> => CompareToReferenceOutput(
            image,
            ImageComparer.Tolerant(),
            provider,
            testOutputDetails,
            extension,
            appendPixelTypeToFileName,
            appendSourceFileOrDescription);

    public static Image<TPixel> CompareToReferenceOutput<TPixel>(
        this Image<TPixel> image,
        ImageComparer comparer,
        ITestTextureProvider provider,
        FormattableString testOutputDetails,
        string extension = "png",
        bool appendPixelTypeToFileName = false)
        where TPixel : unmanaged, IPixel<TPixel> => image.CompareToReferenceOutput(
            comparer,
            provider,
            (object)testOutputDetails,
            extension,
            appendPixelTypeToFileName);

    /// <summary>
    /// Compares the image against the expected Reference output, throws an exception if the images are not similar enough.
    /// The output file should be named identically to the output produced by <see cref="DebugSave{TPixel}(Image{TPixel}, ITestTextureProvider, object, string, bool)"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    /// <param name="image">The image which should be compared to the reference output.</param>
    /// <param name="comparer">The <see cref="ImageComparer"/> to use.</param>
    /// <param name="provider">The image provider.</param>
    /// <param name="testOutputDetails">Details to be concatenated to the test output file, describing the parameters of the test.</param>
    /// <param name="extension">The extension</param>
    /// <param name="appendPixelTypeToFileName">A boolean indicating whether to append the pixel type to the  output file name.</param>
    /// <param name="appendSourceFileOrDescription">A boolean indicating whether to append SourceFileOrDescription to the test output file name.</param>
    /// <param name="decoder">A custom decoder.</param>
    /// <returns>The image.</returns>
    public static Image<TPixel> CompareToReferenceOutput<TPixel>(
        this Image<TPixel> image,
        ImageComparer comparer,
        ITestTextureProvider provider,
        object testOutputDetails = null,
        string extension = "png",
        bool appendPixelTypeToFileName = false,
        bool appendSourceFileOrDescription = false,
        IImageDecoder decoder = null)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        using (Image<TPixel> referenceImage = GetReferenceOutputImage<TPixel>(
            provider,
            testOutputDetails,
            extension,
            appendPixelTypeToFileName,
            appendSourceFileOrDescription,
            decoder))
        {
            comparer.VerifySimilarity(referenceImage, image);
        }

        return image;
    }

    /// <summary>
    /// Compares all six faces of the cubemap's first mipmap against their individual reference images.
    /// Reference files must be named "{testName}_{facePrefix}posX{faceSuffix}.png" etc.,
    /// matching the saved debug output.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    /// <param name="cubemap">The decoded cubemap.</param>
    /// <param name="comparer">The comparer to use for every face.</param>
    /// <param name="provider">The test texture provider.</param>
    /// <param name="faceSuffix">
    /// Optional suffix appended after the face name in the reference filename. Useful for
    /// parameterized cubemap tests that share a single test name across multiple inputs
    /// (e.g. a block-size suffix).
    /// </param>
    public static void CompareFacesToReferenceOutput<TPixel>(
        this CubemapTexture cubemap,
        ImageComparer comparer,
        ITestTextureProvider provider,
        string faceSuffix = null)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        CompareFace<TPixel>(cubemap.PositiveX, BuildDetails("posX", faceSuffix), comparer, provider);
        CompareFace<TPixel>(cubemap.NegativeX, BuildDetails("negX", faceSuffix), comparer, provider);
        CompareFace<TPixel>(cubemap.PositiveY, BuildDetails("posY", faceSuffix), comparer, provider);
        CompareFace<TPixel>(cubemap.NegativeY, BuildDetails("negY", faceSuffix), comparer, provider);
        CompareFace<TPixel>(cubemap.PositiveZ, BuildDetails("posZ", faceSuffix), comparer, provider);
        CompareFace<TPixel>(cubemap.NegativeZ, BuildDetails("negZ", faceSuffix), comparer, provider);
    }

    private static string BuildDetails(string faceName, string suffix)
        => string.IsNullOrEmpty(suffix) ? faceName : $"{faceName}_{suffix}";

    private static void CompareFace<TPixel>(
        FlatTexture face,
        string details,
        ImageComparer comparer,
        ITestTextureProvider provider)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        using Image faceImage = face.MipMaps[0].GetImage();
        (faceImage as Image<TPixel>).CompareToReferenceOutput(comparer, provider, testOutputDetails: details);
    }

    public static Image<TPixel> GetReferenceOutputImage<TPixel>(
        this ITestTextureProvider provider,
        object testOutputDetails = null,
        string extension = "png",
        bool appendPixelTypeToFileName = false,
        bool appendSourceFileOrDescription = false,
        IImageDecoder decoder = null)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        string referenceOutputFile = provider.Utility.GetReferenceOutputFileName(
            extension,
            testOutputDetails,
            appendPixelTypeToFileName,
            appendSourceFileOrDescription);

        if (!File.Exists(referenceOutputFile))
        {
            throw new FileNotFoundException($"Reference output file {referenceOutputFile} is missing", referenceOutputFile);
        }

        IImageFormat format = TestEnvironment.GetImageFormat(referenceOutputFile);
        decoder ??= TestEnvironment.GetReferenceDecoder(referenceOutputFile);

        ImageSharp.Configuration configuration = ImageSharp.Configuration.Default.Clone();
        configuration.ImageFormatsManager.SetDecoder(format, decoder);
        DecoderOptions options = new()
        {
            Configuration = configuration
        };

        return Image.Load<TPixel>(options, referenceOutputFile);
    }
}
