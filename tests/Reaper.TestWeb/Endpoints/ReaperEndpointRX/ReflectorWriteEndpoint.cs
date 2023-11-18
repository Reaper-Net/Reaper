namespace Reaper.TestWeb.Endpoints.ReaperEndpointRX;

[ReaperRoute(HttpVerbs.Post, "/rerx/reflector")]
public class ReflectorWriteEndpoint : ReaperEndpointRX<ReflectorWriteEndpoint.ReflectorWriteRequest>
{
    public override Task HandleAsync(ReflectorWriteRequest request)
    {
        return Context.Response.WriteAsync(request.Message);
    }

    public class ReflectorWriteRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}