using BenchmarkWeb.Services;
using FastEndpoints;

namespace BenchmarkWeb.FastEndpoints;

public class TestEndpoint(GetMeAStringService svc) : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/ep");
        AllowAnonymous();
    }

    public override async Task<string> ExecuteAsync(CancellationToken ct)
    {
        return await svc.GetMeAString();
    }

}