using System.Runtime.CompilerServices;
using Reaper;

[assembly: InternalsVisibleTo("IntegrationTests")]

var builder = WebApplication.CreateSlimBuilder(args);
builder.UseReaper();

var app = builder.Build();

app.UseReaperMiddleware();
app.MapReaperEndpoints();

app.Run();

public partial class Program { }