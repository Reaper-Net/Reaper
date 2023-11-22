using System.Net;
using System.Net.Http.Json;
using DotNet.Testcontainers.Configurations;
using Reaper.TestWeb.Endpoints.ReaperEndpoint_ReqResp;

namespace IntegrationTests.Tests;

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointRRTests(HttpClient client)
{
    [Fact]
    public async Task ReflectorEndpointIsReflecting()
    {
        var expected = new ReflectorEndpoint.ReflectorRequestResponse()
        {
            Message = "Reflect Me!"
        };
        var resp = await client.PostAsJsonAsync("/rerr/reflector", expected);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<ReflectorEndpoint.ReflectorRequestResponse>();
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
        var resp = await client.PostAsJsonAsync("/rerr/fromsource/1/Hello?queryValue=2&another=3", requestBody);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<FromSourcesEndpoint.FromSourceRequestResponse>();
        Assert.Equivalent(expected, json);
    }

    [Fact]
    public async Task ConstrainedRouteIsConstraining()
    {
        var guid = Guid.NewGuid();
        var expected = new ConstrainedRouteEndpoint.ConstrainedRouteEndpointRequestResponse
        {
            First = 1,
            Second = guid,
            Third = DateTime.Today,
            Fourth = true,
            Fifth = "abc",
            Sixth = 6
        };
        
        var resp = await client.GetAsync($"/rerr/croute/1/{guid}/{DateTime.Today:yyyy-MM-dd}/true/abc/6");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<ConstrainedRouteEndpoint.ConstrainedRouteEndpointRequestResponse>();
        Assert.Equivalent(expected, json);
        resp = await client.GetAsync($"/rerr/croute/1/notaguid/{DateTime.Today:yyyy-MM-dd}/true/abc/6");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        resp = await client.GetAsync($"/rerr/croute/1/{guid}/1234/true/abc/6");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        resp = await client.GetAsync($"/rerr/croute/1/{guid}/{DateTime.Today:yyyy-MM-dd}/test/abc/6");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
    
    [Fact]
    public async Task OptionalSourcesRouteIsOptional()
    {
        var requestBody = new OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse.JsonBoundOptional
        {
            AnotherTest = 4,
            Test = "Help me, I'm stuck in some JSON!"
        };
        var expected = new OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse
        {
            Test = 1,
            TestString = "Hello",
            QueryValue = 2,
            AnotherQueryValue = 3,
            Json = requestBody
        };
        var resp = await client.PostAsJsonAsync("/rerr/optfsource/1/Hello?queryValue=2&another=3", requestBody);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadFromJsonAsync<OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse>();
        Assert.Equivalent(expected, json);

        // Optional Route/Query Values
        expected.TestString = null;
        expected.QueryValue = null;
        resp = await client.PostAsJsonAsync("/rerr/optfsource/1?another=3", requestBody);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        json = await resp.Content.ReadFromJsonAsync<OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse>();
        Assert.Equivalent(expected, json);


        expected.Json = null;
        resp = await client.PostAsJsonAsync("/rerr/optfsource/1?another=3", (object?)null);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        json = await resp.Content.ReadFromJsonAsync<OptionalFromSourcesEndpoint.OptionalFromSourcesRequestResponse>();
        Assert.Equivalent(expected, json);
    }
}