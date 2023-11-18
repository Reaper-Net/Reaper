using System.Runtime.CompilerServices;
using Reaper;
using Reaper.TestWeb;

[assembly: InternalsVisibleTo("IntegrationTests")]

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddScoped<HelloWorldProvider>();
builder.UseReaper();

var app = builder.Build();

app.UseReaperMiddleware();
app.MapReaperEndpoints();

app.Run();

public partial class Program { }