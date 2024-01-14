using Microsoft.AspNetCore.Http;
using Reaper.Response;
using UnitTests.Resources;

namespace UnitTests;

public class StatusCodeHelperTests
{
    [Fact]
    public async Task TestStatusCodesAreOperative()
    {
        var harness = new EndpointHarness();
        await harness.NotFound();
        Assert.Equal(StatusCodes.Status404NotFound, harness.Response.StatusCode);
        // Note that this should not work real-world as it also starts the response
        await harness.Ok();
        Assert.Equal(StatusCodes.Status200OK, harness.Response.StatusCode);
        await harness.BadRequest();
        Assert.Equal(StatusCodes.Status400BadRequest, harness.Response.StatusCode);
    }
}