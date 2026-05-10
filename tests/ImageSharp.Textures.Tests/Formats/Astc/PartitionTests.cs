// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class PartitionTests
{
    [Fact]
    public void GetASTCPartition_WithSpecificParameters_ShouldReturnExpectedAssignment()
    {
        int[] expected =
        [
            0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 0, 1, 1, 1, 2, 2, 2,
            0, 0, 0, 0, 1, 1, 1, 2, 2, 2
        ];

        Partition partition = Partition.GetASTCPartition(Footprint.Get10x6(), 3, 557);

        Assert.Equal(expected, partition.Assignment);
    }

    [Fact]
    public void GetASTCPartition_WithDifferentIds_ShouldProduceUniqueAssignments()
    {
        Partition partition0 = Partition.GetASTCPartition(Footprint.Get6x6(), 2, 0);
        Partition partition1 = Partition.GetASTCPartition(Footprint.Get6x6(), 2, 1);

        Assert.NotEqual(partition1.Assignment, partition0.Assignment);
    }
}
