// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace SixLabors.ImageSharp.Textures.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            this.AddDiagnoser(MemoryDiagnoser.Default);
        }

        public class ShortRun : Config
        {
            public ShortRun()
            {
                this.AddJob(
                    Job.Default.WithRuntime(ClrRuntime.Net472).WithLaunchCount(1).WithWarmupCount(3).WithIterationCount(3),
                    Job.Default.WithRuntime(CoreRuntime.Core31).WithLaunchCount(1).WithWarmupCount(3).WithIterationCount(3));
            }
        }
    }
}
