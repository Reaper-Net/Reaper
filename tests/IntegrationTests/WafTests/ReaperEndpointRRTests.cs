using System.Net;
using System.Net.Http.Json;
using Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

namespace IntegrationTests.WafTests;

// ReSharper disable once InconsistentNaming
public class ReaperEndpointRRTests(WafTextFixture fixture) : IClassFixture<WafTextFixture>
{
    [Fact]
    public async Task ReflectorEndpointIsReflecting()
    {
        var expected = new ReflectorEndpoint.ReflectorRequestResponse()
        {
            Message = "Reflect Me!"
        };
        var resp = await fixture.Client.PostAsJsonAsync("/rerr/reflector", expected);
        var json = await resp.Content.ReadFromJsonAsync<ReflectorEndpoint.ReflectorRequestResponse>();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equivalent(expected, json);
    }
}