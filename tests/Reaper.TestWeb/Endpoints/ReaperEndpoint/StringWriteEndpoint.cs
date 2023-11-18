namespace Reaper.TestWeb.Endpoints.ReaperEndpoint;

[ReaperRoute(HttpVerbs.Get, "/re/string")]
public class StringWriteEndpoint : Reaper.ReaperEndpoint
{
    public override Task HandleAsync()
    {
        return Context.Response.WriteAsync("Hello, World!");
    }
}