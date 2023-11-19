using BenchmarkWeb.Dtos;
using FastEndpoints;

namespace BenchmarkWeb.FastEndpoints;

public class TypicalEndpointDoSomething : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/typical/dosomething");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}

public class TypicalEndpointAcceptSomething : Endpoint<SampleRequest>
{
    public override void Configure()
    {
        Post("/typical/acceptsomething");
        AllowAnonymous();
    }

    public override Task HandleAsync(SampleRequest req, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}

public class TypicalEndpointReturnSomething : Endpoint<SampleRequest, SampleResponse>
{
    public override void Configure()
    {
        Post("/typical/returnsomething");
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