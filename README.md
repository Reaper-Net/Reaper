![Reaper](https://avatars.githubusercontent.com/u/151158047)

# Reaper

Reaper is a .NET 8+ source-generator based, AOT focused library for writing your endpoints as classes in ASP.NET Core.

Inspired by the awesome, and more full-featured [FastEndpoints](https://fast-endpoints.com/), Reaper aims to provide a
REPR pattern implementation experience with a focus on performance and simplicity.

## Motivation

Minimal APIs are great. They're fast, they're simple, and they're easy to reason about. However trying to separate
your endpoints results in a tonne of classes.

FastEndpoints is even better. It's fast (obviously), very well structured, very well documented, and provides a tonne
of excellent features out of the box.

But what if you want to sit in the middleground like us? Having FastEndpoints-style REPR endpoint definitions, but with
Native AOT, lower runtime overhead, and an even more minimal approach?

That's where we found ourselves whilst building out microservices to be deployed to ACA and looking for a super-simple
way to build our endpoints in a pattern that we know and love, with super minimalistic memory footprints and Minimal API
performance. For these smaller services, Minimal APIs were a better choice, but we wanted them to be more structured. For
larger services, FastEndpoints is still used and is likely a much better choice.

So is Reaper good for you? As with everything in development, it depends.

## Getting Started

Reaper only supports .NET 8+.

Add [Reaper.Core]() and [Reaper.SourceGenerator]() from NuGet.

Edit your csproj to allow the generated namespace:

```xml
<PropertyGroup>
    <InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);Reaper.Generated</InterceptorsPreviewNamespaces>
</PropertyGroup>
```

Add the following to your Program.cs:

```csharp
builder.UseReaper();

// ... var app = builder.Build(); ...

app.UseReaperMiddleware();
app.MapReaperEndpoints();
```

Create your first Reaper Endpoint:

```csharp
public class TestRequest
{
    public string Test { get; set; }
}

public class TestResponse
{
    public string Input { get; set; }
    public DateTime GeneratedAt { get; set; }
}

[ReaperRoute(HttpVerbs.Post, "reflector")]
public class ReflectorEndpoint : ReaperEndpoint<TestRequest, TestResponse>
{
    public override async Task<TestResponse> HandleAsync(TestRequest request)
    {
        return new TestResponse()
        {
            GeneratedAt = DateTime.UtcNow,
            Input = request.Test
        };
    }
}
```

Enjoy.

## Good Endpoints

The idea even if you have multiple input/output types, is to always consume and return a specific type. This not only
means Reaper doesn't have to do too much binding work at any point, it also helps your endpoints to be more well
defined.

For example, if you wanted to return a list from an endpoint then sure, you could do:

```csharp
public class AListingEndpoint : ReaperEndpointXR<List<MyDto>>
```

But in our very opinionated fashion, it's better to do:

```csharp
public class AListingResponse { public IReadOnlyCollection<MyDto> Items { get; set; } }

public class AListingEndpoint : ReaperEndpointXR<AListingResponse>
```

Why? Well, first off it's a bit more explicit, you're not using generic types for your endpoints, rather a defined DTO.
Also if you've ever built a real app, you'll know that _things change_, like a lot.

What if you did want to add something else to the return from this endpoint? Without changing your implementation of
clients etc, you can't. You'd have to change the type of the endpoint, which is a breaking change. With the above, you
could add additional properties with no cost (assuming your client serializer isn't too strict of course).

When it comes to *Request* objects, we take a different approach from what you may be used to in Minimal APIs or
Controllers, but we do reuse their `[FromBody]`, `[FromQuery]` and `[FromRoute]` attributes. It's more akin to what is
available in FastEndpoints, though more explicit as you may expect.

With a route of `/test/{id}`, you'd write something like this:

```csharp
// Controller Action
[HttpGet("/test/{id}")]
public IActionResult Test(int id) { /* ... */ }

// Minimal APIs
app.MapGet("/test/{id}", (int id) => { /* ... */ });

// FastEndpoints
public class RequestDto { public string Id { get; set; } }
public class TestEndpoint : Endpoint<RequestDto> { /* ... */ }

// Reaper
public class RequestDto { [FromRoute] public string Id { get; set; } }
public class TestEndpoint : ReaperEndpointRX<RequestDto> { /* ... */ }
```

Notice the explicit `[FromRoute]` attribute. This is because we don't want to do any magic binding above JSON in the body,
and we don't want to

### Other Endpoint Bases

Reaper provides a few other endpoint bases for your convenience:

```csharp
public class NothingEndpoint : ReaperEndpoint { /* Use the HttpContext for anything directly */ }
public class RequestOnlyEndpoint : ReaperEndpointRX<TRequest> { /* Use the Request only */ }
public class ResponseOnlyEndpoint : ReaperEndpointXR<TResponse> { /* Use the Response only */ }
```

### Native AOT Support

The core of Reaper is Native AOT compatible.

We currently generate a context for JSON Source Generation named `ReaperJsonSerializerContext` which will work for all
of your request and response objects.

It's also registered automatically against the HttpJsonOptions, if you need to use them elsewhere you can register it
in the `.TypeResolverChain` of your `JsonSerializerOptions` like this:


```csharp
options.SerializerOptions.TypeInfoResolverChain.Insert(0, ReaperJsonSerializerContext.Default);
```

If you are (de)serializing other types, it's recommended to create a new context with the objects you require.

### Implementation

Your Endpoint is injected as a *singleton*. This means that you should not store any state in your Endpoint (not that you
would anyway, right?). Your HandleAsync method is invoked on a per-request basis.

To resolve services, you can currently use the `HttpContext` which is exposed via `.Context` within your endpoints.

An example would be:

```csharp
var svc = Context.RequestServices.GetRequiredService<IMyService>();
```

### What's coming

- [ ] Convenience methods for sending responses, where the type is too restrictive
- [ ] Ability to bind Request object from route, etc (e.g per-prop `[FromRoute]`)
- [ ] Automatic (and customisable) Mapper support
- [x] Automatic generation of Source Generatable DTOs (Request/Response)
- [ ] More documentation
- [x] Tests, obvs
- [ ] More examples
- [ ] Support for [FluentValidation](https://github.com/FluentValidation/FluentValidation)
- [ ] Support for [MessagePack](https://github.com/MessagePack-CSharp/MessagePack-CSharp)
- [ ] Support for [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [ ] 🤨 Our own bare metal (read: Kestrel) routing implementation? Who knows. Maybe.

## Benchmarks

Our own internal tool for benchmarking is not scientific (it's mainly designed to compare our own relative performance
over time), but it does have somewhat representative results to our goals (below ordered by req/sec).

| Framework     | Startup Time | Memory Usage (MiB) - Startup | Memory Usage (MiB) - Load Test | Requests/sec |
|---------------|--------------|------------------------------|--------------------------------|--------------|
| minimal-aot   | 21           | 20.81                        | 26.96                          | 144059.81    |
| reaper-aot    | 21           | 18.89                        | 30.83                          | 139910.28    |
| minimal       | 103          | 21.68                        | 258.2                          | 123264.17    |
| reaper        | 109          | 20.41                        | 294.2                          | 121946.15    |
| carter        | 115          | 23.1                         | 269.6                          | 121725.32    |
| fastendpoints | 134          | 23.86                        | 303.6                          | 118512.82    |
| controllers   | 143          | 24.14                        | 308.9                          | 106056.19    |

We've submitted to the TechEmpower Framework Benchmark, however preliminary results (from an M1 Ultra, 128GB RAM) are
available for [plaintext](https://www.techempower.com/benchmarks/#section=test&shareid=75585734-6c92-4a79-8cc9-dab0979ffb38&hw=ph&test=plaintext)
and [json](https://www.techempower.com/benchmarks/#section=test&shareid=75585734-6c92-4a79-8cc9-dab0979ffb38&hw=ph&test=json).

## Prerelease notice

Reaper is currently in prerelease. It may or may not support everything you need. It may or may not be stable. It may or
may not be a good idea to use it in production.

Code is messy right now. What's committed is an early proof of concept. It's not pretty but it works. This will be
tidied up in due course.

We are building Reaper alongside our own microservice requirements which are currently running in production. If you
have any feedback, please feel free to open an issue or PR.