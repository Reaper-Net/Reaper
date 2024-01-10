namespace Reaper.TestWeb.Endpoints.ReaperEndpoint;

[ReaperRoute(HttpVerbs.Get, "/re/singleton")]
public class SingletonIncrementorEndpoint([FromKeyedServices("hw_singleton")]HelloWorldProvider hwProvider) : Reaper.ReaperEndpoint
{
    public override Task HandleAsync()
    {
        return Context.Response.WriteAsync(hwProvider.GetHelloWorld());
    }
}