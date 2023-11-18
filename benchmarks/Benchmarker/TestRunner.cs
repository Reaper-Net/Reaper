using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging.Abstractions;

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
        TestcontainersSettings.Logger = NullLogger.Instance;
        //ConsoleLogger.Instance.DebugLogLevelEnabled = true;
        await wrkImage.CreateAsync();
    }

    public async Task<TestResult> ExecuteTestAsync(string container)
    {
        var network = new NetworkBuilder()
            .WithName("reaper-test-network")
            .Build();
        var appImage = await CreateAppImageAsync(container);
        var containers = CreateContainersWithNetworkAsync(appImage, network);
        await network.CreateAsync();
        
        // Start the app image
        await containers.app.StartAsync();
        var output = await containers.app.GetLogsAsync();
        var split = output.Stdout.Split('\n');
        var startupTimeStr = split.First(m => m.Contains("BenchmarkWeb Startup:"));
        // Get startup time
        var startupTime = long.Parse(startupTimeStr.Substring(startupTimeStr.LastIndexOf(':') + 2).Split(' ')[0]);
        // Get memory usage
        var memStartup = await DockerStats.GetMemoryUsageForContainer("reaper-test-app");
        // Start the wrk container
        await containers.wrk.StartAsync();
        await Task.Delay(7000);
        output = await containers.wrk.GetLogsAsync();
        split = output.Stdout.Split('\n');
        var requestsSecStr = split.First(m => m.Contains("Requests/sec:"));
        var requestsSec = decimal.Parse(requestsSecStr.Substring(requestsSecStr.LastIndexOf(':') + 2));
        var memLoadTest = await DockerStats.GetMemoryUsageForContainer("reaper-test-app");
        await containers.wrk.StopAsync();
        await containers.wrk.DisposeAsync();
        await containers.app.StopAsync();
        await containers.app.DisposeAsync();
        await network.DeleteAsync();

        return new()
        {
            StartupTimeMs = startupTime,
            MemoryStartup = memStartup,
            MemoryLoadTest = memLoadTest,
            Container = container,
            RequestsSec = requestsSec
        };
    }

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