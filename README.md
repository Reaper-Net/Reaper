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
    public override async Task ExecuteAsync(TestRequest request)
    {
        Result = new TestResponse()
        {
            GeneratedAt = DateTime.UtcNow,
            Input = request.Test
        };
    }
}
```

Enjoy.

## Responses

(This is a recent change and a work in progress)

By default, your endpoint will return a 200 OK response with the Result property (which is typed). If you need to send
something other than the typed response, you can use the `StatusCode` method (or others below).

```csharp
public override async Task ExecuteAsync(MyRequestType request) {
    if (request.Something) {
        await BadRequest();
    }
    
    if (creditsAvailable < 0) {
        await StatusCode(402);
        return;
    }
    
    Result = new MyResponseType();
}
```

Convenience methods available are `Ok`, `NotFound`, `BadRequest`, `NoContent`, `InternalServerError`.

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
Also if you've ever built a real app, you'll know that _things change_, like, a lot.

What if you did want to add something else to the return from this endpoint? Without changing your implementation of
clients etc, you can't. You'd have to change the type of the endpoint, which is a breaking change. With the above, you
could add additional properties with no cost (assuming your client serializer isn't too strict of course).

### Request Binding

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
public class TestEndpoint : Endpoint<RequestDto> { 
    public override void Configure() {
        Get("/test/{id}");
    }
    /* ... */
}

// Reaper
public class RequestDto { [FromRoute] public string Id { get; set; } }
[ReaperRoute(HttpVerbs.Get, "/test/{id}")]
public class TestEndpoint : ReaperEndpointRX<RequestDto> { /* ... */ }
```

Notice the explicit `[FromRoute]` attribute. This is because there is no magic binding other than converting a whole
Request DTO from JSON.

What this means is, if you have any `[From*]` attributes, the request object will not be bound from JSON. If you need
this in addition, create another object (it can be nested, though make sure it's uniquely named for the JSON Source
Generator) and use it within the base Request DTO with `[FromBody]` for example:

```csharp
public class RequestDto
{
    [FromRoute] public string Id { get; set; }
    
    [FromBody] public RequestBodyDto Body { get; set; }
    
    public class RequestBodyDto
    {
        public string Name { get; set; }
    }
}
```

This follows the philosophy of "less magic" and more definition that is prevalent throughout Reaper.

### Other Endpoint Bases

Reaper provides a few other endpoint bases for your convenience:

```csharp
public class NothingEndpoint : ReaperEndpoint { /* Use the HttpContext for anything directly */ }
public class RequestOnlyEndpoint : ReaperEndpointRX<TRequest> { /* Use the Request only */ }
public class ResponseOnlyEndpoint : ReaperEndpointXR<TResponse> { /* Use the Response only */ }
```

### Validators

To add validation support (via FluentValidation), add [Reaper.Validation]() from NuGet.

Before your call to `UseReaper()`, also add `UseReaperValidation()`.

You can then create validators by extending  from `RequestValidator<TRequest>` and using FluentValidator just as you
normally would:

```csharp
public class TestRequest
{
    public string Test { get; set; }
}

public class TestRequestValidator : RequestValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Test).NotEmpty();
    }
}
```

Note again that validators are created as singletons so don't maintain state in your validator. Soon, the same type of
mapping will be applied as for endpoints (reusing `[ReaperScoped]`).

Validation results are formatted using a compatible version of the
[RFC7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807).

You can override the response by implementing your own `IValidationFailureHandler`, you're basically responsible for
doing everything to return a valid response (ie. you get the HttpContext, go wild). **This API is subject to change.**

For Native AOT support, the actual result type returned is `Reaper.Validation.Responses.ValidationProblemDetails` and
lives in its own JsonSerializerContext. If you're modifying the type returned yourself and need Native AOT support,
you'll need your own context (see below).

### Native AOT Support

The core of Reaper is Native AOT compatible.

We currently generate a context for JSON Source Generation named `ReaperJsonSerializerContext` which will work for all
of your request and response objects.

It's also registered automatically against the Http.JsonOptions, if you need to use them elsewhere you can register it
in the `.TypeResolverChain` of your `JsonSerializerOptions` like this:


```csharp
options.SerializerOptions.TypeInfoResolverChain.Insert(0, ReaperJsonSerializerContext.Default);
```

If you are (de)serializing other types, it's recommended to create a new context with the objects you require. Due to
the (super hacky) way that the context generator works, we're actually generating it in memory, so it's not possible to
extend our context with your own types (even if you add another partial class). There's a [huge discussion](https://github.com/dotnet/roslyn/issues/57239)
for chaining generators that is probably going nowhere, so we'll have to wait and see if this gets better.

### Implementation

Your Endpoint is injected as a *singleton* by default. This means that you should not store any state in your Endpoint
(not that you would anyway, right?). Your ExecuteAsync method is invoked on a per-request basis.

To resolve services further services (scoped etc), use the `Resolve<TService>()` method (which includes singletons etc).

An example would be:

```csharp
var myService = Resolve<IMyService>();
```

You can also use constructor injection, with the default rules being a singleton service being injected by the
constructor, and use Resolve for any scoped services.

Alternatively (though there may be a very minor performance hit), apply the `[ReaperScoped]` attribute to the endpoint
and constructor injection will work the same way as you may be familiar with.

### What's coming

- [x] Convenience methods for sending responses, where the type is too restrictive
- [x] Ability to bind Request object from route, etc (e.g per-prop `[FromRoute]`)
- [ ] Automatic (and customisable) Mapper support
- [x] Automatic generation of Source Generatable DTOs (Request/Response)
- [ ] More documentation
- [x] Tests, obvs
- [ ] More examples
- [x] Support for [FluentValidation](https://github.com/FluentValidation/FluentValidation)
- [ ] Support for [MessagePack](https://github.com/MessagePack-CSharp/MessagePack-CSharp)
- [ ] Support for [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [ ] 🤨 Our own bare metal (read: Kestrel) routing implementation? Who knows. Maybe.

## Benchmarks

Our own internal tool for benchmarking is not scientific (it's mainly designed to compare our own relative performance
over time), but it does have somewhat representative results to our goals (below ordered by req/sec).

This is a sample injecting a (singleton) service from the most recent version of our tool. The service simply creates
a memory stream, writes the "Hello, World!" string to it in 2 parts, reads it back as a string, and returns it to the
endpoint for sending back to the client.

The possible reason that we're faster in this scenario as we resolve the service up front, whereas Minimal APIs resolve
them per request as they support scoped. This is basically the exact scenario that we're working towards.

| Framework     | Startup Time | Memory Usage (MiB) - Startup | Memory Usage (MiB) - Load Test | Requests/sec |
|---------------|--------------|------------------------------|--------------------------------|--------------|
| reaper-aot    | 21           | 20                           | 88                             | 121,284      |
| minimal-aot   | 21           | 18                           | 85                             | 119,071      |
| reaper        | 108          | 20                           | 312                            | 110,220      |
| carter        | 118          | 20                           | 313                            | 106,719      |
| minimal       | 98           | 20                           | 313                            | 105,830      |
| fastendpoints | 137          | 23                           | 317                            | 99,591       |
| controllers   | 145          | 23                           | 316                            | 98,128       |

This is from our original benchmark tool which just hits an endpoint with no interaction.

| Framework     | Startup Time | Memory Usage (MiB) - Startup | Memory Usage (MiB) - Load Test | Requests/sec |
|---------------|--------------|------------------------------|--------------------------------|--------------|
| minimal-aot   | 21           | 21                           | 27                             | 144,060      |
| reaper-aot    | 21           | 19                           | 31                             | 139,910      |
| minimal       | 103          | 22                           | 258                            | 123,264      |
| reaper        | 109          | 20                           | 294                            | 121,946      |
| carter        | 115          | 23                           | 270                            | 121,725      |
| fastendpoints | 134          | 24                           | 304                            | 118,513      |
| controllers   | 143          | 24                           | 309                            | 106,056      |

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

**Important**: Note that the API is subject to change. We're trying to make things as customisable as possible without
sacrificing performance or, of course, AOT compatibility. But do note that in this version, if you've overridden certain
things, they may change in the future.