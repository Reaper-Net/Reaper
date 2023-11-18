using Reaper;
using Reaper.Attributes;

namespace BenchmarkWeb.Reaper;

[ReaperRoute(HttpVerbs.Get, "/ep")]
public class TestEndpoint : ReaperEndpointXR<string>
{
    public override Task<string> HandleAsync()
    {
        return Task.FromResult("Hello, World!");
    }
}