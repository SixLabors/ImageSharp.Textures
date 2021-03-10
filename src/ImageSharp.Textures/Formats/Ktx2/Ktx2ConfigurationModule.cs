// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Textures.Formats.Ktx2
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the ktx format.
    /// </summary>
    public class Ktx2ConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
        public void Configure(Configuration configuration)
        {
            configuration.ImageFormatsManager.SetDecoder(Ktx2Format.Instance, new Ktx2Decoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new Ktx2ImageFormatDetector());
        }
    }
}
