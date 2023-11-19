using System.Text.Json;
using Benchmarker;
using Spectre.Console;

AnsiConsole.MarkupLine("[bold]💀 Reaper Benchmarker[/]");

var testRunner = new TestRunner();
await testRunner.InitialiseAsync();

var carter = await testRunner.ExecuteTestAsync("carter");
var controllers = await testRunner.ExecuteTestAsync("controllers");
var fastendpoints = await testRunner.ExecuteTestAsync("fastendpoints");
var minimal = await testRunner.ExecuteTestAsync("minimal");
var minimalAot = await testRunner.ExecuteTestAsync("minimal-aot");
var reaper = await testRunner.ExecuteTestAsync("reaper");
var reaperAot = await testRunner.ExecuteTestAsync("reaper-aot");

var output = new[] { carter, controllers, fastendpoints, minimal, minimalAot, reaper, reaperAot };

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