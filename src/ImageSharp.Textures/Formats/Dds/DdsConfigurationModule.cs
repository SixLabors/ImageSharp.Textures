// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Textures.Formats.Ktx;

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the png format.
    /// </summary>
    public sealed class DdsConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetDecoder(DdsFormat.Instance, new DdsDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new DdsImageFormatDetector());

            configuration.ImageFormatsManager.SetDecoder(KtxFormat.Instance, new KtxDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new KtxImageFormatDetector());
        }
    }
}
