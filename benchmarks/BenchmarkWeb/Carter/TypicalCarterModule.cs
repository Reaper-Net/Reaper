using BenchmarkWeb.Dtos;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.Carter;

public class TypicalCarterModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/typical/dosomething", () => new OkResult());
        app.MapPost("/typical/acceptsomething", (SampleRequest req) => new OkResult());
        app.MapPost("/typical/returnsomething", (SampleRequest req) => new ObjectResult(new SampleResponse
        {
            Output = req.Input,
            SomeOtherOutput = req.SomeOtherInput,
            SomeBool = req.SomeBool,
            GeneratedAt = DateTime.UtcNow
        }));
    }
}