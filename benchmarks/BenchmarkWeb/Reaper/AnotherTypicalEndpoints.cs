using BenchmarkWeb.Dtos;
using Reaper;
using Reaper.Attributes;

namespace BenchmarkWeb.Reaper;

[ReaperRoute(HttpVerbs.Get, "/anothertypical/dosomething")]
public class AnotherTypicalEndpointDoSomething : ReaperEndpoint
{
    public override Task HandleAsync()
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/anothertypical/acceptsomething")]
public class AnotherTypicalEndpointAcceptSomething : ReaperEndpointRX<SampleRequest>
{
    public override Task HandleAsync(SampleRequest request)
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/anothertypical/returnsomething")]
public class AnotherTypicalEndpointReturnSomething : ReaperEndpoint<SampleRequest, SampleResponse>
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