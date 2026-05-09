// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.IO;
using SixLabors.ImageSharp.Textures;
using SixLabors.ImageSharp.Textures.Formats.Ktx2;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx2;

public class Ktx2DecoderCoreTests
{
    [Fact]
    public void DecodeTexture_NonSeekableStream_Throws()
    {
        using var inner = new MemoryStream(new byte[4096]);
        using var stream = new NonSeekableStream(inner);

        Assert.Throws<NotSupportedException>(() => new Ktx2Decoder().DecodeTexture(Configuration.Default, stream));
    }

    [Fact]
    public void Identify_NonSeekableStream_Throws()
    {
        using var inner = new MemoryStream(new byte[4096]);
        using var stream = new NonSeekableStream(inner);

        Assert.Throws<NotSupportedException>(() => new Ktx2Decoder().Identify(Configuration.Default, stream));
    }
}
