using BenchmarkWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.Controllers;

public class TestController(GetMeAStringService svc) : Controller
{   
    [HttpGet("/ep")]
    public async Task<IActionResult> Get()
    {
        return new OkObjectResult(await svc.GetMeAString());
    }
}