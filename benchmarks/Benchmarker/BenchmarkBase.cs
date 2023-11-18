using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Benchmarker;

public class BenchmarkBase : ManualConfig
{
    public BenchmarkBase(string constant)
    {
        AddJob(Job.Default.WithRuntime(CoreRuntime.Core80)
            .WithArguments(new [] { new MsBuildArgument("/p:DefineConstants=MINIMAL")})
            .WithIterationCount(100));

        AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig()));
    }
}