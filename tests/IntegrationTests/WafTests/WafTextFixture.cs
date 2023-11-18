using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.WafTests;

public class WafTextFixture : IAsyncLifetime
{
    public WebApplicationFactory<Program> App { get; private set; }
    public HttpClient Client { get; private set; }
    
    public Task InitializeAsync()
    {
        App = new WebApplicationFactory<Program>().WithWebHostBuilder(
            b =>
            {
                b.ConfigureLogging(l => l.ClearProviders().AddDebug());
            });
        Client = App.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await App.DisposeAsync();
    }
}