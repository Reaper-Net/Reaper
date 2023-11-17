using System.Collections.ObjectModel;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Reaper.MinimalSupport;

namespace Reaper.Mapper;

public static class ReaperMapper
{
    public static void MapEndpoint<TEndpoint>(IEndpointRouteBuilder endpoints, Action<ReaperEndpointBuilder<TEndpoint>> builderAction)
        where TEndpoint : ReaperEndpointBase
    {
        var endpointBuilder = new ReaperEndpointBuilder<TEndpoint>();
        builderAction(endpointBuilder);

        if (endpointBuilder.ReaperHandler)
        {
            // TODO later, Mpack etc
        }
        else
        {
            MapRequestInternal<TEndpoint, EmptyRequestResponse, EmptyRequestResponse>(endpoints, endpointBuilder);
        }
    }

    public static void MapEndpointWithRequest<TEndpoint, TRequest>(IEndpointRouteBuilder endpoints, Action<ReaperEndpointBuilder<TEndpoint>> builderAction)
        where TEndpoint : ReaperEndpointBase
    {
        var endpointBuilder = new ReaperEndpointBuilder<TEndpoint>();
        builderAction(endpointBuilder);
        MapRequestInternal<TEndpoint, TRequest, EmptyRequestResponse>(endpoints, endpointBuilder);
    }

    public static void MapEndpointWithResponse<TEndpoint, TResponse>(IEndpointRouteBuilder endpoints, Action<ReaperEndpointBuilder<TEndpoint>> builderAction)
        where TEndpoint : ReaperEndpointBase
    {
        var endpointBuilder = new ReaperEndpointBuilder<TEndpoint>();
        builderAction(endpointBuilder);
        MapRequestInternal<TEndpoint, EmptyRequestResponse, TResponse>(endpoints, endpointBuilder);
    }

    public static void MapEndpointWithRequestAndResponse<TEndpoint, TRequest, TResponse>(IEndpointRouteBuilder endpoints, Action<ReaperEndpointBuilder<TEndpoint>> builderAction)
        where TEndpoint : ReaperEndpointBase
    {
        var endpointBuilder = new ReaperEndpointBuilder<TEndpoint>();
        builderAction(endpointBuilder);
        
        MapRequestInternal<TEndpoint, TRequest, TResponse>(endpoints, endpointBuilder);
    }

    private static readonly string[] jsonContentType = new [] { "application/json" };

    private static void MapRequestInternal<TEndpoint, TRequest, TResponse>(IEndpointRouteBuilder endpoints, ReaperEndpointBuilder<TEndpoint> builder)
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
            CreateRequestDelegateMethod<TEndpoint, TRequest, TResponse>(builder)
        );
        if (builder.RequestType != null)
        {
            epBuilder.WithMetadata(new AcceptsMetadata(type: builder.RequestType, isOptional: false, contentTypes: jsonContentType));
        }
    }

    private static Func<Delegate, RequestDelegateFactoryOptions, RequestDelegateMetadataResult?, RequestDelegateResult> CreateRequestDelegateMethod<TEndpoint, TRequest, TResponse>(ReaperEndpointBuilder<TEndpoint> builder)
        where TEndpoint : ReaperEndpointBase
    {
        Func<Delegate, RequestDelegateFactoryOptions, RequestDelegateMetadataResult?, RequestDelegateResult> createRequestDelegate;
        
        JsonOptions FallbackJsonOptions = new();
        if (builder.RequestType == null && builder.ResponseType == null)
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var metadata = inferredMetadataResult?.EndpointMetadata ?? ReadOnlyCollection<object>.Empty;

                RequestDelegate handler = async (HttpContext ctx) =>
                {
                    var ep = ctx.RequestServices.GetRequiredService<TEndpoint>();
                    await ((Func<TEndpoint, Task>)del)(ep);
                };

                return new RequestDelegateResult(handler, metadata);
            };
        } else if (builder.RequestType != null && builder.ResponseType == null)
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var metadata = inferredMetadataResult?.EndpointMetadata ?? ReadOnlyCollection<object>.Empty;
                var serviceProvider = options.ServiceProvider ?? options.EndpointBuilder.ApplicationServices;
                var logOrThrowExceptionHelper = new LogOrThrowExceptionHelper(serviceProvider, options);
                var jsonOptions = serviceProvider?.GetService<IOptions<JsonOptions>>()?.Value ?? FallbackJsonOptions;
                var jsonSerializerOptions = jsonOptions.SerializerOptions;
                jsonSerializerOptions.MakeReadOnly();
                
                RequestDelegate handler = async (HttpContext ctx) =>
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

                return new RequestDelegateResult(handler, metadata);
            };
        } else if (builder.RequestType == null && builder.ResponseType != null)
        {
            createRequestDelegate = (del, options, inferredMetadataResult) =>
            {
                var metadata = inferredMetadataResult?.EndpointMetadata ?? ReadOnlyCollection<object>.Empty;
                var serviceProvider = options.ServiceProvider ?? options.EndpointBuilder.ApplicationServices;
                var logOrThrowExceptionHelper = new LogOrThrowExceptionHelper(serviceProvider, options);
                var jsonOptions = serviceProvider?.GetService<IOptions<JsonOptions>>()?.Value ?? FallbackJsonOptions;
                var jsonSerializerOptions = jsonOptions.SerializerOptions;
                jsonSerializerOptions.MakeReadOnly();
                
                RequestDelegate handler = async (HttpContext ctx) =>
                {
                    var ep = ctx.RequestServices.GetRequiredService<TEndpoint>();
                    
                    var jsonTypeInfoResponse = (JsonTypeInfo<TResponse?>)jsonSerializerOptions.GetTypeInfo(typeof(TResponse));
                    var res = await ((Func<TEndpoint, Task<TResponse>>)del)(ep);
                    await ResponseHelpers.ExecuteReturnAsync(res, ctx, jsonTypeInfoResponse);
                };

                return new RequestDelegateResult(handler, metadata);
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
                
                RequestDelegate handler = async (HttpContext ctx) =>
                {
                    var ep = ctx.RequestServices.GetRequiredService<TEndpoint>();
                    
                    var jsonTypeInfo = (JsonTypeInfo<TRequest>)jsonSerializerOptions.GetTypeInfo(typeof(TRequest));
                    var jsonTypeInfoResponse = (JsonTypeInfo<TResponse?>)jsonSerializerOptions.GetTypeInfo(typeof(TResponse));
                    var reqData = await JsonBodyResolver.TryResolveBodyAsync<TRequest>(ctx, logOrThrowExceptionHelper, typeof(TRequest).Name, "req", jsonTypeInfo);
                    if (!reqData.Item1)
                    {
                        return;
                    }
                    var res = await ((Func<TEndpoint, TRequest, Task<TResponse>>)del)(ep, reqData.Item2!);
                    await ResponseHelpers.ExecuteReturnAsync(res, ctx, jsonTypeInfoResponse);
                };

                return new RequestDelegateResult(handler, metadata);
            };
        }

        return createRequestDelegate;
    }
    
    private struct EmptyRequestResponse {}
}