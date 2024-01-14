namespace Reaper.TestWeb.Endpoints.ReaperEndpointXR;

[ReaperRoute(HttpVerbs.Get, "/rexr/json")]
public class JsonEndpoint : ReaperEndpointXR<JsonEndpoint.JsonResponse>
{
    public override Task ExecuteAsync()
    {
        Result = new JsonResponse
        {
            Message = "Hello, World!"
        };
        return Task.CompletedTask;
    }

    public class JsonResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}