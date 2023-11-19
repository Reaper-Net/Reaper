using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.Controllers;

public class TestController : Controller
{
    [HttpGet("/ep")]
    public IActionResult Get()
    {
        return new OkObjectResult("Hello, World!");
    }
}