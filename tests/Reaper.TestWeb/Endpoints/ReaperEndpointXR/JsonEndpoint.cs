namespace Reaper.TestWeb.Endpoints.ReaperEndpointXR;

[ReaperRoute(HttpVerbs.Get, "/rexr/json")]
public class JsonEndpoint : ReaperEndpointXR<JsonEndpoint.JsonResponse>
{
    public override Task<JsonResponse> HandleAsync()
    {
        return Task.FromResult(new JsonResponse
        {
            Message = "Hello, World!"
        });
    }

    public class JsonResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}