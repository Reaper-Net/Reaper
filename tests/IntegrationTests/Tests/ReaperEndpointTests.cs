using System.Net;

namespace IntegrationTests.Tests;

public abstract class ReaperEndpointTests(HttpClient client)
{
    [Fact]
    public async Task NoneEndpointIs200Ok()
    {
        var resp = await client.GetAsync("/re/none");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    
    [Fact]
    public async Task StringWriteEndpointIsWriting()
    {
        var resp = await client.GetAsync("/re/string");
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World!", str);
    }
}