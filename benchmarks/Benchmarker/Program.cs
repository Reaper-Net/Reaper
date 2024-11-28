using System.Text;
using System.Text.Json;
using Benchmarker;
using CommandLine;
using Spectre.Console;

var parseOpts = Parser.Default.ParseArguments<BenchmarkerOptions>(args);
var opts = parseOpts.Value;
if (opts == null)
{
    return;
}

var tests = opts.RunTests.ToList();

if (opts.AllTests)
{
    tests = ["carter", "controllers", "fastendpoints", "minimal", "minimal-aot", "reaper", "reaper-aot"];
}

AnsiConsole.MarkupLine("[bold]💀 Reaper Benchmarker[/]");
AnsiConsole.MarkupLine("[dim][bold]{0}[/] threads, [bold]{1}[/] HTTP connections, for [bold]{2}[/] seconds[/]", opts.Threads, opts.Http, opts.Duration);
AnsiConsole.MarkupLine("[dim]Included tests: [bold]{0}[/][/]", string.Join(", ", tests));

var testRunner = new TestRunner();
await testRunner.InitialiseAsync();

List<TestResult> output = [];

foreach(var test in tests)
{
    output.Add(await testRunner.ExecuteTestAsync(test));
}

// Carter
//output.Add(await testRunner.ExecuteTestAsync("carter"));
// Controllers
//output.Add(await testRunner.ExecuteTestAsync("controllers"));
// FastEndpoints
//output.Add(await testRunner.ExecuteTestAsync("fastendpoints"));
// Minimal
//output.Add(await testRunner.ExecuteTestAsync("minimal"));
// Minimal AOT
//output.Add(await testRunner.ExecuteTestAsync("minimal-aot"));
// Reaper
//output.Add(await testRunner.ExecuteTestAsync("reaper"));
// Reaper AOT
//output.Add(await testRunner.ExecuteTestAsync("reaper-aot"));

var csvOut = new StringBuilder();
csvOut.AppendLine("Framework,Startup Time,Memory Usage (MiB) - Startup,Memory Usage (MiB) - Load Test,Requests/sec");
foreach(var result in output) {
    csvOut.AppendLine($"{result.Container},{result.StartupTimeMs},{result.MemoryStartup},{result.MemoryLoadTest},{result.RequestsSec}");
}
var csv = csvOut.ToString();

if (!opts.NoFile)
{
    var fileName = $"benchmark-run-{DateTime.Now:dd-MMM_HH-mm-ss}.csv";
    File.WriteAllText(fileName, csv);
    AnsiConsole.MarkupLine($"[dim]Output written to {fileName}[/]");
}

if (AnsiConsole.Confirm("Do you want some output in this console too?"))
{
    var fmt = AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title("Which format?")
        .AddChoices("json", "csv"));
    if (fmt == "csv")
    {
        AnsiConsole.WriteLine(csv);
    }
    else
    {
        AnsiConsole.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}

public class BenchmarkerOptions
{
    [Option('t', "threads", Required = false, HelpText = "Threads to use for wrk", Default = 10)]
    public int Threads { get; set; }
    [Option('h', "http", Required = false, HelpText = "HTTP Connections to use for wrk", Default = 100)]
    public int Http { get; set; }
    [Option('d', "duration", Required = false, HelpText = "Seconds to run wrk for", Default = 7)]
    public int Duration { get; set; }
    
    [Option('a', "all", Required = false, HelpText = "Run all tests", Default = false)]
    public bool AllTests { get; set; }
    
    [Option("no-file", Required = false, HelpText = "Don't write output to a file automatically", Default = false)]
    public bool NoFile { get; set; }
    
    [Option('r', "run", Required = false, Separator = ',',
        HelpText = "Tests to run (carter, controllers, fastendpoints, minimal, minimal-aot, reaper, reaper-aot",
        Default = new [] { "fastendpoints", "minimal", "reaper" })]
    public IEnumerable<string> RunTests { get; set; }
}