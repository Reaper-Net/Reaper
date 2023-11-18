using System.Net;
using System.Net.Http.Json;
using Reaper.TestWeb.Endpoints.ReaperEndpointRX;

namespace IntegrationTests.WafTests;

// ReSharper disable once InconsistentNaming
public class ReaperEndpointRXTests(WafTextFixture fixture) : IClassFixture<WafTextFixture>
{
    [Fact]
    public async Task ReflectorEndpointIsReflecting()
    {
        var expected = "Reflect me!";
        var resp = await fixture.Client.PostAsJsonAsync("/rerx/reflector", new ReflectorWriteEndpoint.ReflectorWriteRequest
        {
            Message = expected
        });
        var str = await resp.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(expected, str);
    }
}