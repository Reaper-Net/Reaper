using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Reaper;
using Reaper.TestWeb;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

[assembly: InternalsVisibleTo("IntegrationTests")]

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddScoped<HelloWorldProvider>();
builder.Services.AddKeyedSingleton<HelloWorldProvider>("hw_singleton");

builder.UseReaperValidation();
builder.UseReaper();

var app = builder.Build();

var jsonOptions = app.Services.GetService<IOptions<JsonOptions>>()!.Value;

foreach (var item in jsonOptions.SerializerOptions.TypeInfoResolverChain)
{
    Console.WriteLine(item.GetType().FullName);
}
var jsonTypeInfo = (JsonTypeInfo<ProblemDetails?>)jsonOptions.SerializerOptions.GetTypeInfo(typeof(ProblemDetails));
Console.WriteLine(jsonTypeInfo);

app.UseReaperMiddleware();
app.MapReaperEndpoints();

app.Run();

public partial class Program { }