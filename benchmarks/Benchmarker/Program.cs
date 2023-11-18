#if FALSE
using Benchmarker;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

var slnDir = CommonDirectoryPath.GetSolutionDirectory();
Console.WriteLine("Docker SLN Path {0}", slnDir.DirectoryPath);
var prjDir = CommonDirectoryPath.GetProjectDirectory();
Console.WriteLine("Docker Proj Path {0}", prjDir.DirectoryPath);

var image = new ImageFromDockerfileBuilder()
    .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "benchmarks/BenchmarkWeb")
    .WithDockerfile("minimal.dockerfile")
    .WithDeleteIfExists(true)
    .Build();
Console.WriteLine("Building Minimal API image...");
await image.CreateAsync();
Console.WriteLine("Built. Building wrk...");

var wrkImage = new ImageFromDockerfileBuilder()
    .WithDockerfileDirectory(prjDir, string.Empty)
    .WithDockerfile("wrk.dockerfile")
    .WithDeleteIfExists(true)
    .Build();
await wrkImage.CreateAsync();

var network = new NetworkBuilder()
    .WithName(Guid.NewGuid().ToString("D"))
    .Build();

var container = new ContainerBuilder()
    .WithName("reaper-test-cnt")
    .WithImage(image)
    .WithNetwork(network)
    .WithNetworkAliases("reaper-test-cnt")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/ep")))
    .Build();

var wrkContainer = new ContainerBuilder()
    .WithImage(wrkImage)
    .WithCommand("wrk", "-t5", "-c25", "-d5s", "http://reaper-test-cnt:8080/ep")
    .WithEntrypoint("")
    .WithNetwork(network)
    .WithWaitStrategy(Wait.ForUnixContainer())
    .Build();

await network.CreateAsync();

await container.StartAsync();

var memStartup = await DockerStats.GetMemoryUsageForContainer("reaper-test-cnt");

Console.WriteLine("Memory at Startup: " + memStartup);

Console.WriteLine("Beginning Primer... 10s");
await wrkContainer.StartAsync();

await Task.Delay(10000);

var logs = await wrkContainer.GetLogsAsync();
Console.WriteLine(logs.Stdout);
Console.WriteLine(logs.Stderr);

var memPrimed = await DockerStats.GetMemoryUsageForContainer("reaper-test-cnt");
Console.WriteLine("Memory after primer: " + memPrimed);
Console.WriteLine("Memory difference: " + (memPrimed - memStartup));

Console.WriteLine();
Console.WriteLine("====================================");
Console.WriteLine();

Console.WriteLine("Stopping");
if (wrkContainer.State == TestcontainersStates.Running)
{
    await wrkContainer.StopAsync();
}
await container.StopAsync();
await network.DeleteAsync();

Console.WriteLine("Stopped");
#else

using System.Text.Json;
using Benchmarker;

var testRunner = new TestRunner();
await testRunner.InitialiseAsync();
var carter = await testRunner.ExecuteTestAsync("carter");
Console.WriteLine(JsonSerializer.Serialize(carter));
var controllers = await testRunner.ExecuteTestAsync("controllers");
Console.WriteLine(JsonSerializer.Serialize(controllers));
var fastendpoints = await testRunner.ExecuteTestAsync("fastendpoints");
Console.WriteLine(JsonSerializer.Serialize(fastendpoints));
var minimal = await testRunner.ExecuteTestAsync("minimal");
Console.WriteLine(JsonSerializer.Serialize(minimal));
var reaper = await testRunner.ExecuteTestAsync("reaper");
Console.WriteLine(JsonSerializer.Serialize(reaper));


#endif