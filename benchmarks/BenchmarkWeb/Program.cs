using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using BenchmarkWeb.Dtos;

#if REAPER
using Reaper;
#elif FASTEP
using FastEndpoints;
#elif CARTER
using Carter;
#elif MINIMAL
using Microsoft.AspNetCore.Mvc;
#endif

[assembly:InternalsVisibleTo("Benchmarker")]

var sw = new Stopwatch();
sw.Start();

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(
        0, AppJsonSerializerContext.Default);
});

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
app.MapGet("/typical/dosomething", () => TypedResults.Ok());
app.MapPost("/typical/acceptsomething", (SampleRequest req) => TypedResults.Ok());
app.MapPost("/typical/returnsomething", (SampleRequest req) => new SampleResponse
{
    Output = req.Input,
    SomeOtherOutput = req.SomeOtherInput,
    SomeBool = req.SomeBool,
    GeneratedAt = DateTime.UtcNow
});
app.MapGet("/anothertypical/dosomething", () => TypedResults.Ok());
app.MapPost("/anothertypical/acceptsomething", (SampleRequest req) => TypedResults.Ok());
app.MapPost("/anothertypical/returnsomething", (SampleRequest req) => new SampleResponse
{
    Output = req.Input,
    SomeOtherOutput = req.SomeOtherInput,
    SomeBool = req.SomeBool,
    GeneratedAt = DateTime.UtcNow
});
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

[JsonSerializable(typeof(SampleRequest))]
[JsonSerializable(typeof(SampleResponse))]
public partial class AppJsonSerializerContext : System.Text.Json.Serialization.JsonSerializerContext
{
    
}