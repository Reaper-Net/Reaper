namespace Reaper.TestWeb.Endpoints.ReaperEndpoint;

[ReaperRoute(HttpVerbs.Get, "/re/none")]
public class NoneEndpoint : Reaper.ReaperEndpoint
{
    public override Task HandleAsync()
    {
        return Task.CompletedTask;
    }
}