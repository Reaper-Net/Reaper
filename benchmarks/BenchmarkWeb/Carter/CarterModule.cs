using Carter;

namespace BenchmarkWeb.Carter;

public class CarterModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/ep", () => "Hello, World!");
    }
}