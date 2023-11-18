using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Benchmarker;

[Config(typeof(MinimalApiBenchmark))]
public class MinimalApiBenchmark : BenchmarkBase
{
    public MinimalApiBenchmark() : base("MINIMAL") { }

    private static HttpClient Client { get; } = new WebApplicationFactory<BenchmarkWeb.Program>().CreateClient();
    
    [Benchmark]
    public async Task GetAsync()
    {
        var result = await Client.GetAsync("/min-test");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception("Bad");
        }
    }
}