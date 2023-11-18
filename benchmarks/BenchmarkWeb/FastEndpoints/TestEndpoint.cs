using FastEndpoints;

namespace BenchmarkWeb.FastEndpoints;

public class TestEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/ep");
        AllowAnonymous();
    }

    public override Task<string> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult("Hello, World!");
    }

}