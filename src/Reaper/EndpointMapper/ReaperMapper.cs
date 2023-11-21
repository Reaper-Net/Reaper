using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Reaper.RequestDelegateSupport;

namespace Reaper.EndpointMapper;

public static class ReaperEndpointMapper
{
    public static void MapReaperEndpoint<TEndpoint, TRequest, TRequestBody, TResponse>(
        this IEndpointRouteBuilder endpoints,
        ReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse> definition)
        where TEndpoint : ReaperEndpointBase
    {
        if (definition is HandlerReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>)
        {
            // TODO later, Mpack etc
        }
        else if (definition is DirectReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>)
        {
            MapRequestInternal(endpoints, (DirectReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse>)definition);
        }
    }

    internal static void MapRequestInternal<TEndpoint, TRequest, TRequestBody, TResponse>(
        IEndpointRouteBuilder endpoints,
        DirectReaperEndpointDefinition<TEndpoint, TRequest, TRequestBody, TResponse> definition)
        where TEndpoint: ReaperEndpointBase
    {
        var epBuilder = RouteHandlerServices.Map(endpoints,
            definition.Route,
            definition.Handler,
            new [] { definition.Verb },
            emptyMetadataResult,
            definition.RequestDelegateGenerator
        );
        if (definition.IsJsonRequest)
        {
            epBuilder.WithMetadata(new AcceptsMetadata(type: typeof(TRequestBody), isOptional: false, contentTypes: jsonContentType));
        }
    }

    private static readonly Func<MethodInfo, RequestDelegateFactoryOptions?, RequestDelegateMetadataResult> emptyMetadataResult =
        (_, _) => new() { EndpointMetadata = new List<object>() };
    private static readonly string[] jsonContentType = new [] { "application/json" };

    public static JsonOptions FallbackJsonOptions = new();

    /*private static void MapRequestInternal<TEndpoint, TRequest, TRequestBody, TResponse>(IEndpointRouteBuilder endpoints, ReaperEndpointBuilder<TEndpoint> builder)
        where TEndpoint : ReaperEndpointBase
    {
        Delegate handler;
        if (builder.RequestType == null && builder.ResponseType == null)
        {
            handler = async (TEndpoint svc) =>
            {
                await (svc as ReaperEndpoint)!.HandleAsync();
            };
        } else if (builder.RequestType != null && builder.ResponseType == null)
        {
            handler = async (TEndpoint svc, TRequest req) =>
            {
                await (svc as ReaperEndpointRX<TRequest>)!.HandleAsync(req);
            };
        } else if (builder.RequestType == null && builder.ResponseType != null)
        {
            handler = async (TEndpoint svc) => await (svc as ReaperEndpointXR<TResponse>)!.HandleAsync();
        }
        else
        {
            handler = async (TEndpoint svc, TRequest req) => await (svc as ReaperEndpoint<TRequest, TResponse>)!.HandleAsync(req);
        }

        var epBuilder = RouteHandlerServices.Map(endpoints,
            builder.Route,
            handler,
            new [] { builder.Verb },
            ((info, options) => new RequestDelegateMetadataResult { EndpointMetadata = new List<object>() }),
            CreateRequestDelegateMethod<TEndpoint, TRequest, TRequestBody, TResponse>(builder)
        );
        if (builder.RequestType != null)
        {
            var type = builder.RequestBodyType ?? builder.RequestType;
            epBuilder.WithMetadata(new AcceptsMetadata(type: type, isOptional: false, contentTypes: jsonContentType));
        }
    }

    private static Func<Delegate, RequestDelegateFactoryOptions, RequestDelegateMetadataResult?, RequestDelegateResult> CreateRequestDelegateMethod<TEndpoint, TRequest, TRequestBody, TResponse>(ReaperEndpointBuilder<TEndpoint> builder)
        where TEndpoint : ReaperEndpointBase
    {
        Func<Delegate, RequestDelegateFactoryOptions, RequestDelegateMetadataResult?, RequestDelegateResult> createRequestDelegate;

        JsonOptions FallbackJsonOptions = new();
        if (builder.RequestType == null && builder.ResponseType == null)
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var serviceProvider = options.ServiceProvider ?? options.EndpointBuilder.ApplicationServices;
                ReaperEndpoint ep = (serviceProvider.GetRequiredService<TEndpoint>() as ReaperEndpoint)!;
                Task Handler(HttpContext _) => ep.HandleAsync();
                return new RequestDelegateResult(Handler, ReadOnlyCollection<object>.Empty);
            };
        } else if (builder.RequestType != null && builder.ResponseType == null)
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var serviceProvider = options.ServiceProvider ?? options.EndpointBuilder.ApplicationServices;
                var logOrThrowExceptionHelper = new LogOrThrowExceptionHelper(serviceProvider, options);
                var jsonOptions = serviceProvider?.GetService<IOptions<JsonOptions>>()?.Value ?? FallbackJsonOptions;
                var jsonSerializerOptions = jsonOptions.SerializerOptions;
                jsonSerializerOptions.MakeReadOnly();

                RequestDelegate handler;
                if (typeof(TRequest) != typeof(TRequestBody))
                {
                    // If this differs, it means we have a FromBody to parse out and stick in the Request
                    var filler = (Action<TRequest, TRequestBody>)builder.RequestBodyFiller!;
                    handler = async (HttpContext ctx) =>
                    {
                        var ep = ctx.RequestServices.GetRequiredService<TEndpoint>();

                        var jsonTypeInfo = (JsonTypeInfo<TRequestBody>)jsonSerializerOptions.GetTypeInfo(typeof(TRequestBody));
                        var reqData = await JsonBodyResolver.TryResolveBodyAsync<TRequestBody>(ctx, logOrThrowExceptionHelper, typeof(TRequestBody).Name, "req", jsonTypeInfo);
                        if (!reqData.Item1)
                        {
                            return;
                        }

                        var map = Activator.CreateInstance<TRequest>();
                        filler(map, reqData.Item2!);

                        await ((Func<TEndpoint, TRequest, Task>)del)(ep, map);
                    };
                }
                else
                {
                    handler = async (HttpContext ctx) =>
                    {
                        var ep = ctx.RequestServices.GetRequiredService<TEndpoint>();

                        var jsonTypeInfo = (JsonTypeInfo<TRequest>)jsonSerializerOptions.GetTypeInfo(typeof(TRequest));
                        var reqData = await JsonBodyResolver.TryResolveBodyAsync<TRequest>(ctx, logOrThrowExceptionHelper, typeof(TRequest).Name, "req", jsonTypeInfo);
                        if (!reqData.Item1)
                        {
                            return;
                        }

                        await ((Func<TEndpoint, TRequest, Task>)del)(ep, reqData.Item2!);
                    };
                }

                return new RequestDelegateResult(handler, ReadOnlyCollection<object>.Empty);
            };
        } else if (builder.RequestType == null && builder.ResponseType != null)
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var serviceProvider = options.ServiceProvider ?? options.EndpointBuilder!.ApplicationServices;
                var logOrThrowExceptionHelper = new LogOrThrowExceptionHelper(serviceProvider, options);
                var jsonOptions = serviceProvider?.GetService<IOptions<JsonOptions>>()?.Value ?? FallbackJsonOptions;
                var jsonSerializerOptions = jsonOptions.SerializerOptions;
                jsonSerializerOptions.MakeReadOnly();
                var jsonTypeInfoResponse = (JsonTypeInfo<TResponse?>)jsonSerializerOptions.GetTypeInfo(typeof(TResponse));
                if (typeof(TResponse) == typeof(string))
                {
                    ReaperEndpointXR<string> ep = (serviceProvider!.GetRequiredService<TEndpoint>() as ReaperEndpointXR<string>)!;
                    async Task Handler(HttpContext ctx) => await ctx.Response.WriteAsync(await ep.HandleAsync());
                    return new RequestDelegateResult(Handler, ReadOnlyCollection<object>.Empty);
                }
                else
                {
                    ReaperEndpointXR<TResponse> ep = (serviceProvider!.GetRequiredService<TEndpoint>() as ReaperEndpointXR<TResponse>)!;
                    async Task Handler(HttpContext ctx) => await ResponseHelpers.ExecuteReturnAsync(await ep.HandleAsync(), ctx, jsonTypeInfoResponse);
                    return new RequestDelegateResult(Handler, ReadOnlyCollection<object>.Empty);
                }
            };
        }
        else
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var metadata = inferredMetadataResult?.EndpointMetadata ?? ReadOnlyCollection<object>.Empty;
                var serviceProvider = options.ServiceProvider ?? options.EndpointBuilder.ApplicationServices;
                var logOrThrowExceptionHelper = new LogOrThrowExceptionHelper(serviceProvider, options);
                var jsonOptions = serviceProvider?.GetService<IOptions<JsonOptions>>()?.Value ?? FallbackJsonOptions;
                var jsonSerializerOptions = jsonOptions.SerializerOptions;
                jsonSerializerOptions.MakeReadOnly();
                var jsonTypeInfoResponse = (JsonTypeInfo<TResponse?>)jsonSerializerOptions.GetTypeInfo(typeof(TResponse));


                var ep = serviceProvider!.GetRequiredService<TEndpoint>();

                RequestDelegate handler;
                if (typeof(TRequest) != typeof(TRequestBody))
                {
                    // If this differs, it means we have a FromBody to parse out and stick in the Request
                    var filler = (Action<TRequest, TRequestBody>)builder.RequestBodyFiller!;
                    var jsonTypeInfo = (JsonTypeInfo<TRequestBody>)jsonSerializerOptions.GetTypeInfo(typeof(TRequestBody));
                    handler = async (HttpContext ctx) =>
                    {
                        var reqData = await JsonBodyResolver.TryResolveBodyAsync<TRequestBody>(ctx, logOrThrowExceptionHelper, typeof(TRequestBody).Name, "req", jsonTypeInfo);
                        if (!reqData.Item1)
                        {
                            return;
                        }

                        var map = Activator.CreateInstance<TRequest>();
                        filler(map, reqData.Item2!);
                        var res = await ((Func<TEndpoint, TRequest, Task<TResponse>>)del)(ep, map);
                        await ResponseHelpers.ExecuteReturnAsync(res, ctx, jsonTypeInfoResponse);
                    };
                }
                else
                {
                    var jsonTypeInfo = (JsonTypeInfo<TRequest>)jsonSerializerOptions.GetTypeInfo(typeof(TRequest));
                    handler = async (HttpContext ctx) =>
                    {
                        var reqData = await JsonBodyResolver.TryResolveBodyAsync<TRequest>(ctx, logOrThrowExceptionHelper, typeof(TRequest).Name, "req", jsonTypeInfo);
                        if (!reqData.Item1)
                        {
                            return;
                        }

                        var res = await ((Func<TEndpoint, TRequest, Task<TResponse>>)del)(ep, reqData.Item2!);
                        await ResponseHelpers.ExecuteReturnAsync(res, ctx, jsonTypeInfoResponse);
                    };
                }

                return new RequestDelegateResult(handler, metadata);
            };
        }

        return createRequestDelegate;
    }

    private struct EmptyRequestResponse {}*/
}