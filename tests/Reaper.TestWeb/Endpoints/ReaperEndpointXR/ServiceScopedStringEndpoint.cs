namespace Reaper.TestWeb.Endpoints.ReaperEndpointXR;

[ReaperRoute(HttpVerbs.Get, "/rexr/service")]
public class ServiceScopedStringEndpoint : ReaperEndpointXR<string>
{
    public override Task<string> HandleAsync()
    {
        var service = Context.RequestServices.GetRequiredService<HelloWorldProvider>();
        return Task.FromResult(service.GetHelloWorld());
    }
}