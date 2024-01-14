using BenchmarkWeb.Services;
using Reaper;
using Reaper.Attributes;

namespace BenchmarkWeb.Reaper;

[ReaperRoute(HttpVerbs.Get, "/ep")]
public class TestEndpoint(GetMeAStringService svc) : ReaperEndpointXR<string>
{
    public override async Task ExecuteAsync()
    {
        Result = await svc.GetMeAString();
    }
}