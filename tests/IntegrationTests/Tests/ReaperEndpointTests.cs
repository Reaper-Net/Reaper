using System.Net;
using System.Net.Http.Json;
using Reaper.TestWeb.Endpoints.ReaperEndpoint;

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
    
    [Fact]
    public async Task SingletonIncrementorEndpointIsIncrementing()
    {
        var resp = await client.GetAsync("/re/singleton");
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World! Counter: 1", str);
        resp = await client.GetAsync("/re/singleton");
        str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World! Counter: 2", str);
    }
    
    [Fact]
    public async Task ScopedIncrementorEndpointIsNotIncrementing()
    {
        var resp = await client.GetAsync("/re/scoped");
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World! Counter: 1", str);
        resp = await client.GetAsync("/re/scoped");
        str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World! Counter: 1", str);
    }

    [Fact]
    public async Task Status200Is200()
    {
        var resp = await client.GetAsync("/re/200");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
    
    [Fact]
    public async Task Status400Is400()
    {
        var resp = await client.GetAsync("/re/400");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
    
    [Fact]
    public async Task Status404Is404()
    {
        var resp = await client.GetAsync("/re/404");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
    
    [Fact]
    public async Task Status200WriterIs200()
    {
        var resp = await client.GetAsync("/re/w200");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Hello, World!", await resp.Content.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task Status400WriterIs400()
    {
        var resp = await client.GetAsync("/re/w400");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Equal("Hello, World!", await resp.Content.ReadAsStringAsync());
    }
    
    [Fact]
    public async Task Status404WriterIs404()
    {
        var resp = await client.GetAsync("/re/w404");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.Equal("Hello, World!", await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Status200JsonWriterIs200()
    {
        var resp = await client.GetAsync("/re/j200");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var sampleResponse = new SampleResponse
        {
            Message = "Hello, World!"
        };
        Assert.Equal(sampleResponse, await resp.Content.ReadFromJsonAsync<SampleResponse>());
    }
}