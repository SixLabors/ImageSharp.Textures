// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using AwesomeAssertions;
using SixLabors.ImageSharp.Textures.Astc.ColorEncoding;
using SixLabors.ImageSharp.Textures.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class PartitionTests
{
    [Fact]
    public void PartitionMetric_WithSimplePartitions_ShouldCalculateCorrectDistance()
    {
        Partition partitionA = new(Footprint.Get6x6(), 2)
        {
            Assignment =
            [
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 1
            ]
        };

        Partition partitionB = new(Footprint.Get6x6(), 2)
        {
            Assignment =
            [
                1, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0
            ]
        };

        int distance = Partition.PartitionMetric(partitionA, partitionB);

        distance.Should().Be(2);
    }

    [Fact]
    public void PartitionMetric_WithDifferentPartCounts_ShouldCalculateCorrectDistance()
    {
        Partition partitionA = new(Footprint.Get4x4(), 2)
        {
            Assignment =
            [
                2, 2, 2, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 1
            ]
        };

        Partition partitionB = new(Footprint.Get4x4(), 3)
        {
            Assignment =
            [
                1, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            ]
        };

        int distance = Partition.PartitionMetric(partitionA, partitionB);

        distance.Should().Be(3);
    }

    [Fact]
    public void PartitionMetric_WithDifferentMapping_ShouldCalculateCorrectDistance()
    {
        Partition partitionA = new(Footprint.Get4x4(), 2)
        {
            Assignment =
            [
                0, 1, 2, 2,
                2, 2, 2, 2,
                2, 2, 2, 2,
                2, 2, 2, 2
            ]
        };

        Partition partitionB = new(Footprint.Get4x4(), 3)
        {
            Assignment =
            [
                1, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0
            ]
        };

        int distance = Partition.PartitionMetric(partitionA, partitionB);

        distance.Should().Be(1);
    }

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

        partition.Assignment.Should().Equal(expected);
    }

    [Fact]
    public void GetASTCPartition_WithDifferentIds_ShouldProduceUniqueAssignments()
    {
        Partition partition0 = Partition.GetASTCPartition(Footprint.Get6x6(), 2, 0);
        Partition partition1 = Partition.GetASTCPartition(Footprint.Get6x6(), 2, 1);

        partition0.Assignment.Should().NotEqual(partition1.Assignment);
    }

    [Fact]
    public void FindClosestASTCPartition_ShouldPreservePartitionCount()
    {
        Partition partition = new(Footprint.Get6x6(), 2)
        {
            Assignment =
            [
                0, 0, 1, 1, 1, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0,
                1, 1, 1, 1, 1, 1
            ]
        };

        Partition closestAstcPartition = Partition.FindClosestASTCPartition(partition);

        closestAstcPartition.PartitionCount.Should().Be(partition.PartitionCount);
    }

    [Fact]
    public void FindClosestASTCPartition_WithModifiedPartition_ShouldReturnValidASTCPartition()
    {
        Partition astcPartition = Partition.GetASTCPartition(Footprint.Get12x12(), 3, 0x3CB);
        Partition modifiedPartition = new(astcPartition.Footprint, astcPartition.PartitionCount)
        {
            Assignment = [.. astcPartition.Assignment]
        };
        modifiedPartition.Assignment[0]++;

        // Find closest ASTC partition
        Partition closestPartition = Partition.FindClosestASTCPartition(modifiedPartition);

        // The closest partition should be a valid ASTC partition with the same footprint and number of parts
        closestPartition.Footprint.Should().Be(astcPartition.Footprint);
        closestPartition.PartitionCount.Should().Be(astcPartition.PartitionCount);
        closestPartition.PartitionId.Should().HaveValue("returned partition should have a valid ID");

        // Verify we can retrieve the same partition again using its ID
        Partition verifyPartition = Partition.GetASTCPartition(
            closestPartition.Footprint,
            closestPartition.PartitionCount,
            closestPartition.PartitionId!.Value);
        verifyPartition.Should().Be(closestPartition);
    }

    [Theory]
    [InlineData(FootprintType.Footprint4x4)]
    [InlineData(FootprintType.Footprint5x4)]
    [InlineData(FootprintType.Footprint5x5)]
    [InlineData(FootprintType.Footprint6x5)]
    [InlineData(FootprintType.Footprint6x6)]
    [InlineData(FootprintType.Footprint8x5)]
    [InlineData(FootprintType.Footprint8x6)]
    [InlineData(FootprintType.Footprint8x8)]
    [InlineData(FootprintType.Footprint10x5)]
    [InlineData(FootprintType.Footprint10x6)]
    [InlineData(FootprintType.Footprint10x8)]
    [InlineData(FootprintType.Footprint10x10)]
    [InlineData(FootprintType.Footprint12x10)]
    [InlineData(FootprintType.Footprint12x12)]
    public void FindClosestASTCPartition_WithRandomPartitions_ShouldReturnFewerOrEqualSubsets(FootprintType footprintType)
    {
        Footprint footprint = Footprint.FromFootprintType(footprintType);
        Random random = new(unchecked((int)0xdeadbeef));

        const int numTests = 15; // Tests per footprint type
        for (int i = 0; i < numTests; i++)
        {
            // Create random partition
            int numParts = 2 + random.Next(3); // 2, 3, or 4 parts
            int[] assignment = new int[footprint.PixelCount];
            for (int j = 0; j < footprint.PixelCount; j++)
            {
                assignment[j] = random.Next(numParts);
            }

            Partition partition = new(footprint, numParts)
            {
                Assignment = assignment
            };

            Partition astcPartition = Partition.FindClosestASTCPartition(partition);

            // Matched partition should have fewer or equal subsets
            astcPartition.PartitionCount
                .Should()
                .BeLessThanOrEqualTo(
                    partition.PartitionCount,
                    $"Footprint {footprintType}, Test #{i}: Selected partition with ID {astcPartition.PartitionId?.ToString(CultureInfo.InvariantCulture) ?? "null"}");
        }
    }
}
