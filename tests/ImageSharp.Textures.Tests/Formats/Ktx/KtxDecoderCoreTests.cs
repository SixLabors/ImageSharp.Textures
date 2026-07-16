// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.IO;
using SixLabors.ImageSharp.Textures;
using SixLabors.ImageSharp.Textures.Formats.Ktx;
using SixLabors.ImageSharp.Textures.Tests.TestUtilities;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Ktx;

public class KtxDecoderCoreTests
{
    [Fact]
    public void DecodeTexture_NonSeekableStream_Throws()
    {
        using var inner = new MemoryStream(new byte[4096]);
        using var stream = new NonSeekableStream(inner);

        Assert.Throws<NotSupportedException>(() => new KtxDecoder().DecodeTexture(Configuration.Default, stream));
    }

    [Fact]
    public void Identify_NonSeekableStream_Throws()
    {
        using var inner = new MemoryStream(new byte[4096]);
        using var stream = new NonSeekableStream(inner);

        Assert.Throws<NotSupportedException>(() => new KtxDecoder().Identify(Configuration.Default, stream));
    }
}
