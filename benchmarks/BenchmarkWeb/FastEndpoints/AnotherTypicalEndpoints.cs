using BenchmarkWeb.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.FastEndpoints;

public class AnotherTypicalEndpointDoSomething : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/anothertypical/dosomething");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return base.HandleAsync(ct);
    }
}

public class AnotherTypicalEndpointAcceptSomething : Endpoint<SampleRequest>
{
    public override void Configure()
    {
        Post("/anothertypical/acceptsomething");
        AllowAnonymous();
    }

    public override Task HandleAsync(SampleRequest req, CancellationToken ct)
    {
        return base.HandleAsync(req, ct);
    }
}

public class AnotherTypicalEndpointReturnSomething : Endpoint<SampleRequest, SampleResponse>
{
    public override void Configure()
    {
        Post("/anothertypical/returnsomething");
        AllowAnonymous();
    }

    public override Task<SampleResponse> ExecuteAsync(SampleRequest req, CancellationToken ct)
    {
        return Task.FromResult(new SampleResponse()
        {
            Output = req.Input,
            SomeOtherOutput = req.SomeOtherInput,
            SomeBool = req.SomeBool,
            GeneratedAt = DateTime.UtcNow
        });
    }
}