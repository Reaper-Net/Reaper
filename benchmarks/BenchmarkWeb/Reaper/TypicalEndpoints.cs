using BenchmarkWeb.Dtos;
using Reaper;
using Reaper.Attributes;

namespace BenchmarkWeb.Reaper;

[ReaperRoute(HttpVerbs.Get, "/typical/dosomething")]
public class TypicalEndpointDoSomething : ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/typical/acceptsomething")]
public class TypicalEndpointAcceptSomething : ReaperEndpointRX<SampleRequest>
{
    public override Task ExecuteAsync(SampleRequest request)
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/typical/returnsomething")]
public class TypicalEndpointReturnSomething : ReaperEndpoint<SampleRequest, SampleResponse>
{
    public override Task ExecuteAsync(SampleRequest request)
    {
        Result = new SampleResponse()
        {
            Output = request.Input,
            SomeOtherOutput = request.SomeOtherInput,
            SomeBool = request.SomeBool,
            GeneratedAt = DateTime.UtcNow
        };
        return Task.CompletedTask;
    }
}