using BenchmarkWeb.Dtos;
using Reaper;
using Reaper.Attributes;

namespace BenchmarkWeb.Reaper;

[ReaperRoute(HttpVerbs.Get, "/typical/dosomething")]
public class TypicalEndpointDoSomething : ReaperEndpoint
{
    public override Task HandleAsync()
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/typical/acceptsomething")]
public class TypicalEndpointAcceptSomething : ReaperEndpointRX<SampleRequest>
{
    public override Task HandleAsync(SampleRequest request)
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/typical/returnsomething")]
public class TypicalEndpointReturnSomething : ReaperEndpoint<SampleRequest, SampleResponse>
{
    public override Task<SampleResponse> HandleAsync(SampleRequest request)
    {
        return Task.FromResult(new SampleResponse()
        {
            Output = request.Input,
            SomeOtherOutput = request.SomeOtherInput,
            SomeBool = request.SomeBool,
            GeneratedAt = DateTime.UtcNow
        });
    }
}