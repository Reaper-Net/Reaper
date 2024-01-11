using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Reaper.Response;
using Reaper.TestWeb.Endpoints.ReaperEndpointRX;
using ValidationProblemDetails = Reaper.Validation.Responses.ValidationProblemDetails;

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

    [Fact]
    public async Task MissingResponseBodyShouldThrow400()
    {
        var resp = await client.PostAsync("/rerx/validator", null);
        var details = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(details);
        Assert.Equal("https://reaper.divergent.dev/validation/body-required-not-provided", details.Type);
    }

    [Fact]
    public async Task InvalidValidationShouldThrow400()
    {
        var resp = await client.PostAsJsonAsync("/rerx/validator", new ValidatorWriteRequest
        {
            Message = null
        });
        var details = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(details);
        Assert.Equal("https://reaper.divergent.dev/validation/failure", details.Type);
        Assert.Single(details.ValidationFailures!);
    }
    
    [Fact]
    public async Task ValidValidationShouldNotThrow400()
    {
        var resp = await client.PostAsJsonAsync("/rerx/validator", new ValidatorWriteRequest
        {
            Message = "Hello"
        });
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    public class ExpectedValidationFailure
    {
        public string Key { get; set; }
        public string Code { get; set; }
        public string Error { get; set; }
    }
}