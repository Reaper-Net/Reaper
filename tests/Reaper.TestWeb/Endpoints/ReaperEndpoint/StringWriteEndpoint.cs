namespace Reaper.TestWeb.Endpoints.ReaperEndpoint;

[ReaperRoute(HttpVerbs.Get, "/re/string")]
public class StringWriteEndpoint : Reaper.ReaperEndpoint
{
    public override Task ExecuteAsync()
    {
        return Context.Response.WriteAsync("Hello, World!");
    }
}