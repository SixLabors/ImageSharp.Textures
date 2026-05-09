// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.IO;

namespace SixLabors.ImageSharp.Textures.Tests.TestUtilities;

/// <summary>
/// A stream wrapper that reports <see cref="Stream.CanSeek"/> as <c>false</c>, for testing
/// decoder behavior against non-seekable input (e.g. network streams, pipes).
/// </summary>
public sealed class NonSeekableStream : Stream
{
    private readonly Stream inner;

    public NonSeekableStream(Stream inner)
        => this.inner = inner;

    public override bool CanRead => this.inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
        => this.inner.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush() => this.inner.Flush();
}
