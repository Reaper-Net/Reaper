using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.WafTests;

// ReSharper disable once InconsistentNaming
public class ReaperEndpointXRTests(WafTextFixture fixture) : IClassFixture<WafTextFixture>
{
    [Fact]
    public async Task JsonEndpointIsReturning()
    {
        var resp = await fixture.Client.GetAsync("/rexr/json");
        var json = await resp.Content.ReadFromJsonAsync<Reaper.TestWeb.Endpoints.ReaperEndpointXR.JsonEndpoint.JsonResponse>();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.NotNull(json);
        Assert.Equal("Hello, World!", json.Message);
    }
    
    [Fact]
    public async Task StringWriteEndpointIsWriting()
    {
        var resp = await fixture.Client.GetAsync("/rexr/string");
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World!", str);
    }
    
    [Fact]
    public async Task ScopedServiceEndpointIsWriting()
    {
        var expected = "Hello, World! Counter: 1";
        var resp = await fixture.Client.GetAsync("/rexr/service");
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(expected, str);
        
        // Scoped test
        resp = await fixture.Client.GetAsync("/rexr/service");
        str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(expected, str);
    }
}