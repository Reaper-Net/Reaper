namespace Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

[ReaperRoute(HttpVerbs.Post, "/rerr/reflector")]
public class ReflectorEndpoint : ReaperEndpoint<ReflectorEndpoint.ReflectorRequestResponse, ReflectorEndpoint.ReflectorRequestResponse>
{
    public override Task ExecuteAsync(ReflectorRequestResponse request)
    {
        Result = request;
        return Task.CompletedTask;
    }

    public class ReflectorRequestResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}