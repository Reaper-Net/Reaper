using System.Net;
using System.Net.Http.Json;
using DotNet.Testcontainers.Configurations;
using Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

namespace IntegrationTests.WafTests;

[Collection("WAF")]
// ReSharper disable once InconsistentNaming
public class ReaperEndpointRRTests(WafTextFixture fixture)
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

    [Fact]
    public async Task FromSourcesEndpointIsFromSourcing()
    {
        var requestBody = new FromSourcesEndpoint.FromSourceRequestResponse.JsonBound
        {
            AnotherTest = 4,
            Test = "Help me, I'm stuck in some JSON!"
        };
        var expected = new FromSourcesEndpoint.FromSourceRequestResponse
        {
            Test = 1,
            TestString = "Hello",
            QueryValue = 2,
            AnotherQueryValue = 3,
            Json = requestBody
        };
        var resp = await fixture.Client.PostAsJsonAsync("/rerr/fromsource/1/Hello?queryValue=2&another=3", requestBody);
        var str = await resp.Content.ReadAsStringAsync();
        //Assert.Equal("System.InvalidCastException: Unable to cast object of type", str);
        var json = await resp.Content.ReadFromJsonAsync<FromSourcesEndpoint.FromSourceRequestResponse>();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equivalent(expected, json);
    }
}