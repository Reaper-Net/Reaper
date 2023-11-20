using System.Net;
using System.Net.Http.Json;
using Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

namespace IntegrationTests.AotTests;

[Collection("AOT")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointRRTests(AotTestFixture fixture)
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