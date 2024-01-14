namespace Reaper.TestWeb.Endpoints.ReaperEndpointXR;

[ReaperRoute(HttpVerbs.Get, "/rexr/string")]
public class StringEndpoint : ReaperEndpointXR<string>
{
    public override Task ExecuteAsync()
    {
        Result = "Hello, World!";
        return Task.CompletedTask;
    }
}