// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the ktx format.
    /// </summary>
    public class KtxConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetDecoder(KtxFormat.Instance, new KtxDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new KtxImageFormatDetector());
        }
    }
}
