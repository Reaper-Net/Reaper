using System.Diagnostics;
using System.Runtime.CompilerServices;

#if REAPER
using Reaper;
#elif FASTEP
using FastEndpoints;
#elif CARTER
using Carter;
#endif

[assembly:InternalsVisibleTo("Benchmarker")]

var sw = new Stopwatch();
sw.Start();

var builder = WebApplication.CreateSlimBuilder(args);

#if REAPER
builder.UseReaper();
#elif FASTEP
builder.Services.AddFastEndpoints();
#elif CARTER
builder.Services.AddCarter();
#elif CTRL
builder.Services.AddControllers();
#endif

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
    sw.Stop();
    Console.WriteLine("BenchmarkWeb Startup: " + sw.ElapsedMilliseconds);
});

#if MINIMAL
app.MapGet("/ep", () => "Hello, World!");
#elif REAPER
app.UseReaperMiddleware();
app.MapReaperEndpoints();
#elif FASTEP
app.UseFastEndpoints();
#elif CARTER
app.MapCarter();
#elif CTRL
app.MapControllers();
#endif

app.Run();

namespace BenchmarkWeb
{
    public partial class Program
    {
    }
}