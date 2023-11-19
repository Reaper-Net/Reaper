using BenchmarkWeb.Dtos;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.Carter;

public class AnotherTypicalCarterModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/anothertypical/dosomething", () => TypedResults.Ok());
        app.MapPost("/anothertypical/acceptsomething", (SampleRequest req) => TypedResults.Ok());
        app.MapPost("/anothertypical/returnsomething", (SampleRequest req) => new SampleResponse
        {
            Output = req.Input,
            SomeOtherOutput = req.SomeOtherInput,
            SomeBool = req.SomeBool,
            GeneratedAt = DateTime.UtcNow
        });
    }
}