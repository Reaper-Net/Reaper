using BenchmarkWeb.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.Controllers;

[Route("typical")]
public class TypicalController : Controller
{
    [HttpGet("dosomething")]
    public async Task<IActionResult> DoSomething()
    {
        return new OkResult();
    }

    [HttpPost("acceptsomething")]
    public async Task<IActionResult> AcceptSomething(SampleRequest req)
    {
        return new OkResult();
    }

    [HttpPost("returnsomething")]
    public async Task<IActionResult> ReturnSomething(SampleRequest req)
    {
        return new ObjectResult(new SampleResponse
        {
            Output = req.Input,
            SomeOtherOutput = req.SomeOtherInput,
            SomeBool = req.SomeBool,
            GeneratedAt = DateTime.UtcNow
        });
    }
}