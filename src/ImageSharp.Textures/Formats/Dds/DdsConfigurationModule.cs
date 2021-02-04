// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

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
            // configuration.ImageFormatsManager.SetEncoder(DdsFormat.Instance, new DdsEncoder());
            configuration.ImageFormatsManager.SetDecoder(DdsFormat.Instance, new DdsDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new DdsImageFormatDetector());
        }
    }
}
