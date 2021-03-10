// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Dds
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for texture formats.
    /// </summary>
    public sealed class DdsConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetDecoder(DdsFormat.Instance, new DdsDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new DdsImageFormatDetector());
        }
    }
}
