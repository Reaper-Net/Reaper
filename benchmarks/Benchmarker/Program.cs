using System.Text.Json;
using Benchmarker;
using FastEndpoints;
using Spectre.Console;

AnsiConsole.MarkupLine("[bold]💀 Reaper Benchmarker[/]");

var testRunner = new TestRunner();
await testRunner.InitialiseAsync();

List<TestResult> output = new();

// Carter
//output.Add(await testRunner.ExecuteTestAsync("carter"));
// Controllers
//output.Add(await testRunner.ExecuteTestAsync("controllers"));
// FastEndpoints
output.Add(await testRunner.ExecuteTestAsync("fastendpoints"));
// Minimal
output.Add(await testRunner.ExecuteTestAsync("minimal"));
// Minimal AOT
//output.Add(await testRunner.ExecuteTestAsync("minimal-aot"));
// Reaper
output.Add(await testRunner.ExecuteTestAsync("reaper"));
// Reaper AOT
//output.Add(await testRunner.ExecuteTestAsync("reaper-aot"));

if (AnsiConsole.Confirm("Do you want some output?"))
{
    var fmt = AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title("Which format?")
        .AddChoices("json", "csv"));
    if (fmt == "csv")
    {
        AnsiConsole.WriteLine("Framework,Startup Time,Memory Usage (MiB) - Startup,Memory Usage (MiB) - Load Test,Requests/sec");
        foreach(var result in output) {
            AnsiConsole.WriteLine($"{result.Container},{result.StartupTimeMs},{result.MemoryStartup},{result.MemoryLoadTest},{result.RequestsSec}");
        }
    }
    else
    {
        AnsiConsole.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}