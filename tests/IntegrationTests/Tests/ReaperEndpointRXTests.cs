using System.Net;
using System.Net.Http.Json;
using Reaper.TestWeb.Endpoints.ReaperEndpointRX;

namespace IntegrationTests.Tests;

// ReSharper disable once InconsistentNaming
public abstract class ReaperEndpointRXTests(HttpClient client)
{
    [Fact]
    public async Task ReflectorEndpointIsReflecting()
    {
        var expected = "Reflect me!";
        var resp = await client.PostAsJsonAsync("/rerx/reflector", new ReflectorWriteEndpoint.ReflectorWriteRequest
        {
            Message = expected
        });
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(expected, str);
    }
}