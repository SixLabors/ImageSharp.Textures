// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Textures.Compression.Astc.BiseEncoding;
using SixLabors.ImageSharp.Textures.Compression.Astc.Core;

namespace SixLabors.ImageSharp.Textures.Tests.Formats.Astc;

public class WeightInfillTests
{
    [Theory]
    [InlineData(4, 4, 3, 32)]
    [InlineData(4, 4, 7, 48)]
    [InlineData(2, 4, 7, 24)]
    [InlineData(2, 4, 1, 8)]
    [InlineData(4, 5, 2, 32)]
    [InlineData(4, 4, 2, 26)]
    [InlineData(4, 5, 5, 52)]
    [InlineData(4, 4, 5, 42)]
    [InlineData(3, 3, 4, 21)]
    [InlineData(4, 4, 4, 38)]
    [InlineData(3, 7, 4, 49)]
    [InlineData(4, 3, 19, 52)]
    [InlineData(4, 4, 19, 70)]
    public void CountBitsForWeights_WithVariousParameters_ShouldReturnCorrectBitCount(
        int width, int height, int range, int expectedBitCount)
    {
        int bitCount = BoundedIntegerSequenceCodec.GetBitCountForRange(width * height, range);

        Assert.Equal(expectedBitCount, bitCount);
    }

    [Fact]
    public void InfillWeights_With3x3Grid_ShouldBilinearlyInterpolateTo5x5()
    {
        int[] weights = [1, 3, 5, 3, 5, 7, 5, 7, 9];
        int[] expected = [1, 2, 3, 4, 5, 2, 3, 4, 5, 6, 3, 4, 5, 6, 7, 4, 5, 6, 7, 8, 5, 6, 7, 8, 9];

        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint5x5);
        DecimationInfo di = DecimationTable.Get(footprint, 3, 3);
        int[] result = new int[footprint.PixelCount];
        DecimationTable.InfillWeights(weights, di, result);

        Assert.Equal(expected.Length, result.Length);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DecimationTable_Get_ReturnsSameInstanceForSameInputs()
    {
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint6x6);
        DecimationInfo first = DecimationTable.Get(footprint, 4, 4);
        DecimationInfo second = DecimationTable.Get(footprint, 4, 4);

        Assert.Same(first, second);
    }

    [Fact]
    public async Task DecimationTable_Get_UnderConcurrentAccess_AllThreadsSeeSameInstance()
    {
        Footprint footprint = Footprint.FromFootprintType(FootprintType.Footprint10x8);
        const int gridX = 7;
        const int gridY = 5;
        const int threadCount = 32;

        using Barrier barrier = new(threadCount);
        DecimationInfo[] results = new DecimationInfo[threadCount];
        Task[] tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int idx = i;
            tasks[i] = Task.Run(() =>
            {
                barrier.SignalAndWait();
                results[idx] = DecimationTable.Get(footprint, gridX, gridY);
            });
        }

        await Task.WhenAll(tasks);

        DecimationInfo winner = results[0];
        Assert.NotNull(winner);
        for (int i = 1; i < threadCount; i++)
        {
            Assert.Same(winner, results[i]);
        }
    }
}
