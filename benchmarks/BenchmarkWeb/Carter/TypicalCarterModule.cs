using BenchmarkWeb.Dtos;
using Carter;

namespace BenchmarkWeb.Carter;

public class TypicalCarterModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/typical/dosomething", () => TypedResults.Ok());
        app.MapPost("/typical/acceptsomething", (SampleRequest req) => TypedResults.Ok());
        app.MapPost("/typical/returnsomething", (SampleRequest req) => new SampleResponse
        {
            Output = req.Input,
            SomeOtherOutput = req.SomeOtherInput,
            SomeBool = req.SomeBool,
            GeneratedAt = DateTime.UtcNow
        });
    }
}