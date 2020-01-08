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
            this.Add(MemoryDiagnoser.Default);
        }

        public class ShortRun : Config
        {
            public ShortRun()
            {
                this.Add(
                    Job.Default.With(ClrRuntime.Net472).WithLaunchCount(1).WithWarmupCount(3).WithIterationCount(3),
                    Job.Default.With(CoreRuntime.Core31).WithLaunchCount(1).WithWarmupCount(3).WithIterationCount(3)
                );
            }
        }
    }
}
