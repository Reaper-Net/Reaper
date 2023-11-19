using BenchmarkWeb.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkWeb.Controllers;

public class AnotherTypicalController
{
    public async Task<IActionResult> DoSomething()
    {
        return new OkResult();
    }

    [HttpPost]
    public async Task<IActionResult> AcceptSomething(SampleRequest req)
    {
        return new OkResult();
    }

    [HttpPost]
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