using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Benchmarker;

[Config(typeof(ReaperBenchmark))]
public class ReaperBenchmark : BenchmarkBase
{
    public ReaperBenchmark() : base("REAPER") { }

    private static HttpClient Client { get; } = new WebApplicationFactory<BenchmarkWeb.Program>().CreateClient();
    
    [Benchmark]
    public async Task GetAsync()
    {
        var result = await Client.GetAsync("/min-test");
        //if (!result.IsSuccessStatusCode)
        //{
            throw new Exception("Bad");
        //}
    }
}