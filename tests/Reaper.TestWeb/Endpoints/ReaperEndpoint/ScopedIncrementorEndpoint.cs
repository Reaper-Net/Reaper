namespace Reaper.TestWeb.Endpoints.ReaperEndpoint;

[ReaperRoute(HttpVerbs.Get, "/re/scoped")]
[ReaperScoped]
public class ScopedIncrementorEndpoint(HelloWorldProvider hwProvider) : Reaper.ReaperEndpoint
{
    public override Task HandleAsync()
    {
        return Context.Response.WriteAsync(hwProvider.GetHelloWorld());
    }
}