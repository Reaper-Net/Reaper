namespace Reaper.TestWeb.Endpoints.ReaperEndpointXR;

[ReaperRoute(HttpVerbs.Get, "/rexr/service")]
public class ServiceScopedStringEndpoint : ReaperEndpointXR<string>
{
    public override Task ExecuteAsync()
    {
        var service = Resolve<HelloWorldProvider>();
        Result = service.GetHelloWorld();
        return Task.CompletedTask;
    }
}