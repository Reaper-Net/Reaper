namespace Reaper.TestWeb.Endpoints.ReaperEndpointXR;

[ReaperRoute(HttpVerbs.Get, "/rexr/string")]
public class StringEndpoint : ReaperEndpointXR<string>
{
    public override Task<string> HandleAsync()
    {
        return Task.FromResult("Hello, World!");
    }
}