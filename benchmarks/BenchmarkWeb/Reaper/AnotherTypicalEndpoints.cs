using BenchmarkWeb.Dtos;
using Reaper;
using Reaper.Attributes;

namespace BenchmarkWeb.Reaper;

[ReaperRoute(HttpVerbs.Get, "/anothertypical/dosomething")]
public class AnotherTypicalEndpointDoSomething : ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/anothertypical/acceptsomething")]
public class AnotherTypicalEndpointAcceptSomething : ReaperEndpointRX<SampleRequest>
{
    public override Task ExecuteAsync(SampleRequest request)
    {
        return Task.CompletedTask;
    }
}

[ReaperRoute(HttpVerbs.Post, "/anothertypical/returnsomething")]
public class AnotherTypicalEndpointReturnSomething : ReaperEndpoint<SampleRequest, SampleResponse>
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