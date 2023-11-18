using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Benchmarker")]

var builder = WebApplication.CreateSlimBuilder(args);

var app = builder.Build();

#if MINIMAL
app.MapGet("/min-test", () => "Hello, World!");
#endif

app.Run();

namespace BenchmarkWeb
{
    public partial class Program
    {
    }
}