using BenchmarkDotNet.Running;
using Benchmarker;

BenchmarkRunner.Run(
    new []
    {
        typeof(MinimalApiBenchmark),
        typeof(ReaperBenchmark)
    });