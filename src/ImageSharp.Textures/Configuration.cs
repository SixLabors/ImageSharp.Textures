// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Textures.Formats;
using SixLabors.ImageSharp.Textures.Formats.Dds;
using SixLabors.ImageSharp.Textures.Formats.Ktx;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.IO;

namespace SixLabors.ImageSharp.Textures
{
    /// <summary>
    /// Provides configuration code which allows altering default behaviour or extending the library.
    /// </summary>
    public sealed class Configuration
    {
        /// <summary>
        /// A lazily initialized configuration default instance.
        /// </summary>
        private static readonly Lazy<Configuration> Lazy = new Lazy<Configuration>(CreateDefaultInstance);

        private int maxDegreeOfParallelism = Environment.ProcessorCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        public Configuration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        /// <param name="configurationModules">A collection of configuration modules to register</param>
        public Configuration(params IConfigurationModule[] configurationModules)
        {
            if (configurationModules != null)
            {
                foreach (IConfigurationModule p in configurationModules)
                {
                    p.Configure(this);
                }
            }
        }

        /// <summary>
        /// Gets the default <see cref="Configuration"/> instance.
        /// </summary>
        public static Configuration Default { get; } = Lazy.Value;

        /// <summary>
        /// Gets or sets the maximum number of concurrent tasks enabled in ImageSharp algorithms
        /// configured with this <see cref="Configuration"/> instance.
        /// Initialized with <see cref="Environment.ProcessorCount"/> by default.
        /// </summary>
        public int MaxDegreeOfParallelism
        {
            get => this.maxDegreeOfParallelism;
            set
            {
                if (value == 0 || value < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.MaxDegreeOfParallelism));
                }

                this.maxDegreeOfParallelism = value;
            }
        }

        /// <summary>
        /// Gets the currently registered <see cref="ITextureFormat"/>s.
        /// </summary>
        public IEnumerable<ITextureFormat> ImageFormats => this.ImageFormatsManager.ImageFormats;

        /// <summary>
        /// Gets or sets the position in a stream to use for reading when using a seekable stream as an image data source.
        /// </summary>
        public ReadOrigin ReadOrigin { get; set; } = ReadOrigin.Current;

        /// <summary>
        /// Gets or sets the <see cref="TextureFormatManager"/> that is currently in use.
        /// </summary>
        public TextureFormatManager ImageFormatsManager { get; set; } = new TextureFormatManager();

        /// <summary>
        /// Gets or sets the <see cref="MemoryAllocator"/> that is currently in use.
        /// </summary>
        public MemoryAllocator MemoryAllocator { get; set; } = ArrayPoolMemoryAllocator.CreateDefault();

        /// <summary>
        /// Gets the maximum header size of all the formats.
        /// </summary>
        internal int MaxHeaderSize => this.ImageFormatsManager.MaxHeaderSize;

        /// <summary>
        /// Gets or sets the filesystem helper for accessing the local file system.
        /// </summary>
        internal IFileSystem FileSystem { get; set; } = new LocalFileSystem();

        /// <summary>
        /// Gets or sets the working buffer size hint for image processors.
        /// The default value is 1MB.
        /// </summary>
        /// <remarks>
        /// Currently only used by Resize.
        /// </remarks>
        internal int WorkingBufferSizeHintInBytes { get; set; } = 1 * 1024 * 1024;

        /// <summary>
        /// Registers a new format provider.
        /// </summary>
        /// <param name="configuration">The configuration provider to call configure on.</param>
        public void Configure(IConfigurationModule configuration)
        {
            Guard.NotNull(configuration, nameof(configuration));
            configuration.Configure(this);
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="Configuration"/>.
        /// </summary>
        /// <returns>A new configuration instance.</returns>
        public Configuration Clone() => new Configuration
        {
            MaxDegreeOfParallelism = this.MaxDegreeOfParallelism,
            ImageFormatsManager = this.ImageFormatsManager,
            MemoryAllocator = this.MemoryAllocator,
            ReadOrigin = this.ReadOrigin,
            FileSystem = this.FileSystem,
            WorkingBufferSizeHintInBytes = this.WorkingBufferSizeHintInBytes,
        };

        /// <summary>
        /// Creates the default instance with the following <see cref="IConfigurationModule"/>s preregistered:
        /// <see cref="DdsConfigurationModule"/>
        /// </summary>
        /// <returns>The default configuration of <see cref="Configuration"/>.</returns>
        internal static Configuration CreateDefaultInstance() => new Configuration(
                new DdsConfigurationModule(),
                new KtxConfigurationModule(),
                new Ktx2ConfigurationModule());
    }
}
