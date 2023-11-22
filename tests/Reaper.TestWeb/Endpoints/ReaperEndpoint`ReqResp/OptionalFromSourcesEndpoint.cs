using Microsoft.AspNetCore.Mvc;

namespace Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

[ReaperRoute(HttpVerbs.Post, "/rerr/optfsource/{test?}/{anothertest?}")]
public class OptionalFromSourcesEndpoint : ReaperEndpoint<OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse, OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse>
{
    public override Task<OptionalFromSourcesRequestResponse> HandleAsync(OptionalFromSourcesRequestResponse request)
    {
        return Task.FromResult(request);
    }

    public class OptionalFromSourcesRequestResponse
    {
        [FromRoute] public int? Test { get; set; }

        [FromRoute(Name = "anothertest")] public string? TestString { get; set; } = default!;
        
        [FromQuery] public int? QueryValue { get; set; }
        
        [FromQuery(Name = "another")] public int? AnotherQueryValue { get; set; }
        
        [FromBody] public JsonBoundOptional? Json { get; set; }

        public class JsonBoundOptional
        {
            public string Test { get; set; } = string.Empty;
            
            public int AnotherTest { get; set; }
        }
    }
}