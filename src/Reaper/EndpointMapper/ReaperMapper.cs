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
}