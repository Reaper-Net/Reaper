using System.Text;
using System.Text.Json;
using BenchmarkWeb.Dtos;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;

namespace Benchmarker;

public class TestRunner()
{
    private static readonly IFutureDockerImage wrkImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(Paths.ProjectPath, string.Empty)
        .WithDockerfile("wrk.dockerfile")
        .WithDeleteIfExists(true)
        .Build();

    public async Task InitialiseAsync()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.SimpleDotsScrolling)
            .StartAsync("Initialising...", async ctx =>
            {
                ctx.Status("Building reusable wrk container image");
                TestcontainersSettings.Logger = NullLogger.Instance;
                //ConsoleLogger.Instance.DebugLogLevelEnabled = true;
                await wrkImage.CreateAsync();

                AnsiConsole.WriteLine("Initialised.");
            });
    }

    public async Task<TestResult> ExecuteTestAsync(string container)
    {
        long startupTime = 0;
        decimal memStartup = 0, memLoadTest = 0, requestsSec = 0;
        AnsiConsole.MarkupLine($"Running [bold]{container}[/]");
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Aesthetic)
            .StartAsync("Executing '" + container + "'...", async ctx =>
            {
                ctx.Status("Creating Network & building app image...");
                var network = new NetworkBuilder()
                    .WithName("reaper-test-network")
                    .Build();
                var appImage = await CreateAppImageAsync(container);
                var containers = CreateContainersWithNetworkAsync(appImage, network);
                await network.CreateAsync();

                // Start the app image
                await using (var app = containers.app)
                {
                    ctx.Spinner(Spinner.Known.Arc);
                    ctx.Status("Starting app container...");
                    await app.StartAsync();
                    startupTime = await GetTimerFromLogsAsLongAsync(app, "BenchmarkWeb Startup:");
                    memStartup = await DockerStats.GetMemoryUsageForContainer("reaper-test-app");
            
                    ctx.Status("Hitting each endpoint as warmup...");
                    // Send warmup requests
                    await RunWarmupAsync(app);
                    // Send high load request to basic /ep
                    await using (var wrk = containers.wrk)
                    {
                        ctx.Spinner(Spinner.Known.Monkey);
                        ctx.Status("Starting wrk & executing high load simulation...");
                        await containers.wrk.StartAsync();
                        await Task.Delay(7000);
                        requestsSec = await GetTimerFromLogsAsDecimalAsync(wrk, "Requests/sec:");
                        await wrk.StopAsync();
                    }
                    memLoadTest = await DockerStats.GetMemoryUsageForContainer("reaper-test-app");
                    await app.StopAsync();
                }
                await network.DeleteAsync();

                AnsiConsole.MarkupLineInterpolated($"[dim]:check_mark_button:  Startup Time: {startupTime}ms, Startup Mem: {memStartup}, Req/s: {requestsSec}, End Mem: {memLoadTest}[/]");
            });

        return new()
        {
            StartupTimeMs = startupTime,
            MemoryStartup = memStartup,
            MemoryLoadTest = memLoadTest,
            Container = container,
            RequestsSec = requestsSec
        };
    }

    private async Task RunWarmupAsync(IContainer container)
    {
        var routes = new Dictionary<string, string>
        {
            {"/ep", "GET"},
            {"/typical/dosomething", "GET"},
            {"/typical/acceptsomething", "POST"},
            {"/typical/returnsomething", "POST"},
            {"/anothertypical/dosomething", "GET"},
            {"/anothertypical/acceptsomething", "POST"},
            {"/anothertypical/returnsomething", "POST"}
        };

        using (var httpClient = new HttpClient())
        {
            var sampleRequest = new SampleRequest
            {
                Input = "Warmup",
                SomeOtherInput = "Warmup",
                SomeBool = true
            };

            var stringContent = new StringContent(JsonSerializer.Serialize(sampleRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }), Encoding.UTF8, "application/json");
            foreach (var route in routes)
            {
                var requestUri = new UriBuilder(Uri.UriSchemeHttp, container.Hostname, container.GetMappedPublicPort(8080), route.Key).Uri;
                
                var request = new HttpRequestMessage(new HttpMethod(route.Value), requestUri);
                if (route.Value == "POST")
                {
                    request.Content = stringContent;
                }

                var resp = await httpClient.SendAsync(request);
                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception("Container failed to handle request " + route.Key + " with status: " + resp.StatusCode + ".");
                }
            }
        }
    }

    private async Task<string> GetTimerFromLogsAsync(IContainer container, string prefix)
    {
        var logs = await container.GetLogsAsync();
        var split = logs.Stdout.Split('\n');
        var valueLine = split.First(m => m.Contains(prefix));
        //Console.WriteLine(valueLine);
        var value = valueLine.Substring(valueLine.LastIndexOf(':') + 2).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        //Console.WriteLine(value);
        return value;
    }
    
    private async Task<decimal> GetTimerFromLogsAsDecimalAsync(IContainer container, string prefix) =>
        decimal.Parse(await GetTimerFromLogsAsync(container, prefix));
    
    private async Task<long> GetTimerFromLogsAsLongAsync(IContainer container, string prefix) =>
        long.Parse(await GetTimerFromLogsAsync(container, prefix));

    private async Task<IImage> CreateAppImageAsync(string container)
    {
        var appImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "benchmarks/BenchmarkWeb")
            .WithDockerfile(container + ".dockerfile")
            .WithDeleteIfExists(true)
            .Build();
        await appImage.CreateAsync();
        return appImage;
    }

    private (IContainer wrk, IContainer app) CreateContainersWithNetworkAsync(IImage appImage, INetwork network)
    {
        var wrk = new ContainerBuilder()
            .WithCommand("wrk", "-t5", "-c25", "-d5s", "http://reaper-test-app:8080/ep")
            .WithImage(wrkImage)
            .WithName("reaper-test-wrk")
            .WithNetwork(network)
            .WithWaitStrategy(Wait.ForUnixContainer())
            .Build();
        var app = new ContainerBuilder()
            .WithImage(appImage)
            .WithName("reaper-test-app")
            .WithNetwork(network)
            .WithNetworkAliases("reaper-test-app")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("BenchmarkWeb Startup: [0-9]+"))
            .Build();
        return (wrk, app);
    }
}

public record TestResult
{
    public string Container { get; set; }
    public long StartupTimeMs { get; set; }
    public decimal MemoryStartup { get; set; }
    public decimal MemoryLoadTest { get; set; }
    public decimal RequestsSec { get; set; }
}