namespace Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

[ReaperRoute(HttpVerbs.Post, "/rerr/reflector")]
public class ReflectorEndpoint : ReaperEndpoint<ReflectorEndpoint.ReflectorRequestResponse, ReflectorEndpoint.ReflectorRequestResponse>
{
    public override Task<ReflectorRequestResponse> HandleAsync(ReflectorRequestResponse request)
    {
        return Task.FromResult(request);
    }

    public class ReflectorRequestResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}