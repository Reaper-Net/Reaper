![Reaper](https://avatars.githubusercontent.com/u/151158047)

# Reaper

Reaper is a .NET 8+ library and source generator for building Minimal APIs, but not as you know them.

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

### Other Endpoint Bases

Reaper provides a few other endpoint bases for your convenience:

```csharp
public class NothingEndpoint : ReaperEndpoint { /* Use the HttpContext for anything directly */ }
public class RequestOnlyEndpoint : ReaperEndpointRX<TRequest> { /* Use the Request only */ }
public class ResponseOnlyEndpoint : ReaperEndpointXR<TResponse> { /* Use the Response only */ }
```

### Native AOT Support

The core of Reaper is Native AOT compatible but you'll (currently!) need to use JSON Source Generation on your
Request/Response objects. [See official guidance](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0)
on how to do this, and you have to configure the internal Serializer Options to use the context. Essentially, it's
what is below, but see the [Minimal API Request Delegate Generator Guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/aot/request-delegate-generator/rdg?view=aspnetcore-8.0)
for more.

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(
                         0, AppJsonSerializerContext.Default);
});
```

Note that this will be enabled / completed for you automatically in the future.

### Implementation

Your Endpoint is injected as a *singleton*. This means that you should not store any state in your Endpoint (not that you
would anyway, right?). Your HandleAsync method is invoked on a per-request basis.

### To resolve services

Currently, the `HttpContext` is exposed via `.Context` within your endpoints.

To resolve services (Scoped or otherwise), simply use:

```csharp
var svc = Context.RequestServices.GetRequiredService<IMyService>();
```

### What's coming

- [ ] Convenience methods for sending responses, where the type is too restrictive
- [ ] Ability to bind Request object from route, etc (e.g per-prop `[FromRoute]`)
- [ ] Automatic (and customisable) Mapper support
- [ ] Automatic generation of Source Generatable DTOs (Request/Response)
- [ ] More documentation
- [ ] Tests, obvs
- [ ] More examples
- [ ] Support for [FluentValidation](https://github.com/FluentValidation/FluentValidation)
- [ ] Support for [MessagePack](https://github.com/MessagePack-CSharp/MessagePack-CSharp)
- [ ] Support for [MemoryPack](https://github.com/Cysharp/MemoryPack)

## Prerelease notice

Reaper is currently in prerelease. It may or may not support everything you need. It may or may not be stable. It may or
may not be a good idea to use it in production.

Code is messy right now. What's committed is an early proof of concept. It's not pretty but it works. This will be
tidied up in due course.

We are building Reaper alongside our own microservice requirements which are currently running in production. If you
have any feedback, please feel free to open an issue or PR.