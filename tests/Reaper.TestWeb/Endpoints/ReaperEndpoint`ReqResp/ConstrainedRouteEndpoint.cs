using Microsoft.AspNetCore.Mvc;

namespace Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

[ReaperRoute(HttpVerbs.Get, "/rerr/croute/{first:int}/{second:guid}/{third:datetime}/{fourth:bool}/{fifth:length(1,5)}/{sixth:range(5,10)}")]
public class ConstrainedRouteEndpoint : ReaperEndpoint<ConstrainedRouteEndpoint.ConstrainedRouteEndpointRequestResponse, ConstrainedRouteEndpoint.ConstrainedRouteEndpointRequestResponse>
{
    public override Task ExecuteAsync(ConstrainedRouteEndpointRequestResponse request)
    {
        Result = request;
        return Task.CompletedTask;
    }

    public class ConstrainedRouteEndpointRequestResponse
    {
        [FromRoute] public int First { get; set; }
        [FromRoute] public Guid Second { get; set; }
        [FromRoute] public DateTime Third { get; set; }
        [FromRoute] public bool Fourth { get; set; }
        [FromRoute] public string? Fifth { get; set; }
        [FromRoute] public int Sixth { get; set; }
    }
}