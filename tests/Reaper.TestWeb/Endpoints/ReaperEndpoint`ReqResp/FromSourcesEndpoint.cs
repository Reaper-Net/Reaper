using Microsoft.AspNetCore.Mvc;

namespace Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

[ReaperRoute(HttpVerbs.Post, "/rerr/fromsource/{test}/{anothertest}")]
public class FromSourcesEndpoint : ReaperEndpoint<FromSourcesEndpoint.FromSourceRequestResponse, FromSourcesEndpoint.FromSourceRequestResponse>
{
    public override Task<FromSourceRequestResponse> HandleAsync(FromSourceRequestResponse request)
    {
        return Task.FromResult(request);
    }

    public class FromSourceRequestResponse
    {
        [FromRoute] public int Test { get; set; }

        [FromRoute(Name = "anothertest")] public string TestString { get; set; } = default!;
        
        [FromQuery] public int QueryValue { get; set; }
        
        [FromQuery(Name = "another")] public int AnotherQueryValue { get; set; }

        [FromBody] public JsonBound Json { get; set; } = default!;

        public class JsonBound
        {
            public string Test { get; set; } = string.Empty;
            
            public int AnotherTest { get; set; }
        }
    }
}