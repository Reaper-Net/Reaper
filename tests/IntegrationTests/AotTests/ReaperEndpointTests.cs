using System.Net;

namespace IntegrationTests.AotTests;

[Collection("AOT")]
public class ReaperEndpointTests(AotTestFixture fixture)
{
    [Fact]
    public async Task NoneEndpointIs200Ok()
    {
        var resp = await fixture.Client.GetAsync("/re/none");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    
    [Fact]
    public async Task StringWriteEndpointIsWriting()
    {
        var resp = await fixture.Client.GetAsync("/re/string");
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World!", str);
    }
}