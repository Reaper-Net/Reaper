using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.AotTests;

public class AotTestFixture : IAsyncLifetime
{
    public IContainer Container { get; private set; }
    public HttpClient Client { get; private set; }
    
    public async Task InitializeAsync()
    {
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("tests/Reaper.TestWeb/Dockerfile")
            .Build();
        await image.CreateAsync();
        Container = new ContainerBuilder()
            .WithImage(image)
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .Build();
        await Container.StartAsync();

        var baseAddress = new UriBuilder(Uri.UriSchemeHttp, Container.Hostname, Container.GetMappedPublicPort(8080)).Uri;
        Client = new HttpClient()
        {
            BaseAddress = baseAddress
        };
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
        Client.Dispose();
    }
}