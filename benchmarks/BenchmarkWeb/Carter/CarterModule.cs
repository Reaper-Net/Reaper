using BenchmarkWeb.Services;
using Carter;

namespace BenchmarkWeb.Carter;

public class CarterModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/ep", async (GetMeAStringService svc) => await svc.GetMeAString());
    }
}